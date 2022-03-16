// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Keypear.Shared.Models.InMemory;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel.NewCommands;

[Command("record", "rec",
    Description = "creates a new Record within a Vault.")]
public class RecordCommand
{
    private readonly IConsole _console;
    private readonly Common? _common;

    public RecordCommand(IConsole console, Common common)
    {
        _console = console;
        _common = common;
    }

    [Option]
    [Required]
    public string? Vault { get; set; }

    [Option]
    [Required]
    public string? Template { get; set; }

    public async Task<int> OnExecuteAsync()
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
            _console.Error.WriteLine("No Vaults match the specified reference.");
            return 1;
        }
        if (vaults.Count() > 1)
        {
            _console.Error.WriteLine("Multiple Vaults match the specified reference.");
            return 1;
        }

        var vault = vaults.Single();

        if (!File.Exists(Template))
        {
            _console.Error.WriteLine("Could not find template file.");
            return 1;
        }

        var templateBody = File.ReadAllText(Template);
        var templateInput = JsonSerializer.Deserialize<NewRecordInput>(templateBody);

        var record = new Record();
        record.Summary = new();
        record.Summary.Label = templateInput!.Label;
        record.Summary.Username = templateInput.Username;
        record.Summary.Address = templateInput.Address;
        record.Summary.Tags = templateInput.Tags;

        record.Content = new();
        record.Content.Password = templateInput.Password;
        record.Content.Memo = templateInput.Memo;

        var id = await client.SaveRecordAsync(vault, record);

        sess.Details.Vaults = client.Vaults;
        sess.Save();

        _console.WriteLine(id);

        return 0;
    }

    public class NewRecordInput
    {
        public string? Label { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? Address { get; set; }

        public string? Tags { get; set; }

        public string? Memo { get; set; }
    }
}
