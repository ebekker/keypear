////using Keypear.Server.GrpcServer;
////using Keypear.Server.GrpcServer.Services;
////using Keypear.Server.Shared.Data;
////using Keypear.Shared.Utils;
////using Microsoft.EntityFrameworkCore;
////using NLog;
////using NLog.Web;

////// Local logger outside of the M.E.Logging framework
////var preLogger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
////preLogger.Debug("init main");

////// Assemble our dependencies
////var builder = WebApplication.CreateBuilder(args);

////// Additional configuration is required to successfully run gRPC on macOS.
////// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

////builder.Logging.ClearProviders();
////builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
////builder.Host.UseNLog();

////var serverConfig = builder.Configuration
////    .GetSection(ServerConfig.DefaultConfigName)
////    .Get<ServerConfig>();
////KpCommon.ThrowIfNull(serverConfig);

////KpCommon.ThrowIfNull(serverConfig.ConnectionStringName);
////var connName = serverConfig.ConnectionStringName;
////var connString = builder.Configuration.GetConnectionString(connName);
////KpCommon.ThrowIfNull(connString,
////    messageFormat: $"connection string missing: [{connName}]");

////switch (serverConfig.DbDriver)
////{
////    case "sqlite":
////        preLogger.Info("registering DB Context via SQLite");
////        //builder.Services.AddSqlite<KyprDbContext>(connString);
////        builder.Services.AddDbContext<KyprDbContext>(builder =>
////        {
////            builder.UseSqlite(connString);
////        });
////        break;
////    default:
////        throw new Exception($"don't know DbDriver [{serverConfig.DbDriver}]");
////}

////// Add services to the container.
////builder.Services.AddGrpc();

////// Build our Web App
////var app = builder.Build();
////var logger = app.Services.GetService<ILogger<Program>>()!;

////logger.LogInformation("WebApplicationb built, wiring up routes...");

////// Configure the HTTP request pipeline.
////app.MapGrpcService<KyprService>();
////app.MapGet("/", () =>
////    "Communication with gRPC endpoints must be made through a gRPC client."
////    + " To learn how to create a client,"
////    + " visit: https://go.microsoft.com/fwlink/?linkid=2086909");

////try
////{
////    if (serverConfig.MigrateOnStart)
////    {
////        logger.LogInformation("DB Migration requested in config...");
////        // Need to create our own scope to retrieve scoped deps
////        using (var scope = app.Services.CreateScope())
////        {
////            using var db = scope.ServiceProvider.GetRequiredService<KyprDbContext>();
////            logger.LogInformation("...migrating...");
////            await db.Database.MigrateAsync();
////            logger.LogInformation("...done");
////        }
////    }

////    logger.LogInformation("running application...");
////    app.Run();
////}
////finally
////{
////    // Ensure to flush and stop internal timers/threads before
////    // application-exit (Avoid segmentation fault on Linux)
////    logger.LogInformation("shutting down NLog...");
////    NLog.LogManager.Shutdown();
////}
