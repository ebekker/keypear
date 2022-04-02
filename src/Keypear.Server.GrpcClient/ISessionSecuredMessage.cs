// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.Krypto;

namespace Keypear.Server.GrpcClient;

public interface ISessionSecuredMessage
{
    void Encrypt(SecretKeyEncryption ske, byte[] key);
    void Decrypt(SecretKeyEncryption ske, byte[] key);
}
