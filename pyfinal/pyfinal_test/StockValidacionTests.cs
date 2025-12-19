using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using pyfinal.Data;
using pyfinal.Models;
using pyfinal.Controllers;

namespace pyfinal_Tests
{
    public class StockValidacionTests
    {
        private AppDbContext GetContextoEnMemoria()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task PostDetallePedido_SinStockSuficiente_DebeRetornarBadRequest()
        {
            // 1. ARRANGE (Preparar el escenario)
            var context = GetContextoEnMemoria();

            // Creamos un producto con poco stock (Solo 5 unidades)
            var producto = new Producto
            {
                Id = 1,
                Nombre = "Monitor 24 pulgadas",
                Stock = 5,
                PrecioVenta = 500.0m
            };
            context.Productos.Add(producto);
            await context.SaveChangesAsync();

            var controller = new DetallePedidosController(context);

            // Intentamos vender 10 unidades (Más de lo que tenemos)
            var detalleExcesivo = new DetallePedido
            {
                ProductoId = 1,
                Cantidad = 10,
                PrecioUnitario = 500.0m
            };

            // 2. ACT (Ejecutar la acción)
            var resultado = await controller.PostDetallePedido(detalleExcesivo);

            // 3. ASSERT (Verificar que el sistema rechace la venta)
            // Esperamos un "BadRequest" (Código 400)
            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }
    }
}