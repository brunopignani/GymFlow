using System.Collections.Generic;
using GymFlow.Dominio;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.ContextoBD
{
    public class GymFlowDbContext : DbContext
    {
        public GymFlowDbContext(DbContextOptions<GymFlowDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Actividad> Actividades { get; set; }
        public DbSet<Cuota> Cuotas { get; set; }
        public DbSet<CuotaCliente> CuotasCliente { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<CuentaCorriente> CuentasCorriente { get; set; }
        public DbSet<Movimiento> Movimientos { get; set; }
        public DbSet<TipoMovimiento> TiposMovimiento { get; set; }
        public DbSet<Comprobante> Comprobantes { get; set; }
        public DbSet<ReciboPago> RecibosPago { get; set; }
        public DbSet<NotaCredito> NotasCredito { get; set; }
        public DbSet<NotaVenta> NotasVenta { get; set; }
        public DbSet<AjusteCuenta> AjustesCuenta { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<Formulario> Formularios { get; set; }
        public DbSet<Accion> Acciones { get; set; }
        public DbSet<Grupo> Grupos { get; set; }
        public DbSet<AuditoriaSesion> AuditoriasSesion { get; set; }
        public DbSet<AuditoriaCliente> AuditoriasCliente { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Persona>().UseTptMappingStrategy().ToTable("Personas");
            modelBuilder.Entity<Cliente>().ToTable("Clientes");
            modelBuilder.Entity<Empleado>().ToTable("Empleados");

            modelBuilder.Entity<CuentaCorriente>().ToTable("T_CUENTA_CORRIENTE");
            modelBuilder.Entity<Movimiento>().ToTable("T_MOVIMIENTO");
            modelBuilder.Entity<TipoMovimiento>().ToTable("T_TIPO_MOVIMIENTO");
            modelBuilder.Entity<Comprobante>().UseTptMappingStrategy().ToTable("T_COMPROBANTE");

            modelBuilder.Entity<Cuota>().Property(c => c.Precio).HasPrecision(18, 2);
            modelBuilder.Entity<Movimiento>().Property(m => m.Importe).HasPrecision(18, 2);
            modelBuilder.Entity<Comprobante>().Property(c => c.Total).HasPrecision(18, 2);
            modelBuilder.Entity<Comprobante>().Property(c => c.Numero).HasMaxLength(11).IsRequired();
            modelBuilder.Entity<Comprobante>()
                .HasIndex(c => c.Numero)
                .IsUnique();

            modelBuilder.Entity<CuentaCorriente>().HasKey(cc => cc.Id);
            modelBuilder.Entity<Movimiento>().HasKey(m => m.Id);
            modelBuilder.Entity<TipoMovimiento>().HasKey(tm => tm.Id);
            modelBuilder.Entity<Comprobante>().HasKey(c => c.Id);
            modelBuilder.Entity<ReciboPago>().ToTable("T_RECIBO_PAGO");
            modelBuilder.Entity<NotaCredito>().ToTable("T_NOTA_CREDITO");
            modelBuilder.Entity<NotaVenta>().ToTable("T_NOTA_VENTA");
            modelBuilder.Entity<AjusteCuenta>().ToTable("T_AJUSTE_CUENTA");

            modelBuilder.Entity<AjusteCuenta>().Property(a => a.Motivo).HasMaxLength(500).IsRequired();
            modelBuilder.Entity<AjusteCuenta>()
                .HasOne(a => a.Empleado)
                .WithMany()
                .HasForeignKey(a => a.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Persona>()
                .HasIndex(p => p.Dni)
                .IsUnique();

            modelBuilder.Entity<Persona>()
                .HasOne(p => p.Usuario)
                .WithOne()
                .HasForeignKey<Persona>(p => p.UsuarioId)
                .IsRequired(false);

            modelBuilder.Entity<CuotaCliente>()
                .HasOne(cc => cc.Cliente)
                .WithMany(c => c.CuotasCliente)
                .HasForeignKey(cc => cc.ClienteId);

            modelBuilder.Entity<CuotaCliente>()
                .HasOne(cc => cc.Cuota)
                .WithMany()
                .HasForeignKey(cc => cc.CuotaId);

            modelBuilder.Entity<Inscripcion>()
                .HasOne(i => i.Cliente)
                .WithMany(c => c.Inscripciones)
                .HasForeignKey(i => i.ClienteId);

            modelBuilder.Entity<Inscripcion>()
                .HasOne(i => i.Actividad)
                .WithMany(a => a.Inscripciones)
                .HasForeignKey(i => i.ActividadId);

            modelBuilder.Entity<CuentaCorriente>()
                .HasOne(cc => cc.Cliente)
                .WithOne(c => c.Cuenta)
                .HasForeignKey<CuentaCorriente>(cc => cc.ClienteId);

            modelBuilder.Entity<Movimiento>()
                .HasOne<CuentaCorriente>()
                .WithMany(cc => cc.Movimientos)
                .HasForeignKey(m => m.CuentaCorrienteId);

            modelBuilder.Entity<Movimiento>()
                .HasOne(m => m.TipoMovimiento)
                .WithMany()
                .HasForeignKey(m => m.TipoMovimientoId);

            modelBuilder.Entity<Movimiento>()
                .HasOne(m => m.Comprobante)
                .WithMany()
                .HasForeignKey(m => m.ComprobanteId);

            modelBuilder.Entity<ReciboPago>()
                .HasOne(rp => rp.Empleado)
                .WithMany(e => e.Recibos)
                .HasForeignKey(rp => rp.EmpleadoId);

            modelBuilder.Entity<NotaVenta>()
                .HasOne(nv => nv.CuotaCliente)
                .WithMany()
                .HasForeignKey(nv => nv.CuotaClienteId);

            modelBuilder.Entity<NotaVenta>()
                .Property(nv => nv.Periodo)
                .HasColumnType("date");

            modelBuilder.Entity<NotaVenta>()
                .HasIndex(nv => new { nv.CuotaClienteId, nv.Periodo })
                .IsUnique();

            modelBuilder.Entity<NotaCredito>()
                .HasOne(nc => nc.NotaVenta)
                .WithMany()
                .HasForeignKey(nc => nc.NotaVentaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NotaCredito>()
                .HasIndex(nc => nc.NotaVentaId);

            ConfigurarSeguridad(modelBuilder);
            ConfigurarAuditoria(modelBuilder);
        }

        private static void ConfigurarSeguridad(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<ComponentePermiso>();

            modelBuilder.Entity<Modulo>(e =>
            {
                e.ToTable("T_MODULO");
                e.HasKey(m => m.Id);
                e.Property(m => m.Nombre).HasMaxLength(100).IsRequired();
                e.HasIndex(m => m.Nombre).IsUnique();
            });

            modelBuilder.Entity<Formulario>(e =>
            {
                e.ToTable("T_FORMULARIO");
                e.HasKey(f => f.Id);
                e.Property(f => f.Nombre).HasMaxLength(100).IsRequired();
                e.HasOne(f => f.Modulo)
                    .WithMany(m => m.Formularios)
                    .HasForeignKey(f => f.ModuloId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(f => new { f.ModuloId, f.Nombre }).IsUnique();
            });

            modelBuilder.Entity<Accion>(e =>
            {
                e.ToTable("T_ACCION");
                e.HasKey(a => a.Id);
                e.Property(a => a.Codigo).HasMaxLength(80).IsRequired();
                e.Property(a => a.Nombre).HasMaxLength(150).IsRequired();
                e.HasIndex(a => a.Codigo).IsUnique();
                e.HasOne(a => a.Formulario)
                    .WithMany(f => f.Acciones)
                    .HasForeignKey(a => a.FormularioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Grupo>(e =>
            {
                e.ToTable("T_GRUPO");
                e.HasKey(g => g.Id);
                e.Property(g => g.Nombre).HasMaxLength(100).IsRequired();
                e.Property(g => g.Descripcion).HasMaxLength(250);
                e.Property(g => g.Estado).HasMaxLength(20).IsRequired();
                e.HasIndex(g => g.Nombre).IsUnique();

                e.HasMany(g => g.Acciones)
                    .WithMany()
                    .UsingEntity<Dictionary<string, object>>(
                        "T_GRUPO_ACCION",
                        r => r.HasOne<Accion>().WithMany().HasForeignKey("AccionId").OnDelete(DeleteBehavior.Restrict),
                        l => l.HasOne<Grupo>().WithMany().HasForeignKey("GrupoId").OnDelete(DeleteBehavior.Cascade),
                        j => j.HasKey("GrupoId", "AccionId"));

                e.HasMany(g => g.Subgrupos)
                    .WithMany()
                    .UsingEntity<Dictionary<string, object>>(
                        "T_GRUPO_GRUPO",
                        r => r.HasOne<Grupo>().WithMany().HasForeignKey("GrupoHijoId").OnDelete(DeleteBehavior.NoAction),
                        l => l.HasOne<Grupo>().WithMany().HasForeignKey("GrupoPadreId").OnDelete(DeleteBehavior.NoAction),
                        j =>
                        {
                            j.HasKey("GrupoPadreId", "GrupoHijoId");
                            j.ToTable("T_GRUPO_GRUPO", t => t.HasCheckConstraint("CK_T_GRUPO_GRUPO_NoAutoReferencia", "[GrupoPadreId] <> [GrupoHijoId]"));
                        });
            });

            modelBuilder.Entity<Usuario>(e =>
            {
                e.HasMany(u => u.Grupos)
                    .WithMany()
                    .UsingEntity<Dictionary<string, object>>(
                        "T_USUARIO_GRUPO",
                        r => r.HasOne<Grupo>().WithMany().HasForeignKey("GrupoId").OnDelete(DeleteBehavior.Restrict),
                        l => l.HasOne<Usuario>().WithMany().HasForeignKey("UsuarioId").OnDelete(DeleteBehavior.Restrict),
                        j => j.HasKey("UsuarioId", "GrupoId"));

                e.HasMany(u => u.AccionesDirectas)
                    .WithMany()
                    .UsingEntity<Dictionary<string, object>>(
                        "T_USUARIO_ACCION",
                        r => r.HasOne<Accion>().WithMany().HasForeignKey("AccionId").OnDelete(DeleteBehavior.Restrict),
                        l => l.HasOne<Usuario>().WithMany().HasForeignKey("UsuarioId").OnDelete(DeleteBehavior.Restrict),
                        j => j.HasKey("UsuarioId", "AccionId"));
            });
        }

        private static void ConfigurarAuditoria(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditoriaSesion>(e =>
            {
                e.ToTable("T_AUDITORIA_SESION");
                e.HasKey(a => a.Id);
                e.Property(a => a.FechaHora).IsRequired();
                e.Property(a => a.TipoEvento).HasMaxLength(20).IsRequired();
                e.Property(a => a.NombreUsuario).HasMaxLength(100).IsRequired();
                e.HasOne<Usuario>()
                    .WithMany()
                    .HasForeignKey(a => a.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(a => a.FechaHora);
                e.HasIndex(a => a.UsuarioId);
            });

            modelBuilder.Entity<AuditoriaCliente>(e =>
            {
                e.ToTable("T_AUDITORIA_CLIENTE");
                e.HasKey(a => a.Id);
                e.Property(a => a.FechaHora).IsRequired();
                e.Property(a => a.Operacion).HasMaxLength(20).IsRequired();
                e.Property(a => a.NombreUsuario).HasMaxLength(100).IsRequired();
                e.Property(a => a.ValoresAnteriores).HasColumnType("nvarchar(max)").IsRequired(false);
                e.Property(a => a.ValoresNuevos).HasColumnType("nvarchar(max)").IsRequired(false);
                e.HasOne<Usuario>()
                    .WithMany()
                    .HasForeignKey(a => a.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(a => a.FechaHora);
                e.HasIndex(a => a.ClienteId);
                e.HasIndex(a => a.DniCliente);
            });
        }
    }
}
