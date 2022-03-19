// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.Text.Json;
using Keypear.Server.LocalServer;
using Keypear.Shared;
using Microsoft.Extensions.Logging;

namespace Keypear.CliClient;

public class Common
{
    private readonly ILogger _logger;
    private readonly KyprDbContext _db;

    public static readonly JsonSerializerOptions DefaultJsonOutput = new(JsonSerializerDefaults.General);
    public static readonly JsonSerializerOptions IndentedJsonOutput = new()
    {
        WriteIndented = true,
    };

    public Common(ILogger<Common> logger, KyprDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public static IServiceProvider? AdditionalServices { get; set; }


    //public KyprClient GetClient(KyprSession? session = null)
    //{
    //    if (session == null)
    //    {
    //        _logger.LogInformation("No Existing Session");
    //    }
    //    else
    //    {
    //        _logger.LogInformation("Existing Session:");
    //        _logger.LogInformation(JsonSerializer.Serialize(session));
    //    }

    //    var server = new Server.LocalServer.ServerImpl(_db, session);
    //    var client = new KyprClient(server);

    //    return client;
    //}
}
