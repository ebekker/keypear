using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Google.Protobuf.Collections;
using Grpc.Core;
using Keypear.Server.GrpcServer.RpcModel;
using Keypear.Server.Shared.Data;
using Keypear.Server.Shared.Models.Persisted;
using Keypear.Shared.Utils;
using Microsoft.EntityFrameworkCore;
using static Keypear.Server.GrpcClient.GrpcUtils;

namespace Keypear.Server.GrpcServer.Services;

public class KyprService : KyprCore.KyprCoreBase
{
    private readonly ILogger _logger;
    private readonly KyprDbContext _db;

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

        var sess = new KyprSession
        {
            SessionId = Guid.NewGuid().ToString(),
            SessionKey = FromBytes(acct.Id.ToByteArray()),
        };
        return new()
        {
            Session = sess,
        };
    }

    public override async Task<GetAccountResult> GetAccount(
       GetAccountInput input, ServerCallContext context)
    {
        ThrowIfNull(input.Username);

        var details = await GetAccountByUsernameAsync(input.Username);

        if (details == null)
        {
            return new()
            {
                Account = null,
            };
        }

        return new()
        {
            Account = details,
        };
    }

    public override async Task<CreateVaultResult> CreateVault(
        CreateVaultInput input, ServerCallContext context)
    {
        var acct = await ResolveSession(context);

        ThrowIfNull(input.Vault);
        ThrowIfNull(input.Vault.SecretKeyEnc);
        ThrowIfNull(input.Vault.SummaryEnc);

        var vault = new Vault
        {
            Id = Guid.NewGuid(),
            CreatedDateTime = DateTime.Now,
            SummaryEnc = ToBytes(input.Vault.SummaryEnc),
        };

        var grant = new Grant
        {
            VaultId = vault.Id,
            AccountId = acct.Id,
            CreatedDateTime = DateTime.Now,
            SecretKeyEnc = ToBytes(input.Vault.SecretKeyEnc),
        };

        _db.Vaults.Add(vault);
        _db.Grants.Add(grant);
        await _db.SaveChangesAsync();

        return new()
        {
            Vault = new()
            {
                VaultId = vault.Id.ToString(),
                SecretKeyEnc = FromBytes(grant.SecretKeyEnc),
                SummaryEnc = FromBytes(vault.SummaryEnc),
            }
        };
    }

    public override async Task<SaveVaultResult> SaveVault(SaveVaultInput input,
        ServerCallContext context)
    {
        var acct = await ResolveSession(context);

        ThrowIfNull(input.Vault);
        ThrowIfNull(input.Vault.VaultId);
        ThrowIfNull(input.Vault.SecretKeyEnc);
        ThrowIfNull(input.Vault.SummaryEnc);

        var vaultId = Guid.Parse(input.Vault.VaultId);

        var grant = await _db.Grants
            .Include(x => x.Vault)
            .Where(x => x.VaultId == vaultId
                    && x.AccountId == acct.Id).SingleOrDefaultAsync();

        ThrowIfNull(grant,
            messageFormat: "invalid or inaccessible Vault reference",
            statusCode: StatusCode.NotFound);

        var vault = grant.Vault!;
        vault.SummaryEnc = ToBytes(input.Vault.SummaryEnc);
        await _db.SaveChangesAsync();

        return new()
        {
            VaultId = vault.Id.ToString(),
        };
    }

    public override async Task<ListVaultsResult> ListVaults(ListVaultsInput input,
        ServerCallContext context)
    {
        var acct = await ResolveSession(context);

        var vaultIds = await _db.Grants
            .Where(x => x.AccountId == acct.Id)
            .Select(x => x.VaultId.ToString())
            .ToListAsync();

        var result = new ListVaultsResult();
        result.VaultIds.AddRange(vaultIds);
        return result;
    }

    public override async Task<GetVaultResult> GetVault(GetVaultInput input,
        ServerCallContext context)
    {
        var acct = await ResolveSession(context);

        ThrowIfNull(input.VaultId);

        var vaultId = Guid.Parse(input.VaultId);
        var grant = await _db.Grants
            .Include(x => x.Vault)
            .Where(x => x.AccountId == acct.Id
                    && x.VaultId == vaultId)
            .SingleOrDefaultAsync();

        if (grant == null)
        {
            return null;
        }

        return new()
        {
            Vault = new()
            {
                VaultId = grant.Vault!.Id.ToString(),
                SecretKeyEnc = FromBytes(grant.SecretKeyEnc),
                SummaryEnc = FromBytes(grant.Vault.SummaryEnc),
            },
        };
    }

    public override async Task<SaveRecordResult> SaveRecord(SaveRecordInput input,
        ServerCallContext context)
    {
        var acct = await ResolveSession(context);

        ThrowIfNull(input.Record);
        ThrowIfNull(input.Record.VaultId);
        ThrowIfNull(input.Record.SummaryEnc);
        ThrowIfNull(input.Record.ContentEnc);

        var vaultId = Guid.Parse(input.Record.VaultId);
        var grant = await _db.Grants
            .Include(x => x.Vault)
            .Where(x => x.AccountId == acct.Id
                    && x.VaultId == vaultId)
            .SingleOrDefaultAsync();

        ThrowIfNull(grant,
            messageFormat: "invalid or inaccessible Vault reference",
            statusCode: StatusCode.NotFound);

        var x = Guid.TryParse(input.Record.RecordId, out var y);
        _logger.LogInformation($"Parsing Record ID: [{input.Record.RecordId == null}][{input.Record.RecordId}][{x}][{y}]");
        Guid? recordId = input.Record.RecordId == null
            ? null
            : Guid.Parse(input.Record.RecordId);
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

        record.SummaryEnc = ToBytes(input.Record.SummaryEnc);
        record.ContentEnc = ToBytes(input.Record.ContentEnc);

        await _db.SaveChangesAsync();

        return new()
        {
            Record = new()
            {
                RecordId = record.Id.ToString(),
                VaultId = record.VaultId.ToString(),
                SummaryEnc = FromBytes(record.SummaryEnc),
                ContentEnc = FromBytes(record.ContentEnc),
            },
        };
    }

    public override async Task<GetRecordsResult> GetRecords(GetRecordsInput input,
        ServerCallContext context)
    {
        var acct = await ResolveSession(context);

        ThrowIfNull(input.VaultId);

        var vaultId = Guid.Parse(input.VaultId);
        var grant = await _db.Grants
                    .Include(x => x.Vault)
                    .Where(x => x.AccountId == acct.Id
                            && x.VaultId == vaultId)
                    .SingleOrDefaultAsync();

        ThrowIfNull(grant,
            messageFormat: "invalid or inaccessible Vault",
            statusCode: StatusCode.NotFound);

        var records = await _db.Records
            .Where(x => x.VaultId == vaultId)
            .ToListAsync();

        var result = new GetRecordsResult();
        result.Records.AddRange(records.Select(x =>
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

    private async Task<Account> ResolveSession(ServerCallContext context)
    {
        var acct = await TryResolveSession(context);

        ThrowIfNull(acct,
            messageFormat: "invalid account",
            statusCode: StatusCode.Unauthenticated);

        return acct;
    }

    private async Task<Account?> TryResolveSession(ServerCallContext context)
    {
        var sessionId = context.RequestHeaders.GetValue(SessionIdHeaderName);
        var sessionKey = context.RequestHeaders.GetValueBytes(SessionKeyHeaderName);

        ThrowIfNull(sessionId, statusCode: StatusCode.Unauthenticated);
        ThrowIfNull(sessionKey, statusCode: StatusCode.Unauthenticated);

        var acctId = new Guid(sessionKey);
        return await _db.Accounts.FirstOrDefaultAsync(
            x => x.Id == acctId);
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
