// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.ComponentModel.DataAnnotations;
using Keypear.ClientShared;
using Keypear.Shared;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel;

public class LoginCommand
{
    private readonly MainCommand _main;
    private readonly IConsole _console;
    private readonly Common _common;

    public LoginCommand(MainCommand main, IConsole console, Common common)
    {
        _main = main;
        _console = console;
        _common = common;
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
        }

        var sess = _main.GetSession();
        using var client = sess.GetClient();

        await client.AuthenticateAccountAsync(Email!, Password);
        await client.RefreshVaultsAsync();

        sess.Init(client);
        sess.Save();

        var sessKey = sess.SessionEnvVar;
        _console.WriteLine($"$ export KYPR_SESSION = \"{sessKey}\"");
        _console.WriteLine($"> $env:KYPR_SESSION = \"{sessKey}\"");
    }
}
