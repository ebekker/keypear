// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Grpc.Core;
using Grpc.Core.Interceptors;
using Keypear.Server.GrpcClient;
using Keypear.Server.GrpcServer.RpcModel;
using Keypear.Shared.Krypto;
using Keypear.Shared.Utils;
using static Keypear.Server.GrpcClient.GrpcUtils;

namespace Keypear.Server.GrpcServer;

internal class GrpcServerInterceptor : Interceptor
{
    private static (SecretKeyEncryption, byte[]) ResolveSessionKey(Metadata? headers)
    {
        if (headers != null
            && headers.GetValue(SessionIdHeaderName) is string id)
        {
            var sess = Services.KyprService.GetEncryption(headers);
            if (sess != null)
            {
                return (sess.Value.ske, sess.Value.key);
            }
        }

        throw new RpcException(new Status(StatusCode.Unauthenticated,
            "missing session headers"));
    }


    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        SecretKeyEncryption? ske = null;
        byte[]? key = null;

        if (request is ISessionSecuredMessage ssmRequ)
        {
            if (ske == null || key == null)
                (ske, key) = ResolveSessionKey(context.RequestHeaders);

            try
            {
                KpCommon.ThrowIfNull(ske);
                KpCommon.ThrowIfNull(key);

                ssmRequ.Decrypt(ske, key);
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal,
                    "failed to decrypt session-secured input message: " + ex.Message));
            }
        }

        var response = await base.UnaryServerHandler(request, context, continuation);
        if (response is ISessionSecuredMessage ssmResp)
        {
            if (ske == null || key == null)
                (ske, key) = ResolveSessionKey(context.RequestHeaders);

            try
            {
                ssmResp.Encrypt(ske, key);
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal,
                    "failed to encrypt session-secured reply message: " + ex.Message));
            }
        }

        return response;
    }

    public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented,
            "secure streaming calls are not supported"));
    }

    public override Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request, IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented,
            "secure streaming calls are not supported"));
    }

    public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented,
            "secure streaming calls are not supported"));
    }
}
