// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Server.Shared.Models.Persisted;

public class Tenant
{
    public Guid Id { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public string? Label { get; set; }

    public string? Email { get; set; }
}
