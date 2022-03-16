// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.CliClient;
using Keypear.CliClient.CliModel;
using Keypear.ClientShared;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.HelpText;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

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
        builder.UseSqlite("Filename=_TMP/foo.db");
    })
    .AddScoped<Common>()
    .BuildServiceProvider();

var app = new CommandLineApplication<MainCommand>();
app.HelpTextGenerator = new DefaultHelpTextGenerator
{
    SortCommandsByName = false,
};

app.HelpOption();
app.Conventions
    .UseDefaultConventions()
    .UseConstructorInjection(services);

return await app.ExecuteAsync(args);
