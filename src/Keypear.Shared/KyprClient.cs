// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.Krypto;
using Keypear.Shared.Models.InMemory;
using Keypear.Shared.Models.Inner;
using Keypear.Shared.Utils;

namespace Keypear.Shared;

public class KyprClient : IDisposable
{
    private readonly IKyprServer _server;

    private readonly PbKeyDerivation _pbkd = new();
    private readonly SecretKeyEncryption _ske = new();
    private readonly PublicKeyEncryption _pke = new();
    private readonly PublicKeySignature _pks = new();

    public KyprClient(IKyprServer server)
    {
        _server = server;
    }

    public KyprSession? Session { get; set; }
    public Account? Account { get; set; }
    public List<Vault> Vaults { get; set; } = new();

    public async Task CreateAccountAsync(string username, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        var passBytes = KpEncoding.NormalizeEncode(password);

        var acct = new Account
        {
            Username = username,
        };

        acct.MasterKeyAlgor = _pbkd.Algor;
        acct.MasterKeySalt = _pbkd.GenerateSalt();
        acct.MasterKey = _pbkd.DeriveKey(passBytes, acct.MasterKeySalt, _ske.SecretKeySize);

        acct.PublicKeyAlgor = _ske.Algor;
        _pke.GenerateKeyPair(out var prvKey, out var pubKey);
        acct.PublicKey = pubKey;
        acct.PrivateKey = prvKey;
        acct.PrivateKeyEnc = _ske.Encrypt(prvKey, acct.MasterKey);

        acct.SigPublicKeyAlgor = _pks.Algor;
        _pks.GenerateKeyPair(out prvKey, out pubKey);
        acct.SigPublicKey = pubKey;
        acct.SigPrivateKey = prvKey;
        acct.SigPrivateKeyEnc = _ske.Encrypt(prvKey, acct.MasterKey);

        var details = await _server.CreateAccountAsync(new()
        {
            Username = acct.Username,
            MasterKeySalt = acct.MasterKeySalt,
            PublicKey = acct.PublicKey,
            PrivateKeyEnc = acct.PrivateKeyEnc,
            SigPublicKey = acct.SigPublicKey,
            SigPrivateKeyEnc = acct.SigPrivateKeyEnc,
        });

        acct.Id = details.AccountId;
        Account = acct;

        Session = await _server.AuthenticateAccountAsync(details);
    }

    public async Task AuthenticateAccountAsync(string username, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        var passBytes = KpEncoding.NormalizeEncode(password);

        var details = await _server.GetAccountAsync(username);
        if (details == null)
        {
            throw new Exception("authentication failed");
        }

        var acct = new Account
        {
            Id = details.AccountId,
            Username = username,
        };

        acct.MasterKeyAlgor = _pbkd.Algor;
        acct.MasterKeySalt = details.MasterKeySalt;
        acct.MasterKey = _pbkd.DeriveKey(passBytes, details.MasterKeySalt!, _ske.SecretKeySize);

        acct.PublicKeyAlgor = _ske.Algor;
        acct.PublicKey = details.PublicKey;
        acct.PrivateKeyEnc = details.PrivateKeyEnc;
        acct.PrivateKey = _ske.Decrypt(details.PrivateKeyEnc!, acct.MasterKey);

        acct.SigPublicKeyAlgor = _pks.Algor;
        acct.SigPublicKey = details.SigPublicKey;
        acct.SigPrivateKeyEnc = details.SigPrivateKeyEnc;
        acct.SigPrivateKey = _ske.Decrypt(details.SigPrivateKeyEnc!, acct.MasterKey);

        Account = acct;

        Session = await _server.AuthenticateAccountAsync(details);
    }

    public bool IsAccountLocked()
    {
        KpCommon.ThrowIfNull(Account);

        return Account.MasterKey == null;
    }

    public void LockAccount()
    {
        KpCommon.ThrowIfNull(Account);

        Account.AnchorKey = null;
        Account.MasterKey = null;
        Account.PrivateKey = null;
        Account.SigPrivateKey = null;

        foreach (var v in Vaults)
        {
            LockVault(v);
        }
    }

