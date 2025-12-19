using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pyfinal.Controllers; // Cambia esto por tu namespace
using pyfinal.Data;       // Cambia esto por tu namespace
using pyfinal.Models;     // Cambia esto por tu namespace

namespace pyfinal_Tests
{
    public class StockVentasTests
    {
        // 1. Configuramos una base de datos ficticia
        private AppDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var databaseContext = new AppDbContext(options);
            databaseContext.Database.EnsureCreated();
            return databaseContext;
        }

        [Fact] // Indica que esto es una prueba
        public async Task AlCrearPedido_ElStockDebeDisminuir()
        {
            // --- ARRANGE (Preparar el escenario) ---
            var context = GetDatabaseContext();

            // Creamos un producto con 20 unidades
            var producto = new Producto
            {
                Id = 1,
                Nombre = "Laptop Gamer",
                Stock = 20,
                PrecioVenta = 1500,
                Codigo = "LPT-001"
            };
            context.Productos.Add(producto);
            await context.SaveChangesAsync();

            // Crear la cabecera del pedido para que el controlador la encuentre
            var pedido = new Pedido
            {
                Fecha = DateTime.Now,
                Total = 0.0m,
                Estado = "Pendiente",
                UsuarioId = 1
            };
            context.Pedidos.Add(pedido);
            await context.SaveChangesAsync();

            var controller = new DetallePedidosController(context);

            // Queremos vender 5 unidades
            var detalle = new DetallePedido
            {
                ProductoId = 1,
                Cantidad = 5,
                PrecioUnitario = 1500,
                PedidoId = pedido.Id // <-- referenciar el pedido creado
            };

            // --- ACT (Ejecutar la acción) ---
            await controller.PostDetallePedido(detalle);

            // --- ASSERT (Verificar que funcionó) ---
            var productoResult = await context.Productos.FindAsync(1);

            Assert.NotNull(productoResult);
            // Verificamos: 20 iniciales - 5 vendidos = debe quedar 15
            Assert.Equal(15, productoResult!.Stock);
        }
    }
}