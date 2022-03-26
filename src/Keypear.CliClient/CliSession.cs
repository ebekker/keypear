// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.Text.Json;
using Keypear.Shared;
using Keypear.Shared.Krypto;
using Keypear.Shared.Models.InMemory;
using Keypear.Shared.Utils;

namespace Keypear.CliClient;

public class CliSession
{
    public const string SessionEnvVarName = "KYPR_SESSION";
    public const string SessionFileName = "kypr_session.dat";

    private readonly SecretKeyEncryption _ske = new();

    private readonly Func<KyprSession?, IKyprServer> _serverFactory;
    private byte[]? _localSessionKey;

    public CliSession(Func<KyprSession?, IKyprServer> serverFactory, byte[]? sessionKey = null)
    {
        _serverFactory = serverFactory;
        _localSessionKey = sessionKey;
    }

    public SessionDetails? Details { get; private set; }

    public bool IsInit => _localSessionKey != null && Details != null;

    public string SessionEnvVar
    {
        get
        {
            if (!IsInit)
            {
                throw new InvalidOperationException("session has not been initialized or loaded");
            }

            return Convert.ToBase64String(_localSessionKey!);
        }
    }

    public KyprClient GetClient(bool skipCopyToClient = false)
    {
        var session = Details?.ServerSession;

        var client = new KyprClient(_serverFactory(session));
        if (!skipCopyToClient && this.Details != null)
        {
            CopyToClient(client);
        }
        return client;
    }

    public void Clear()
    {
        _localSessionKey = null;
        Details = null;
    }

    public void Init(KyprClient client)
    {
        Init(new SessionDetails
        {
            ServerSession = client.Session,
            Account = client.Account,
            Vaults = client.Vaults,
        });
    }

    public void Init(SessionDetails? details)
    {
        _localSessionKey = _ske.GenerateKey();
        Details = details ?? new();
    }

    public void Save(KyprClient? copyFromClient = null)
    {
        if (!IsInit)
        {
            throw new InvalidOperationException("session has not been initialized or loaded");
        }

        var filePath = CalculateSessionDataPath();

        if (copyFromClient != null)
        {
            CopyFromClient(copyFromClient);
        }

        SaveMsgPackEnc(filePath);
        //SaveTestJson(filePath);
    }

    public void Load(KyprClient? copyToClient = null)
    {
        if (!ResolveSessionKey())
        {
            throw new InvalidOperationException("could not resolve local Session Key");
        }

        if (!ResolveSessionData())
        {
            throw new Exception("could not resolve local Session Data");
        }

        if (copyToClient != null && this.Details != null)
        {
            CopyToClient(copyToClient);
        }
    }

    public bool TryLoad(KyprClient? copyToClient = null)
    {
        try
        {
            if (ResolveSessionKey() && ResolveSessionData())
            {
                if (copyToClient != null && this.Details != null)
                {
                    CopyToClient(copyToClient);
                }

                return true;
            }
        }
        catch (Exception)
        { }

        return false;
    }

    public void CopyFromClient(KyprClient client)
    {
        if (Details == null)
        {
            Details = new();
        }

        Details.ServerSession = client.Session;
        Details.Account = client.Account;
        Details.Vaults = client.Vaults;
    }

    public bool CopyToClient(KyprClient client)
    {
        if (this.Details != null)
        {
            client.Session = this.Details.ServerSession;
            client.Account = this.Details.Account;
            client.Vaults = this.Details.Vaults;
            return true;
        }
        return false;
    }

    public bool Delete()
    {
        var filePath = CalculateSessionDataPath();
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }
        else
        {
            return false;
        }
    }

    private string CalculateSessionDataPath() =>
        Path.Combine(Directory.GetCurrentDirectory(), "_TMP", SessionFileName);

    private bool ResolveSessionKey()
    {
        if (_localSessionKey == null)
        {
            var lsk = Environment.GetEnvironmentVariable(SessionEnvVarName);
            if (lsk == null)
            {
                return false;
            }
            _localSessionKey = Convert.FromBase64String(lsk);
        }
        return true;
    }

    private bool ResolveSessionData()
    {
        var filePath = CalculateSessionDataPath();
        if (!File.Exists(filePath))
        {
            return false;
        }

        LoadMsgPackEnc(filePath);
        //LoadTestJson(filePath);

        return true;
    }

    private void SaveMsgPackEnc(string filePath)
    {
        var fileParent = Directory.GetParent(filePath)!;
        if (!Directory.Exists(fileParent.FullName))
        {
            Directory.CreateDirectory(fileParent.FullName);
        }

        var dataRaw = KpMsgPack.DynSer(Details);
        var dataEnc = _ske.Encrypt(dataRaw, _localSessionKey!);

        File.WriteAllBytes(filePath, dataEnc);

        SaveTestJson(filePath + ".shdw");
    }

    private void LoadMsgPackEnc(string filePath)
    {
        var dataEnc = File.ReadAllBytes(filePath);
        var dataRaw = _ske.Decrypt(dataEnc, _localSessionKey!);
        Details = KpMsgPack.DynDes<SessionDetails>(dataRaw);
    }


    private static readonly JsonSerializerOptions TestJsonSerOpts = new()
    {
        WriteIndented = true,
    };

    private void SaveTestJson(string filePath)
    {
        var fileParent = Directory.GetParent(filePath)!;
        if (!Directory.Exists(fileParent.FullName))
        {
            Directory.CreateDirectory(fileParent.FullName);
        }

        File.WriteAllText(filePath,
            JsonSerializer.Serialize(Details, TestJsonSerOpts));
    }

    private void LoadTestJson(string filePath)
    {
        Details = JsonSerializer.Deserialize<SessionDetails>(
            File.ReadAllText(filePath), TestJsonSerOpts);
    }

    public class SessionDetails
    {
        public KyprSession? ServerSession { get; set; }

        public Account? Account { get; set; }

        public List<Vault> Vaults { get; set; } = new();
    }
}
