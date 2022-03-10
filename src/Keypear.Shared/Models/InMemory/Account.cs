// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Models.InMemory;

public class Account
{
    public Guid? Id { get; set; }

    public string? Username { get; set; }

    public string? MasterKeyAlgor { get; set; }
    public byte[]? MasterKeySalt { get; set; }
    public byte[]? MasterKey { get; set; }

    // If enabled
    public string? AnchorKeyAlgor { get; set; }
    public byte[]? AnchorKey { get; set; }

    public string? PublicKeyAlgor { get; set; }
    public byte[]? PublicKey { get; set; }
    public byte[]? PrivateKey { get; set; }
    public byte[]? PrivateKeyEnc { get; set; }

    public string? SigPublicKeyAlgor { get; set; }
    public byte[]? SigPublicKey { get; set; }
    public byte[]? SigPrivateKey { get; set; }
    public byte[]? SigPrivateKeyEnc { get; set; }
}
