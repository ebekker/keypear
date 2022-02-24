// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Keypear.Shared;

public static class SharedUtils
{
    public static void ThrowIfNull(
        [NotNull] object? value,
        [CallerArgumentExpression("value")] string? valueName = null,
        string messageFormat = "value {0} is null")
    {
        if (value == null)
        {
            throw new Exception(String.Format(messageFormat, valueName));
        }
    }
}
