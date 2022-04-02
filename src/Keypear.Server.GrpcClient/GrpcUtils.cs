// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Google.Protobuf;
using Keypear.Server.GrpcServer.RpcModel;

namespace Keypear.Server.GrpcClient;

public static class GrpcUtils
{
    public const string SessionIdHeaderName = nameof(KyprSession.SessionId);
    //// Needs to have the `-bin` suffix for binary values
    //public const string SessionKeyHeaderName = nameof(KyprSession.SessionKey) + "-bin";

    public static byte[]? ToBytes(ByteString? bs) => bs == null ? null : bs.ToByteArray();

    public static ByteString? FromBytes(byte[]? b) => b == null ? null : ByteString.CopyFrom(b);
}
