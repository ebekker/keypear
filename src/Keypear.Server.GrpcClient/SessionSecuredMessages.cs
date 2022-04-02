// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.Buffers;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Keypear.Server.GrpcClient;
using Keypear.Shared.Krypto;
using Keypear.Shared.Utils;
using MessagePack;
using MessagePack.Formatters;
using static Keypear.Server.GrpcClient.GrpcUtils;

namespace Keypear.Server.GrpcServer.RpcModel;



//public sealed partial class GetAccountInput : ISessionSecuredMessage
//{
//    public void Encrypt(SecretKeyEncryption ske, byte[] key)
//    {
//        KpCommon.ThrowIfNull(this.InnerMessage);
//        var bytes = GrpcMsgPack.DynSer(this.InnerMessage);
//        bytes = ske.Encrypt(bytes, key);
//        this.InnerMessageEnc = FromBytes(bytes);
//        this.InnerMessage = null;
//    }

//    public void Decrypt(SecretKeyEncryption ske, byte[] key)
//    {
//        KpCommon.ThrowIfNull(this.InnerMessageEnc);
//        var bytes = ToBytes(this.InnerMessageEnc);
//        bytes = ske.Decrypt(bytes!, key);
//        this.InnerMessage = GrpcMsgPack.DynDes<Types.Inner>(bytes!);
//        this.InnerMessageEnc = ByteString.Empty;
//    }
//}

//public sealed partial class GetAccountResult : ISessionSecuredMessage
//{
//    public void Encrypt(SecretKeyEncryption ske, byte[] key)
//    {
//        KpCommon.ThrowIfNull(this.InnerMessage);
//        var bytes = GrpcMsgPack.DynSer(this.InnerMessage);
//        bytes = ske.Encrypt(bytes, key);
//        this.InnerMessageEnc = FromBytes(bytes);
//        this.InnerMessage = null;
//    }

//    public void Decrypt(SecretKeyEncryption ske, byte[] key)
//    {
//        KpCommon.ThrowIfNull(this.InnerMessageEnc);
//        var bytes = ToBytes(this.InnerMessageEnc);
//        bytes = ske.Decrypt(bytes!, key);
//        this.InnerMessage = GrpcMsgPack.DynDes<Types.Inner>(bytes!);
//        this.InnerMessageEnc = ByteString.Empty;
//    }
//}


public sealed partial class CreateVaultInput : ISessionSecuredMessage
{
    public void Encrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessage);
        var bytes = GrpcMsgPack.DynSer(this.InnerMessage);
        bytes = ske.Encrypt(bytes, key);
        this.InnerMessageEnc = FromBytes(bytes);
        this.InnerMessage = null;
    }

    public void Decrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessageEnc);
        var bytes = ToBytes(this.InnerMessageEnc);
        bytes = ske.Decrypt(bytes!, key);
        this.InnerMessage = GrpcMsgPack.DynDes<Types.Inner>(bytes!);
        this.InnerMessageEnc = ByteString.Empty;
    }
}

public sealed partial class CreateVaultResult : ISessionSecuredMessage
{
    public void Encrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessage);
        var bytes = GrpcMsgPack.DynSer(this.InnerMessage);
        bytes = ske.Encrypt(bytes, key);
        this.InnerMessageEnc = FromBytes(bytes);
        this.InnerMessage = null;
    }

    public void Decrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessageEnc);
        var bytes = ToBytes(this.InnerMessageEnc);
        bytes = ske.Decrypt(bytes!, key);
        this.InnerMessage = GrpcMsgPack.DynDes<Types.Inner>(bytes!);
        this.InnerMessageEnc = ByteString.Empty;
    }
}


public sealed partial class SaveVaultInput : ISessionSecuredMessage
{
    public void Encrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessage);
        var bytes = GrpcMsgPack.DynSer(this.InnerMessage);
        bytes = ske.Encrypt(bytes, key);
        this.InnerMessageEnc = FromBytes(bytes);
        this.InnerMessage = null;
    }

    public void Decrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessageEnc);
        var bytes = ToBytes(this.InnerMessageEnc);
        bytes = ske.Decrypt(bytes!, key);
        this.InnerMessage = GrpcMsgPack.DynDes<Types.Inner>(bytes!);
        this.InnerMessageEnc = ByteString.Empty;
    }
}

