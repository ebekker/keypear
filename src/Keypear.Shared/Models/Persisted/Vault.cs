// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Models.Persisted;

public class Vault
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public Guid CreatedBy { get; set; }

    public byte[]? SummaryEnc { get; set; }
    
    public byte[]? FastContentEnc { get; set; }

    public byte[]? FullContentEnc { get; set; }
}
