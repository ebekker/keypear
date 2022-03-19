// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.CliClient.CliModel.ListCommands;

[Command("templates", "templ",
    Description = "lists Templates available for file input.")]
public class ListTemplatesCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;

    public ListTemplatesCommand(ListCommand parent, IConsole console)
    {
        _main = parent.Main;
        _console = console;
    }

    public int OnExecute()
    {
        var templates = Templates.Templates.GetTemplates().ToArray();
        var longest = templates.Max(x => x.Key.Length) + 3;

        foreach (var t in templates)
        {
            _console.WriteLine($"{t.Key.PadRight(longest, '.')}:  {t.Value?.Description}");
        }

        return 0;
    }
}
