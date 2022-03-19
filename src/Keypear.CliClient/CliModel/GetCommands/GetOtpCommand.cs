// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using OtpNet;

namespace Keypear.CliClient.CliModel.GetCommands;

[Command("otp",
    Description = "get the generated OTP for a given Record.")]
public class GetOtpCommand
{
    public const string TotpConfigFieldName = "kypr::totp::config";
    public const string TotpSeedFieldName = "kypr::totp::seed";
    public const string TotpConfigVersion = "v1";

    private readonly MainCommand _main;
    private readonly IConsole _console;

    public GetOtpCommand(GetCommand parent, IConsole console)
    {
        _main = parent.Main;
        _console = console;
    }

    [Option]
    public string? Vault { get; set; }

    [Option]
    [Required]
    public string? Record { get; set; }

    public int OnExecute()
    {
        var sess = _main.GetSession();
        using var client = sess.GetClient();

        var vault = Vault == null
            ? null
            : _main.GetVault(client, Vault!);
        if (vault == null && Vault != null)
        {
            return 1;
        }

        var record = _main.GetRecord(client, ref vault, Record!);
        if (record == null)
        {
            return 1;
        }

        client.UnlockRecord(vault!, record);

        string? totpConfig = null;
        string? totpSeed = null;

        if (record.Content!.Fields != null)
        {
            totpConfig = record.Content.Fields.FirstOrDefault(x =>
                object.Equals(TotpConfigFieldName, x.Name))?.Value;
            totpSeed = record.Content.Fields.FirstOrDefault(x =>
                object.Equals(TotpSeedFieldName, x.Name))?.Value;
        }

        if (totpConfig == null || totpSeed == null)
        {
            _console.WriteError("record is not configured for OTP generation");
            return 1;
        }

        var totpConfigParts = totpConfig.Split(';');
        if (totpConfigParts.Length < 3 || totpConfigParts[0] != TotpConfigVersion)
        {
            _console.WriteError("TOTP configuration is invalid or incompatible");
            return 1;
        }

        if (!int.TryParse(totpConfigParts[1], out var totpStep)
            || !int.TryParse(totpConfigParts[2], out var totpSize))
        {
            _console.WriteError("TOTP configuration is malformed");
            return 1;
        }

        totpSeed = totpSeed.Replace(" ", "");
        var totpKey = Base32Encoding.ToBytes(totpSeed);
        var totp = new Totp(totpKey, step: totpStep, totpSize: totpSize);

        _console.WriteLine(totp.ComputeTotp());

        return 0;
    }
}
