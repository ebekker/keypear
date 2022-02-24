// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.PAKE.ScottBradySRP;
public class SrpClient
{
#pragma warning disable IDE1006 // Naming Styles

    private readonly Func<byte[], byte[]> _H;
    private readonly int _g;
    private readonly BigInteger _N;

    private BigInteger _A;
    private BigInteger _a;

    public SrpClient(Func<byte[], byte[]> hasher, int g, BigInteger N)
    {
        this._H = hasher;
        this._g = g;
        this._N = N;
    }

    public BigInteger GenerateVerifier(string I, string P, byte[] s)
    {
        ArgumentNullException.ThrowIfNull(I);
        ArgumentNullException.ThrowIfNull(P);
        ArgumentNullException.ThrowIfNull(s);

        // x = H(s | H(I | ":" | P))
        var x = GeneratePrivateKey(I, P, s);

        // v = g^x
        var v = BigInteger.ModPow(_g, x, _N);

        return v;
    }

    internal BigInteger GenerateAValues()
    {
        _a = BigInteger.Abs(new BigInteger(SodiumCore.GetRandomBytes(32)));

        // A = g^a
        _A = BigInteger.ModPow(_g, _a, _N);

        return _A;
    }

    internal BigInteger GenerateTestVectorAValues(BigInteger a)
    {
        // a = random()
        _a = a;

        // A = g^a
        _A = BigInteger.ModPow(_g, _a, _N);

        return _A;
    }

    public BigInteger ComputeSessionKey(string I, string P, byte[] s, BigInteger B)
    {
        ArgumentNullException.ThrowIfNull(I);
        ArgumentNullException.ThrowIfNull(P);
        ArgumentNullException.ThrowIfNull(s);

        var u = Helpers.Computeu(_H, _A, B);
        var x = GeneratePrivateKey(I, P, s);
        var k = Helpers.Computek(_g, _N, _H);

        // (a + ux)
        var exp = _a + u * x;

        // (B - kg ^ x)
        var val = mod(B - (BigInteger.ModPow(_g, x, _N) * k % _N), _N);

        // S = (B - kg ^ x) ^ (a + ux)
        return BigInteger.ModPow(val, exp, _N);
    }

    public BigInteger GenerateClientProof(BigInteger B, BigInteger S)
    {
        return Helpers.ComputeClientProof(_N, _H, _A, B, S);
    }

    public bool ValidateServerProof(BigInteger M2, BigInteger M1, BigInteger S)
    {
        return M2 == Helpers.ComputeServerProof(_N, _H, _A, M1, S);
    }

    private BigInteger GeneratePrivateKey(string I, string P, byte[] s)
    {
        // x = H(s | H(I | ":" | P))
        var x = _H(s.Concat(_H(Encoding.UTF8.GetBytes(I + ":" + P))).ToArray());

        return x.ToSrpBigInt();
    }

    private static BigInteger mod(BigInteger x, BigInteger m)
    {
        return (x % m + m) % m;
    }

#pragma warning restore IDE1006 // Naming Styles
}
