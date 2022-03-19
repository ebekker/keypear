// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared;

namespace Keypear.CliClient.CliModel.ListCommands;

[Command("records", "rec",
    Description = "lists Records within a Vault.")]
public class ListRecordsCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;

    public ListRecordsCommand(ListCommand parent, IConsole console)
    {
        _main = parent.Main;
        _console = console;
    }

    [Option(Description = "Vault reference whose records should be listed")]
    public string? Vault { get; set; }

    [Option(Description = "flag to specify listing records for all Vaults")]
    public bool AllVaults { get; set; }

    [Option(Description = "flag to specify listing deleted records for selected Vault(s)")]
    public bool Deleted { get; set; }

    [Option(Description = "flag to specify producing formatted/indented JSON output")]
    public bool Indented { get; set; }

    public int OnExecute()
    {
        var sess = _main.GetSession();
        using var client = sess.GetClient();

        if (!AllVaults && string.IsNullOrEmpty(Vault))
        {
            _console.WriteError("you must specify a Vault reference or the 'all vaults' flag");
            return 1;
        }

        if (AllVaults)
        {
            var output = client.ListVaults().Select(x => GetRecords(client, x));
            _console.WriteLine(JsonSerializer.Serialize(output,
                Indented ? Common.IndentedJsonOutput : Common.DefaultJsonOutput));
        }
        else
        {
            var vault = _main.GetVault(client, Vault!);
            if (vault == null)
            {
                return 1;
            }

            var output = GetRecords(client, vault);
            _console.WriteLine(JsonSerializer.Serialize(output,
                Indented ? Common.IndentedJsonOutput : Common.DefaultJsonOutput));
        }

        return 0;
    }

    private ListRecordsOutput GetRecords(KyprClient client, Shared.Models.InMemory.Vault vault)
    {
        var output = new ListRecordsOutput();

        client.UnlockVault(vault);
        client.UnlockRecordSummaries(vault);

        output.Vault = new()
        {
            Id = vault.Id!.Value,
            Label = vault.Summary!.Label,
        };

        var records = Deleted
            ? client.ListDeletedRecords(vault)
            : client.ListRecords(vault);

        foreach (var r in records)
        {
            output.Records.Add(new()
            {
                Id = r.Id!.Value,
                Label = r.Summary!.Label,
            });
        }

        return output;
    }

    public class ListRecordsOutput
    {
        public ListVaultsCommand.ListVaultsOutput.VaultEntry? Vault { get; set; }

        public List<RecordEntry> Records { get; set; } = new();

        public class RecordEntry
        {
            public Guid Id { get; set; }

            public string? Label { get; set; }
        }
    }
}
