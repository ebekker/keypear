// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.CliClient.CliModel;

[Command(Description = "list entities of a given type.")]
[Subcommand(
    typeof(ListCommands.ListTemplatesCommand)
    , typeof(ListCommands.ListVaultsCommand)
    , typeof(ListCommands.ListRecordsCommand)
    )]
public class ListCommand
{
    public ListCommand(MainCommand main)
    {
        Main = main;
    }

    public MainCommand Main { get; set; }

    public void OnExecute(CommandLineApplication app) => app.ShowHelp();
}
