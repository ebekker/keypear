// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Server.GrpcServer.Services;
using Keypear.Server.Shared.Data;
using Keypear.Shared.Utils;
using Microsoft.EntityFrameworkCore;
using Keypear.Server.GrpcClient;

namespace Keypear.Server.GrpcServer;

// Minimal APIs are fine for simple projects, but for setting up
// integration tests using TestServer, the classic setup using
// Program + Startup is the easier and more configurable approach

public class Startup
{
    private readonly IConfiguration _config;

    public Startup(IConfiguration configuration)
    {
        _config = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Local logger outside of the M.E.Logging framework
        var preLogger = Program.GetPrelogger<Startup>();

        var serverConfig = _config
            .GetSection(ServerConfig.DefaultConfigName)
            .Get<ServerConfig>();
        KpCommon.ThrowIfNull(serverConfig);

        KpCommon.ThrowIfNull(serverConfig.ConnectionStringName);
        var connName = serverConfig.ConnectionStringName;
        var connString = _config.GetConnectionString(connName);
        KpCommon.ThrowIfNull(connString,
            messageFormat: $"connection string missing: [{connName}]");

        switch (serverConfig.DbDriver)
        {
            case "sqlite":
                preLogger.Info("registering DB Context via SQLite");
                //builder.Services.AddSqlite<KyprDbContext>(connString);
                services.AddDbContext<KyprDbContext>(builder =>
                {
                    builder.UseSqlite(connString);
                });
                break;
            default:
                throw new Exception($"don't know DbDriver [{serverConfig.DbDriver}]");
        }

        if (serverConfig.MigrateOnStart)
        {
            services.AddHostedService<MigrateOnStartHostedService>();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS,
        // visit https://go.microsoft.com/fwlink/?linkid=2099682
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<GrpcServerInterceptor>();
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        logger.LogInformation("WebApplicationb built, wiring up routes...");

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<KyprService>();
            endpoints.MapGet("/", () =>
                "Communication with gRPC endpoints must be made through a gRPC client."
                + " To learn how to create a client,"
                + " visit: https://go.microsoft.com/fwlink/?linkid=2086909");
        });
    }

    public class MigrateOnStartHostedService : IHostedService
    {
        private readonly ILogger<MigrateOnStartHostedService> _logger;
        private readonly IServiceProvider _services;

        public MigrateOnStartHostedService(ILogger<MigrateOnStartHostedService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("DB Migration requested in config...");

            // Need to create our own scope to retrieve scoped deps
            using var scope = _services.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<KyprDbContext>();

            _logger.LogInformation("...migrating...");
            await db.Database.MigrateAsync();
            _logger.LogInformation("...done");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
