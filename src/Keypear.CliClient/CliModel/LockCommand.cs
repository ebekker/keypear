// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.CliClient.CliModel;

[Command(Description = "Logs out of your Account and destroy your Session.")]
public class LockCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;

    public LockCommand(MainCommand main, IConsole console)
    {
        _main = main;
        _console = console;
    }

    public int OnExecute()
    {
        var sess = _main.GetSession(skipLoad: true);
        using var client = sess.GetClient(skipCopyToClient: true);

        if (sess.TryLoad(client))
        {
            client.LockAccount();
            sess.Save(client);
            return 0;
        }

        return 1;
    }
}
