// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.CliClient.CliModel;

[Subcommand(
    typeof(SetCommands.SetVaultCommand)
    , typeof(SetCommands.SetRecordCommand)
    )]
public class SetCommand
{
    public SetCommand(MainCommand main)
    {
        Main = main;
    }

    public MainCommand Main { get; set; }

    public void OnExecute(CommandLineApplication app) => app.ShowHelp();
}
