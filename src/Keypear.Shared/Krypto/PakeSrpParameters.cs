// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.PAKE.ScottBradySRP;

namespace Keypear.Shared.Krypto;

internal class PakeSrpParameters
{
    internal string Algor => "SRP-Group4096bit-Argon2";

    internal readonly Func<byte[], byte[], byte[]> _hasher = (salt, data) => PasswordHash.ArgonHashBinary(data, salt);
    internal readonly SRPGroupParameters _groupParameters = SRPGroupParameters.Group4096bit;
}
