using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Grpc.Core;
using Keypear.Server.GrpcServer.RpcModel;
using Keypear.Server.Shared.Data;
using Keypear.Server.Shared.Models.Persisted;
using Keypear.Shared.Krypto;
using Microsoft.EntityFrameworkCore;
using static Keypear.Server.GrpcClient.GrpcUtils;

namespace Keypear.Server.GrpcServer.Services;

public class KyprService : KyprCore.KyprCoreBase
{
    private static readonly SecretKeyEncryption _ske = new();

    private readonly ILogger _logger;
    private readonly KyprDbContext _db;

    private static readonly Dictionary<string, SessionDetails> _sessions = new();

    public KyprService(ILogger<KyprService> logger, KyprDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public override async Task<CreateAccountResult> CreateAccount(
        CreateAccountInput input, ServerCallContext context)
    {
        ThrowIfNull(input.Account);
        ThrowIfNull(input.Account.Username);
        ThrowIfNull(input.Account.PublicKey);
        ThrowIfNull(input.Account.PrivateKeyEnc);
        ThrowIfNull(input.Account.SigPublicKey);
        ThrowIfNull(input.Account.SigPrivateKeyEnc);

        _logger.LogInformation("Creating Account: " + input.Account.Username);

        var acct = await _db.Accounts.FirstOrDefaultAsync(
            x => x.Username == input.Account.Username);
        if (acct != null)
        {
            throw new RpcException(new Status(
                StatusCode.AlreadyExists,
                "duplicate username found"));
        }

        var account = new Account
        {
            CreatedDateTime = DateTime.Now,
            Username = input.Account.Username,
            MasterKeySalt = ToBytes(input.Account.MasterKeySalt),
            PublicKey = ToBytes(input.Account.PublicKey),
            PrivateKeyEnc = ToBytes(input.Account.PrivateKeyEnc),
            SigPublicKey = ToBytes(input.Account.SigPublicKey),
            SigPrivateKeyEnc = ToBytes(input.Account.SigPrivateKeyEnc),
        };

        // TODO: need to do a lock on username to make sure no
        // chance of duplicate usernames created during this time

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        var details = await GetAccountByUsernameAsync(input.Account.Username);
        ThrowIfNull(details,
            messageFormat: "failed to create Account",
            statusCode: StatusCode.Internal);

        return new()
        {
            Account = details,
        };
    }

    public override async Task<AuthenticateAccountResult> AuthenticateAccount(
        AuthenticateAccountInput input, ServerCallContext context)
    {
        ThrowIfNull(input.AccountId);

        var acctId = Guid.Parse(input.AccountId);
        var acct = await _db.Accounts.FirstOrDefaultAsync(
            x => x.Id == acctId);

        ThrowIfNull(acct,
            messageFormat: "invalid Account",
            statusCode: StatusCode.NotFound);

        var sess = CreateSession(acct);

        return new()
        {
            Session = sess,
        };
    }

    public override async Task<GetAccountResult> GetAccount(
       GetAccountInput input, ServerCallContext context)
    {
        ThrowIfNull(input.InnerMessage);
        ThrowIfNull(input.InnerMessage.Username);

        var details = await GetAccountByUsernameAsync(input.InnerMessage.Username);

        if (details == null)
        {
            return new()
            {
                InnerMessage = new()
                {
                    Account = null,
                }
            };
        }

        return new()
        {
            InnerMessage = new()
            {
                Account = details,
            }
        };
    }

    public override async Task<CreateVaultResult> CreateVault(
        CreateVaultInput input, ServerCallContext context)
    {
        var sess = await ResolveSession(context);

        ThrowIfNull(input.InnerMessage);
        ThrowIfNull(input.InnerMessage.Vault);
        ThrowIfNull(input.InnerMessage.Vault.SecretKeyEnc);
        ThrowIfNull(input.InnerMessage.Vault.SummaryEnc);

        var vault = new Vault
        {
            Id = Guid.NewGuid(),
            CreatedDateTime = DateTime.Now,
            SummaryEnc = ToBytes(input.InnerMessage.Vault.SummaryEnc),
        };

        var grant = new Grant
        {
            VaultId = vault.Id,
            AccountId = sess.acct.Id,
            CreatedDateTime = DateTime.Now,
            SecretKeyEnc = ToBytes(input.InnerMessage.Vault.SecretKeyEnc),
        };

        _db.Vaults.Add(vault);
        _db.Grants.Add(grant);
        await _db.SaveChangesAsync();

        return new()
        {
            InnerMessage = new()
            {
                Vault = new()
                {
                    VaultId = vault.Id.ToString(),
                    SecretKeyEnc = FromBytes(grant.SecretKeyEnc),
                    SummaryEnc = FromBytes(vault.SummaryEnc),
                }
            },
        };
    }

    public override async Task<SaveVaultResult> SaveVault(SaveVaultInput input,
        ServerCallContext context)
    {
        var sess = await ResolveSession(context);

        ThrowIfNull(input.InnerMessage);
        ThrowIfNull(input.InnerMessage.Vault);
        ThrowIfNull(input.InnerMessage.Vault.VaultId);
        ThrowIfNull(input.InnerMessage.Vault.SecretKeyEnc);
        ThrowIfNull(input.InnerMessage.Vault.SummaryEnc);

        var vaultId = Guid.Parse(input.InnerMessage.Vault.VaultId);

        var grant = await _db.Grants
            .Include(x => x.Vault)
            .Where(x => x.VaultId == vaultId
                    && x.AccountId == sess.acct.Id).SingleOrDefaultAsync();

        ThrowIfNull(grant,
            messageFormat: "invalid or inaccessible Vault reference",
            statusCode: StatusCode.NotFound);

        var vault = grant.Vault!;
        vault.SummaryEnc = ToBytes(input.InnerMessage.Vault.SummaryEnc);
        await _db.SaveChangesAsync();

        return new()
        {
            InnerMessage = new()
            {
                VaultId = vault.Id.ToString(),
            }
        };
    }

    public override async Task<ListVaultsResult> ListVaults(ListVaultsInput input,
        ServerCallContext context)
    {
        var sess = await ResolveSession(context);

        var vaultIds = await _db.Grants
            .Where(x => x.AccountId == sess.acct.Id)
            .Select(x => x.VaultId.ToString())
            .ToListAsync();

        var result = new ListVaultsResult()
        {
            InnerMessage = new(),
        };
        result.InnerMessage.VaultIds.AddRange(vaultIds);

        return result;
    }

    public override async Task<GetVaultResult?> GetVault(GetVaultInput input,
        ServerCallContext context)
    {
        var sess = await ResolveSession(context);

        ThrowIfNull(input.InnerMessage);
        ThrowIfNull(input.InnerMessage.VaultId);

        var vaultId = Guid.Parse(input.InnerMessage.VaultId);
        var grant = await _db.Grants
            .Include(x => x.Vault)
            .Where(x => x.AccountId == sess.acct.Id
                    && x.VaultId == vaultId)
            .SingleOrDefaultAsync();

        if (grant == null)
        {
            return null;
        }

        return new()
        {
            InnerMessage = new()
            {
                Vault = new()
                {
                    VaultId = grant.Vault!.Id.ToString(),
                    SecretKeyEnc = FromBytes(grant.SecretKeyEnc),
                    SummaryEnc = FromBytes(grant.Vault.SummaryEnc),
                },
            }
        };
    }

    public override async Task<SaveRecordResult> SaveRecord(SaveRecordInput input,
        ServerCallContext context)
    {
        var sess = await ResolveSession(context);

        ThrowIfNull(input.InnerMessage);
        ThrowIfNull(input.InnerMessage.Record);
        ThrowIfNull(input.InnerMessage.Record.VaultId);
        ThrowIfNull(input.InnerMessage.Record.SummaryEnc);
        ThrowIfNull(input.InnerMessage.Record.ContentEnc);

        var vaultId = Guid.Parse(input.InnerMessage.Record.VaultId);
        var grant = await _db.Grants
            .Include(x => x.Vault)
            .Where(x => x.AccountId == sess.acct.Id
                    && x.VaultId == vaultId)
            .SingleOrDefaultAsync();

        ThrowIfNull(grant,
            messageFormat: "invalid or inaccessible Vault reference",
            statusCode: StatusCode.NotFound);

        var x = Guid.TryParse(input.InnerMessage.Record.RecordId, out var y);
        _logger.LogInformation($"Parsing Record ID: [{input.InnerMessage.Record.RecordId == null}][{input.InnerMessage.Record.RecordId}][{x}][{y}]");
        Guid? recordId = input.InnerMessage.Record.RecordId == null
            ? null
            : Guid.Parse(input.InnerMessage.Record.RecordId);
        Record? record;
        if (recordId == null)
        {
            record = new()
            {
                VaultId = vaultId,
                CreatedDateTime = DateTime.Now,
            };
            _db.Records.Add(record);
        }
        else
        {
            record = await _db.Records
                .Where(x => x.Id == recordId.Value
                        && x.VaultId == vaultId)
                .SingleOrDefaultAsync();
        }

        ThrowIfNull(record,
            messageFormat: "invalid or inaccessible existing Record",
            statusCode: StatusCode.NotFound);

        record.SummaryEnc = ToBytes(input.InnerMessage.Record.SummaryEnc);
        record.ContentEnc = ToBytes(input.InnerMessage.Record.ContentEnc);

        await _db.SaveChangesAsync();

        return new()
        {
            InnerMessage = new()
            {
                Record = new()
                {
                    RecordId = record.Id.ToString(),
                    VaultId = record.VaultId.ToString(),
                    SummaryEnc = FromBytes(record.SummaryEnc),
                    ContentEnc = FromBytes(record.ContentEnc),
                },
            }
        };
    }

    public override async Task<GetRecordsResult> GetRecords(GetRecordsInput input,
        ServerCallContext context)
    {
        var sess = await ResolveSession(context);

        ThrowIfNull(input.InnerMessage);
        ThrowIfNull(input.InnerMessage.VaultId);

        var vaultId = Guid.Parse(input.InnerMessage.VaultId);
        var grant = await _db.Grants
                    .Include(x => x.Vault)
                    .Where(x => x.AccountId == sess.acct.Id
                            && x.VaultId == vaultId)
                    .SingleOrDefaultAsync();

        ThrowIfNull(grant,
            messageFormat: "invalid or inaccessible Vault",
            statusCode: StatusCode.NotFound);

        var records = await _db.Records
            .Where(x => x.VaultId == vaultId)
            .ToListAsync();

        var result = new GetRecordsResult
        {
            InnerMessage = new(),
        };
        result.InnerMessage.Records.AddRange(records.Select(x =>
            new RecordDetails()
            {
                RecordId = x.Id.ToString(),
                VaultId = x.VaultId.ToString(),
                SummaryEnc = FromBytes(x.SummaryEnc),
                ContentEnc = FromBytes(x.ContentEnc),
            }));

        return result;
    }

    // ----------------------------------------------------------------

    public static void ThrowIfNull(
        [NotNull] object? value,
        [CallerArgumentExpression("value")] string? valueName = null,
        string messageFormat = "value {0} is null",
        StatusCode statusCode = StatusCode.InvalidArgument)
    {
        if (value == null)
        {
            throw new RpcException(new Status(
                statusCode,
                String.Format(messageFormat, valueName)));
        }
    }

    public static (SecretKeyEncryption ske, byte[] key)? GetEncryption(Metadata headers)
    {
        var sessionId = headers.GetValue(SessionIdHeaderName);
        //var sessionKey = context.RequestHeaders.GetValueBytes(SessionKeyHeaderName);

        ThrowIfNull(sessionId, statusCode: StatusCode.Unauthenticated);
        //ThrowIfNull(sessionKey, statusCode: StatusCode.Unauthenticated);

        if (_sessions.TryGetValue(sessionId, out var details))
        {
            return (_ske, details.Key)!;
        }
        else
        {
            return null;
        }
    }

    public class SessionDetails
    {
        public string? Id { get; set; }
        public Guid AccountId { get; set; }
        public string? Algor { get; set; }
        public byte[]? Key { get; set; }
    }

    private KyprSession CreateSession(Account acct)
    {
        var details = new SessionDetails
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = acct.Id,
            Algor = _ske.Algor,
            Key = _ske.GenerateKey(),
        };

        var sess = new KyprSession
        {
            SessionId = details.Id,
            EncryptionAlgor = details.Algor,
            EncryptionKey = FromBytes(details.Key),
        };

        _sessions.Add(details.Id, details);

        return sess;
    }

