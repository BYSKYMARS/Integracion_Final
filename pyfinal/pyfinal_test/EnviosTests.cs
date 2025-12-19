using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using pyfinal.Data;       // Acceso a tu AppDbContext
using pyfinal.Models;     // Acceso a tus modelos Envio e HistorialEnvio
using pyfinal.Controllers; // Acceso a HistorialEnviosController

namespace pyfinal_Tests
{
    public class EnviosTests
    {
        // Configuración de la Base de Datos en Memoria
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
        public async Task PostHistorial_DebeRegistrarHitoCorrectamente()
        {
            // 1. ARRANGE (Preparar el escenario)
            var context = GetContextoEnMemoria();

            // Creamos un Envío Padre (Necesario para que la FK funcione)
            var envioPadre = new Envio
            {
                Id = 1,
                DireccionDestino = "Jr. Huallaga 123",
                FechaSalida = DateTime.Now,
                PedidoId = 50,    // Dato ficticio obligatorio
                RepartidorId = 5  // Dato ficticio obligatorio
            };
            context.Envios.Add(envioPadre);
            await context.SaveChangesAsync();

            // Instanciamos el controlador
            var controller = new HistorialEnviosController(context);

            // Preparamos el nuevo hito que va a reportar el repartidor
            var nuevoHito = new HistorialEnvio
            {
                EnvioId = 1,          // Vinculado al envío de arriba
                Estado = "Entregado", // El estado que queremos registrar
                FechaHora = DateTime.Now
            };

            // 2. ACT (Actuar)
            var resultado = await controller.PostHistorialEnvio(nuevoHito);

            // 3. ASSERT (Verificar)
            // Verificamos que el registro exista en la tabla 'HistorialesEnvio'
            // Usamos el nombre exacto de tu DbSet
            var hitoEnBd = await context.HistorialesEnvio
                                        .FirstOrDefaultAsync(h => h.EnvioId == 1 && h.Estado == "Entregado");

            Assert.NotNull(hitoEnBd); // Debe encontrarlo, no puede ser null
            Assert.Equal("Entregado", hitoEnBd.Estado);
        }
    }
}