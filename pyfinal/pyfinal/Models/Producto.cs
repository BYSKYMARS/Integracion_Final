using System.ComponentModel.DataAnnotations; // <--- Importante para las validaciones

namespace pyfinal.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es obligatorio")]
        public string Codigo { get; set; } = string.Empty;

        // Validamos que el precio sea mayor o igual a 0
        [Range(0, double.MaxValue, ErrorMessage = "El precio de venta no puede ser negativo")]
        public decimal PrecioVenta { get; set; }

        // Validamos que el stock sea mayor o igual a 0
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; } 

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public int ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }
    }
}