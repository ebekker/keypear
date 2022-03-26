// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Server.Shared.Models.Persisted;

public class Vault
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public DateTime? DeletedDateTime { get; set; }

    public Guid CreatedBy { get; set; }

    public byte[]? SummaryEnc { get; set; }
}
