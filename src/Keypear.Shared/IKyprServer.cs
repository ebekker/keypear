﻿// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.Models.Service;

namespace Keypear.Shared;

public interface IKyprServer : IDisposable
{
    KyprSession? Session { get; set; }

    Task<KyprSession> AuthenticateAccountAsync(AccountDetails input);

    Task<AccountDetails> CreateAccountAsync(AccountDetails input);

    Task<AccountDetails?> GetAccountAsync(string username);

    Task<VaultDetails> CreateVaultAsync(VaultDetails input);

    Task<Guid[]> ListVaultsAsync();

    Task<VaultDetails?> GetVaultAsync(Guid vaultId);

    Task<Guid> SaveVaultAsync(VaultDetails input);

    Task<RecordDetails> SaveRecordAsync(RecordDetails input);

    Task<RecordDetails[]> GetRecordsAsync(Guid vaultId);
}
