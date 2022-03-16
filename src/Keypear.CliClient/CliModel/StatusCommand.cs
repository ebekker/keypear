// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel;

public class StatusCommand
{
    private readonly IConsole _console;
    private readonly Common? _common;

    public StatusCommand(IConsole console, Common common)
    {
        _console = console;
        _common = common;
    }

    public void OnExecute()
    {
        var sess = new CliSession();
        if (sess.TryLoad())
        {
            _console.WriteLine("Unlocked");
        }
        else
        {
            _console.WriteLine("Locked");
        }
    }
}
