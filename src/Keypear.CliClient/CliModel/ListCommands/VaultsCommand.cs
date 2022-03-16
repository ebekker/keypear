// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel.ListCommands;

public class VaultsCommand
{
    private readonly IConsole _console;
    private readonly Common? _common;

    public VaultsCommand(IConsole console, Common common)
    {
        _console = console;
        _common = common;
    }

    [Option]
    public bool Refresh { get; set; }

    [Option]
    public bool Deleted { get; set; }

    [Option]
    public bool Indented { get; set; }

    public async Task<int> OnExecuteAsync()
    {
        var sess = new CliSession();
        sess.Load();

        var client = _common!.GetClient(sess.Details!.ServerSession);

        client.Session = sess.Details.ServerSession;
        client.Account = sess.Details.Account;
        client.Vaults = sess.Details.Vaults;

        if (Refresh)
        {
            await client.RefreshVaultsAsync();
            sess.Details.Vaults = client.Vaults;
            sess.Save();
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
