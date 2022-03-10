// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Models.Persisted;

public class Record
{
    public Guid Id { get; set; }

    public Guid VaultId { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public byte[]? SummaryEnc { get; set; }
    public byte[]? ContentEnc { get; set; }
}
