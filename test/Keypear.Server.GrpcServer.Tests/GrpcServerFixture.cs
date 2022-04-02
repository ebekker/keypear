// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Server.Shared.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Keypear.Server.GrpcServer.Tests;

//// Since these assemblies both have a global-namespaced Program
//// type, and we need to reference one of them below, we need to
//// define extern aliases to them with each reference in .csproj
//extern alias GrpcClient;
//extern alias GrpcServer;

public class GrpcServerFixture : IDisposable
{
    private TestServer? _server;
    private IHost? _host;
    private HttpMessageHandler? _handler;
    private Action<IWebHostBuilder>? _configureWebHost;

    public IConfiguration? Configuration { get; set; }

    public GrpcServerFixture()
    {
        //LoggerFactory = new LoggerFactory();
        //LoggerFactory.AddProvider(new ForwardingLoggerProvider((logLevel, category, eventId, message, exception) =>
        //{
        //    LoggedMessage?.Invoke(logLevel, category, eventId, message, exception);
        //}));
    }

    public void Dispose()
    {
        _handler?.Dispose();
        _host?.Dispose();
        _server?.Dispose();
    }
    public void ConfigureWebHost(Action<IWebHostBuilder> configure)
    {
        _configureWebHost = configure;
    }
    public HttpMessageHandler Handler
    {
        get
        {
            EnsureServer();
            return _handler!;
        }
    }

    public async Task WithDbContext(Func<KyprDbContext, Task> invoke)
    {
        using (var scope = _host!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<KyprDbContext>();
            await invoke(db);
        }
    }

    ////private EnsureDbHostedService? _ensureDb;

    private void EnsureServer()
    {
        if (_host == null)
        {
            var builder = new HostBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureServices(services =>
                {
                    //services.AddSingleton<ILoggerFactory>(LoggerFactory);
                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseTestServer();

                    if (Configuration == null)
                    {
                        builder.UseStartup<Startup>();
                    }
                    else
                    {
                        builder.UseStartup<Startup>(context => new Startup(Configuration));
                    }

                    builder.ConfigureTestServices(services =>
                    {
                        ////services.AddHostedService<EnsureDbHostedService>(services =>
                        ////{
                        ////    var logger = services.GetRequiredService<ILogger<EnsureDbHostedService>>();
                        ////    _ensureDb = new EnsureDbHostedService(logger, services);
                        ////    return _ensureDb;
                        ////});
                    });

                    _configureWebHost?.Invoke(builder);
                })
                .UseNLog();
            _host = builder.Start();
            _server = _host.GetTestServer();
            _handler = _server.CreateHandler();
        }
    }

    ////public class EnsureDbHostedService : IHostedService
    ////{
    ////    private readonly ILogger _logger;
    ////    private readonly IServiceProvider _services;

    ////    public EnsureDbHostedService(ILogger<EnsureDbHostedService> logger, IServiceProvider services)
    ////    {
    ////        _logger = logger;
    ////        _services = services;
    ////    }

    ////    public async Task StartAsync(CancellationToken cancellationToken)
    ////    {
    ////        _logger.LogInformation("DB Migration requested in config...");

    ////        // Need to create our own scope to retrieve scoped deps
    ////        using var scope = _services.CreateScope();
    ////        using var db = scope.ServiceProvider.GetRequiredService<KyprDbContext>();

    ////        _logger.LogInformation("...Ensuring DB...");
    ////        await db.Database.EnsureDeletedAsync();
    ////        await db.Database.EnsureCreatedAsync();
    ////        _logger.LogInformation("...done");
    ////    }

    ////    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    ////}
}
