// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.ComponentModel.DataAnnotations;
using Keypear.ClientShared;
using Keypear.Shared;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel;

[Command(Description = "Logs out of your Account and destroy your Session.")]
public class UnlockCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;

    public UnlockCommand(MainCommand main, IConsole console)
    {
        _main = main;
        _console = console;
    }

    [Option]
    public string? Password { get; set; }

    public int OnExecute()
    {
        var sess = _main.GetSession(skipLoad: true);
        using var client = sess.GetClient(skipCopyToClient: true);

        if (!sess.TryLoad(client))
        {
            return 1;
        }

        if (client.IsAccountLocked())
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                Password = Prompt.GetPassword("Master Password: ");
            }

            client.UnlockAccount(Password);

            sess.Save(client);
        }

        return 0;
    }
}
