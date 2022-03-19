// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel;

public class StatusCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;

    public StatusCommand(MainCommand main, IConsole console)
    {
        _main = main;
        _console = console;
    }

    public void OnExecute()
    {
        var sess = _main.GetSession(skipLoad: true);
        using var client = sess.GetClient(skipCopyToClient: true);

        if (!sess.TryLoad(client))
        {
            _console.WriteLine("none");
        }
        else if (sess.Details?.ServerSession == null
            || sess.Details.Account == null)
        {
            _console.WriteLine("session-locked");
        }
        else if (client.IsAccountLocked())
        {
            _console.WriteLine("account-locked");
        }
        else
        {
            _console.WriteLine("unlocked");
        }
    }
}
