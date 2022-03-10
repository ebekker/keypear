// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Models.Inner;

[MPObject(true)]
public class RecordField
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Value { get; set; }
}
