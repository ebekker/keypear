// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.Security.Cryptography;
using Keypear.Server.LocalServer;
using Keypear.Shared.Models.InMemory;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Keypear.Shared.Tests;

public class KyprClientTests : IDisposable
{
    private readonly ITestOutputHelper _testOut;
    private readonly ILoggerFactory _loggerFactory;
    private readonly SqliteConnection _conn;
    private readonly DbContextOptions<KyprDbContext> _opts;
    private readonly KyprDbContext _db1;
    private readonly KyprDbContext _db2;
    private readonly IKyprServer _server1;
    private readonly IKyprServer _server2;

    public KyprClientTests(ITestOutputHelper testOutputHelper)
    {
        _testOut = testOutputHelper;
        _loggerFactory = new LoggerFactory();

        _conn = new SqliteConnection("Filename=:memory:");
        _conn.Open();

        _opts = new DbContextOptionsBuilder<KyprDbContext>()
            .UseSqlite<KyprDbContext>(_conn)
            .Options;

        _db1 = new KyprDbContext(_opts);
        _db1.Database.EnsureCreated();
        _server1 = new ServerImpl(_loggerFactory.CreateLogger<ServerImpl>(), _db1);

        _db2 = new KyprDbContext(_opts);
        _db2.Database.EnsureCreated();
        _server2 = new ServerImpl(_loggerFactory.CreateLogger<ServerImpl>(), _db2);
    }

    public void Dispose()
    {
        _server2.Dispose();
        _db2.Dispose();

        _server1.Dispose();
        _db1.Dispose();

        _conn.Dispose();
    }

    [Fact]
    public async Task Create_Account_Unique_Master_Keys()
    {
        var username = "jdoe@example.com";
        var password = "foo bar non";

        using var client1 = new KyprClient(_server1);
        using var client2 = new KyprClient(_server1);

        await client1.CreateAccountAsync(username + "1", password);
        await client2.CreateAccountAsync(username + "2", password);

        var acct1 = client1.Account;
        var acct2 = client2.Account;

        Assert.NotEqual(
            acct1!.MasterKey,
            acct2!.MasterKey);
    }

    [Fact]
    public async Task Create_and_Lock_and_Fail_Unlock_Account_With_Bad_Password()
    {
        var username = "jdoe@example.com";
        var password = "foo bar non";

        using var client = new KyprClient(_server1);

        await client.CreateAccountAsync(username, password);
        client.LockAccount();

        Assert.Throws<CryptographicException>(() =>
            client.UnlockAccount("not" + password));
    }

    [Fact]
    public async Task Create_and_Lock_and_Unlock_Account()
    {
        var username = "jdoe@example.com";
        var password = "foo bar non";

        using var client = new KyprClient(_server1);

        await client.CreateAccountAsync(username, password);
        client.LockAccount();

        client.UnlockAccount(password);
    }

    [Fact]
    public async Task Create_Vault_Unique_Vault_Keys()
    {
        var username = "jdoe@example.com";
        var password = "foo bar non";
        var vaultLabel1 = "Vault #1";
        var vaultLabel2 = "Vault #2";

        using var client1 = new KyprClient(_server1);
        using var client2 = new KyprClient(_server1);

        await client1.CreateAccountAsync(username + "1", password);
        var acct1 = client1.Account;
        var acct1Vault1 = await client1.CreateVaultAsync(vaultLabel1);
        var acct1Vault2 = await client1.CreateVaultAsync(vaultLabel2);

        await client2.CreateAccountAsync(username + "2", password);
        var acct2 = client2.Account;
        var acct2Vault1 = await client1.CreateVaultAsync(vaultLabel1);
        var acct2Vault2 = await client1.CreateVaultAsync(vaultLabel2);

        Assert.NotEqual(
            acct1Vault1!.SecretKey,
            acct1Vault2!.SecretKey);

        Assert.NotEqual(
            acct1Vault1!.SecretKey,
            acct2Vault1!.SecretKey);
    }

    [Fact]
    public async Task Create_Vaults_List_Vaults()
    {
        var username = "jdoe@example.com";
        var password = "foo bar non";

        var vaultLabels = Enumerable.Range(1, 10)
            .Select(_ => "Vault " + Guid.NewGuid())
            .ToList();

        using var client1 = new KyprClient(_server1);
        await client1.CreateAccountAsync(username + "1", password);
        var acct1 = client1.Account;
        var vaults1 = await Task.WhenAll(vaultLabels.Select(async x => await client1.CreateVaultAsync(x)).ToList());

        using var client2 = new KyprClient(_server1);
        await client2.CreateAccountAsync(username + "2", password);
        var acct2 = client2.Account;

        var vaults2 = await Task.WhenAll(vaultLabels.Select(async x => await client2.CreateVaultAsync(x)).ToList());

        Assert.Equal(vaults1, client1.ListVaults());
        Assert.Equal(vaults2, client2.ListVaults());
    }

