// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Models.Persisted;

public class Team
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public string? Label { get; set; }
}
