// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Grpc.Core;
using Grpc.Net.Client;
using Keypear.Shared;
using Keypear.Shared.Models.Service;
using RPC = Keypear.Server.GrpcServer.RpcModel;
using static Keypear.Server.GrpcClient.GrpcUtils;
using Keypear.Shared.Utils;

namespace Keypear.Server.GrpcClient;

public class ServiceClient : IKyprServer
{
    private readonly ServiceClientBuilder _builder;
    private readonly GrpcChannel _channel;
    private readonly RPC.KyprCore.KyprCoreClient _grpcClient;

    private KyprSession? _session;
    private Metadata? _sessionHeaders;

    internal ServiceClient(ServiceClientBuilder builder,
        string address, KyprSession? session)
    {
        _builder = builder;
        _channel = GrpcChannel.ForAddress(address);
        _grpcClient = new RPC.KyprCore.KyprCoreClient(_channel);

        _session = session;
    }

    internal ServiceClient(ServiceClientBuilder builder,
        GrpcChannel channel, KyprSession? session)
    {
        _builder = builder;
        _channel = channel;
        _grpcClient = new RPC.KyprCore.KyprCoreClient(_channel);

        _session = session;
    }

    public void Dispose()
    {
        _channel.Dispose();
    }

    public KyprSession? Session
    {
        get => _session;
        set
        {
            _session = value;
            if (_session != null)
            {
                _sessionHeaders = new Metadata
                {
                    new(SessionIdHeaderName, _session.SessionId),
                    new(SessionKeyHeaderName, _session.SessionKey!),
                };
            }
        }
    }

    public async Task<AccountDetails?> GetAccountAsync(string username)
    {
        ArgumentNullException.ThrowIfNull(username);

        var input = new RPC.GetAccountInput
        {
            Username = username,
        };

        var result = await _grpcClient.GetAccountAsync(input);
        if (result.Account == null)
        {
            return null;
        }

        return new()
        {
            AccountId = Guid.Parse(result.Account.AccountId),
            Username = result.Account.Username,
            MasterKeySalt = ToBytes(result.Account.MasterKeySalt),
            PublicKey = ToBytes(result.Account.PublicKey),
            PrivateKeyEnc = ToBytes(result.Account.PrivateKeyEnc),
            SigPublicKey = ToBytes(result.Account.SigPublicKey),
            SigPrivateKeyEnc = ToBytes(result.Account.SigPrivateKeyEnc),
        };
    }

    public async Task<AccountDetails> CreateAccountAsync(AccountDetails input)
    {
        KpCommon.ThrowIfNull(input.Username);
        KpCommon.ThrowIfNull(input.PublicKey);
        KpCommon.ThrowIfNull(input.PrivateKeyEnc);
        KpCommon.ThrowIfNull(input.SigPublicKey);
        KpCommon.ThrowIfNull(input.SigPrivateKeyEnc);

        var result = await _grpcClient.CreateAccountAsync(new()
        {
            Account = new()
            {
                Username = input.Username,
                MasterKeySalt = FromBytes(input.MasterKeySalt),
                PublicKey = FromBytes(input.PublicKey),
                PrivateKeyEnc = FromBytes(input.PrivateKeyEnc),
                SigPublicKey = FromBytes(input.SigPublicKey),
                SigPrivateKeyEnc = FromBytes(input.SigPrivateKeyEnc),
            },
        });

        KpCommon.ThrowIfNull(result.Account);

        return new()
        {
            AccountId = Guid.Parse(result.Account.AccountId),
            Username = result.Account.Username,
            MasterKeySalt = ToBytes(result.Account.MasterKeySalt),
            PublicKey = ToBytes(result.Account.PublicKey),
            PrivateKeyEnc = ToBytes(result.Account.PrivateKeyEnc),
            SigPublicKey = ToBytes(result.Account.SigPublicKey),
            SigPrivateKeyEnc = ToBytes(result.Account.SigPrivateKeyEnc),
        };
    }

    public async Task<KyprSession> AuthenticateAccountAsync(AccountDetails input)
    {
        KpCommon.ThrowIfNull(input.AccountId);

        var result = await _grpcClient.AuthenticateAccountAsync(new()
        {
            AccountId = input.AccountId?.ToString(),
        });

        KpCommon.ThrowIfNull(result.Session);
        KpCommon.ThrowIfNull(result.Session.SessionId);
        KpCommon.ThrowIfNull(result.Session.SessionKey);

        var session = new KyprSession
        {
            SessionId = result.Session.SessionId,
            SessionKey = ToBytes(result.Session.SessionKey),
        };

        Session = session;

        return session;
    }

