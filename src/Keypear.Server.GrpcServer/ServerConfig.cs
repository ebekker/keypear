// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Server.GrpcServer;

public class ServerConfig
{
    public const string DefaultConfigName = "GrpcServer";

    public string DbDriver { get; set; } = "sqlite";

    public bool MigrateOnStart { get; set; }

    public string? ConnectionStringName { get; set; }
}
