// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Keypear.Shared.Utils;

public static class KpCommon
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

    public static void SafeClear(ref byte[]? data)
    {
        if (data != null)
        {
            Array.Clear(data);
            data = null;
        }
    }

    public static bool Search(string q, params string?[]? fields) => Search(q, (IEnumerable<string?>?)fields);

    public static bool Search(string q, IEnumerable<string?>? fields)
    {
        if (fields != null)
        {
            foreach (var f in fields)
            {
                if (f != null && f.Contains(q, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