    [Fact]
    public async Task Create_Lock_and_Unlock_Vault()
    {
        var username = "jdoe@example.com";
        var password = "foo bar non";

        using var client = new KyprClient(_server1);

        await client.CreateAccountAsync(username, password);

        var vault = await client.CreateVaultAsync("vault1");
        Assert.NotEmpty(vault.SecretKey);

        client.LockVault(vault);
        Assert.Null(vault.SecretKey);

        client.UnlockVault(vault);
        Assert.NotEmpty(vault.SecretKey);

        client.LockAccount();
        Assert.Null(vault.SecretKey);
    }

    [Fact]
    public async Task Create_and_Search_Records()
    {
        var username = "jdoe@example.com";
        var password = "foo bar non";

        var recordLabels = Enumerable.Range(1, 10)
            .Select(_ => Guid.NewGuid())
            .SelectMany(x => Enumerable.Range(1, 5).Select(_ =>
                x + "-_" + Guid.NewGuid()))
            .ToList();

        using var client = new KyprClient(_server1);

        await client.CreateAccountAsync(username, password);

        var vault = await client.CreateVaultAsync("Vault #1");

        foreach (var rl in recordLabels)
        {
            await client.SaveRecordAsync(vault, new()
            {
                Summary = new()
                {
                    Label = "Record " + rl,
                    Username = Guid.NewGuid().ToString(),
                },
                Content = new()
                {
                    Password = "fubaz",
                }
            });
        }

        foreach (var rl in recordLabels)
        {
            var rlParts = rl.Split("_");
            var hits1 = client.SearchRecords(vault, rlParts[0]);
            var hits2 = client.SearchRecords(vault, rlParts[1]);

            Assert.Equal(5, hits1.Count());
            Assert.Single(hits2);
        }

        Assert.Equal(vault.Records, client.SearchRecords(vault, "RECORD"));
    }

    [Fact]
    public async Task Create_Lock_and_Unlock_Records()
    {
        var username = "jdoe@example.com";
        var password = "foo bar non";

        var recordLabels = Enumerable.Range(1, 10)
            .Select(_ => Guid.NewGuid())
            .SelectMany(x => Enumerable.Range(1, 5).Select(_ =>
                x + "-_" + Guid.NewGuid()))
            .ToList();
        var recordValues = recordLabels.ToDictionary(x => x, x => Guid.NewGuid().ToString());

        using var client = new KyprClient(_server1);

        await client.CreateAccountAsync(username, password);

        var vault = await client.CreateVaultAsync("vault1");

        foreach (var rl in recordLabels)
        {
            await client.SaveRecordAsync(vault, new()
            {
                Summary = new()
                {
                    Label = "Record " + rl,
                    Username = "user-" + recordValues[rl],
                },
                Content = new()
                {
                    Password = "value-" + recordValues[rl],
                }
            });
        }

        client.LockAccount();
        // Vault should be locked
        Assert.Null(vault.SecretKey);
        // All records should be locked
        foreach (var r in vault.Records)
        {
            Assert.Null(r.Summary);
            Assert.Null(r.SummarySer);

            Assert.Null(r.Content);
            Assert.Null(r.ContentSer);
        }

        client.UnlockAccount(password);
        client.UnlockVault(vault);
        Assert.NotEmpty(vault.SecretKey);
        client.UnlockRecordSummaries(vault);
        foreach (var r in vault.Records)
        {
            Assert.NotNull(r.Summary);
            Assert.NotEmpty(r.SummarySer);

            Assert.Null(r.Content);
            Assert.Null(r.ContentSer);
        }

        foreach (var r in vault.Records)
        {
            client.UnlockRecord(vault, r);

            Assert.NotNull(r.Summary);
            Assert.NotEmpty(r.SummarySer);

            Assert.NotNull(r.Content);
            Assert.NotEmpty(r.ContentSer);

        }

        foreach (var rl in recordLabels)
        {
            var rlParts = rl.Split("_");
            var hits1 = client.SearchRecords(vault, rlParts[0]);
            var hits2 = client.SearchRecords(vault, rlParts[1]);

            Assert.Equal(5, hits1.Count());
            Assert.Single(hits2);
        }

        Assert.Equal(vault.Records, client.SearchRecords(vault, "RECORD"));
    }

