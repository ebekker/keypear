// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Models.Persisted;

public class Account
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public string? Username { get; set; }

    public byte[]? Verifier { get; set; }

    public byte[]? MasterKeySalt { get; set; }

    public byte[]? PublicKey { get; set; }

    public byte[]? PrivateKeyEnc { get; set; }

    public byte[]? SigPublicKey { get; set; }

    public byte[]? SigPrivateKeyEnc { get; set; }
}
