// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.Numerics;
using Keypear.Shared.PAKE.ScottBradySRP;

namespace Keypear.Shared.Tests.SRP;

public class ScottBradySRPTests
{
    [Fact]
    public void Test_TestVectors()
    {
        var client = new SrpClient(TestVectors.H, TestVectors.g, TestVectors.N);
        var server = new SrpServer(TestVectors.H, TestVectors.g, TestVectors.N);

        // generate password verifier to store 
        BigInteger v = client.GenerateVerifier(TestVectors.I, TestVectors.P, TestVectors.s);
        Assert.Equal(TestVectors.expected_v, v);

        var A = client.GenerateTestVectorAValues(TestVectors.a);
        Assert.Equal(TestVectors.expected_A, A);

        var B = server.GenerateTestVectorBValues(v, TestVectors.b);
        Assert.Equal(TestVectors.expected_B, B);

        var clientS = client.ComputeSessionKey(TestVectors.I, TestVectors.P, TestVectors.s, B);
        var serverS = server.ComputeSessionKey(v, A);
        Assert.Equal(clientS, serverS);
        Assert.Equal(TestVectors.expected_S, clientS);

        var M1 = client.GenerateClientProof(B, clientS);
        Assert.True(server.ValidateClientProof(M1, A, serverS));

        var M2 = server.GenerateServerProof(A, M1, serverS);
        Assert.True(client.ValidateServerProof(M2, M1, clientS));
    }

    [Fact]
    public void Test_WithSha256Hasher()
    {
        var hasher = (byte[] data) => Sodium.CryptoHash.Sha256(data);
        var g = SRPGroupParameters.Group1024bit.g;
        var N = SRPGroupParameters.Group1024bit.N;

        var I = "jdoe@example.com";
        var P = "foo bar non";
        var s = Sodium.SodiumCore.GetRandomBytes(32);

        var client = new SrpClient(hasher, g, N);
        var server = new SrpServer(hasher, g, N);

        // generate password verifier to store 
        BigInteger v = client.GenerateVerifier(I, P, s);

        var A = client.GenerateAValues(out var clientSrpState);

        var B = server.GenerateBValues(v, out var serverSrpState);

        var clientS = client.ComputeSessionKey(I, P, s, B);
        var serverS = server.ComputeSessionKey(v, A);
        Assert.Equal(clientS, serverS);

        // Restore state in new client and server
        client = new SrpClient(hasher, g, N, A, clientSrpState);
        server = new SrpServer(hasher, g, N, B, serverSrpState);
        var clientS2 = client.ComputeSessionKey(I, P, s, B);
        var serverS2 = server.ComputeSessionKey(v, A);
        Assert.Equal(clientS2, serverS);
        Assert.Equal(clientS, serverS2);

        var M1 = client.GenerateClientProof(B, clientS);
        Assert.True(server.ValidateClientProof(M1, A, serverS));

        var M2 = server.GenerateServerProof(A, M1, serverS);
        Assert.True(client.ValidateServerProof(M2, M1, clientS));
    }

    [Theory]
    [MemberData(nameof(_allSrpGroups))]
    internal void Test_WithArgon2Hasher(string groupName, SRPGroupParameters gp)
    {
        Console.WriteLine("Testing with SRP Group [{0}]", groupName);

        var salt = Sodium.PasswordHash.ArgonGenerateSalt();
        var hasher = (byte[] data) => Sodium.PasswordHash.ArgonHashBinary(data, salt);

        var g = gp.g; // SRPGroupParameters.Group1024bit.g; // 
        var N = gp.N; // SRPGroupParameters.Group1024bit.N; // 

        var I = "jdoe@example.com";
        var P = "foo bar non";
        var s = Sodium.SodiumCore.GetRandomBytes(32);

        var client = new SrpClient(hasher, g, N);
        var server = new SrpServer(hasher, g, N);

        // generate password verifier to store 
        BigInteger v = client.GenerateVerifier(I, P, s);

        var A = client.GenerateAValues(out var clientSrpState);
        Assert.True(A >= BigInteger.Zero);
        //var A = client.GenerateTestVectorAValues(
        //    BigInteger.Negate(BigInteger.Abs(new BigInteger(Sodium.SodiumCore.GetRandomBytes(32)))));

        var B = server.GenerateBValues(v, out var serverSrpState);
        Assert.True(B >= BigInteger.Zero);
        //var B = server.GenerateTestVectorBValues(v,
        //    BigInteger.Negate(BigInteger.Abs(new BigInteger(Sodium.SodiumCore.GetRandomBytes(32)))));

        var clientS = client.ComputeSessionKey(I, P, s, B);
        var serverS = server.ComputeSessionKey(v, A);
        Assert.Equal(clientS, serverS);

        // Restore state in new client and server
        client = new SrpClient(hasher, g, N, A, clientSrpState);
        server = new SrpServer(hasher, g, N, B, serverSrpState);
        var clientS2 = client.ComputeSessionKey(I, P, s, B);
        var serverS2 = server.ComputeSessionKey(v, A);
        Assert.Equal(clientS2, serverS);
        Assert.Equal(clientS, serverS2);

        var M1 = client.GenerateClientProof(B, clientS);
        Assert.True(server.ValidateClientProof(M1, A, serverS));

        var M2 = server.GenerateServerProof(A, M1, serverS);
        Assert.True(client.ValidateServerProof(M2, M1, clientS));
    }

    public static readonly IEnumerable<object[]> _allSrpGroups = new[]
    {
        new object[] { nameof(SRPGroupParameters.Group1024bit), SRPGroupParameters.Group1024bit, },
        new object[] { nameof(SRPGroupParameters.Group1536bit), SRPGroupParameters.Group1536bit, },
        new object[] { nameof(SRPGroupParameters.Group2048bit), SRPGroupParameters.Group2048bit, },
        new object[] { nameof(SRPGroupParameters.Group3072bit), SRPGroupParameters.Group3072bit, },
        new object[] { nameof(SRPGroupParameters.Group4096bit), SRPGroupParameters.Group4096bit, },
        new object[] { nameof(SRPGroupParameters.Group6144bit), SRPGroupParameters.Group6144bit, },
        new object[] { nameof(SRPGroupParameters.Group8192bit), SRPGroupParameters.Group8192bit, },
    };
}
