using System.ComponentModel.DataAnnotations;

namespace pyfinal.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        // VALIDACIÓN: El total debe ser 0 o mayor
        [Range(0, double.MaxValue, ErrorMessage = "El total del pedido no puede ser negativo")]
        public decimal Total { get; set; }

        // VALIDACIÓN: Estado obligatorio y corto (ej: "Pendiente", "Pagado")
        [Required(ErrorMessage = "El estado es obligatorio")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder los 20 caracteres")]
        public string Estado { get; set; } = "Pendiente";

        [Required]
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public List<DetallePedido> DetallesP { get; set; } = new();
    }
}