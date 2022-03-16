// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.ComponentModel.DataAnnotations;
using Keypear.ClientShared;
using Keypear.Shared;
using McMaster.Extensions.CommandLineUtils;

namespace Keypear.CliClient.CliModel;

public class LoginCommand
{
    private readonly IConsole _console;
    private readonly Common _common;

    public LoginCommand(IConsole console, Common common)
    {
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

        var _client = _common.GetClient();

        await _client.AuthenticateAccountAsync(Email!, Password);
        await _client.RefreshVaultsAsync();

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
