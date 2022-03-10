// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.Utils;

namespace Keypear.Shared.Krypto;

/// <summary>
/// Implementation of <i>public-key encryption</i>.
/// </summary>
public class PublicKeyEncryption
{
    /// <summary>
    /// The Libsodium PublicKeyBox API in <i>Combined Mode</i> leverages
    /// the <c>X25519</c> curve for key exchange
    /// with <c>XSalsa2020</c> stream cipher for encryption
    /// and <c>Poly1305</c> MAC for the authentication tag.
    /// </summary>
    public string Algor => "NaCl-X25519-XSalsa20-Poly1305";

    public int PrivateKeySize => 32;
    public int PublicKeySize => 32;

    public int NonceSize => 24;

    public void GenerateKeyPair(out byte[] privateKey, out byte[] publicKey)
    {
        using var kp = Sodium.PublicKeyBox.GenerateKeyPair();
        privateKey = kp.PrivateKey;
        publicKey = kp.PublicKey;
    }

    public byte[] VerifiedEncrypt(byte[] clearData, byte[] recvPublicKey, byte[] sendPrivateKey)
    {
        var nonce = Sodium.PublicKeyBox.GenerateNonce();
        var cryptData = Sodium.PublicKeyBox.Create(clearData, nonce, sendPrivateKey, recvPublicKey);
        var total = new NonceAndCrypt
        {
            Nonce = nonce,
            Crypt = cryptData,
        };

        return KpMsgPack.Ser(total);
    }

    public byte[] VerifiedDecrypt(byte[] cryptData, byte[] sendPublicKey, byte[] recvPrivateKey)
    {
        var total = KpMsgPack.Des<NonceAndCrypt>(cryptData);
        return Sodium.PublicKeyBox.Open(total.Crypt, total.Nonce, recvPrivateKey, sendPublicKey);
    }

    public byte[] UnverifiedEncrypt(byte[] clearData, byte[] recvPublicKey)
    {
        var crypt = Sodium.SealedPublicKeyBox.Create(clearData, recvPublicKey);
        return crypt;
    }

    public byte[] UnverifiedDecrypt(byte[] cryptData, byte[] recvPublicKey, byte[] recvPrivateKey)
    {
        var clear = Sodium.SealedPublicKeyBox.Open(cryptData, recvPrivateKey, recvPublicKey);
        return clear;
    }

    [MPObject]
    public class NonceAndCrypt
    {
        [MPKey(0)]
        public byte[]? Nonce { get; set; }

        [MPKey(1)]
        public byte[]? Crypt { get; set; }
    }
}
