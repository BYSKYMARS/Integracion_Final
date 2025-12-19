using pyfinal.Models;

namespace pyfinal.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Asegura que la BD exista
            context.Database.EnsureCreated();

            // Validar si ya existen usuarios
            if (context.Usuarios.Any())
            {
                return; // La BD ya tiene datos, no hacemos nada
            }

            // --- CREAR EL SUPER ADMIN ---
            var admin = new Usuario
            {
                Nombre = "Administrador Por Defecto",
                Email = "Admin@admin.com",
                Rol = "Admin",
                // Contraseña inicial: "Admin123" (IMPORTANTE: Debe estar hasheada)
                // Asegúrate de tener BCrypt instalado, si no, usa tu método de hash
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
            };

            context.Usuarios.Add(admin);
            context.SaveChanges();
        }
    }
}