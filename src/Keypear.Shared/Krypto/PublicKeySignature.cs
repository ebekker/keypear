// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Krypto;

/// <summary>
/// Implementation of <i>public-key signature</i>.
/// </summary>
public class PublicKeySignature
{
    /// <summary>
    /// The Libsodium PublicBoxAuth API leverages
    /// the <c>Ed25519</c> single-part signature.
    /// </summary>
    public string Algor => "NaCl-Ed25519";

    public int PrivateKeySize => 32;
    public int PublicKeySize => 32;

    public void GenerateKeyPair(out byte[] privateKey, out byte[] publicKey)
    {
        using var kp = Sodium.PublicKeyAuth.GenerateKeyPair();
        privateKey = kp.PrivateKey;
        publicKey = kp.PublicKey;
    }

    public byte[] Sign(byte[] data, byte[] privateKey)
    {
        return Sodium.PublicKeyAuth.Sign(data, privateKey);
    }

    public bool Verify(byte[] signedData, byte[] publicKey)
    {
        try
        {
            _ = Sodium.PublicKeyAuth.Verify(signedData, publicKey);
            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}
