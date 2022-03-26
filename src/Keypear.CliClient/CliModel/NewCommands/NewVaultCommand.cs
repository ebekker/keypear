// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel.NewCommands;

[Command("vault",
    Description = "creates a new Vault to contain Records.")]
public class NewVaultCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;

    public NewVaultCommand(NewCommand parent, IConsole console)
    {
        _main = parent.Main;
        _console = console;
    }

    [Option]
    [Required]
    public string? Label { get; set; }

    public async Task<int> OnExecuteAsync()
    {
        var sess = _main.GetSession();
        using var client = sess.GetClient();

        var vault = await client.CreateVaultAsync(Label!);
        sess.Save(client);

        _console.WriteLine(vault.Id!);

        return 0;
    }
}
