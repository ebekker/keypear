// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.Shared.Models.Persisted;
using Microsoft.EntityFrameworkCore;

namespace Keypear.Server.LocalServer;

public class KyprDbContext : DbContext
{
    public KyprDbContext()
    { }

    public KyprDbContext(DbContextOptions<KyprDbContext> options)
        : base(options)
    { }

    public DbSet<Account> Accounts { get; set; } = default!;

    public DbSet<Vault> Vaults { get; set; } = default!;

    public DbSet<Grant>  Grants { get; set; } = default!;

    public DbSet<Record> Records { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // This is necessary for supporting EF Core Migrations tooling
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Filename=:memory:");
        }
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(build =>
        {
            build.HasKey(x => x.Id);
            build.HasAlternateKey(x => x.Username);
        });

        modelBuilder.Entity<Vault>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Record>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Grant>()
            .HasKey(x => new { x.AccountId, x.VaultId });
    }
}
