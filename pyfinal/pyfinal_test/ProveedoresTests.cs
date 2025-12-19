using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using pyfinal.Data;
using pyfinal.Models;
using pyfinal.Controllers;

namespace pyfinal_Tests
{
    public class ProveedoresTests
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
        public async Task PostProveedor_RucDuplicado_DebeRetornarConflict()
        {
            // 1. ARRANGE (Preparar)
            var context = GetContextoEnMemoria();

            // Paso A: Crear un proveedor existente
            var proveedorOriginal = new Proveedor
            {
                RazonSocial = "Distribuidora Lima SAC",
                RUC = "20100200300", // RUC que vamos a duplicar
                Telefono = "999000111"
            };
            context.Proveedores.Add(proveedorOriginal);
            await context.SaveChangesAsync();

            var controller = new ProveedoresController(context);

            // Paso B: Intentar registrar otro proveedor con el MISMO RUC
            var proveedorDuplicado = new Proveedor
            {
                RazonSocial = "Empresa Copiona SRL",
                RUC = "20100200300", // ¡Es el mismo RUC!
                Telefono = "555444333"
            };

            // 2. ACT (Actuar)
            var resultado = await controller.PostProveedor(proveedorDuplicado);

            // 3. ASSERT (Verificar)
            // Verificamos que devuelva Conflict (Código 409)
            Assert.IsType<ConflictObjectResult>(resultado.Result);
        }
    }
}