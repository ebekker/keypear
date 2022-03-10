// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.Models.Inner;

namespace Keypear.Shared.Models.InMemory;

public class Record
{
    public Guid? Id { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public DateTime? DeletedDateTime { get; set; }

    public RecordSummary? Summary { get; set; }
    public RecordContent? Content { get; set; }

    public byte[]? SummarySer { get; set; }
    public byte[]? ContentSer { get; set; }

    public byte[]? SummaryEnc { get; set; }
    public byte[]? ContentEnc { get; set; }
}
