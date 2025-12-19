using System.ComponentModel.DataAnnotations;

namespace pyfinal.Models
{
    public class Envio
    {
        public int Id { get; set; }

        // VALIDACIÓN: Dirección obligatoria y máximo 200 letras
        [Required(ErrorMessage = "La dirección de destino es obligatoria")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string DireccionDestino { get; set; } = string.Empty;

        public DateTime? FechaSalida { get; set; }

        [Required]
        public int PedidoId { get; set; }
        public Pedido? Pedido { get; set; }

        [Required]
        public int RepartidorId { get; set; }
        public Usuario? Repartidor { get; set; }

        public List<HistorialEnvio> Historial { get; set; } = new();
    }
}