// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.ComponentModel.DataAnnotations;
using Keypear.ClientShared;
using Keypear.Shared;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore;

namespace Keypear.CliClient.CliModel;

[Command("register", "reg",
    ShowInHelpText = false, // hidden for now
    Description = "create a new account")]
class RegisterCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;

    public RegisterCommand(MainCommand main, IConsole console)
    {
        _main = main;
        _console = console;
    }

    [Option]
    [Required]
    public string? Email { get; set; }

    [Option]
    public string? Password { get; set; }

    public async Task OnExecuteAsync()
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            Password = Prompt.GetPassword("Master Password: ");
            var confirm = Prompt.GetPassword("Confirm Password: ");
            if (!string.Equals(Password, confirm))
            {
                throw new Exception("passwords don't match");
            }
        }

        var sess = _main.GetSession();
        using var client = sess.GetClient();

        await client.CreateAccountAsync(Email!, Password);
        sess.Init(client);
        sess.Save();

        var sessKey = sess.SessionEnvVar;
        _console.WriteLine($"$ export KYPR_SESSION = \"{sessKey}\"");
        _console.WriteLine($"> $env:KYPR_SESSION = \"{sessKey}\"");
    }
}
