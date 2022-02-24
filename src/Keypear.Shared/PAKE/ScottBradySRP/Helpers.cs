// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.Globalization;

namespace Keypear.Shared.PAKE.ScottBradySRP;
internal static class Helpers
{
    public static byte[] ToBytes(this string hex)
    {
        var hexAsBytes = new byte[hex.Length / 2];

        for (var i = 0; i < hex.Length; i += 2)
        {
            hexAsBytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return hexAsBytes;
    }

    // both unsigned and big endian
    public static BigInteger ToSrpBigInt(this byte[] bytes)
    {
        return new BigInteger(bytes, true, true);
    }

    // Add padding character back to hex before parsing
    public static BigInteger ToSrpBigInt(this string hex)
    {
        return BigInteger.Parse("0" + hex, NumberStyles.HexNumber);
    }

    public static BigInteger Computek(int g, BigInteger N, Func<byte[], byte[]> H)
    {
        // k = H(N, g)
        var NBytes = N.ToByteArray(true, true);
        var gBytes = PadBytes(BitConverter.GetBytes(g).Reverse().ToArray(), NBytes.Length);

        var k = H(NBytes.Concat(gBytes).ToArray());

        return new BigInteger(k, isBigEndian: true);
    }

    public static BigInteger Computeu(Func<byte[], byte[]> H, BigInteger A, BigInteger B)
    {
        var aArr = A.ToByteArray(true, true);
        var bArr = B.ToByteArray(true, true);
        var h = H(aArr.Concat(bArr).ToArray());
        return h.ToSrpBigInt();
    }

    public static BigInteger ComputeClientProof(
        BigInteger N,
        Func<byte[], byte[]> H,
        BigInteger A,
        BigInteger B,
        BigInteger S)
    {
        var padLength = N.ToByteArray(true, true).Length;

        // M1 = H( A | B | S )
        return H((PadBytes(A.ToByteArray(true, true), padLength))
                .Concat(PadBytes(B.ToByteArray(true, true), padLength))
                .Concat(PadBytes(S.ToByteArray(true, true), padLength))
                .ToArray())
            .ToSrpBigInt();
    }

    public static BigInteger ComputeServerProof(BigInteger N, Func<byte[], byte[]> H, BigInteger A, BigInteger M1, BigInteger S)
    {
        var padLength = N.ToByteArray(true, true).Length;

        // M2 = H( A | M1 | S )
        return H((PadBytes(A.ToByteArray(true, true), padLength))
                .Concat(PadBytes(M1.ToByteArray(true, true), padLength))
                .Concat(PadBytes(S.ToByteArray(true, true), padLength))
                .ToArray())
            .ToSrpBigInt();
    }

    public static byte[] PadBytes(byte[] bytes, int length)
    {
        var paddedBytes = new byte[length];
        Array.Copy(bytes, 0, paddedBytes, length - bytes.Length, bytes.Length);

        return paddedBytes;
    }
}
