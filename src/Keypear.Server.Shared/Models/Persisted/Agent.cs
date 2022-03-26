// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Server.Shared.Models.Persisted;

public class Agent
{
    public Guid Id { get; set; }

    public Guid? AccountId { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public string? Label { get; set; }

    public string? Type { get; set; }

    public string? Version { get; set; }

    public string? Address { get; set; }

    public string? OSType { get; set; }

    public string? OSVersion { get; set; }

    public string? Meta { get; set; }
}
