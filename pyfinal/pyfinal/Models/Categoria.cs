using System.ComponentModel.DataAnnotations;

namespace pyfinal.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        // VALIDACIÓN: Obligatorio y máximo 100 caracteres
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        public List<Producto> Productos { get; set; } = new();
    }
}