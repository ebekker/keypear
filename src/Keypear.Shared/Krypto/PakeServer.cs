// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.PAKE.ScottBradySRP;
using static Keypear.Shared.Utils.KpCommon;

namespace Keypear.Shared.Krypto;

public class PakeServer : IPakeServer
{
    private readonly PakeSrpParameters _parameters = new();
    private readonly SrpState _state = new();

    private readonly byte[] _saltKey = GenericHash.GenerateKey();

    public string Algor => _parameters.Algor;

    /// <summary>
    /// Used to initiate both a Registration and a new Session.
    /// </summary>
    public IPakeServer.InutResult Init(IPakeServer.InitInput input)
    {
        ThrowIfNull(input.Username);

        var (hSalt, vSalt) = ComputeSalts(input.Username);
        var result = new IPakeServer.InutResult
        {
            HasherSalt = hSalt,
            VerifierSalt = vSalt,
        };

        return result;
    }

    /// <summary>
    /// Start Registration of a new account.
    /// </summary>
    public IPakeServer.RegisterResult Register(IPakeServer.RegisterInput input)
    {
        ThrowIfNull(input.Username);
        ThrowIfNull(input.Verifier);
        ThrowIfNull(input.EphemeralA);

        // Make sure username was not already previously registered
        if (_state.ContainsUsername(input.Username))
        {
            throw new Exception("username already registered");
        }

        // Make sure ephA is not already in use, used as a Session state ID
        var ephA = new BigInteger(input.EphemeralA);
        if (_state.ContainsSession(ephA))
        {
            throw new Exception("ephemeral A already in use");
        }

        // We immediately start preparing a session as a final validation that
        // the registration succeededed and to have an immediate shared key

        var h = ComputeHasher(input.Username);
        var srp = new SrpServer(h,
            _parameters._groupParameters.g,
            _parameters._groupParameters.N);

        var biV = new BigInteger(input.Verifier);
        var ephB = srp.GenerateBValues(biV, out var srpState);

        // Remember the verifier and B for this username
        var pr = new PendingRegistration(input.Username, biV, ephA, ephB, srpState);
        _state.AddPending(input.Username, pr);

        return new()
        {
            EphemeralB = ephB.ToByteArray(),
        };
    }

    /// <summary>
    /// Finish Reigstration a new account and establish a first Session.
    /// </summary>
    public IPakeServer.RegisterFiniResult RegisterFini(IPakeServer.RegisterFiniInput input)
    {
        ThrowIfNull(input.Username);
        ThrowIfNull(input.ClientProof);

        if (!_state.TryGetPending(input.Username, out var pr) || pr == null)
        {
            throw new Exception("username not found");
        }

        var h = ComputeHasher(input.Username);
        var srp = new SrpServer(h,
            _parameters._groupParameters.g,
            _parameters._groupParameters.N,
            pr.EphemeralB, pr.SrpState);

        var sKey = srp.ComputeSessionKey(pr.Verifier, pr.EphemeralA);
        var cProof = new BigInteger(input.ClientProof);
        if (!srp.ValidateClientProof(cProof, pr.EphemeralA, sKey))
        {
            throw new Exception("client proof is invalid");
        }
        var sProof = srp.GenerateServerProof(pr.EphemeralA, cProof, sKey);

        _state.RemovePending(input.Username);
        _state.AddVerifier(input.Username, pr.Verifier);

        return new()
        {
            ServerProof = sProof.ToByteArray(),
        };
    }

    /// <summary>
    /// Start a new Session.
    /// </summary>
    public IPakeServer.PrepareSessionResult PrepareSession(IPakeServer.PrepareSessionInput input)
    {
        ThrowIfNull(input.Username);
        ThrowIfNull(input.EphemeralA);

        if (!_state.TryGetVerifier(input.Username, out var biV))
        {
            throw new Exception("username is not registered");
        }

        // Ephemeral A is also used as unique Session ID
        var ephA = new BigInteger(input.EphemeralA);
        if (_state.ContainsSession(ephA))
        {
            throw new Exception("ephemeral A already in use");
        }

        var h = ComputeHasher(input.Username);
        var srp = new SrpServer(h,
            _parameters._groupParameters.g,
            _parameters._groupParameters.N);

        var ephB = srp.GenerateBValues(biV, out var srpState);
        var sKey = srp.ComputeSessionKey(biV, ephA);
        var sess = new SrpSession(input.Username, ephA, ephB, sKey, srpState)
        {
            PreparedTime = DateTime.Now,
        };

        _state.AddSession(ephA, sess);

        var result = new IPakeServer.PrepareSessionResult
        {
            EphemeralB = sess.EphemeralB.ToByteArray(),
        };

        return result;
    }

