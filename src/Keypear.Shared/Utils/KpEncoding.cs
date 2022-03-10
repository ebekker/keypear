// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Utils;

public class KpEncoding
{
    public static string Normalize(string s) => s.Normalize(NormalizationForm.FormKD);

    public static byte[] Encode(string s) => Encoding.UTF8.GetBytes(s);

    public static string Decode(byte[] b) => Encoding.UTF8.GetString(b);

    public static byte[] NormalizeEncode(string s) => Encode(Normalize(s));
}
