using Keypear.Shared.Models.Persisted;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Keypear.Server.LocalServer.Tests;

public class KyprDbContextTests : IDisposable
{
    private readonly ITestOutputHelper _testOut;
    private readonly SqliteConnection _conn;
    private readonly DbContextOptions _opts;
    private readonly KyprDbContext _db;

    public KyprDbContextTests(ITestOutputHelper testOutputHelper)
    {
        _testOut = testOutputHelper;

        _conn = new SqliteConnection("Filename=:memory:");
        _conn.Open();

        _opts = new DbContextOptionsBuilder<KyprDbContext>()
            .UseSqlite<KyprDbContext>(_conn)
            .Options;

        _db = new KyprDbContext(_opts);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _conn.Dispose();
    }

    [Fact]
    public async Task Create_Some_Accounts()
    {
        var accts = new List<Account>
        {
            new() { Username = "one" },
            new() { Username = "TWO" },
            new() { Username = "III" },
        };

        foreach (var a in accts)
        {
            _db.Accounts.Add(a);
        }
        await _db.SaveChangesAsync();

        Assert.Equal(accts.Count, await _db.Accounts.CountAsync());

        foreach (var a in _db.Accounts)
        {
            _testOut.WriteLine(System.Text.Json.JsonSerializer.Serialize(a));
        }


        _db.Accounts.Add(accts.First());
    }
}