    public async Task<VaultDetails> CreateVaultAsync(VaultDetails input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.SecretKeyEnc);
        ArgumentNullException.ThrowIfNull(input.SummaryEnc);

        KpCommon.ThrowIfNull(_session);
        KpCommon.ThrowIfNull(_sessionHeaders);

        var result = await _grpcClient.CreateVaultAsync(new()
        {
            Vault = new()
            {
                SecretKeyEnc = FromBytes(input.SecretKeyEnc),
                SummaryEnc = FromBytes(input.SummaryEnc),
            },
        }, headers: _sessionHeaders);

        KpCommon.ThrowIfNull(result.Vault);

        return new()
        {
            VaultId = Guid.Parse(result.Vault.VaultId),
            SecretKeyEnc = ToBytes(result.Vault.SecretKeyEnc),
            SummaryEnc = ToBytes(result.Vault.SummaryEnc),
        };
    }

    public async Task<Guid> SaveVaultAsync(VaultDetails input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.VaultId);
        ArgumentNullException.ThrowIfNull(input.SecretKeyEnc);
        ArgumentNullException.ThrowIfNull(input.SummaryEnc);

        KpCommon.ThrowIfNull(_session);
        KpCommon.ThrowIfNull(_sessionHeaders);

        var result = await _grpcClient.SaveVaultAsync(new()
        {
            Vault = new()
            {
                VaultId = input.VaultId?.ToString(),
                SecretKeyEnc = FromBytes(input.SecretKeyEnc),
                SummaryEnc = FromBytes(input.SummaryEnc),
            }
        }, headers: _sessionHeaders);

        KpCommon.ThrowIfNull(result.VaultId);

        return Guid.Parse(result.VaultId);
    }

    public async Task<Guid[]> ListVaultsAsync()
    {
        KpCommon.ThrowIfNull(_session);
        KpCommon.ThrowIfNull(_sessionHeaders);

        var result = await _grpcClient.ListVaultsAsync(new(),
            headers: _sessionHeaders);

        KpCommon.ThrowIfNull(result.VaultIds);

        return result.VaultIds.Select(x => Guid.Parse(x)).ToArray();
    }

    public async Task<VaultDetails?> GetVaultAsync(Guid vaultId)
    {
        KpCommon.ThrowIfNull(_session);
        KpCommon.ThrowIfNull(_sessionHeaders);

        var result = await _grpcClient.GetVaultAsync(new()
        {
            VaultId = vaultId.ToString(),
        }, headers: _sessionHeaders);

        if (result.Vault == null)
        {
            return null;
        }

        return new()
        {
            VaultId = Guid.Parse(result.Vault.VaultId),
            SecretKeyEnc = ToBytes(result.Vault.SecretKeyEnc),
            SummaryEnc = ToBytes(result.Vault.SummaryEnc),
        };
    }

     public async Task<RecordDetails> SaveRecordAsync(RecordDetails input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.VaultId);
        ArgumentNullException.ThrowIfNull(input.SummaryEnc);
        ArgumentNullException.ThrowIfNull(input.ContentEnc);

        KpCommon.ThrowIfNull(_session);
        KpCommon.ThrowIfNull(_sessionHeaders);

        var result = await _grpcClient.SaveRecordAsync(new()
        {
            Record = new()
            {
                RecordId = input.RecordId?.ToString(),
                VaultId = input.VaultId?.ToString(),
                SummaryEnc = FromBytes(input.SummaryEnc),
                ContentEnc = FromBytes(input.ContentEnc),
            }
        }, headers: _sessionHeaders);

        KpCommon.ThrowIfNull(result.Record);

        return new()
        {
            RecordId = Guid.Parse(result.Record.RecordId),
            VaultId = Guid.Parse(result.Record.VaultId),
            SummaryEnc = ToBytes(result.Record.SummaryEnc),
            ContentEnc = ToBytes(result.Record.ContentEnc),
        };
    }

    public async Task<RecordDetails[]> GetRecordsAsync(Guid vaultId)
    {
        KpCommon.ThrowIfNull(_session);
        KpCommon.ThrowIfNull(_sessionHeaders);

        var result = await _grpcClient.GetRecordsAsync(new()
        {
            VaultId = vaultId.ToString(),
        }, headers: _sessionHeaders);

        KpCommon.ThrowIfNull(result.Records);

        return result.Records.Select(x => new RecordDetails
        {
            RecordId = Guid.Parse(x.RecordId),
            VaultId = Guid.Parse(x.VaultId),
            SummaryEnc = ToBytes(x.SummaryEnc),
            ContentEnc = ToBytes(x.ContentEnc),
        }).ToArray();
    }
}
