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
    private readonly IConsole _console;
    private readonly KyprClient _client;

    public RegisterCommand(IConsole console, KyprClient client)
    {
        _client = client;
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

        await _client.CreateAccountAsync(Email!, Password);

        var sess = new CliSession();
        sess.Init(new()
        {
            ServerSession = _client.Session,
            Account = _client.Account,
            Vaults = _client.Vaults
        });

        sess.Save();
        var sessKey = sess.SessionEnvVar;
        _console.WriteLine($"$ export KYPR_SESSION = \"{sessKey}\"");
        _console.WriteLine($"> $env:KYPR_SESSION = \"{sessKey}\"");
    }
}
