using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using pyfinal.Data;
using pyfinal.Models;
using pyfinal.Controllers;
using Moq;
using System.IdentityModel.Tokens.Jwt; // Necesario para leer el token en el test

namespace pyfinal_Tests
{
    public class AuthTests
    {
        // Método helper para crear la BD en memoria (reutilizable)
        private AppDbContext GetContextoEnMemoria()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Nombre único por ejecución
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task Login_ContrasenaIncorrecta_DebeRetornarUnauthorized()
        {
            // 1. ARRANGE
            var context = GetContextoEnMemoria();

            string passwordReal = "ContrasenaSegura123";
            string hashEnBd = BCrypt.Net.BCrypt.HashPassword(passwordReal);

            var usuarioRegistrado = new Usuario
            {
                Id = 1,
                Email = "usuario@prueba.com",
                PasswordHash = hashEnBd,
                Nombre = "Usuario Test", // Agregado porque ahora es [Required]
                Rol = "Vendedor"
            };
            context.Usuarios.Add(usuarioRegistrado);
            await context.SaveChangesAsync();

            var mockConfig = new Mock<IConfiguration>();
            // Configuramos un dummy key por si el controller lo pide antes de fallar
            mockConfig.Setup(c => c["Jwt:Key"]).Returns("ClaveSuperSecreta_Para_Tests_Unitarios_Mínimo_32_Chars");

            var controller = new AuthController(context, mockConfig.Object);

            var intentoFallido = new LoginDto
            {
                Email = "usuario@prueba.com",
                PasswordHash = "123456" // Contraseña INCORRECTA
            };

            // 2. ACT
            var resultado = await controller.Login(intentoFallido);

            // 3. ASSERT
            Assert.IsType<UnauthorizedObjectResult>(resultado);
        }

        [Fact]
        public async Task Login_Admin_CredencialesCorrectas_DebeRetornarTokenConPermisos()
        {
            // 1. ARRANGE
            var context = GetContextoEnMemoria();

            string passwordReal = "123456";
            string hashEnBd = BCrypt.Net.BCrypt.HashPassword(passwordReal);

            // Creamos un ADMIN para probar que se generen los permisos
            var usuarioValido = new Usuario
            {
                Id = 1,
                Email = "jefe@pyfinal.com",
                PasswordHash = hashEnBd,
                Nombre = "Jefe Logística",
                Rol = "Admin" // <--- Importante: Es Admin
            };
            context.Usuarios.Add(usuarioValido);
            await context.SaveChangesAsync();

            // Configuración Mock del JWT
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["Jwt:Key"]).Returns("Clave_Muy_Secreta_Y_Larga_Para_Que_Funcione_El_Test_123");
            mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            mockConfig.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            var controller = new AuthController(context, mockConfig.Object);

            var loginCorrecto = new LoginDto
            {
                Email = "jefe@pyfinal.com",
                PasswordHash = "123456" // Contraseña correcta
            };

            // 2. ACT
            var resultado = await controller.Login(loginCorrecto);

            // 3. ASSERT
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var valorRetornado = okResult.Value as dynamic;

            // Extraemos el token usando Reflection
            string tokenGenerado = valorRetornado.GetType().GetProperty("token").GetValue(valorRetornado, null);

            Assert.False(string.IsNullOrEmpty(tokenGenerado));

            // PRUEBA AVANZADA: ¿El token realmente tiene los permisos de Admin?
            // Leemos el token sin validarlo (solo para ver qué tiene dentro)
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenGenerado);

            // Verificamos que tenga una Claim específica de Admin que agregamos en el Controller
            // Por ejemplo: "PuedeCrearUsuarios"
            var claimPermiso = jwtToken.Claims.FirstOrDefault(c => c.Type == "PuedeCrearUsuarios");

            Assert.NotNull(claimPermiso); // Debe existir
            Assert.Equal("True", claimPermiso.Value); // Debe ser True
        }
    }
}