public sealed partial class SaveVaultResult : ISessionSecuredMessage
{
    public void Encrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessage);
        var bytes = GrpcMsgPack.DynSer(this.InnerMessage);
        bytes = ske.Encrypt(bytes, key);
        this.InnerMessageEnc = FromBytes(bytes);
        this.InnerMessage = null;
    }

    public void Decrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessageEnc);
        var bytes = ToBytes(this.InnerMessageEnc);
        bytes = ske.Decrypt(bytes!, key);
        this.InnerMessage = GrpcMsgPack.DynDes<Types.Inner>(bytes!);
        this.InnerMessageEnc = ByteString.Empty;
    }
}

public sealed partial class ListVaultsInput : ISessionSecuredMessage
{
    public void Encrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessage);
        var bytes = GrpcMsgPack.DynSer(this.InnerMessage);
        bytes = ske.Encrypt(bytes, key);
        this.InnerMessageEnc = FromBytes(bytes);
        this.InnerMessage = null;
    }

    public void Decrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessageEnc);
        var bytes = ToBytes(this.InnerMessageEnc);
        bytes = ske.Decrypt(bytes!, key);
        this.InnerMessage = GrpcMsgPack.DynDes<Types.Inner>(bytes!);
        this.InnerMessageEnc = ByteString.Empty;
    }
}


public sealed partial class ListVaultsResult : ISessionSecuredMessage
{
    public void Encrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessage);
        var bytes = GrpcMsgPack.DynSer(this.InnerMessage);
        bytes = ske.Encrypt(bytes, key);
        this.InnerMessageEnc = FromBytes(bytes);
        this.InnerMessage = null;
    }

    public void Decrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessageEnc);
        var bytes = ToBytes(this.InnerMessageEnc);
        bytes = ske.Decrypt(bytes!, key);
        this.InnerMessage = GrpcMsgPack.DynDes<Types.Inner>(bytes!);
        this.InnerMessageEnc = ByteString.Empty;
    }
}


public sealed partial class GetVaultInput : ISessionSecuredMessage
{
    public void Encrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessage);
        var bytes = GrpcMsgPack.DynSer(this.InnerMessage);
        bytes = ske.Encrypt(bytes, key);
        this.InnerMessageEnc = FromBytes(bytes);
        this.InnerMessage = null;
    }

    public void Decrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessageEnc);
        var bytes = ToBytes(this.InnerMessageEnc);
        bytes = ske.Decrypt(bytes!, key);
        this.InnerMessage = GrpcMsgPack.DynDes<Types.Inner>(bytes!);
        this.InnerMessageEnc = ByteString.Empty;
    }
}

public sealed partial class GetVaultResult : ISessionSecuredMessage
{
    public void Encrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessage);
        var bytes = GrpcMsgPack.DynSer(this.InnerMessage);
        bytes = ske.Encrypt(bytes, key);
        this.InnerMessageEnc = FromBytes(bytes);
        this.InnerMessage = null;
    }

    public void Decrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessageEnc);
        var bytes = ToBytes(this.InnerMessageEnc);
        bytes = ske.Decrypt(bytes!, key);
        this.InnerMessage = GrpcMsgPack.DynDes<Types.Inner>(bytes!);
        this.InnerMessageEnc = ByteString.Empty;
    }
}


public sealed partial class SaveRecordInput : ISessionSecuredMessage
{
    public void Encrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessage);
        var bytes = GrpcMsgPack.DynSer(this.InnerMessage);
        bytes = ske.Encrypt(bytes, key);
        this.InnerMessageEnc = FromBytes(bytes);
        this.InnerMessage = null;
    }

    public void Decrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessageEnc);
        var bytes = ToBytes(this.InnerMessageEnc);
        bytes = ske.Decrypt(bytes!, key);
        this.InnerMessage = GrpcMsgPack.DynDes<Types.Inner>(bytes!);
        this.InnerMessageEnc = ByteString.Empty;
    }
}

public sealed partial class SaveRecordResult : ISessionSecuredMessage
{
    public void Encrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessage);
        var bytes = GrpcMsgPack.DynSer(this.InnerMessage);
        bytes = ske.Encrypt(bytes, key);
        this.InnerMessageEnc = FromBytes(bytes);
        this.InnerMessage = null;
    }

    public void Decrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessageEnc);
        var bytes = ToBytes(this.InnerMessageEnc);
        bytes = ske.Decrypt(bytes!, key);
        this.InnerMessage = GrpcMsgPack.DynDes<Types.Inner>(bytes!);
        this.InnerMessageEnc = ByteString.Empty;
    }
}


