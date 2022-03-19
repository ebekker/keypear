// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.CliClient.CliModel;

[Command(Description = "creates a new entity of a given type.")]
[Subcommand(
    typeof(NewCommands.NewVaultCommand)
    , typeof(NewCommands.NewRecordCommand)
    )]
public class NewCommand
{
    public NewCommand(MainCommand main)
    {
        Main = main;
    }

    public MainCommand Main { get; set; }

    public void OnExecute(CommandLineApplication app) => app.ShowHelp();
}
