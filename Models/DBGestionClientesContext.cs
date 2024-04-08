using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WebGestionClientes.Models
{
    public partial class DBGestionClientesContext : DbContext
    {
        public DBGestionClientesContext()
        {
        }

        public DBGestionClientesContext(DbContextOptions<DBGestionClientesContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Cliente> Clientes { get; set; } = null!;
        public virtual DbSet<CuentaCorriente> CuentaCorrientes { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Cliente");

                entity.Property(e => e.Apellido).HasMaxLength(50);

                entity.Property(e => e.Estado).HasMaxLength(10);

                entity.Property(e => e.Nombre).HasMaxLength(50);

                entity.Property(e => e.Saldo).HasColumnType("decimal(10, 2)");
            });

            modelBuilder.Entity<CuentaCorriente>(entity =>
            {
                entity.HasKey(e => e.MovimientoId)
                    .HasName("PK__CuentaCo__BF923C2C29A1D401");

                entity.ToTable("CuentaCorriente");

                entity.Property(e => e.Descripcion).HasMaxLength(100);

                entity.Property(e => e.FhMovimiento).HasColumnType("datetime");

                entity.Property(e => e.ImporteCredito).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ImporteDebito).HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.Cliente)
                    .WithOne(p => p.CuentaCorriente)
                    .HasForeignKey<CuentaCorriente>(d => d.ClienteId)
                    .HasConstraintName("FK_Cliente");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
