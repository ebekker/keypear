// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

//namespace Keypear.Server.GrpcServer.Tests;

//// Since these assemblies both have a global-namespaced Program
//// type, and we need to reference one of them below, we need to
//// define extern aliases to them with each reference in .csproj
//extern alias GrpcClient;
//extern alias GrpcServer;

using System.Security.Cryptography;
using System.Text.Json;
using Grpc.Net.Client;
using Keypear.Server.GrpcClient;
using Keypear.Shared.Models.InMemory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Keypear.Server.GrpcServer.Tests;

public class KyprClientGrpcTests : IDisposable, IClassFixture<GrpcServerFixture>
{
    private readonly ITestOutputHelper _testOut;
    private readonly GrpcServerFixture _fixture;

    private readonly ILoggerFactory _loggerFactory;
    ////private readonly IKyprServer _server1;
    ////private readonly IKyprServer _server2;

    public KyprClientGrpcTests(ITestOutputHelper testOutputHelper, GrpcServerFixture fixture)
    {
        _testOut = testOutputHelper;
        _fixture = fixture;

        _loggerFactory = new LoggerFactory();

        ////_server1 = new ServiceClientBuilder
        ////{
        ////    Address = "https://localhost:5001",
        ////}.Build();

        ////_server2 = new ServiceClientBuilder
        ////{
        ////    Address = "https://localhost:5001",
        ////}.Build();
    }

    public void Dispose()
    {
        ////_server2.Dispose();
        ////_server1.Dispose();
    }

    private async Task<IKyprServer> GetTestServiceClient(bool resetDb = true)
    {
        _fixture.Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:default"] = "Filename=kypr-grpcserver.db",
                ["GrpcServer:ConnectionStringName"] = "default",
            }).Build();

        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            HttpHandler = _fixture.Handler,
        });

        if (resetDb)
        {
            await _fixture.WithDbContext(async db =>
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();
            });
        }

        var server1 = new ServiceClient(ServiceClientBuilder.Empty, channel, null);

        return server1;
    }

    [Fact]
    public async Task Create_Account_Unique_Master_Keys()
    {
        using var server1 = await GetTestServiceClient();

        var username = "jdoe@example.com";
        var password = "foo bar non";

        using var client1 = new KyprClient(server1);
        using var client2 = new KyprClient(server1);

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
        using var server1 = await GetTestServiceClient();

        var username = "jdoe@example.com";
        var password = "foo bar non";

        using var client = new KyprClient(server1);

        await client.CreateAccountAsync(username, password);
        client.LockAccount();

        Assert.Throws<CryptographicException>(() =>
            client.UnlockAccount("not" + password));
    }

    [Fact]
    public async Task Create_and_Lock_and_Unlock_Account()
    {
        using var server1 = await GetTestServiceClient();

        var username = "jdoe@example.com";
        var password = "foo bar non";

        using var client = new KyprClient(server1);

        await client.CreateAccountAsync(username, password);
        client.LockAccount();

        client.UnlockAccount(password);
    }

    [Fact]
    public async Task Create_Vault_Unique_Vault_Keys()
    {
        using var server1 = await GetTestServiceClient();

        var username = "jdoe@example.com";
        var password = "foo bar non";
        var vaultLabel1 = "Vault #1";
        var vaultLabel2 = "Vault #2";

        using var client1 = new KyprClient(server1);
        using var client2 = new KyprClient(server1);

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
        using var server1 = await GetTestServiceClient();

        var username = "jdoe@example.com";
        var password = "foo bar non";

        var vaultLabels = Enumerable.Range(1, 10)
            .Select(_ => "Vault " + Guid.NewGuid())
            .ToList();

        using var client1 = new KyprClient(server1);
        await client1.CreateAccountAsync(username + "1", password);
        var vaults1 = vaultLabels
            .Select(x => client1.CreateVaultAsync(x).Result)
            .OrderBy(x => x.Id)
            .Select(x => JsonSerializer.Serialize(x))
            .ToList();

        using var client2 = new KyprClient(server1);
        await client2.CreateAccountAsync(username + "2", password);
        var vaults2 = vaultLabels
            .Select(x => client2.CreateVaultAsync(x).Result)
            .OrderBy(x => x.Id)
            .Select(x => JsonSerializer.Serialize(x))
            .ToList();

        var listedVaults1 = client1.ListVaults()
            .OrderBy(x => x.Id)
            .Select(x => JsonSerializer.Serialize(x))
            .ToList();
        var listedVaults2 = client2.ListVaults()
            .OrderBy(x => x.Id)
            .Select(x => JsonSerializer.Serialize(x))
            .ToList();

        Assert.Equal(vaultLabels.Count, listedVaults1.Count);
        Assert.Equal(vaultLabels.Count, listedVaults2.Count);

        Assert.Equal(vaults1, listedVaults1);
        Assert.Equal(vaults2, listedVaults2);

        using var client3 = new KyprClient(server1);
        await client3.AuthenticateAccountAsync(username + "1", password);
        await client3.RefreshVaultsAsync();
        client3.UnlockVaults();
        var listedVaults3 = client3.ListVaults()
            .OrderBy(x => x.Id)
            .Select(x => JsonSerializer.Serialize(x))
            .ToList();

        _testOut.WriteLine(JsonSerializer.Serialize(vaults1));
        _testOut.WriteLine(JsonSerializer.Serialize(listedVaults3));

        Assert.Equal(vaultLabels.Count, listedVaults3.Count);
        Assert.Equal(vaults1, listedVaults3);
    }

    [Fact]
    public async Task Create_Lock_and_Unlock_Vault()
    {
        using var server1 = await GetTestServiceClient();

        var username = "jdoe@example.com";
        var password = "foo bar non";

        using var client = new KyprClient(server1);

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
        using var server1 = await GetTestServiceClient();

        var username = "jdoe@example.com";
        var password = "foo bar non";

        var recordLabels = Enumerable.Range(1, 10)
            .Select(_ => Guid.NewGuid())
            .SelectMany(x => Enumerable.Range(1, 5).Select(_ =>
                x + "-_" + Guid.NewGuid()))
            .ToList();

        using var client = new KyprClient(server1);

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
        using var server1 = await GetTestServiceClient();

        var username = "jdoe@example.com";
        var password = "foo bar non";

        var recordLabels = Enumerable.Range(1, 10)
            .Select(_ => Guid.NewGuid())
            .SelectMany(x => Enumerable.Range(1, 5).Select(_ =>
                x + "-_" + Guid.NewGuid()))
            .ToList();
        var recordValues = recordLabels.ToDictionary(x => x, x => Guid.NewGuid().ToString());

        using var client = new KyprClient(server1);

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
        using var server1 = await GetTestServiceClient();
        using var server2 = await GetTestServiceClient();

        var username = "jdoe@example.com";
        var password = "foo bar non";

        var recordLabels = Enumerable.Range(1, 10)
            .Select(_ => Guid.NewGuid())
            .SelectMany(x => Enumerable.Range(1, 5).Select(_ =>
                x + "-_" + Guid.NewGuid()))
            .ToList();
        var recordValues = recordLabels.ToDictionary(x => x, x => Guid.NewGuid().ToString());

        using (var client1 = new KyprClient(server1))
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

        using var client1a = new KyprClient(server2);
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
        using var server1 = await GetTestServiceClient();
        using var server2 = await GetTestServiceClient();

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

        using (var client1 = new KyprClient(server1))
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

        using var client2 = new KyprClient(server2);

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
