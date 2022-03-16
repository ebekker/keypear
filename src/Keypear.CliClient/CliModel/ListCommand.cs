// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Keypear.Shared;
using Keypear.Shared.Models.InMemory;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel;

[Command(Description = "list entities of a given type.")]
[Subcommand(
    typeof(ListCommands.VaultsCommand)
    , typeof(ListCommands.RecordsCommand))]
public class ListCommand
{
    public void OnExecute(CommandLineApplication app) => app.ShowHelp();

    //private readonly IConsole _console;
    //private readonly Common? _common;

    //public ListCommand(IConsole console, Common common)
    //{
    //    _console = console;
    //    _common = common;
    //}

    //[Option]
    //public bool Refresh { get; set; }

    //[Option]
    //public string? Vault { get; set; }

    //[Option]
    //public bool Deleted { get; set; }

    //[Option]
    //public bool Indented { get; set; }

    //public async Task<int> OnExecuteAsync()
    //{
    //    var sess = new CliSession();
    //    sess.Load();

    //    var client = _common!.GetClient(sess.Details!.ServerSession);

    //    client.Session = sess.Details.ServerSession;
    //    client.Account = sess.Details.Account;
    //    client.Vaults = sess.Details.Vaults;

    //    if (Refresh)
    //    {
    //        await client.RefreshVaultsAsync();
    //    }

    //    client.UnlockVaults();

    //    Vault? vault = null;
    //    if (!string.IsNullOrEmpty(Vault))
    //    {
    //        // First try by Id
    //        var vaults = client.ListVaults().Where(x =>
    //            x.Id.ToString()!.StartsWith(Vault!, StringComparison.OrdinalIgnoreCase));
    //        if (vaults.Count() == 0)
    //        {
    //            // Next try by label
    //            vaults = client.ListVaults().Where(x =>
    //                x.Summary!.Label!.StartsWith(Vault!, StringComparison.OrdinalIgnoreCase));
    //        }
    //        if (vaults.Count() == 0)
    //        {
    //            Console.Error.WriteLine("No Vaults match the specified reference.");
    //            return 1;
    //        }
    //        if (vaults.Count() > 1)
    //        {
    //            Console.Error.WriteLine("Multiple Vaults match the specified reference.");
    //            return 1;
    //        }

    //        vault = vaults.Single();
    //    }

    //    if (vault == null)
    //    {
    //        var output = new ListCommandVaultsOutput();

    //        foreach (var v in client.ListVaults())
    //        {
    //            output.Vaults.Add(new()
    //            {
    //                Id = v.Id!.Value,
    //                Label = v.Summary!.Label,
    //            });
    //        }

    //        _console.WriteLine(JsonSerializer.Serialize(output,
    //            Indented ? Common.IndentedJsonOutput : Common.DefaultJsonOutput));
    //    }
    //    else
    //    {
    //        var output = new ListCommandRecordsOutput();

    //        client.UnlockRecordSummaries(vault);

    //        var records = Deleted
    //            ? client.ListDeletedRecords(vault)
    //            : client.ListRecords(vault);

    //        foreach (var r in records)
    //        {
    //            output.Records.Add(new()
    //            {
    //                Id = r.Id!.Value,
    //                Label = r.Summary!.Label,
    //            });
    //        }

    //        _console.WriteLine(JsonSerializer.Serialize(output,
    //            Indented ? Common.IndentedJsonOutput : Common.DefaultJsonOutput));
    //    }

    //    return 0;
    //}

    //public class ListCommandVaultsOutput
    //{
    //    public List<VaultEntry> Vaults { get; set; } = new();

    //    public class VaultEntry
    //    {
    //        public Guid Id { get; set; }

    //        public string? Label { get; set; }
    //    }
    //}

    //public class ListCommandRecordsOutput
    //{
    //    public List<RecordEntry> Records { get; set; } = new();

    //    public class RecordEntry
    //    {
    //        public Guid Id { get; set; }

    //        public string? Label { get; set; }
    //    }
    //}
}
