using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Keypear.CliClient.CliModel.ListCommands;
using McMaster.Extensions.CommandLineUtils;
using Xunit;

namespace Keypear.CliClient.Tests;

public class CliTests
{
    [Fact]
    public async Task Toplevel_Help()
    {
        var cli = new CliTester();
        await cli.InitAsync();

        var ret = await cli.InvokeAsync("--help");
        Assert.Equal(0, ret);
    }

    [Fact]
    public async Task Register_New_Account()
    {
        using var cli = new CliTester();
        await cli.InitAsync();

        var ret = await cli.InvokeAsync("reg", "--email", "john.doe@example.com", "--password", "foobar");
        Assert.Equal(0, ret);

        var sessionKey = cli.Console.OutWriter.GetStringBuilder().ToString();
        Assert.NotEmpty(sessionKey);
    }

    [Fact]
    public async Task Create_Vaults()
    {
        using var cli = new CliTester();
        await cli.InitAsync();

        int ret;

        ret = await cli.InvokeAsync("reg", "--email", "john.doe@example.com", "--password", "foobar", "--raw-session");
        Assert.Equal(0, ret);

        var sessionKey = cli.ReadOut();
        Assert.NotEmpty(sessionKey);

        cli.SetEnv("KYPR_SESSION", sessionKey);

        ret = await cli.InvokeAsync("list", "vaults");
        Assert.Equal(0, ret);

        var output = cli.ReadOut();
        Assert.NotEmpty(output);

        var vaults = JsonSerializer.Deserialize<ListVaultsCommand.ListVaultsOutput>(output);
        Assert.Empty(vaults!.Vaults);

        ret += await cli.InvokeAsync("new", "vault", "--label", "v1");
        ret += await cli.InvokeAsync("new", "vault", "--label", "v2");
        ret += await cli.InvokeAsync("new", "vault", "--label", "v3");
        var vaultIds = cli.ReadOut().Trim().Split(Environment.NewLine);
        Assert.Equal(3, vaultIds.Length);

        ret += await cli.InvokeAsync("list", "vaults");
        Assert.Equal(0, ret);

        output = cli.ReadOut();
        vaults = JsonSerializer.Deserialize<ListVaultsCommand.ListVaultsOutput>(output);
        Assert.Equal(3, vaults!.Vaults.Count);

        Assert.True(vaults.Vaults.All(x => vaultIds.Contains(x.Id.ToString())));
        Assert.True(vaultIds.All(x => vaults.Vaults.Any(y => y.Id.ToString() == x)));
    }

    [Fact]
    public async Task Create_TOTP_Record_and_Generate_Code()
    {
        using var cli = new CliTester();
        await cli.InitAsync();

        int ret;

        await cli.InvokeSuccessfullyAsync("reg", "--email", "john.doe@example.com", "--password", "foobar", "--raw-session");
        var sessionKey = cli.ReadOut();
        cli.SetEnv("KYPR_SESSION", sessionKey);

        await cli.InvokeSuccessfullyAsync("new", "vault", "--label", "v1");
        _ = cli.ReadOut();

        await cli.InvokeSuccessfullyAsync("new", "rec", "--vault", "v1", "--file", "samples/record-totp1.json");
        var recId = cli.ReadOut().Trim();

        await cli.InvokeSuccessfullyAsync("get", "otp", "--record", recId);
        var otpCode = cli.ReadOut().Trim();
        Assert.NotEmpty(otpCode);
        Assert.Equal(6, otpCode.Length);

        var otpSecret = "I65VU7K5ZQL7WB4E";
        var otpCodeUrl = $"https://authenticationtest.com/totp/?secret={otpSecret}";
        using var http = new HttpClient();
        var otpJson = await http.GetStringAsync(otpCodeUrl);
        var otpResp = JsonSerializer.Deserialize<AuthTestOtpResponse>(otpJson);

        Assert.Equal(otpResp!.code, otpCode);
    }

    internal class AuthTestOtpResponse
    {
        public string? code { get; set; }
        public string? readme { get; set; }
    }

