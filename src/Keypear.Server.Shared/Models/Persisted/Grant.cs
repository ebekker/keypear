// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

namespace Keypear.Server.Shared.Models.Persisted;

public class Grant
{
    public Guid VaultId { get; set; }
    public Vault? Vault { get; set; }

    public Guid AccountId { get; set; }
    public Account? Account { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Secret key (symmetric) for the associated Vault,
    /// encrypted with the associated Account public key and
    /// decrypted with the associated Account private key.
    /// </summary>
    public byte[]? SecretKeyEnc { get; set; }
}
