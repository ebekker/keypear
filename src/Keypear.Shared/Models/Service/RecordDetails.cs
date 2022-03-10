// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Models.Service;

public class RecordDetails
{
    public Guid? VaultId { get; set; }

    public Guid? RecordId { get; set; }

    public byte[]? SummaryEnc { get; set; }

    public byte[]? ContentEnc { get; set; }
}