    internal class CliTester : IDisposable
    {
        private readonly Cli _cli;

        private readonly string _dbPath = $"test.db";
        private readonly Dictionary<string, string> _env = new();

        public CliTester()
        {
            _cli = new Cli();
        }

        public TestConsole Console { get; } = new();

        public void SetEnv(string name, string value)
        {
            _env[name] = value;
            Environment.SetEnvironmentVariable(name, value);
        }

        public string ReadErr()
        {
            var sb = Console.ErrWriter.GetStringBuilder();
            var errVal = sb.ToString();

            sb.Clear();

            return errVal;
        }

        public string ReadOut()
        {
            var sb = Console.OutWriter.GetStringBuilder();
            var outVal = sb.ToString();

            sb.Clear();

            return outVal;
        }

        public void Dispose()
        {
            //GC.Collect();
            //Thread.Sleep(25000);
            //System.IO.File.Delete(_dbPath);

            foreach (var kv in _env)
            {
                Environment.SetEnvironmentVariable(kv.Key, null);
            }

            Assert.Equal(0, _cli.InvokeAsync("--destroy-db").Result);
        }

        public async Task InitAsync()
        {
            _cli.SqliteDbFile = _dbPath;
            _cli.CustomConsole = this.Console;

            await _cli.InitAsync();

            Assert.Equal(0, await _cli.InvokeAsync("--migrate-db"));
        }

        public async Task<int> InvokeAsync(params string[] args)
        {
            var ret = await _cli.InvokeAsync(args)!;

            return ret;
        }

        public async Task InvokeSuccessfullyAsync(params string[] args)
        {
            var ret = await _cli.InvokeAsync(args);
            Assert.Equal(0, ret);
        }
    }

    internal class TestConsole : IConsole
    {
        /// <summary>
        /// A shared instance of <see cref="PhysicalConsole"/>.
        /// </summary>
        public static IConsole Singleton { get; } = new TestConsole();

        public TestConsole()
        { }

        public StringWriter ErrWriter { get; } = new();

        public StringReader InReader { get; } = new("");

        public StringWriter OutWriter { get; } = new();

        /// <summary>
        /// <see cref="Console.CancelKeyPress"/>.
        /// </summary>
        public event ConsoleCancelEventHandler? CancelKeyPress
        {
            add => Console.CancelKeyPress += value;
            remove
            {
                try
                {
                    Console.CancelKeyPress -= value;
                }
                catch (PlatformNotSupportedException)
                {
                    // https://github.com/natemcmaster/CommandLineUtils/issues/344
                    // Suppress this error during unsubscription on some Xamarin platforms.
                }
            }
        }

        /// <summary>
        /// <see cref="Console.Error"/>.
        /// </summary>
        public TextWriter Error => this.ErrWriter;

        /// <summary>
        /// <see cref="Console.In"/>.
        /// </summary>
        public TextReader In => this.InReader;

        /// <summary>
        /// <see cref="Console.Out"/>.
        /// </summary>
        public TextWriter Out => this.OutWriter;

        /// <summary>
        /// <see cref="Console.IsInputRedirected"/>.
        /// </summary>
        public bool IsInputRedirected => Console.IsInputRedirected;

        /// <summary>
        /// <see cref="Console.IsOutputRedirected"/>.
        /// </summary>
        public bool IsOutputRedirected => Console.IsOutputRedirected;

        /// <summary>
        /// <see cref="Console.IsErrorRedirected"/>.
        /// </summary>
        public bool IsErrorRedirected => Console.IsErrorRedirected;

        /// <summary>
        /// <see cref="Console.ForegroundColor"/>.
        /// </summary>
        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        /// <summary>
        /// <see cref="Console.BackgroundColor"/>.
        /// </summary>
        public ConsoleColor BackgroundColor
        {
            get => Console.BackgroundColor;
            set => Console.BackgroundColor = value;
        }

        /// <summary>
        /// <see cref="Console.ResetColor"/>.
        /// </summary>
        public void ResetColor() => Console.ResetColor();
    }
}