    private async Task<(SessionDetails details, Account acct)> ResolveSession(ServerCallContext context)
    {
        var (details, acct) = await TryResolveSession(context);

        ThrowIfNull(details,
            messageFormat: "invalid or missing session",
            statusCode: StatusCode.Unauthenticated);
        ThrowIfNull(acct,
            messageFormat: "invalid account",
            statusCode: StatusCode.Unauthenticated);

        return (details, acct);
    }

    private async Task<(SessionDetails? details, Account? acct)> TryResolveSession(ServerCallContext context)
    {
        var sessionId = context.RequestHeaders.GetValue(SessionIdHeaderName);
        //var sessionKey = context.RequestHeaders.GetValueBytes(SessionKeyHeaderName);

        ThrowIfNull(sessionId, statusCode: StatusCode.Unauthenticated);
        //ThrowIfNull(sessionKey, statusCode: StatusCode.Unauthenticated);

        if (_sessions.TryGetValue(sessionId, out var details))
        {
            var acct = await _db.Accounts.FirstOrDefaultAsync(
                x => x.Id == details.AccountId);
            return (details, acct);
        }
        else
        {
            return (null, null);
        }
    }

    private async Task<AccountDetails?> GetAccountByUsernameAsync(string username)
    {
        var acct = await _db.Accounts.FirstOrDefaultAsync(
            x => x.Username == username);

        if (acct == null)
        {
            return null;
        }

        return new AccountDetails
        {
            AccountId = acct.Id.ToString(),
            Username = acct.Username,
            MasterKeySalt = FromBytes(acct.MasterKeySalt),
            PublicKey = FromBytes(acct.PublicKey),
            PrivateKeyEnc = FromBytes(acct.PrivateKeyEnc),
            SigPublicKey = FromBytes(acct.SigPublicKey),
            SigPrivateKeyEnc = FromBytes(acct.SigPrivateKeyEnc),
        };
    }
}
