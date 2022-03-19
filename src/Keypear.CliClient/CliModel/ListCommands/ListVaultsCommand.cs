// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel.ListCommands;

[Command("vaults",
    Description = "lists all the Vaults granted to the current Account.")]
public class ListVaultsCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;

    public ListVaultsCommand(ListCommand parent, IConsole console)
    {
        _main = parent.Main;
        _console = console;
    }

    [Option]
    public bool Refresh { get; set; }

    [Option]
    public bool Deleted { get; set; }

    [Option]
    public bool Indented { get; set; }

    public async Task<int> OnExecuteAsync()
    {
        var sess = _main.GetSession();
        using var client = sess.GetClient();

        if (Refresh)
        {
            await client.RefreshVaultsAsync();
            sess.Save(client);
        }

        client.UnlockVaults();

        var output = new ListVaultsOutput();

        foreach (var v in client.ListVaults())
        {
            output.Vaults.Add(new()
            {
                Id = v.Id!.Value,
                Label = v.Summary!.Label,
            });
        }

        _console.WriteLine(JsonSerializer.Serialize(output,
            Indented ? Common.IndentedJsonOutput : Common.DefaultJsonOutput));

        return 0;
    }

    public class ListVaultsOutput
    {
        public List<VaultEntry> Vaults { get; set; } = new();

        public class VaultEntry
        {
            public Guid Id { get; set; }

            public string? Label { get; set; }
        }
    }
}
