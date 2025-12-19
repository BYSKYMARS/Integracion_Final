using System.ComponentModel.DataAnnotations; // <--- Necesario para las validaciones

namespace pyfinal.Models
{
    public class DetallePedido
    {
        public int Id { get; set; }

        // VALIDACIÓN: La cantidad debe ser mínimo 1. No se permiten 0 ni negativos.
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero")]
        public int Cantidad { get; set; }

        // VALIDACIÓN: El precio no puede ser negativo
        [Range(0, double.MaxValue, ErrorMessage = "El precio unitario no puede ser negativo")]
        public decimal PrecioUnitario { get; set; }

        public int PedidoId { get; set; }
        public Pedido? Pedido { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }
    }
}