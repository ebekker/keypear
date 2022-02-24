// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.PAKE.ScottBradySRP;
using static Keypear.Shared.SharedUtils;

namespace Keypear.Shared;

public class KyprPakeClient
{
    private readonly SrpParameters _parameters = new();

    private readonly KyprPakeServer _server = new();

    public void Register(string username, string password)
    {
        var preInput = new KyprPakeServer.PrepareParametersInput
        {
            Username = username,
        };

        var prepResult = _server.PrepareParameters(preInput);
        ThrowIfNull(prepResult.HasherSalt);
        ThrowIfNull(prepResult.VerifierSalt);

        var h = (byte[] data) => _parameters._hasher(prepResult.HasherSalt, data);
        var srp = new SrpClient(h,
            _parameters._groupParameters.g,
            _parameters._groupParameters.N);
        var v = srp.GenerateVerifier(username, password, prepResult.VerifierSalt);

        var regInput = new KyprPakeServer.RegisterInput
        {
            Username = username,
            Verifier = v.ToByteArray()
        };

        _server.Register(regInput);
    }

    public KyprSession StartSession(string username, string password)
    {
        var prepInput = new KyprPakeServer.PrepareParametersInput
        {
            Username = username,
        };

        var prepResult = _server.PrepareParameters(prepInput);
        ThrowIfNull(prepResult.HasherSalt);
        ThrowIfNull(prepResult.VerifierSalt);

        var h = (byte[] data) => _parameters._hasher(prepResult.HasherSalt, data);
        var srp = new SrpClient(h,
            _parameters._groupParameters.g,
            _parameters._groupParameters.N);

        var biA = srp.GenerateAValues();

        var sessInput = new KyprPakeServer.PrepareSessionInput
        {
            Username = username,
            EphemeralA = biA.ToByteArray(),
        };

        var sessResult = _server.PrepareSession(sessInput);
        ThrowIfNull(sessResult.EphemeralB);

        var biB = new BigInteger(sessResult.EphemeralB);
        var skey = srp.ComputeSessionKey(username, password, prepResult.VerifierSalt, biB);
        var clientProof = srp.GenerateClientProof(biB, skey);

        var verifyInput = new KyprPakeServer.VerifySessionInput
        {
            EphemeralA = biA.ToByteArray(),
            ClientProof = clientProof.ToByteArray(),
        };

        var verifyResult = _server.VerifySession(verifyInput);
        ThrowIfNull(verifyResult.ServerProof);

        var serverProof = new BigInteger(verifyResult.ServerProof);
        if (!srp.ValidateServerProof(serverProof, clientProof, skey))
        {
            throw new Exception("server proof is invalid");
        }

        return new()
        {
            SessionId = biA.ToByteArray(),
            SessionKey = skey.ToByteArray(),
        };
    }
}
