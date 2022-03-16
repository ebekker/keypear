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

    private byte[]? _localSessionKey;

    public CliSession(byte[]? sessionKey = null)
    {
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

    public void Clear()
    {
        _localSessionKey = null;
        Details = null;
    }

    public void Init(SessionDetails? details)
    {
        _localSessionKey = _ske.GenerateKey();
        Details = details ?? new();
    }

    public void Save()
    {
        if (!IsInit)
        {
            throw new InvalidOperationException("session has not been initialized or loaded");
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "_TMP", SessionFileName);

        //SaveTestJson(filePath);
        SaveMsgPackEnc(filePath);
    }

    public void Load()
    {
        if (!ResolveSessionKey())
        {
            throw new InvalidOperationException("could not resolve local Session Key");
        }

        if (!ResolveSessionData())
        {
            throw new Exception("could not resolve local Session Data");
        }
    }

    public bool TryLoad()
    {
        try
        {
            return ResolveSessionKey() && ResolveSessionData();
        }
        catch (Exception)
        {
            return false;
        }
    }

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
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "_TMP", SessionFileName);
        if (!File.Exists(filePath))
        {
            return false;
        }

        //LoadTestJson(filePath);
        LoadMsgPackEnc(filePath);

        return true;
    }

    private void SaveMsgPackEnc(string filePath)
    {
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