public sealed partial class GetRecordsInput : ISessionSecuredMessage
{
    public void Encrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessage);
        var bytes = GrpcMsgPack.DynSer(this.InnerMessage);
        bytes = ske.Encrypt(bytes, key);
        this.InnerMessageEnc = FromBytes(bytes);
        this.InnerMessage = null;
    }

    public void Decrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessageEnc);
        var bytes = ToBytes(this.InnerMessageEnc);
        bytes = ske.Decrypt(bytes!, key);
        this.InnerMessage = GrpcMsgPack.DynDes<Types.Inner>(bytes!);
        this.InnerMessageEnc = ByteString.Empty;
    }
}

public sealed partial class GetRecordsResult : ISessionSecuredMessage
{
    public void Encrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessage);
        var bytes = GrpcMsgPack.DynSer(this.InnerMessage);
        bytes = ske.Encrypt(bytes, key);
        this.InnerMessageEnc = FromBytes(bytes);
        this.InnerMessage = null;
    }

    public void Decrypt(SecretKeyEncryption ske, byte[] key)
    {
        KpCommon.ThrowIfNull(this.InnerMessageEnc);
        var bytes = ToBytes(this.InnerMessageEnc);
        bytes = ske.Decrypt(bytes!, key);
        this.InnerMessage = GrpcMsgPack.DynDes<Types.Inner>(bytes!);
        this.InnerMessageEnc = ByteString.Empty;
    }
}


public static class GrpcMsgPack
{
    public static byte[] DynSer<T>(T obj) => MessagePackSerializer.Serialize<T>(obj,
        GrpcMsgPackResolver.Options);

    public static T DynDes<T>(byte[] ser) => MessagePackSerializer.Deserialize<T>(ser,
        GrpcMsgPackResolver.Options);
}

public class GrpcMsgPackResolver : IFormatterResolver
{
    public static readonly GrpcMsgPackResolver Instance = new();

    public static readonly IFormatterResolver Composite =
        MessagePack.Resolvers.CompositeResolver.Create(Instance,
            MessagePack.Resolvers.ContractlessStandardResolverAllowPrivate.Instance);

    //public static readonly MessagePackSerializerOptions Options =
    //    MessagePack.Resolvers.ContractlessStandardResolver.Options.WithResolver(Instance);
    public static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard.WithResolver(Composite);

    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        System.IO.File.AppendAllText("foo.log", $"GET FORMATTER: [{typeof(T).FullName}]\r\n");

        if (typeof(T) == typeof(ByteString))
        {
            return (IMessagePackFormatter<T>)GrpcMsgPackByteStringFormatter.Instance;
        }

        if (typeof(T) == typeof(RepeatedField<string>))
        {
            return (IMessagePackFormatter<T>)GrpcMsgPackRepeatedFieldFormatter.Instance;
        }

        return null;
    }
}

public class GrpcMsgPackByteStringFormatter : IMessagePackFormatter<ByteString>
{
    public static readonly GrpcMsgPackByteStringFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, ByteString value, MessagePackSerializerOptions options)
    {
        writer.Write(value.Span);
    }

    public ByteString Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        options.Security.DepthStep(ref reader);

        var bytes = reader.ReadBytes();
        if (bytes == null)
        {
            return ByteString.Empty;
        }

        reader.Depth--;

        return ByteString.CopyFrom(bytes.Value.ToArray());
    }
}

public class GrpcMsgPackRepeatedFieldFormatter : IMessagePackFormatter<RepeatedField<string>>
{
    public static readonly GrpcMsgPackRepeatedFieldFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, RepeatedField<string> value, MessagePackSerializerOptions options)
    {
        var itemFormatter = options.Resolver.GetFormatter<string>();
        writer.WriteArrayHeader(value.Count);
        foreach (var v in value)
        {
            itemFormatter.Serialize(ref writer, v, options);
        }
    }

    public RepeatedField<string> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var itemFormatter = options.Resolver.GetFormatter<string>();

        options.Security.DepthStep(ref reader);

        var count = reader.ReadArrayHeader();
        var rf = new RepeatedField<string>();
        for (var i = 0; i < count; i++)
        {
            var item = itemFormatter.Deserialize(ref reader, options);
            rf.Add(item);
        }

        reader.Depth--;

        return rf;
    }
}
