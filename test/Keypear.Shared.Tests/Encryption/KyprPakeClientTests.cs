// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.Krypto;

namespace Keypear.Shared.Tests.Encryption;

public class KyprPakeClientTests
{
    [Fact]
    public void Register_Null_Username_Throws()
    {
        var server = new PakeServer();
        var client = new PakeClient(server);

        Assert.ThrowsAny<Exception>(
            () => client.Register(null!, null!));
    }

    [Fact]
    public void Register_Null_Password_Throws()
    {
        var server = new PakeServer();
        var client = new PakeClient(server);

        Assert.ThrowsAny<Exception>(
            () => client.Register("jdoe@example.com", null!));
    }

    [Fact]
    public void Register_No_Exceptions()
    {
        var server = new PakeServer();
        var client = new PakeClient(server);

        client.Register("jdoe@example.com", "foo bar non");
    }

    [Fact]
    public void Start_Session_With_Bad_Username()
    {
        var I = "jdoe@example.com";
        var P = "foo bar non";

        var server = new PakeServer();
        var client = new PakeClient(server);

        client.Register(I, P);
        Assert.ThrowsAny<Exception>(
            () => client.StartSession("notjdo@example.com", P));
    }

    [Fact]
    public void Start_Session_With_Bad_Password()
    {
        var I = "jdoe@example.com";
        var P = "foo bar non";

        var server = new PakeServer();
        var client = new PakeClient(server);

        client.Register(I, P);
        Assert.ThrowsAny<Exception>(
            () => client.StartSession(I, "bad password"));
    }


    [Fact]
    public void Start_Session()
    {
        var I = "jdoe@example.com";
        var P = "foo bar non";

        var server = new PakeServer();
        var client = new PakeClient(server);

        client.Register(I, P);
        client.StartSession(I, P);
    }
}
