// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.CliClient.CliModel.GetCommands;

[Command("vault",
    Description = "get all the details for a Vault.")]
public class GetVaultCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;

    public GetVaultCommand(GetCommand parent, IConsole console)
    {
        _main = parent.Main;
        _console = console;
    }

    [Option]
    [Required]
    public string? Vault { get; set; }

    [Option]
    public string? File { get; set; }

    [Option]
    public bool Indented { get; set; }

    public int OnExecute()
    {
        var sess = _main.GetSession();
        using var client = sess.GetClient();

        var vault = _main.GetVault(client, Vault!);
        if (vault == null)
        {
            return 1;
        }

        var output = new GetVaultOutput
        {
            Id = vault.Id!.Value,
            Label = vault.Summary!.Label,
            Type = vault.Summary!.Type,
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

    public class GetVaultOutput
    {
        public Guid Id { get; set; }

        public string? Type { get; set; }

        public string? Label { get; set; }
    }
}
