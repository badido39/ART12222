using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ART122.Data
{
    public class ImpotDbContext : DbContext
    {
        public ImpotDbContext(DbContextOptions<ImpotDbContext> options)
            : base(options)
        {
        }

        public DbSet<RedevableInfo> Redevables => Set<RedevableInfo>();
        public DbSet<Impot> Impots => Set<Impot>();
        public DbSet<NatureImpot> NatureImpots => Set<NatureImpot>();
        public DbSet<Declaration> Declarations => Set<Declaration>();
        public DbSet<Versement> Versements => Set<Versement>();
        public DbSet<AppSettings> AppSettings => Set<AppSettings>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Impot>()
                .HasOne(i => i.NatureImpot)
                .WithMany()
                .HasForeignKey(i => i.NatureImpotId);

            modelBuilder.Entity<Impot>()
                .HasOne(i => i.RedevableInfo)
                .WithMany(r => r.Impots)
                .HasForeignKey(i => i.RedevableInfoId);

            modelBuilder.Entity<RedevableInfo>()
                .HasIndex(r => r.BP)
                .IsUnique();

            modelBuilder.Entity<RedevableInfo>()
                .HasIndex(r => r.NIF)
                .IsUnique();

            modelBuilder.Entity<RedevableInfo>()
                .HasIndex(r => r.Article)
                .IsUnique();
            modelBuilder.Entity<NatureImpot>()
                .HasIndex(n => n.Name)
                .IsUnique();
        }
    }
}
