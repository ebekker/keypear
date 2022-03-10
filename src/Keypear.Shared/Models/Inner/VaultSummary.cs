// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Models.Inner;

[MPObject(true)]
public class VaultSummary
{
    public string? Type { get; set; }

    public string? Label { get; set; }
}
