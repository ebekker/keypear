// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.PAKE.ScottBradySRP;

namespace Keypear.Shared;

internal class SrpParameters
{
    internal readonly Func<byte[], byte[], byte[]> _hasher = (salt, data) => PasswordHash.ArgonHashBinary(data, salt);
    internal readonly SRPGroupParameters _groupParameters = SRPGroupParameters.Group4096bit;
}
