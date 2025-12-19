using System.ComponentModel.DataAnnotations;
using pyfinal.Models;

namespace pyfinal_Tests
{
    public class DetallePedidoModelTests
    {
        // Función auxiliar para validar modelos manualmente en el test
        private IList<ValidationResult> ValidarModelo(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void DetallePedido_CantidadCero_DebeSerInvalido()
        {
            // 1. ARRANGE
            var detalle = new DetallePedido
            {
                ProductoId = 1,
                PedidoId = 1,
                Cantidad = 0, // ERROR: Esto viola el [Range(1, ...)]
                PrecioUnitario = 10.0m
            };

            // 2. ACT
            var errores = ValidarModelo(detalle);

            // 3. ASSERT
            Assert.NotEmpty(errores); // Debe haber errores
            Assert.Contains(errores, e => e.MemberNames.Contains("Cantidad"));
            Assert.Contains(errores, e => e.ErrorMessage == "La cantidad debe ser mayor a cero");
        }

        [Fact]
        public void DetallePedido_CantidadCorrecta_DebeSerValido()
        {
            // 1. ARRANGE
            var detalle = new DetallePedido
            {
                ProductoId = 1,
                PedidoId = 1,
                Cantidad = 5, // CORRECTO: Es mayor a 0
                PrecioUnitario = 10.0m
            };

            // 2. ACT
            var errores = ValidarModelo(detalle);

            // 3. ASSERT
            Assert.Empty(errores); // No debe haber errores
        }
    }
}