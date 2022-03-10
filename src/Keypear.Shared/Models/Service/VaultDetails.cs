// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Models.Service;

public class VaultDetails
{
    public Guid? VaultId { get; set; }

    public byte[]? SecretKeyEnc { get; set; }

    public byte[]? SummaryEnc { get; set; }

    public byte[]? FastContentEnc { get; set; }

    public byte[]? FullContentEnc { get; set; }
}
