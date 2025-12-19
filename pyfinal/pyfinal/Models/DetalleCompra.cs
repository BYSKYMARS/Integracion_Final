using System.ComponentModel.DataAnnotations;

namespace pyfinal.Models
{
    public class DetalleCompra
    {
        public int Id { get; set; }

        // VALIDACIÓN: Mínimo 1 unidad
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        // VALIDACIÓN: El costo no puede ser negativo
        [Required(ErrorMessage = "El precio de costo es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio de costo no puede ser negativo")]
        public decimal PrecioCosto { get; set; }

        [Required]
        public int CompraId { get; set; }
        public Compra? Compra { get; set; }

        [Required]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }
    }
}