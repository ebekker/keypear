// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Utils;

public static class KpMsgPack
{
    public static byte[] Ser<T>(T obj) => MessagePackSerializer.Serialize<T>(obj);

    public static T Des<T>(byte[] ser) => MessagePackSerializer.Deserialize<T>(ser);

    public static byte[] DynSer<T>(T obj) => MessagePackSerializer.Serialize<T>(obj,
        MessagePack.Resolvers.ContractlessStandardResolver.Options);

    public static T DynSer<T>(byte[] ser) => MessagePackSerializer.Deserialize<T>(ser,
        MessagePack.Resolvers.ContractlessStandardResolver.Options);

}
