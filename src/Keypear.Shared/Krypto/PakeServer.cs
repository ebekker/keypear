// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.PAKE.ScottBradySRP;
using static Keypear.Shared.Utils.KpCommon;


namespace Keypear.Shared.Krypto;

public class PakeServer
{
    private readonly SrpParameters _parameters = new();

    private readonly byte[] _saltKey = GenericHash.GenerateKey();
    private readonly Dictionary<string, BigInteger> _verifiers = new();
    private readonly Dictionary<BigInteger, SrpSession> _sessions = new();

    public PrepareParametersResult PrepareParameters(PrepareParametersInput input)
    {
        ThrowIfNull(input.Username);

        var bytes = Encoding.UTF8.GetBytes(input.Username);
        var hSalt = GenericHash.Hash(bytes, _saltKey, 16);
        var vSalt = GenericHash.Hash(hSalt, _saltKey, 16);

        var result = new PrepareParametersResult
        {
            HasherSalt = hSalt,
            VerifierSalt = vSalt,
        };

        return result;
    }

    public void Register(RegisterInput input)
    {
        ThrowIfNull(input.Username);
        ThrowIfNull(input.Verifier);

        if (_verifiers.ContainsKey(input.Username))
        {
            throw new Exception("username already registered");
        }

        var biV = new BigInteger(input.Verifier);

        _verifiers.Add(input.Username, biV);
    }

    public PrepareSessionResult PrepareSession(PrepareSessionInput input)
    {
        ThrowIfNull(input.Username);
        ThrowIfNull(input.EphemeralA);

        if (!_verifiers.TryGetValue(input.Username, out var v))
        {
            throw new Exception("username is not registered");
        }

        var bytes = Encoding.UTF8.GetBytes(input.Username);
        var hSalt = GenericHash.Hash(bytes, _saltKey, 16);
        var vSalt = GenericHash.Hash(hSalt, _saltKey, 16);

        var h = (byte[] data) => _parameters._hasher(hSalt, data);
        var srp = new SrpServer(h,
            _parameters._groupParameters.g,
            _parameters._groupParameters.N);

        var biA = new BigInteger(input.EphemeralA);
        if (_sessions.ContainsKey(biA))
        {
            throw new Exception("ephemeral A already in use");
        }

        var ses = new SrpSession(
            input.Username,
            biA,
            srp.GenerateBValues(v),
            srp,
            srp.ComputeSessionKey(v, biA));

        _sessions.Add(biA, ses);

        var result = new PrepareSessionResult
        {
            EphemeralB = ses.EphemeralB.ToByteArray(),
        };

        return result;
    }

    public VerifySessionResult VerifySession(VerifySessionInput input)
    {
        ThrowIfNull(input.EphemeralA);
        ThrowIfNull(input.ClientProof);

        var biA = new BigInteger(input.EphemeralA);
        if (!_sessions.TryGetValue(biA, out var ses))
        {
            throw new Exception("session could not be resolved");
        }

        var clientProof = new BigInteger(input.ClientProof);
        if (!ses.Server.ValidateClientProof(clientProof, ses.EphemeralA, ses.SessionKey))
        {
            throw new Exception("client proof is invalid");
        }

        var serverProof = ses.Server.GenerateServerProof(ses.EphemeralA, clientProof, ses.SessionKey);
        var result = new VerifySessionResult
        {
            EphemeralB = ses.EphemeralB.ToByteArray(),
            ServerProof = serverProof.ToByteArray(),
        };

        ses.VerifiedDateTime = DateTime.Now;

        return result;
    }

    public class PrepareParametersInput
    {
        public string? Username { get; set; }
    }

    public class PrepareParametersResult
    {
        public byte[]? HasherSalt { get; set; }
        public byte[]? VerifierSalt { get; set; }
    }

    public class RegisterInput
    {
        public string? Username { get; set; }
        public byte[]? Verifier { get; set; }
    }

    public class PrepareSessionInput
    {
        public string? Username { get; set; }
        public byte[]? EphemeralA { get; set; }
    }

    public class PrepareSessionResult
    {
        public byte[]? EphemeralB { get; set; }
    }

    public class VerifySessionInput
    {
        public byte[]? EphemeralA { get; set; }
        public byte[]? ClientProof { get; set; }
    }

    public class VerifySessionResult
    {
        public byte[]? EphemeralB { get; set; }
        public byte[]? ServerProof { get; set; }
    }

    private class SrpSession
    {
        public SrpSession(string u, BigInteger a, BigInteger b, SrpServer s, BigInteger k)
        {
            CreatedDateTime = DateTime.Now;

            Username = u;
            EphemeralA = a;
            EphemeralB = b;
            Server = s;
            SessionKey = k;
        }

        public DateTime CreatedDateTime { get; }
        public DateTime? VerifiedDateTime { get; set; }

        public string Username { get; }
        public BigInteger EphemeralA { get; }
        public BigInteger EphemeralB { get; }

        public SrpServer Server { get; }
        public BigInteger SessionKey { get; }
    }
}
