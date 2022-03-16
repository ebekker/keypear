// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel;

[Subcommand(
    typeof(StatusCommand)
    , typeof(RegisterCommand)
    , typeof(LoginCommand)
    , typeof(ListCommand)
    , typeof(GetCommand)
    , typeof(NewCommand)
)]
public class MainCommand
{
    public void OnExecute(CommandLineApplication app) => app.ShowHelp();
}
