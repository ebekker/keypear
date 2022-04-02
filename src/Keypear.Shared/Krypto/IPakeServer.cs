// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keypear.Shared.Krypto;

public interface IPakeServer
{
    /// <summary>
    /// Used to initiate both a Registration and a new Session.
    /// </summary>
    InutResult Init(InitInput input);

    /// <summary>
    /// Start Registration of a new account.
    /// </summary>
    RegisterResult Register(RegisterInput input);

    /// <summary>
    /// Finish Reigstration a new account and establish a first Session.
    /// </summary>
    RegisterFiniResult RegisterFini(RegisterFiniInput input);

    /// <summary>
    /// Start a new Session.
    /// </summary>
    PrepareSessionResult PrepareSession(PrepareSessionInput input);

    /// <summary>
    /// Establish a new Session.
    /// </summary>
    VerifySessionResult VerifySession(VerifySessionInput input);


    public class InitInput
    {
        public string? Username { get; set; }
    }

    public class InutResult
    {
        public byte[]? HasherSalt { get; set; }
        public byte[]? VerifierSalt { get; set; }
    }

    public class RegisterInput
    {
        public string? Username { get; set; }
        public byte[]? Verifier { get; set; }
        public byte[]? EphemeralA { get; set; }
    }

    public class RegisterResult
    {
        public byte[]? EphemeralB { get; set; }
    }

    public class RegisterFiniInput
    {
        public string? Username { get; set; }
        public byte[]? ClientProof { get; set; }
    }

    public class RegisterFiniResult
    {
        public byte[]? ServerProof { get; set; }
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
        public byte[]? ServerProof { get; set; }
    }
}
