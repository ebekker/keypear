// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.CliClient.CliModel;
using McMaster.Extensions.CommandLineUtils.HelpText;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Keypear.CliClient;

public class Cli
{
    private ILogger? _logger;
    private CommandLineApplication? _cla;

    public string? SqliteDbFile { get; set; } = "_TMP/foo.db";

    public IConsole? CustomConsole { get; set; }

    public Task InitAsync()
    {
        //var opts = new DbContextOptionsBuilder<Keypear.Server.LocalServer.KyprDbContext>()
        //    .UseSqlite<Keypear.Server.LocalServer.KyprDbContext>("Filename=_TMP/foo.db")
        //    .Options;
        //var db = new Keypear.Server.LocalServer.KyprDbContext(opts);
        //await db.Database.EnsureCreatedAsync();

        //var server = new Keypear.Server.LocalServer.ServerImpl(db);
        //await server.InitAsync();
        //var client = new Keypear.Shared.KyprClient(server);

        var services = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddNLog();
            })
            .AddDbContext<Keypear.Server.LocalServer.KyprDbContext>(builder =>
            {
                //builder.LogTo(x => { }, LogLevel.Error);
                builder.UseSqlite($"Filename={SqliteDbFile}");
            })
            .AddScoped<Common>()
            .BuildServiceProvider();

        _logger = services.GetRequiredService<ILogger<Cli>>();

        // Unfortunately to get around a bug:
        //    https://github.com/natemcmaster/CommandLineUtils/issues/448
        Common.AdditionalServices = services;

        _logger.LogInformation("building CLI command model");

        _cla = CustomConsole == null
            ? new CommandLineApplication<MainCommand>()
            : new CommandLineApplication<MainCommand>(CustomConsole);
        _cla.HelpTextGenerator = new DefaultHelpTextGenerator
        {
            SortCommandsByName = false,
        };

        _cla.HelpOption();
        _cla.Conventions
            .UseDefaultConventions()
            .UseConstructorInjection(services)
            ;

        return Task.CompletedTask;
    }

    public async Task<int> InvokeAsync(params string[] args)
    {
        if (_cla == null)
        {
            throw new InvalidOperationException("application has not been initialized");
        }

        _logger!.LogInformation("invoking app execution");
        return await _cla.ExecuteAsync(args);
    }
}
