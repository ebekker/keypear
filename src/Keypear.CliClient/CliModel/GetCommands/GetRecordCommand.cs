// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.CliClient.CliModel.GetCommands;

[Command("record", "rec",
    Description = "get all the details for a Record.")]
public class GetRecordCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;

    public GetRecordCommand(GetCommand parent, IConsole console)
    {
        _main = parent.Main;
        _console = console;
    }

    [Option]
    public string? Vault { get; set; }

    [Option]
    [Required]
    public string? Record { get; set; }

    [Option]
    public string? File { get; set; }

    [Option]
    public bool Indented { get; set; }

    public int OnExecute()
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
        if (record == null)
        {
            return 1;
        }

        client.UnlockRecord(vault!, record);

        var output = new GetRecordOutput
        {
            Id = record.Id!.Value,
            Label = record.Summary!.Label,
            Type = record.Summary!.Type,
            Address = record.Summary!.Address,  
            Username = record.Summary!.Username,
            Tags = record.Summary!.Tags,
            Memo = record.Content!.Memo,
            Password = record.Content!.Password,
            Fields = record.Content.Fields?.Select(x =>
            new NewCommands.NewRecordCommand.NewRecordInput.Field()
            {
                Name = x.Name,
                Type = x.Type,
                Value = x.Value,
            }).ToArray(),
        };

        var outputSer = JsonSerializer.Serialize(output,
            Indented ? Common.IndentedJsonOutput : Common.DefaultJsonOutput);

        if (File == null)
        {
            _console.WriteLine(outputSer);
        }
        else
        {
            IOFile.WriteAllText(File, outputSer);
        }

        return 0;
    }

    public class GetRecordOutput
        : NewCommands.NewRecordCommand.NewRecordInput
    {
        public Guid Id { get; set; }
    }
}
