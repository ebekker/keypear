// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.PAKE.ScottBradySRP;

public class SrpServer
{
#pragma warning disable IDE1006 // Naming Styles

    private readonly Func<byte[], byte[]> _H;
    private readonly int _g;
    private readonly BigInteger _N;

    private BigInteger _B;
    private BigInteger _b;

    public SrpServer(Func<byte[], byte[]> H, int g, BigInteger N)
    {
        this._H = H;
        this._g = g;
        this._N = N;
    }

    public SrpServer(Func<byte[], byte[]> H, int g, BigInteger N, BigInteger B, byte[] state)
    {
        this._H = H;
        this._g = g;
        this._N = N;

        this._B = B;
        this._b = new BigInteger(state);
    }

    public BigInteger GenerateBValues(BigInteger v, out byte[] state)
    {
        // TODO: this sometimes produces neg value, we need to
        // investigate, but until then this is a cheap, kludgey,
        // brute-force way to avoid that
        var B = GenerateBValuesImpl(v, out state);
        while (B < BigInteger.Zero)
        {
            B = GenerateBValuesImpl(v, out state);
        }
        return B;
    }

    private BigInteger GenerateBValuesImpl(BigInteger v, out byte[] state)
    {
        _b = BigInteger.Abs(new BigInteger(Sodium.SodiumCore.GetRandomBytes(32)));

        var k = Helpers.Computek(_g, _N, _H);

        // kv % N
        var left = (k * v) % _N;

        // g^b % N
        var right = BigInteger.ModPow(_g, _b, _N);

        // B = kv + g^b
        _B = (left + right) % _N;

        state = _b.ToByteArray();

        return _B;
    }

    internal BigInteger GenerateTestVectorBValues(BigInteger v, BigInteger b)
    {
        // b = random()
        _b = b;

        var k = Helpers.Computek(_g, _N, _H);

        // kv % N
        var left = (k * v) % _N;

        // g^b % N
        var right = BigInteger.ModPow(_g, _b, _N);

        // B = kv + g^b
        _B = (left + right) % _N;

        return _B;
    }

    public BigInteger ComputeSessionKey(BigInteger v, BigInteger A)
    {
        var u = Helpers.Computeu(_H, A, _B);

        // (Av^u)
        var left = A * BigInteger.ModPow(v, u, _N) % _N;

        // S = (Av^u) ^ b
        return BigInteger.ModPow(left, _b, _N);
    }

    public bool ValidateClientProof(BigInteger M1, BigInteger A, BigInteger S)
    {
        return M1 == Helpers.ComputeClientProof(_N, _H, A, _B, S);
    }

    public BigInteger GenerateServerProof(BigInteger A, BigInteger M1, BigInteger S)
    {
        return Helpers.ComputeServerProof(_N, _H, A, M1, S);
    }

#pragma warning restore IDE1006 // Naming Styles
}