    [Fact]
    public async Task Create_Records_and_Recover_With_Second_Client()
    {
        var username = "jdoe@example.com";
        var password = "foo bar non";

        var recordLabels = Enumerable.Range(1, 10)
            .Select(_ => Guid.NewGuid())
            .SelectMany(x => Enumerable.Range(1, 5).Select(_ =>
                x + "-_" + Guid.NewGuid()))
            .ToList();
        var recordValues = recordLabels.ToDictionary(x => x, x => Guid.NewGuid().ToString());

        using (var client1 = new KyprClient(_server1))
        {
            await client1.CreateAccountAsync(username, password);

            var vault1 = await client1.CreateVaultAsync("vault1");

            foreach (var rl in recordLabels)
            {
                await client1.SaveRecordAsync(vault1, new()
                {
                    Summary = new()
                    {
                        Label = "Record " + rl,
                        Username = "user-" + recordValues[rl],
                    },
                    Content = new()
                    {
                        Password = "value-" + recordValues[rl],
                    }
                });
            }
        }

        using var client1a = new KyprClient(_server2);
        await client1a.AuthenticateAccountAsync(username, password);
        await client1a.RefreshVaultsAsync();
        var vault1a = client1a.Vaults.Single();
        client1a.UnlockVault(vault1a);
        client1a.UnlockRecordSummaries(vault1a);

        Assert.Equal("vault1", vault1a.Summary!.Label);

        foreach (var rl in recordLabels)
        {
            var rlParts = rl.Split("_");
            var hits1 = client1a.SearchRecords(vault1a, rlParts[0]);
            var hits2 = client1a.SearchRecords(vault1a, rlParts[1]);

            Assert.Equal(5, hits1.Count());
            Assert.Single(hits2);
        }

        Assert.Equal(vault1a.Records, client1a.SearchRecords(vault1a, "RECORD"));
    }

    [Fact]
    public async Task Create_Records_and_Recover_With_Second_Server_and_Session()
    {
        var username = "jdoe@example.com";
        var password = "foo bar non";

        var recordLabels = Enumerable.Range(1, 10)
            .Select(_ => Guid.NewGuid())
            .SelectMany(x => Enumerable.Range(1, 5).Select(_ =>
                x + "-_" + Guid.NewGuid()))
            .ToList();
        var recordValues = recordLabels.ToDictionary(x => x, x => Guid.NewGuid().ToString());

        KyprSession? session;
        Account? account;

        using (var client1 = new KyprClient(_server1))
        {
            await client1.CreateAccountAsync(username, password);
            session = client1.Session;
            account = client1.Account;

            var vault1 = await client1.CreateVaultAsync("vault1");

            foreach (var rl in recordLabels)
            {
                await client1.SaveRecordAsync(vault1, new()
                {
                    Summary = new()
                    {
                        Label = "Record " + rl,
                        Username = "user-" + recordValues[rl],
                    },
                    Content = new()
                    {
                        Password = "value-" + recordValues[rl],
                    }
                });
            }
        }

        using var server = new ServerImpl(_loggerFactory.CreateLogger<ServerImpl>(), _db2, session);
        using var client2 = new KyprClient(server);

        client2.Session = session;
        client2.Account = account;

        // This was automatically locked by
        // client1 when it was disposed
        Assert.True(client2.IsAccountLocked());
        client2.UnlockAccount(password);

        await client2.RefreshVaultsAsync();
        var vault1a = client2.Vaults.Single();
        client2.UnlockVault(vault1a);
        client2.UnlockRecordSummaries(vault1a);

        Assert.Equal("vault1", vault1a.Summary!.Label);

        var records = client2.ListRecords(vault1a).ToArray();
        Assert.Equal(50, records.Length);

        foreach (var rl in recordLabels)
        {
            var rlParts = rl.Split("_");
            var hits1 = client2.SearchRecords(vault1a, rlParts[0]);
            var hits2 = client2.SearchRecords(vault1a, rlParts[1]);

            Assert.Equal(5, hits1.Count());
            Assert.Single(hits2);
        }

        Assert.Equal(vault1a.Records, client2.SearchRecords(vault1a, "RECORD"));
    }
}
