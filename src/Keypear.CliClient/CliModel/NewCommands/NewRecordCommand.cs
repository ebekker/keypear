// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.Models.InMemory;
using Keypear.Shared.Models.Inner;

namespace Keypear.CliClient.CliModel.NewCommands;

[Command("record", "rec",
    Description = "creates a new Record within a Vault.")]
public class NewRecordCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;

    public NewRecordCommand(NewCommand parent, IConsole console)
    {
        _main = parent.Main;
        _console = console;
    }

    [Option(Description = "Vault refrence in which to create the record")]
    [Required]
    public string? Vault { get; set; }

    [Option(Description = "path to the input file containing the new record details")]
    [Required]
    public string? File { get; set; }

    [Option(Description = "if true, the input file is deleted after successfully being process")]
    public bool Delete { get; set; }

    public async Task<int> OnExecuteAsync()
    {
        var sess = _main.GetSession();
        using var client = sess.GetClient();

        var vault = _main.GetVault(client, Vault!);
        if (vault == null)
        {
            return 1;
        }

        if (!IOFile.Exists(File))
        {
            _console.WriteError("could not find input file.");
            return 1;
        }

        var fileContent = IOFile.ReadAllText(File);
        var fileInput = JsonSerializer.Deserialize<NewRecordInput>(fileContent,
            MainCommand.JsonInputOpts);

        if (fileInput == null)
        {
            _console.WriteError("could not read or parse input file.");
            return 1;
        }

        var record = new Record();
        record.Summary = new();
        record.Summary.Label = fileInput.Label;
        record.Summary.Type = fileInput.Type;
        record.Summary.Username = fileInput.Username;
        record.Summary.Address = fileInput.Address;
        record.Summary.Tags = fileInput.Tags;

        record.Content = new();
        record.Content.Password = fileInput.Password;
        record.Content.Memo = fileInput.Memo;

        record.Content.Fields = fileInput.Fields?.Select(x => new RecordField
        {
            Name = x.Name,
            Type = x.Type,
            Value = x.Value,
        }).ToList();

        var id = await client.SaveRecordAsync(vault, record);

        sess.Save(client);

        _console.WriteLine(id);

        if (Delete)
        {
            IOFile.Delete(File);
        }

        return 0;
    }

    public class NewRecordInput
    {
        public string? Label { get; set; }

        public string? Type { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? Address { get; set; }

        public string? Tags { get; set; }

        public string? Memo { get; set; }

        public Field[]? Fields { get; set; }

        public class Field
        {
            public string? Name { get; set; }
            public string? Type { get; set; }
            public string? Value { get; set; }
        }
    }
}
