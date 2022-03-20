// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.CliClient.CliModel;

[Command(Description = "Logs out of your Account and destroy your Session.")]
public class LogoutCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;
    private readonly Common _common;

    public LogoutCommand(MainCommand main, IConsole console, Common common)
    {
        _main = main;
        _console = console;
        _common = common;
    }

    public int OnExecute()
    {
        var sess = _main.GetSession(skipLoad: true);

        if (sess.TryLoad())
        {
            sess.Delete();
        }

        _console.WriteLine("Local Session has been deleted, you can optionally clear your environment:");
        _console.WriteLine($"$ export KYPR_SESSION = \"\"");
        _console.WriteLine($"> $env:KYPR_SESSION = \"\"");
        return 0;
    }
}
