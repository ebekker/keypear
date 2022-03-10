// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Shared.Krypto;

public class PbKeyDerivation
{
    public const PasswordHash.ArgonAlgorithm ArgonAlgorithm = PasswordHash.ArgonAlgorithm.Argon_2ID13;
    public const PasswordHash.StrengthArgon Strength = PasswordHash.StrengthArgon.Medium;

    public string Algor => "Argon2id-Med";

    public byte[] GenerateSalt() =>
        Sodium.PasswordHash.ArgonGenerateSalt();

    public byte[] DeriveKey(byte[] password, byte[] salt, int length) =>
        Sodium.PasswordHash.ArgonHashBinary(password, salt, outputLength: length,
            alg: PasswordHash.ArgonAlgorithm.Argon_2ID13,
            limit: PasswordHash.StrengthArgon.Medium);
}
