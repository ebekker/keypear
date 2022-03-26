// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Server.Shared.Models.Persisted;

public class Associate
{
    public Guid TenantId {  get; set; }

    public Guid AccountId { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public Guid CreatedBy { get; set; }
}
