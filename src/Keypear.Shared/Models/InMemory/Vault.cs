// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.Models.Inner;

namespace Keypear.Shared.Models.InMemory;

public class Vault
{
    public Guid? Id { get; set; }

    public VaultSummary? Summary { get; set; }

    public string? SecretKeyAlgor { get; set; }
    public byte[]? SecretKey { get; set; }
    public byte[]? SecretKeyEnc { get; set; }

    public byte[]? SummarySer { get; set; }
    public byte[]? SummaryEnc { get; set; }

    public List<Record> Records { get; set; } = new();
}
