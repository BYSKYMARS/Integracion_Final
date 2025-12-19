using System.ComponentModel.DataAnnotations; // Necesario para ValidationContext
using pyfinal.Models;

namespace pyfinal_Tests
{
    public class ProductoModelTests
    {
        // MÉTODO AUXILIAR: Simula la validación automática que hace ASP.NET Core
        private IList<ValidationResult> ValidarModelo(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void Producto_PrecioNegativo_DebeSerInvalido()
        {
            // 1. ARRANGE
            var producto = new Producto
            {
                Nombre = "Mouse Gamer",
                Codigo = "M-001",
                Stock = 10,
                PrecioVenta = -25.00m, // ERROR: Precio Negativo
                CategoriaId = 1,
                ProveedorId = 1
            };

            // 2. ACT
            // Pasamos el objeto por nuestro validador manual
            var errores = ValidarModelo(producto);

            // 3. ASSERT
            // Verificamos que existan errores
            Assert.NotEmpty(errores);

            // Verificamos que el error sea específicamente sobre "PrecioVenta"
            Assert.Contains(errores, e => e.MemberNames.Contains("PrecioVenta"));

            // Opcional: Verificamos el mensaje de error exacto
            Assert.Contains(errores, e => e.ErrorMessage == "El precio de venta no puede ser negativo");
        }

        [Fact]
        public void Producto_StockNegativo_DebeSerInvalido()
        {
            // 1. ARRANGE
            var producto = new Producto
            {
                Nombre = "Teclado",
                Codigo = "T-002",
                PrecioVenta = 50.00m,
                Stock = -5, // ERROR: Stock Negativo
                CategoriaId = 1,
                ProveedorId = 1
            };

            // 2. ACT
            var errores = ValidarModelo(producto);

            // 3. ASSERT
            Assert.NotEmpty(errores);
            Assert.Contains(errores, e => e.MemberNames.Contains("Stock"));
        }

        [Fact]
        public void Producto_DatosCorrectos_DebeSerValido()
        {
            // 1. ARRANGE
            var producto = new Producto
            {
                Nombre = "Monitor LED",
                Codigo = "MON-003",
                PrecioVenta = 450.00m, // Precio Positivo
                Stock = 20,            // Stock Positivo
                CategoriaId = 2,
                ProveedorId = 2
            };

            // 2. ACT
            var errores = ValidarModelo(producto);

            // 3. ASSERT
            // La lista de errores debe estar vacía
            Assert.Empty(errores);
        }
    }
}