// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using NLog;
using NLog.Web;

namespace Keypear.Server.GrpcServer;

// Minimal APIs are fine for simple projects, but for setting up
// integration tests using TestServer, the classic setup using
// Program + Startup is the easier and more configurable approach

public class Program
{
    public static async Task Main(string[] args)
    {
        // Local logger outside of the M.E.Logging framework
        var preLogger = GetPrelogger<Program>();

        try
        {
            preLogger.Debug("init main");
            var host = CreateHostBuilder(args).Build();

            preLogger.Info("running application...");
            await host.RunAsync();
        }
        catch (Exception exception)
        {
            preLogger.Error(exception, "Stopped program because of exception");
            throw;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            preLogger.Info("shutting down NLog...");
            NLog.LogManager.Shutdown();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            })
            .UseNLog();  // NLog: Setup NLog for Dependency injection


    private static NLog.Config.ISetupBuilder? _setupBuilder;

    public static Logger GetPrelogger<T>()
    {
        if (_setupBuilder == null)
        {
            lock (typeof(Program))
            {
                if (_setupBuilder == null)
                {
                    _setupBuilder = NLog.LogManager.Setup().LoadConfigurationFromAppSettings();
                }
            }
        }

        return _setupBuilder.GetLogger(typeof(T).FullName);
    }
}