    public void UnlockAccount(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        KpCommon.ThrowIfNull(Account);
        KpCommon.ThrowIfNull(Account.MasterKeySalt);

        KpCommon.ThrowIfNull(Account.PrivateKeyEnc);
        KpCommon.ThrowIfNull(Account.SigPrivateKeyEnc);

        var passBytes = KpEncoding.NormalizeEncode(password);
        Account.MasterKey = _pbkd.DeriveKey(passBytes, Account.MasterKeySalt, _ske.SecretKeySize);

        Account.PrivateKey = _ske.Decrypt(Account.PrivateKeyEnc, Account.MasterKey);
        Account.SigPrivateKey = _ske.Decrypt(Account.SigPrivateKeyEnc, Account.MasterKey);
    }

    public IEnumerable<Vault> ListVaults() => Vaults;

    /// <summary>
    /// Newly created vaults are inintially in the unlocked state.
    /// </summary>
    public async Task<Vault> CreateVaultAsync(string label)
    {
        if (Account == null)
        {
            throw new InvalidOperationException("no account associated with this client");
        }

        var vault = new Vault();
        vault.Summary = new();
        vault.Summary.Label = label;
        vault.SecretKeyAlgor = _ske.Algor;
        vault.SecretKey = _ske.GenerateKey();
        vault.SecretKeyEnc = _ske.Encrypt(vault.SecretKey, Account.MasterKey!);
        vault.SummarySer = KpMsgPack.Ser(vault.Summary);
        vault.SummaryEnc = _ske.Encrypt(vault.SummarySer, vault.SecretKey);

        var fastContent = KpMsgPack.Ser(vault.Records.Select(x => x.Summary).ToList());
        var fullContent = KpMsgPack.Ser(vault.Records.Select(x => x.Content).ToList());

        var details = await _server.CreateVaultAsync(new()
        {
            SecretKeyEnc = vault.SecretKeyEnc,
            SummaryEnc = vault.SummaryEnc,
            FastContentEnc = _ske.Encrypt(fastContent, vault.SecretKey),
            FullContentEnc = _ske.Encrypt(fullContent, vault.SecretKey),
        });

        vault.Id = details.VaultId;
        Vaults.Add(vault);

        return vault;
    }

    /// <summary>
    /// Refreshed vaults are initially in the locked state.
    /// </summary>
    public async Task RefreshVaultsAsync()
    {
        if (Account == null)
        {
            throw new InvalidOperationException("no account associated with this client");
        }

        foreach (var v in Vaults)
        {
            LockRecords(v);
            LockVault(v);
        }
        Vaults.Clear();

        var vaultIds = await _server.ListVaultsAsync();
        foreach (var vid in vaultIds)
        {
            var vaultDetails = (await _server.GetVaultAsync(vid))!;
            var recordsDetails = await _server.GetRecordsAsync(vid);

            // Assemble and store the Vaults and Records into a "locked" state
            var vault = new Vault()
            {
                Id = vaultDetails.VaultId,
                SecretKeyAlgor = _ske.Algor,
                SecretKeyEnc = vaultDetails.SecretKeyEnc,
                SummaryEnc = vaultDetails.SummaryEnc,
            };

            foreach (var rd in recordsDetails)
            {
                vault.Records.Add(new()
                {
                    Id = rd.RecordId,
                    SummaryEnc = rd.SummaryEnc,
                    ContentEnc = rd.ContentEnc,
                });
            }

            Vaults.Add(vault);
        }
    }

    public bool IsVaultLocked(Vault vault)
    {
        return vault.SecretKey == null;
    }

    public void LockVault(Vault vault)
    {
        vault.SecretKey = null;
        vault.Summary = null;
        vault.SummarySer = null;
        LockRecords(vault);
    }

    public void UnlockVault(Vault vault)
    {
        if (IsAccountLocked())
        {
            throw new InvalidOperationException("cannot unlock Vault with locked Account");
        }

        KpCommon.ThrowIfNull(vault.SecretKeyEnc);
        KpCommon.ThrowIfNull(vault.SummaryEnc);
        KpCommon.ThrowIfNull(Account!.MasterKey);

        vault.SecretKey = _ske.Decrypt(vault.SecretKeyEnc, Account.MasterKey);
        vault.SummarySer = _ske.Decrypt(vault.SummaryEnc!, vault.SecretKey);
        vault.Summary = KpMsgPack.Des<VaultSummary>(vault.SummarySer);
    }

