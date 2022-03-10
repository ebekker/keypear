// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Models.Inner;

[MPObject(true)]
public class RecordSummary
{
    public string? Type { get; set; }

    public string? Label { get; set; }

    public string? Username { get; set; }

    public string? Address { get; set; }

    public string? Tags { get; set; }

}
