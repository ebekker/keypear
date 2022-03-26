// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Server.LocalServer;
using Keypear.Shared;
using Keypear.Shared.Models.InMemory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Keypear.CliClient.CliModel;

[Subcommand(
    typeof(StatusCommand)
    , typeof(RegisterCommand)
    , typeof(LoginCommand)
    , typeof(LogoutCommand)
    , typeof(LockCommand)
    , typeof(UnlockCommand)
    , typeof(ListCommand)
    , typeof(GetCommand)
    , typeof(NewCommand)
    , typeof(SetCommand)
)]
public class MainCommand
{
    public static readonly JsonSerializerOptions JsonInputOpts = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private readonly IConsole _console;
    private readonly ILogger _logger;
    private readonly KyprDbContext _db;

    public MainCommand(IConsole console
        // Unfortunately, can't use const injection because of:
        //    https://github.com/natemcmaster/CommandLineUtils/issues/448
        /*ILogger<MainCommand> logger, KyprDbContext db*/)
    {
        //_logger = logger;
        //_db = db;

        _console = console;
        _logger = Common.AdditionalServices!.GetRequiredService<ILogger<MainCommand>>();
        _db = Common.AdditionalServices!.GetRequiredService<KyprDbContext>();
    }

    [Option(Description = "optional CLI Session key")]
    public string? Session { get; set; }

    [Option(ShowInHelpText = false)]
    public bool DestroyDb { get; set; }

    [Option(ShowInHelpText = false)]
    public bool MigrateDb { get; set; }

    public int OnExecute(CommandLineApplication app)
    {
        if (DestroyDb)
        {
            _logger.LogInformation("Destroying DB");
            _db.Database.EnsureDeleted();
            return 0;
        }

        if (MigrateDb)
        {
            _logger.LogInformation("Migrating DB");
            _db.Database.Migrate();
            return 0;
        }

        app.ShowHelp();
        return 1;
    }

    public CliSession GetSession(bool skipLoad = false)
    {
        var serverFactory = (KyprSession? s) => new ServerImpl(
            Common.AdditionalServices!.GetRequiredService<ILogger<ServerImpl>>(), _db, s);

        var sess = string.IsNullOrEmpty(Session)
            ? new CliSession(serverFactory)
            : new CliSession(serverFactory, Convert.FromBase64String(Session));

        if (!skipLoad)
        {
            sess.Load();
        }

        return sess;
    }

    /// <summary>
    /// Resolves a Vault reference to a specific Vault instance, or failing
    /// that, will print an appropriate error message to console and return
    /// <c>null</c>.
    /// </summary>
    public Vault? GetVault(KyprClient client, string vaultRef)
    {
        client.UnlockVaults();

        // First try by Id
        var vaults = client.ListVaults().Where(x =>
            x.Id.ToString()!.StartsWith(vaultRef!, StringComparison.OrdinalIgnoreCase));
        if (vaults.Count() == 0)
        {
            // Next try by label
            vaults = client.ListVaults().Where(x =>
                x.Summary!.Label!.StartsWith(vaultRef!, StringComparison.OrdinalIgnoreCase));
        }
        if (vaults.Count() == 0)
        {
            _console.WriteError("no Vaults match the specified reference.");
            return null;
        }
        if (vaults.Count() > 1)
        {
            _console.WriteError("multiple Vaults match the specified reference.");
            return null;
        }

        return vaults.Single();
    }

    /// <summary>
    /// Resolves a Record reference to a specific Record instance, or failing
    /// that, will print an appropriate error message to console and return
    /// <c>null</c>.
    /// </summary>
    public Record? GetRecord(KyprClient client, ref Vault? vault, string recordRef)
    {
        IEnumerable<Vault>? vaults;

        if (vault != null)
        {
            client.UnlockVault(vault);
            client.UnlockRecordSummaries(vault);
            vaults = new[] { vault };
        }
        else
        {
            client.UnlockVaults();
            vaults = client.ListVaults();
            foreach (var v in vaults)
            {
                client.UnlockRecordSummaries(v);
            }
        }

        var records = vaults
            .SelectMany(x => x.Records.Select(y => (vault: x, record: y)))
            .Where(x => x.record.Id.ToString()!.StartsWith(recordRef!, StringComparison.OrdinalIgnoreCase));
        if (records.Count() == 0)
        {
            // Next try by label
            records = vaults
                .SelectMany(x => x.Records.Select(y => (vault: x, record: y)))
                .Where(x => x.record.Summary!.Label!.StartsWith(recordRef!, StringComparison.OrdinalIgnoreCase));
        }
        if (records.Count() == 0)
        {
            _console.WriteError("no Records match the specified reference.");
            return null;
        }
        if (records.Count() > 1)
        {
            _console.WriteError("multiple Records match the specified reference.");
            return null;
        }

        var match = records.Single();
        vault = match.vault;

        return match.record;
    }
}
