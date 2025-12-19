using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using pyfinal.Data;
using pyfinal.Models;
using pyfinal.Controllers;

namespace pyfinal_Tests
{
    public class StockComprasTests
    {
        // Configuración de la BD en memoria
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
        public async Task PostDetalleCompra_ConDatosValidos_DebeAumentarStock()
        {
            // 1. ARRANGE (Preparar)
            var context = GetContextoEnMemoria();

            // Creamos producto con stock bajo (Ej: 10 unidades)
            var producto = new Producto
            {
                Id = 1,
                Nombre = "Aceite Motor 5W-30",
                Codigo = "OIL-001", // <--- AGREGADO: Es obligatorio en tu nuevo modelo
                Stock = 10,
                PrecioVenta = 45.0m,
                CategoriaId = 1,    // <--- AGREGADO: Para evitar errores de validación
                ProveedorId = 1     // <--- AGREGADO: Para evitar errores de validación
            };
            context.Productos.Add(producto);
            await context.SaveChangesAsync();

            var controller = new DetalleComprasController(context);

            // Simulamos que llegan 50 unidades del proveedor
            var nuevaCompra = new DetalleCompra
            {
                ProductoId = 1,
                Cantidad = 50,
                // CORRECCIÓN AQUÍ: En DetalleCompra es PrecioCosto, no PrecioUnitario
                PrecioCosto = 25.0m,
                CompraId = 1 // Asumimos una compra padre existente o simulada
            };

            // 2. ACT (Actuar)
            // Llamamos al método que registra la compra
            await controller.PostDetalleCompra(nuevaCompra);

            // 3. ASSERT (Verificar)
            var productoActualizado = await context.Productos.FindAsync(1);

            // Verificamos: 10 iniciales + 50 comprados = 60 totales
            Assert.Equal(60, productoActualizado.Stock);
        }
    }
}