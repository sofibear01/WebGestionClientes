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
                entity.ToTable("cliente");

                entity.Property(e => e.ClienteId).HasColumnName("clienteid");
                entity.Property(e => e.Apellido).HasColumnName("apellido").HasMaxLength(50);
                entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(10);
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50);
                entity.Property(e => e.Saldo).HasColumnName("saldo").HasColumnType("decimal(10, 2)");
            });

            modelBuilder.Entity<CuentaCorriente>(entity =>
            {
                entity.HasKey(e => e.MovimientoId)
                    .HasName("PK__CuentaCo__BF923C2C29A1D401");

                entity.ToTable("cuentacorriente");

                entity.Property(e => e.MovimientoId).HasColumnName("movimientoid");
                entity.Property(e => e.ClienteId).HasColumnName("clienteid");
                entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(100);
                entity.Property(e => e.FhMovimiento).HasColumnName("fhmovimiento").HasColumnType("datetime");
                entity.Property(e => e.ImporteCredito).HasColumnName("importecredito").HasColumnType("decimal(10, 2)");
                entity.Property(e => e.ImporteDebito).HasColumnName("importedebito").HasColumnType("decimal(10, 2)");

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
