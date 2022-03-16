// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel.NewCommands;

public class VaultCommand
{
    private readonly IConsole _console;
    private readonly Common? _common;

    public VaultCommand(IConsole console, Common common)
    {
        _console = console;
        _common = common;
    }

    [Option]
    [Required]
    public string? Name { get; set; }

    public async Task<int> OnExecuteAsync()
    {
        var sess = new CliSession();
        sess.Load();

        var client = _common!.GetClient(sess.Details!.ServerSession);

        client.Session = sess.Details.ServerSession;
        client.Account = sess.Details.Account;
        client.Vaults = sess.Details.Vaults;

        var vault = await client.CreateVaultAsync(Name!);

        sess.Details.Vaults = client.Vaults;
        sess.Save();

        _console.WriteLine(vault.Id!);

        return 0;
    }
}
