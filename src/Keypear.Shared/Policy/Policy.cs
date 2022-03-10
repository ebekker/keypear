// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Policy;

[Flags]
public enum Policy
{
    None = 0,

    AnchorKeyRequired
        = 1 << 0,

    NoWebClient
        = 1 << 1,

    RepeatPassword
        = 1 << 2,

    NoShowPassword
        = 1 << 3,
    NoShowFields
        = 1 << 4,
}
