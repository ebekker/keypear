// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.CliClient.CliModel.GetCommands;

[Command("template", "templ",
    Description = "retrieve an input Template.")]
public class GetTemplateCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;

    public GetTemplateCommand(GetCommand parent, IConsole console)
    {
        _main = parent.Main;
        _console = console;
    }

    [Option]
    [Required]
    public string? Name { get; set; }

    [Option]
    public string? File { get; set; }

    public int OnExecute()
    {
        var t = Templates.Templates.GetTemplate(Name!);

        if (t == null)
        {
            _console.WriteError("could not resolve Template for given name");
            return 0;
        }

        if (File != null)
        {
            IOFile.WriteAllText(File, t);
        }
        else
        {
            _console.WriteLine(t);
        }

        return 0;
    }
}
