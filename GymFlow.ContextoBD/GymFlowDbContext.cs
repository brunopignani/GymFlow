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
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<CuentaCorriente> CuentasCorriente { get; set; }
        public DbSet<Movimiento> Movimientos { get; set; }
        public DbSet<TipoMovimiento> TiposMovimiento { get; set; }
        public DbSet<Comprobante> Comprobantes { get; set; }
        public DbSet<ReciboPago> RecibosPago { get; set; }
        public DbSet<NotaCredito> NotasCredito { get; set; }
        public DbSet<NotaVenta> NotasVenta { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Persona>().UseTptMappingStrategy().ToTable("Personas");
            modelBuilder.Entity<Cliente>().ToTable("Clientes");
            modelBuilder.Entity<Empleado>().ToTable("Empleados");

            modelBuilder.Entity<CuentaCorriente>().ToTable("T_CUENTA_CORRIENTE");
            modelBuilder.Entity<Movimiento>().ToTable("T_MOVIMIENTO");
            modelBuilder.Entity<TipoMovimiento>().ToTable("T_TIPO_MOVIMIENTO");
            modelBuilder.Entity<Comprobante>().UseTptMappingStrategy().ToTable("T_COMPROBANTE");

            modelBuilder.Entity<Cliente>().Ignore(c => c.Pagos);

            modelBuilder.Entity<Cuota>().Property(c => c.Precio).HasPrecision(18, 2);
            modelBuilder.Entity<Pago>().Property(p => p.Monto).HasPrecision(18, 2);
            modelBuilder.Entity<Movimiento>().Property(m => m.Importe).HasPrecision(18, 2);
            modelBuilder.Entity<Comprobante>().Property(c => c.Total).HasPrecision(18, 2);

            modelBuilder.Entity<CuentaCorriente>().HasKey(cc => cc.Id);
            modelBuilder.Entity<Movimiento>().HasKey(m => m.Id);
            modelBuilder.Entity<TipoMovimiento>().HasKey(tm => tm.Id);
            modelBuilder.Entity<Comprobante>().HasKey(c => c.Id);
            modelBuilder.Entity<ReciboPago>().ToTable("T_RECIBO_PAGO");
            modelBuilder.Entity<NotaCredito>().ToTable("T_NOTA_CREDITO");
            modelBuilder.Entity<NotaVenta>().ToTable("T_NOTA_VENTA");

            modelBuilder.Entity<Persona>()
                .HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
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

            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Empleado)
                .WithMany(e => e.PagosRegistrados)
                .HasForeignKey(p => p.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pago>()
                .HasOne(p => p.CuotaCliente)
                .WithMany()
                .HasForeignKey(p => p.CuotaClienteId);

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

            modelBuilder.Entity<NotaCredito>()
                .HasOne(nc => nc.NotaVenta)
                .WithMany()
                .HasForeignKey(nc => nc.NotaVentaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NotaCredito>()
                .HasIndex(nc => nc.NotaVentaId);
        }
    }
}
