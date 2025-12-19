using System.ComponentModel.DataAnnotations;

namespace pyfinal.Models
{
    public class Compra
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de la compra es obligatoria")]
        [DataType(DataType.DateTime)]
        public DateTime Fecha { get; set; } = DateTime.Now;

        // VALIDACIÓN: El total no puede ser negativo
        [Range(0, double.MaxValue, ErrorMessage = "El total de la compra no puede ser negativo")]
        public decimal TotalCompra { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        public int ProveedorId { get; set; }

        public Proveedor? Proveedor { get; set; }

        public List<DetalleCompra> DetallesC { get; set; } = new();
    }
}