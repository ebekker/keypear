// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.CliClient.CliModel;

[Subcommand(
    typeof(GetCommands.GetTemplateCommand)
    , typeof(GetCommands.GetPasswordCommand)
    , typeof(GetCommands.GetVaultCommand)
    , typeof(GetCommands.GetRecordCommand)
    )]
public class GetCommand
{
    public GetCommand(MainCommand main)
    {
        Main = main;
    }

    public MainCommand Main { get; set; }

    public void OnExecute(CommandLineApplication app) => app.ShowHelp();
}
