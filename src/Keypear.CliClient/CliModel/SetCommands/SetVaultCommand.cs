// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel.SetCommands;

[Command("vault",
    Description = "update details about an existing Vault.")]
public class SetVaultCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;

    public SetVaultCommand(SetCommand parent, IConsole console)
    {
        _main = parent.Main;
        _console = console;
    }

    [Option]
    [Required]
    public string? Vault { get; set; }

    [Option]
    [Required]
    public string? Label { get; set; }

    public async Task<int> OnExecuteAsync()
    {
        var sess = _main.GetSession();
        using var client = sess.GetClient();

        var vault = _main.GetVault(client, Vault!);
        if (vault == null)
        {
            return 1;
        }

        client.UnlockVault(vault);
        vault.Summary!.Label = Label;
        await client.SaveVaultAsync(vault);

        sess.Save(client);

        _console.WriteLine(vault.Id!);

        return 0;
    }
}
