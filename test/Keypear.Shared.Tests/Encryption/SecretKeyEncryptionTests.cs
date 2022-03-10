// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using P = Keypear.Shared.Models.Persisted;
using M = Keypear.Shared.Models.InMemory;
using I = Keypear.Shared.Models.Inner;
using Keypear.Shared.Krypto;
using Keypear.Shared.Utils;

namespace Keypear.Shared.Tests.Encryption;

public class SecretKeyEncryptionTests
{
    [Fact]
    public void Encrypt_Decrypt_Stuff()
    {
        var saltSeed = "salty";
        var password = "foo bar non";
        var saltBytes = KpEncoding.NormalizeEncode(saltSeed);
        var passBytes = KpEncoding.NormalizeEncode(password);

        // Must be 16 bytes
        var masterSalt = Sodium.GenericHash.Hash(saltBytes, null, 16);
        // Must be 32 bytes
        var masterKey = Sodium.PasswordHash.ArgonHashBinary(passBytes, masterSalt, outputLength: 32);

        var pRec = new P.Record
        {
            Id = Guid.NewGuid(),
            CreatedDateTime = DateTime.Now,
        };

        var recS = new I.RecordSummary();
        var recC = new I.RecordContent();

        recS.Type = "kypr:password";
        recS.Label = "First Secret";
        recS.Username = "jdoe@example.com";
        recS.Address = "https://example.com/";
        recS.Tags = "tag-1 tag-2 tag-3";

        recC.Password = "my secret password";
        recC.Memo = "A little note to myself";
        recC.Fields = new()
        {
            new() { Type = "text", Name = "F1", Value = "V1", },
            new() { Type = "text", Name = "F2", Value = "V2", },
            new() { Type = "text", Name = "F3", Value = "V3", },
        };

        var symm = new Krypto.SecretKeyEncryption();

        var summarySer = KpMsgPack.Ser(recS);
        var contentSer = KpMsgPack.Ser(recC);

        pRec.SummaryEnc = symm.Encrypt(summarySer, masterKey);
        pRec.ContentEnc = symm.Encrypt(contentSer, masterKey);

        var recSSer = symm.Decrypt(pRec.SummaryEnc, masterKey);
        var recCSer = symm.Decrypt(pRec.ContentEnc, masterKey);

        // Make sure Encryption/Decryption works
        Assert.Equal(summarySer, recSSer);
        Assert.Equal(contentSer, recCSer);

        var summaryEnc2 = symm.Encrypt(summarySer, masterKey);
        var contentEnc2 = symm.Encrypt(contentSer, masterKey);

        // Make sure 2 encryptions produce different outputs
        Assert.NotEqual(pRec.SummaryEnc, summaryEnc2);
        Assert.NotEqual(pRec.ContentEnc, contentEnc2);
    }
}