    public void UnlockVaults()
    {
        foreach (var v in ListVaults())
        {
            if (IsVaultLocked(v))
            {
                UnlockVault(v);
            }
        }
    }

    public void LockVaults()
    {
        foreach (var v in ListVaults())
        {
            LockVault(v);
        }
    }

    public async Task<Guid> SaveRecordAsync(Vault vault, Record record)
    {
        if (IsVaultLocked(vault))
        {
            throw new InvalidOperationException("cannot access Records with locked Vault");
        }

        ArgumentNullException.ThrowIfNull(vault);
        ArgumentNullException.ThrowIfNull(record);

        record.SummarySer = KpMsgPack.Ser(record.Summary);
        record.SummaryEnc = _ske.Encrypt(record.SummarySer, vault.SecretKey!);

        record.ContentSer = KpMsgPack.Ser(record.Content);
        record.ContentEnc = _ske.Encrypt(record.ContentSer, vault.SecretKey!);

        var details = await _server.SaveRecordAsync(new()
        {
            VaultId = vault.Id,
            SummaryEnc = record.SummaryEnc,
            ContentEnc = record.ContentEnc,
        });

        record.Id = details.RecordId;
        vault.Records.Add(record);

        return record.Id.Value;
    }

    public bool IsRecordLocked(Record record)
    {
        return record.ContentEnc == null;
    }

    public void LockRecord(Record record)
    {
        record.Summary = null;
        record.SummarySer = null;

        record.Content = null;
        record.ContentSer = null;
    }

    public void UnlockRecord(Vault vault, Record record)
    {
        if (IsVaultLocked(vault))
        {
            throw new InvalidOperationException("cannot access Records with locked Vault");
        }

        KpCommon.ThrowIfNull(record.SummaryEnc);
        KpCommon.ThrowIfNull(record.ContentEnc);

        record.SummarySer = _ske.Decrypt(record.SummaryEnc, vault.SecretKey!);
        record.Summary = KpMsgPack.Des<RecordSummary>(record.SummarySer);

        record.ContentSer = _ske.Decrypt(record.ContentEnc, vault.SecretKey!);
        record.Content = KpMsgPack.Des<RecordContent>(record.ContentSer);
    }

    public void LockRecords(Vault vault)
    {
        foreach (var r in vault.Records)
        {
            LockRecord(r);
        }
    }

    public void UnlockRecordSummaries(Vault vault)
    {
        if (IsVaultLocked(vault))
        {
            throw new InvalidOperationException("cannot access Records with locked Vault");
        }

        foreach (var r in vault.Records)
        {
            if (r.SummaryEnc == null)
            {
                continue;
            }

            r.SummarySer = _ske.Decrypt(r.SummaryEnc, vault.SecretKey!);
            r.Summary = KpMsgPack.Des<RecordSummary>(r.SummarySer);
        }
    }

    public IEnumerable<Record> ListRecords(Vault vault)
    {
        if (IsVaultLocked(vault))
        {
            throw new InvalidOperationException("cannot access Records with locked Vault");
        }

        return vault.Records.Where(x => x.DeletedDateTime == null);
    }

    public IEnumerable<Record> ListDeletedRecords(Vault vault)
    {
        if (IsVaultLocked(vault))
        {
            throw new InvalidOperationException("cannot access Records with locked Vault");
        }

        return vault.Records
            .Where(x => x.DeletedDateTime != null)
            .OrderByDescending(x => x.DeletedDateTime);
    }

    public IEnumerable<Record> SearchRecords(Vault vault, string q)
    {
        if (IsVaultLocked(vault))
        {
            throw new InvalidOperationException("cannot access Records with locked Vault");
        }

        foreach (var r in vault.Records.Where(x => x.DeletedDateTime == null))
        {
            if (r.Summary is RecordSummary rs)
            {
                if (KpCommon.Search(q, rs.Type, rs.Label, rs.Username, rs.Address, rs.Tags))
                {
                    yield return r;
                }
            }
        }
    }

    public void Dispose()
    {
        foreach (var v in Vaults)
        {
            LockRecords(v);
            LockVault(v);
        }
        Vaults.Clear();

        if (Account != null)
        {
            LockAccount();
            Account = null;
        }

        Session = null;
    }
}
