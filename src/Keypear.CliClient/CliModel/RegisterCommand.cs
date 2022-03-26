// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

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

    [Option]
    public bool RawSession { get; set; }

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

        var sess = _main.GetSession(skipLoad: true);
        using var client = sess.GetClient();

        await client.CreateAccountAsync(Email!, Password);
        sess.Init(client);
        sess.Save();

        var sessKey = sess.SessionEnvVar;

        if (RawSession)
        {
            _console.WriteLine(sessKey);
        }
        else
        {
            _console.WriteLine($"$ export KYPR_SESSION = \"{sessKey}\"");
            _console.WriteLine($"> $env:KYPR_SESSION = \"{sessKey}\"");
        }
    }
}
