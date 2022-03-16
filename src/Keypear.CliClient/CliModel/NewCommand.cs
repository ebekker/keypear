// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.ComponentModel.DataAnnotations;
using Keypear.Shared;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel;

[Command(Description = "creates a new entity of a given type.")]
[Subcommand(
    typeof(NewCommands.VaultCommand)
    , typeof(NewCommands.RecordCommand)
    )]
public class NewCommand
{
    //private readonly IConsole _console;
    //private readonly Common? _common;

    //public NewCommand(IConsole console, Common common)
    //{
    //    _console = console;
    //    _common = common;
    //}

    public void OnExecute(CommandLineApplication app) => app.ShowHelp();
}
