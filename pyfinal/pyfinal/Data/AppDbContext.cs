using Microsoft.EntityFrameworkCore;
using pyfinal.Models;

namespace pyfinal.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompra { get; set; }
        public DbSet<Envio> Envios { get; set; }
        public DbSet<HistorialEnvio> HistorialesEnvio { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Solución Compra -> Producto (Cycle)
            modelBuilder.Entity<DetalleCompra>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Solución Venta -> Producto (Cycle)
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. SOLUCIÓN NUEVA: Envio -> Repartidor (Usuario)
            // Esto rompe el ciclo entre Pedido->Usuario y Envio->Usuario
            modelBuilder.Entity<Envio>()
                .HasOne(e => e.Repartidor)
                .WithMany()
                .HasForeignKey(e => e.RepartidorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuraciones de precisión
            modelBuilder.Entity<Producto>().Property(p => p.PrecioVenta).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<DetallePedido>().Property(p => p.PrecioUnitario).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Pedido>().Property(p => p.Total).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Compra>().Property(c => c.TotalCompra).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<DetalleCompra>().Property(d => d.PrecioCosto).HasColumnType("decimal(18,2)");
        }
    }
}