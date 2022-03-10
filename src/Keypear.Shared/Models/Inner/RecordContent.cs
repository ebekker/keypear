// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Models.Inner;

[MPObject(true)]
public class RecordContent
{
    public string? Password { get; set; }

    public string? Memo { get; set; }

    public List<RecordField>? Fields { get; set; }
}
