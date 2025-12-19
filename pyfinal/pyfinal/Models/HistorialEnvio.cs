using System.ComponentModel.DataAnnotations;

namespace pyfinal.Models
{
    public class HistorialEnvio
    {
        public int Id { get; set; }

        // VALIDACIÓN: Obligatorio y máximo 50 caracteres (suficiente para "En camino", "Entregado", etc.)
        [Required(ErrorMessage = "El estado es obligatorio")]
        [StringLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres")]
        public string Estado { get; set; } = string.Empty;

        [Required]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Required]
        public int EnvioId { get; set; }
        public Envio? Envio { get; set; }
    }
}