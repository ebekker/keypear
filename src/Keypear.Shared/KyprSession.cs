// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared;

public class KyprSession
{
    public ReadOnlyMemory<byte> SessionId { get; init; }
    public ReadOnlyMemory<byte> SessionKey { get; init; }
}
