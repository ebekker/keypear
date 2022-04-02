// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.PAKE.ScottBradySRP;
using Keypear.Shared.Utils;
using static Keypear.Shared.Utils.KpCommon;

namespace Keypear.Shared.Krypto;

public class PakeClient
{
    private readonly PakeSrpParameters _parameters = new();

    private readonly IPakeServer _server;

    public string Algor => _parameters.Algor;

    public PakeClient(IPakeServer server)
    {
        _server = server;
    }

    public KyprSession Register(string username, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        username = KpEncoding.Normalize(username);
        password = KpEncoding.Normalize(password);

        var initInput = new IPakeServer.InitInput
        {
            Username = username,
        };

        var initResult = _server.Init(initInput);
        ThrowIfNull(initResult.HasherSalt);
        ThrowIfNull(initResult.VerifierSalt);

        var h = (byte[] data) => _parameters._hasher(initResult.HasherSalt, data);
        var srp = new SrpClient(h,
            _parameters._groupParameters.g,
            _parameters._groupParameters.N);

        var biV = srp.GenerateVerifier(username, password, initResult.VerifierSalt);
        var ephA = srp.GenerateAValues(out var srpState);


        var regiInput = new IPakeServer.RegisterInput
        {
            Username = username,
            Verifier = biV.ToByteArray(),
            EphemeralA = ephA.ToByteArray(),
        };

        var regiResult = _server.Register(regiInput);
        ThrowIfNull(regiResult.EphemeralB);

        var ephB = new BigInteger(regiResult.EphemeralB);
        var sKey = srp.ComputeSessionKey(username, password, initResult.VerifierSalt, ephB);
        var cProof = srp.GenerateClientProof(ephB, sKey);

        var finiInput = new IPakeServer.RegisterFiniInput
        {
            Username = username,
            ClientProof = cProof.ToByteArray(),
        };

        var finiResult = _server.RegisterFini(finiInput);
        ThrowIfNull(regiResult.EphemeralB);
        ThrowIfNull(finiResult.ServerProof);

        var sProof = new BigInteger(finiResult.ServerProof);
        if (!srp.ValidateServerProof(sProof, cProof, sKey))
        {
            throw new Exception("server proof is invalid");
        }

        return new()
        {
            SessionId = SimpleBase.Base32.Crockford.Encode(ephA.ToByteArray()),
            SessionKey = sKey.ToByteArray(),
        };
    }

    public KyprSession StartSession(string username, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        username = KpEncoding.Normalize(username);
        password = KpEncoding.Normalize(password);

        var initInput = new IPakeServer.InitInput
        {
            Username = username,
        };

        var initResult = _server.Init(initInput);
        ThrowIfNull(initResult.HasherSalt);
        ThrowIfNull(initResult.VerifierSalt);

        var h = (byte[] data) => _parameters._hasher(initResult.HasherSalt, data);
        var srp = new SrpClient(h,
            _parameters._groupParameters.g,
            _parameters._groupParameters.N);

        var biA = srp.GenerateAValues(out var srpState);

        var prepInput = new IPakeServer.PrepareSessionInput
        {
            Username = username,
            EphemeralA = biA.ToByteArray(),
        };

        var prepResult = _server.PrepareSession(prepInput);
        ThrowIfNull(prepResult.EphemeralB);

        var biB = new BigInteger(prepResult.EphemeralB);
        var sKey = srp.ComputeSessionKey(username, password, initResult.VerifierSalt, biB);
        var clientProof = srp.GenerateClientProof(biB, sKey);

        var verifyInput = new IPakeServer.VerifySessionInput
        {
            EphemeralA = biA.ToByteArray(),
            ClientProof = clientProof.ToByteArray(),
        };

        var verifyResult = _server.VerifySession(verifyInput);
        ThrowIfNull(verifyResult.ServerProof);

        var serverProof = new BigInteger(verifyResult.ServerProof);
        if (!srp.ValidateServerProof(serverProof, clientProof, sKey))
        {
            throw new Exception("server proof is invalid");
        }

        return new()
        {
            SessionId = SimpleBase.Base32.Crockford.Encode(biA.ToByteArray()),
            SessionKey = sKey.ToByteArray(),
        };
    }
}
