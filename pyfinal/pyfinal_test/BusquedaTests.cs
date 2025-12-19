using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using pyfinal.Data;
using pyfinal.Models;
using pyfinal.Controllers;

namespace pyfinal_Tests
{
    public class BusquedaTests
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
        public async Task GetProductosPorCategoria_DebeRetornarSoloEsaCategoria()
        {
            // 1. ARRANGE
            var context = GetContextoEnMemoria();

            // Insertamos datos mezclados:
            // 2 Productos de Categoría 1 (Tecnología)
            // 1 Producto de Categoría 2 (Ropa)
            context.Productos.AddRange(
                new Producto { Nombre = "Laptop", CategoriaId = 1, PrecioVenta = 1000, Stock = 5, Codigo = "LPT" },
                new Producto { Nombre = "Mouse", CategoriaId = 1, PrecioVenta = 20, Stock = 10, Codigo = "MSE" },
                new Producto { Nombre = "Polo", CategoriaId = 2, PrecioVenta = 15, Stock = 50, Codigo = "POL" }
            );
            await context.SaveChangesAsync();

            var controller = new ProductosController(context);

            // 2. ACT
            // Llamamos a TU NUEVO MÉTODO pidiendo solo Categoría 1
            var resultado = await controller.GetProductosPorCategoria(1);

            // 3. ASSERT
            var productos = resultado.Value;

            Assert.NotNull(productos);

            // Deben ser exactamente 2 (Laptop y Mouse)
            Assert.Equal(2, productos.Count());

            // Aseguramos que NO se coló el Polo (Categoría 2)
            Assert.DoesNotContain(productos, p => p.CategoriaId == 2);
        }
    }
}