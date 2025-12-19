using System.ComponentModel.DataAnnotations;

namespace pyfinal.Models
{
    public class Proveedor
    {
        public int Id { get; set; }

        // VALIDACIÓN: Obligatorio y máximo 150 letras
        [Required(ErrorMessage = "La Razón Social es obligatoria")]
        [StringLength(150, ErrorMessage = "La razón social no puede exceder los 150 caracteres")]
        public string RazonSocial { get; set; } = string.Empty;

        // VALIDACIÓN: Obligatorio y EXACTAMENTE 11 caracteres (Estándar RUC)
        [Required(ErrorMessage = "El RUC es obligatorio")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "El RUC debe tener exactamente 11 dígitos")]
        public string RUC { get; set; } = string.Empty;

        // VALIDACIÓN: Obligatorio y formato teléfono
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        public string Telefono { get; set; } = string.Empty;

        public List<Producto> Productos { get; set; } = new();
    }
}