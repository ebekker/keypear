using Keypear.Shared;
using Keypear.Shared.Models.Persisted;
using Keypear.Shared.Models.Service;
using Keypear.Shared.Utils;
using Microsoft.EntityFrameworkCore;

namespace Keypear.Server.LocalServer;
public class ServerImpl : IKyprServer
{
    private readonly KyprDbContext _db;
    private Account? _account;

    public ServerImpl(KyprDbContext db)
    {
        _db = db;
    }

    public void Dispose()
    { }

    public async Task AuthenticateAccountAsync(AccountDetails input)
    {
        KpCommon.ThrowIfNull(input.AccountId);

        var acct = await _db.Accounts.FirstOrDefaultAsync(
            x => x.Id == input.AccountId.Value);

        if (acct == null)
        {
            throw new Exception("invalid Account");
        }

        _account = acct;
    }

    public async Task<AccountDetails> CreateAccountAsync(AccountDetails input)
    {
        KpCommon.ThrowIfNull(input.Username);
        KpCommon.ThrowIfNull(input.PublicKey);
        KpCommon.ThrowIfNull(input.PrivateKeyEnc);
        KpCommon.ThrowIfNull(input.SigPublicKey);
        KpCommon.ThrowIfNull(input.SigPrivateKeyEnc);

        if (null != await GetAccountAsync(input.Username))
        {
            throw new Exception("duplicate username found");
        }

        var account = new Account
        {
            CreatedDateTime = DateTime.Now,
            Username = input.Username,
            MasterKeySalt = input.MasterKeySalt,
            PublicKey = input.PublicKey,
            PrivateKeyEnc = input.PrivateKeyEnc,
            SigPublicKey = input.SigPublicKey,
            SigPrivateKeyEnc = input.SigPrivateKeyEnc,
        };
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        return (await GetAccountAsync(input.Username))!;
    }

    public async Task<AccountDetails?> GetAccountAsync(string username)
    {
        ArgumentNullException.ThrowIfNull(username);

        var acct = await _db.Accounts.FirstOrDefaultAsync(
            x => x.Username == username);

        if (acct == null)
        {
            return null;
        }

        return new AccountDetails
        {
            AccountId = acct.Id,
            Username = acct.Username,
            MasterKeySalt = acct.MasterKeySalt,
            PublicKey = acct.PublicKey,
            PrivateKeyEnc = acct.PrivateKeyEnc,
            SigPublicKey = acct.SigPublicKey,
            SigPrivateKeyEnc = acct.SigPrivateKeyEnc,
        };
    }

    public async Task<VaultDetails> CreateVaultAsync(VaultDetails input)
    {
        KpCommon.ThrowIfNull(_account, "client is not authenticated");

        var vault = new Vault
        {
            Id = Guid.NewGuid(),
            CreatedDateTime = DateTime.Now,
            SummaryEnc = input.SummaryEnc,
            FastContentEnc = input.FastContentEnc,
            FullContentEnc = input.FullContentEnc,
        };

        var grant = new Grant
        {
            VaultId = vault.Id,
            AccountId = _account.Id,
            CreatedDateTime = DateTime.Now,
            SecretKeyEnc = input.SecretKeyEnc,
        };

        _db.Vaults.Add(vault);
        _db.Grants.Add(grant);
        await _db.SaveChangesAsync();

        return new()
        {
            VaultId = vault.Id,
            SecretKeyEnc = input.SecretKeyEnc,
            SummaryEnc = input.SummaryEnc,
            FastContentEnc = input.FastContentEnc,
            FullContentEnc = input.FullContentEnc,
        };
    }

    public async Task<Guid[]> ListVaultsAsync()
    {
        KpCommon.ThrowIfNull(_account, "client is not authenticated");

        return await _db.Vaults.Select(x => x.Id).ToArrayAsync();
    }

    public async Task<VaultDetails?> GetVaultAsync(Guid vaultId)
    {
        KpCommon.ThrowIfNull(_account, "client is not authenticated");

        var grant = await _db.Grants
            .Include(x => x.Vault)
            .Where(x => x.VaultId == vaultId
                    && x.AccountId == _account.Id).SingleOrDefaultAsync();

        if (grant == null)
        {
            return null;
        }

        return new()
        {
            VaultId = grant.Vault!.Id,
            SecretKeyEnc = grant.SecretKeyEnc,
            SummaryEnc = grant.Vault.SummaryEnc,
            FastContentEnc = grant.Vault.FastContentEnc,
            FullContentEnc = grant.Vault.FullContentEnc,
        };
    }

    public async Task<RecordDetails> SaveRecordAsync(RecordDetails input)
    {
        KpCommon.ThrowIfNull(_account, "client is not authenticated");
        KpCommon.ThrowIfNull(input.VaultId);

        var grant = await _db.Grants
            .Include(x => x.Vault)
            .Where(x => x.VaultId == input.VaultId
                    && x.AccountId == _account.Id).SingleOrDefaultAsync();

        if (grant == null)
        {
            throw new Exception("invalid or inaccessible Vault reference");
        }

        var newRecord = input.RecordId == null;
        var record = newRecord
            ? new Record { VaultId = input.VaultId.Value, CreatedDateTime = DateTime.Now }
            : await _db.Records.Where(x => x.Id == input.RecordId
                    && x.VaultId == input.VaultId).SingleOrDefaultAsync();

        if (record == null)
        {
            throw new Exception("invalid or inaccessible existing Record");
        }

        record.SummaryEnc = input.SummaryEnc;
        record.ContentEnc = input.ContentEnc;
        if (newRecord)
        {
            _db.Records.Add(record);
        }

        await _db.SaveChangesAsync();
        input.RecordId = record.Id;
        return input;
    }

    public async Task<RecordDetails[]> GetRecordsAsync(Guid vaultId)
    {
        KpCommon.ThrowIfNull(_account, "client is not authenticated");

        var grant = await _db.Grants
            .Include(x => x.Vault)
            .Where(x => x.VaultId == vaultId
                    && x.AccountId == _account.Id).SingleOrDefaultAsync();

        if (grant == null)
        {
            throw new Exception("invalid or inaccessible Vault");
        }

        var records = await _db.Records.Where(x => x.VaultId == vaultId).ToListAsync();

        return records.Select(x => new RecordDetails
        {
            RecordId = x.Id,
            VaultId = x.VaultId,
            SummaryEnc = x.SummaryEnc,
            ContentEnc = x.ContentEnc,
        }).ToArray();
    }
}
