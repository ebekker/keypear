// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared;
using Keypear.Shared.Utils;

namespace Keypear.Server.GrpcClient;

public  class ServiceClientBuilder
{
    internal static readonly ServiceClientBuilder Empty = new ServiceClientBuilder();

    public string? Address { get; init; }

    public KyprSession? Session { get; init; }

    public ServiceClient Build()
    {
        KpCommon.ThrowIfNull(Address);

        return new ServiceClient(this, Address, Session);
    }
}