    /// <summary>
    /// Establish a new Session.
    /// </summary>
    public IPakeServer.VerifySessionResult VerifySession(IPakeServer.VerifySessionInput input)
    {
        ThrowIfNull(input.EphemeralA);
        ThrowIfNull(input.ClientProof);

        var ephA = new BigInteger(input.EphemeralA);
        var sess = _state.GetSession(ephA);
        if (sess == null)
        {
            throw new Exception("session could not be resolved");
        }

        var h = ComputeHasher(sess.Username);
        var srp = new SrpServer(h,
            _parameters._groupParameters.g,
            _parameters._groupParameters.N,
            sess.EphemeralB, sess.SrpState);

        var cProof = new BigInteger(input.ClientProof);
        if (!srp.ValidateClientProof(cProof, sess.EphemeralA, sess.SessionKey))
        {
            throw new Exception("client proof is invalid");
        }

        var sProof = srp.GenerateServerProof(sess.EphemeralA, cProof, sess.SessionKey);
        var result = new IPakeServer.VerifySessionResult
        {
            ServerProof = sProof.ToByteArray(),
        };

        sess.VerifiedTime = DateTime.Now;
        sess.LastUsedTime = DateTime.Now;

        return result;
    }

    public void ContinueSession(byte[] ephemeralA)
    {
        ThrowIfNull(ephemeralA);

        var ephA = new BigInteger(ephemeralA);
        var sess = _state.GetSession(ephA);
        if (sess == null)
        {
            throw new Exception("session could not be resolved");
        }

        // TODO: check session expiration?
        //   e.g. if (DateTime.Now - sess.LastUsedTime > TimeSpan.FromMinutes(60))

        sess.LastUsedTime = DateTime.Now;
    }


    private (byte[] hSalt, byte[] vSalt) ComputeSalts(string username)
    {
        var bytes = Encoding.UTF8.GetBytes(username);
        var hSalt = GenericHash.Hash(bytes, _saltKey, 16);
        var vSalt = GenericHash.Hash(hSalt, _saltKey, 16);
        return (hSalt, vSalt);
    }

    private Func<byte[], byte[]> ComputeHasher(string username)
    {
        var (hSalt, vSalt) = ComputeSalts(username);
        var h = (byte[] data) => _parameters._hasher(hSalt, data);
        return h;
    }

    private class PendingRegistration
    {
        public PendingRegistration(string u, BigInteger v, BigInteger A, BigInteger B, byte[] srpState)
        {
            Username = u;
            Verifier = v;
            EphemeralA = A;
            EphemeralB = B;
            SrpState = srpState;
        }

        public string Username { get; }
        public BigInteger Verifier { get; }
        public BigInteger EphemeralA { get; }
        public BigInteger EphemeralB { get; }
        public byte[] SrpState { get; }
    }

    private class SrpSession
    {
        public SrpSession(string u, BigInteger A, BigInteger B, BigInteger k, byte[] srpState)
        {
            Username = u;
            EphemeralA = A;
            EphemeralB = B;
            SessionKey = k;
            SrpState = srpState;
        }

        public string Username { get; }
        public BigInteger EphemeralA { get; }
        public BigInteger EphemeralB { get; }

        public BigInteger SessionKey { get; }
        public byte[] SrpState { get; }

        public DateTime? PreparedTime { get; set; }
        public DateTime? VerifiedTime { get; set; }
        public DateTime? LastUsedTime { get; set; }
    }

    private class SrpState
    {
        private readonly Dictionary<string, PendingRegistration> _pending = new();
        private readonly Dictionary<string, BigInteger> _verifiers = new();
        private readonly Dictionary<BigInteger, SrpSession> _sessions = new();

        // Pending Regirstation State
        public void AddPending(string username, PendingRegistration pr) =>
            _pending.Add(username, pr);
        public bool TryGetPending(string username, out PendingRegistration? pr) =>
            _pending.TryGetValue(username, out pr);
        public void RemovePending(string username) =>
            _pending.Remove(username);

        // Fully Registered State
        public bool ContainsUsername(string username) =>
            _verifiers.ContainsKey(username);
        public void AddVerifier(string username, BigInteger biV) =>
            _verifiers.Add(username, biV);
        public bool TryGetVerifier(string username, out BigInteger biV) =>
            _verifiers.TryGetValue(username, out biV);

        // Established Sessions State
        public bool ContainsSession(BigInteger ephA) =>
            _sessions.ContainsKey(ephA);
        public void AddSession(BigInteger ephA, SrpSession sess) =>
            _sessions.Add(ephA, sess);
        public SrpSession? GetSession(BigInteger ephA) =>
            _sessions.TryGetValue(ephA, out var sess)
                ? sess
                : null;
    }
}
