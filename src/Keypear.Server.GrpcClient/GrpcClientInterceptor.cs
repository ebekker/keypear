// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Grpc.Core;
using Grpc.Core.Interceptors;
using Keypear.Shared.Krypto;
using Keypear.Shared.Utils;
using static Keypear.Server.GrpcClient.GrpcUtils;

namespace Keypear.Server.GrpcClient;

public class GrpcClientInterceptor : Interceptor
{
    private readonly ServiceClient _client;

    public GrpcClientInterceptor(ServiceClient client)
    {
        _client = client;
    }

    private (SecretKeyEncryption ske, byte[] key) ResolveSessionKey(Metadata? headers)
    {
        var sessEnc = _client.GetEncryption();

        if (sessEnc == null)
        {
            throw new InvalidOperationException("no session found");
        }

        return (sessEnc.Value.ske, sessEnc.Value.key);
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        SecretKeyEncryption? ske = null;
        byte[]? key = null;

        if (request is ISessionSecuredMessage ssmRequ)
        {
            if (ske == null || key == null)
                (ske, key) = ResolveSessionKey(context.Options.Headers);

            try
            {
                ssmRequ.Encrypt(ske, key);
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal,
                    "failed to encrypt session-secured input message: " + ex.Message));
            }
        }

        var call = continuation(request, context);
        var response = HandleResponse<TResponse>(call.ResponseAsync, ske, key, context.Options.Headers);

        return new AsyncUnaryCall<TResponse>(
            response,
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> inner, SecretKeyEncryption? ske, byte[]? key, Metadata? headers)
    {
        var response = await inner;
        if (response is ISessionSecuredMessage ssmResp)
        {
            if (ske == null || key == null)
                (ske, key) = ResolveSessionKey(headers);

            try
            {
                ssmResp.Decrypt(ske, key);
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal,
                    "failed to decrypt session-secured reply message: " + ex.Message));
            }
        }

        return response;
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(
        TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        SecretKeyEncryption? ske = null;
        byte[]? key = null;

        if (request is ISessionSecuredMessage ssmRequ)
        {
            if (ske == null || key == null)
                (ske, key) = ResolveSessionKey(context.Options.Headers);

            try
            {
                ssmRequ.Encrypt(ske, key);
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal,
                    "failed to encrypt session-secured input message: " + ex.Message));
            }
        }

        var response = base.BlockingUnaryCall(request, context, continuation);
        if (response is ISessionSecuredMessage ssmResp)
        {
            if (ske == null || key == null)
                (ske, key) = ResolveSessionKey(context.Options.Headers);

            try
            {
                ssmResp.Decrypt(ske, key);
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal,
                    "failed to decrypt session-secured reply message: " + ex.Message));
            }
        }

        return response;
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented,
            "secure streaming calls are not supported"));
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented,
            "secure streaming calls are not supported"));
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented,
            "secure streaming calls are not supported"));
    }
}
