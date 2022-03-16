// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Keypear.Shared;
using Keypear.Shared.Models.InMemory;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel;

public class GetCommand
{
    private readonly IConsole _console;
    private readonly Common? _common;

    public GetCommand(IConsole console, Common common)
    {
        _console = console;
        _common = common;
    }

    [Option]
    [Required]
    public string? Vault { get; set; }

    [Option]
    public bool Indented { get; set; }

    public int OnExecute()
    {
        var sess = new CliSession();
        sess.Load();

        var client = _common!.GetClient(sess.Details!.ServerSession);

        client.Session = sess.Details.ServerSession;
        client.Account = sess.Details.Account;
        client.Vaults = sess.Details.Vaults;

        client.UnlockVaults();

        // First try by Id
        var vaults = client.ListVaults().Where(x =>
            x.Id.ToString()!.StartsWith(Vault!, StringComparison.OrdinalIgnoreCase));
        if (vaults.Count() == 0)
        {
            // Next try by label
            vaults = client.ListVaults().Where(x =>
                x.Summary!.Label!.StartsWith(Vault!, StringComparison.OrdinalIgnoreCase));
        }
        if (vaults.Count() == 0)
        {
            Console.Error.WriteLine("No Vaults match the specified reference.");
            return 1;
        }
        if (vaults.Count() > 1)
        {
            Console.Error.WriteLine("Multiple Vaults match the specified reference.");
            return 1;
        }

        var vault = vaults.Single();

        var output = new GetCommandVaultOutput
        {
            Id = vault.Id!.Value,
            Label = vault.Summary!.Label,
            Type = vault.Summary!.Type,
        };

        _console.WriteLine(JsonSerializer.Serialize(output,
            Indented ? Common.IndentedJsonOutput : Common.DefaultJsonOutput));

        return 0;
    }

    public class GetCommandVaultOutput
    {
        public Guid Id { get; set; }

        public string? Type { get; set; }

        public string? Label { get; set; }
    }
}
