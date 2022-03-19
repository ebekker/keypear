// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.CliClient;

public static class Extensions
{
    public static IConsole WriteError(this IConsole console, string fmt, params object[] args)
    {
        console.ForegroundColor = ConsoleColor.Red;
        console.WriteLine(fmt, args);
        console.ResetColor();
        return console;
    }
}
