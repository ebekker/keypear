// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel.SetCommands;

[Command("record", "rec",
    Description = "update details about an existing Record.")]
public class SetRecordCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;

    public SetRecordCommand(SetCommand parent, IConsole console)
    {
        _main = parent.Main;
        _console = console;
    }

    [Option(Description = "Vault refrence in which to locate the record")]
    public string? Vault { get; set; }

    [Option(Description = "Record refrence which will be updated")]
    [Required]
    public string? Record { get; set; }

    [Option(Description = "path to the input file containing the record details to update")]
    [Required]
    public string? File { get; set; }

    [Option(Description = "if true, the input file is deleted after successfully being process")]
    public bool Delete { get; set; }

    public async Task<int> OnExecuteAsync()
    {
        var sess = _main.GetSession();
        using var client = sess.GetClient();

        var vault = Vault == null
            ? null
            : _main.GetVault(client, Vault!);
        if (vault == null && Vault != null)
        {
            return 1;
        }

        var record = _main.GetRecord(client, ref vault, Record!);
        if (record == null || vault == null)
        {
            return 1;
        }

        if (!IOFile.Exists(File))
        {
            _console.WriteError("could not find input file.");
            return 1;
        }

        var fileContent = IOFile.ReadAllText(File);
        var fileInput = JsonSerializer.Deserialize<SetRecordInput>(fileContent,
            MainCommand.JsonInputOpts);

        if (fileInput == null)
        {
            _console.WriteError("could not read or parse input file.");
            return 1;
        }

        client.UnlockVault(vault);
        client.UnlockRecord(vault, record);

        if (record.Summary == null)
        {
            record.Summary = new();
        }
        if (record.Content == null)
        {
            record.Content = new();
        }

        SetProp(v => record.Summary.Label = v,
            fileInput.Label, fileInput.RemoveLabel);
        SetProp(v => record.Summary.Type = v,
            fileInput.Type, fileInput.RemoveType);
        SetProp(v => record.Summary.Username = v,
            fileInput.Username, fileInput.RemoveUsername);
        SetProp(v => record.Summary.Address = v,
            fileInput.Address, fileInput.RemoveAddress);
        SetProp(v => record.Content.Password = v,
            fileInput.Password, fileInput.RemovePassword);
        SetProp(v => record.Content.Memo = v,
            fileInput.Memo, fileInput.RemoveMemo);

        if (fileInput.Fields?.Length > 0 && record.Content.Fields == null)
        {
            record.Content.Fields = new();
        }

        if (record.Content.Fields != null)
        {
            if (fileInput.RemoveFields?.Length > 0)
            {
                var toDel = record.Content.Fields.Where(x =>
                    fileInput.RemoveFields.Contains(x.Name)
                    && (fileInput.Fields == null
                        || !fileInput.Fields.Any(y => object.Equals(y.Name, x.Name))));
                foreach (var f in toDel)
                {
                    record.Content.Fields.Remove(f);
                }
            }
            if (fileInput.Fields?.Length > 0)
            {
                foreach (var f in fileInput.Fields)
                {
                    var rcf = record.Content.Fields.FirstOrDefault(x => object.Equals(x.Name, f.Name));
                    if (rcf == null)
                    {
                        rcf = new();
                        record.Content.Fields.Add(rcf);
                    }
                    rcf.Name = f.Name;
                    rcf.Type = f.Type;
                    rcf.Value = f.Value;
                }
            }
        }

        var id = await client.SaveRecordAsync(vault, record);

        sess.Save(client);

        _console.WriteLine(id);

        return 0;
    }

    private void SetProp<T>(Action<T?> setter, T newVal, bool removeVal)
    {
        if (!object.Equals(newVal, default(T)))
        {
            setter(newVal);
        }
        else if (removeVal)
        {
            setter(default(T));
        }
    }

    public class SetRecordInput : NewCommands.NewRecordCommand.NewRecordInput
    {
        public bool RemoveLabel { get; set; }
        public bool RemoveType { get; set; }
        public bool RemoveUsername { get; set; }
        public bool RemovePassword { get; set; }
        public bool RemoveAddress { get; set; }
        public bool RemoveTags { get; set; }
        public bool RemoveMemo { get; set; }

        public string[]? RemoveFields;
    }
}
