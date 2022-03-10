// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.Utils;

namespace Keypear.Shared.Krypto;

/// <summary>
/// Implementation of <i>secret-key encryption</i>.
/// </summary>
public class SecretKeyEncryption
{
    /// <summary>
    /// The Libsodium SecretBox API in <i>Combined Mode</i> leverages
    /// the <c>XSalsa2020</c> stream cipher for encryption
    /// with <c>Poly1305</c> MAC for the authentication tag.
    /// </summary>
    public string Algor => "NaCl-XSalsa20-Poly1305";

    public int SecretKeySize => 32;

    public int NonceSize => 24;

    public byte[] GenerateKey()
    {
        return Sodium.SecretBox.GenerateKey();
    }

    public byte[] Encrypt(byte[] clearData, byte[] key)
    {
        var nonce = Sodium.SecretBox.GenerateNonce();
        var crypt = Sodium.SecretBox.Create(clearData, nonce, key);
        var total = new NonceAndCrypt
        {
            Nonce = nonce,
            Crypt = crypt,
        };

        return KpMsgPack.Ser(total);
    }

    public byte[] Decrypt(byte[] cryptData, byte[] key)
    {
        var total = KpMsgPack.Des<NonceAndCrypt>(cryptData);
        return SecretBox.Open(total.Crypt, total.Nonce, key);
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
