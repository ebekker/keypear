using Keypear.Shared.Krypto;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Keypear.Server.LocalServer.Tests;

public class LocalServerTests : IDisposable
{
    private readonly ITestOutputHelper _testOut;
    private readonly ILoggerFactory _loggerFactory;
    private readonly SqliteConnection _conn;
    private readonly DbContextOptions<KyprDbContext> _opts;
    private readonly KyprDbContext _db;
    private readonly ServerImpl _server;

    public LocalServerTests(ITestOutputHelper testOutputHelper)
    {
        _testOut = testOutputHelper;
        _loggerFactory = new LoggerFactory();

        _conn = new SqliteConnection("Filename=:memory:");
        _conn.Open();

        _opts = new DbContextOptionsBuilder<KyprDbContext>()
            .UseSqlite<KyprDbContext>(_conn)
            .Options;

        _db = new KyprDbContext(_opts);
        _db.Database.EnsureCreated();

        _server = new(_loggerFactory.CreateLogger<ServerImpl>(), _db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _conn.Dispose();
    }

    [Fact]
    public async Task Create_Some_Accounts()
    {
        var accts = new List<string>
        {
            "one",
            "TWO",
            "III",
        };

        PublicKeyEncryption _pke = new();
        PublicKeySignature _pks = new();

        foreach (var a in accts)
        {
            _pke.GenerateKeyPair(out var pkePrv, out var pkePub);
            _pks.GenerateKeyPair(out var pksPrv, out var pksPub);
            var ad = await _server.CreateAccountAsync(new()
            {
                Username = a,
                PublicKey = pkePub,
                PrivateKeyEnc = pkePrv,
                SigPublicKey = pksPub,
                SigPrivateKeyEnc = pksPrv,
            });

            _testOut.WriteLine(System.Text.Json.JsonSerializer.Serialize(ad));
        }

        foreach (var a in _db.Accounts)
        {
            _testOut.WriteLine(System.Text.Json.JsonSerializer.Serialize(a));
        }
    }

    [Fact]
    public async Task Create_Duplicate_Account()
    {
        var username = "jdoe@example.com";
        PublicKeyEncryption _pke = new();
        PublicKeySignature _pks = new();

        _pke.GenerateKeyPair(out var pkePrv, out var pkePub);
        _pks.GenerateKeyPair(out var pksPrv, out var pksPub);
        var ad = await _server.CreateAccountAsync(new()
        {
            Username = username,
            PublicKey = pkePub,
            PrivateKeyEnc = pkePrv,
            SigPublicKey = pksPub,
            SigPrivateKeyEnc = pksPrv,
        });

        _pke.GenerateKeyPair(out pkePrv, out pkePub);
        _pks.GenerateKeyPair(out pksPrv, out pksPub);

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            ad = await _server.CreateAccountAsync(new()
            {
                Username = username,
                PublicKey = pkePub,
                PrivateKeyEnc = pkePrv,
                SigPublicKey = pksPub,
                SigPrivateKeyEnc = pksPrv,
            });
        });
    }
}
