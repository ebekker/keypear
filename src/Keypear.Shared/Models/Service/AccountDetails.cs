// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Models.Service;

public class AccountDetails
{
    public Guid? AccountId { get; set; }

    public string? Username { get; set; }

    public byte[]? MasterKeySalt { get; set; }

    public byte[]? PublicKey { get; set; }

    public byte[]? PrivateKeyEnc { get; set; }

    public byte[]? SigPublicKey { get; set; }

    public byte[]? SigPrivateKeyEnc { get; set; }
}
