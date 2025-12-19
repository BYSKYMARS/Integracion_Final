using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pyfinal.Data;
using pyfinal.Models;

namespace pyfinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnviosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnviosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Envios
        [HttpGet]
        [Authorize (Policy = "PuedeVisualizarEnvios")]
        public async Task<ActionResult<IEnumerable<Envio>>> GetEnvios()
        {
            return await _context.Envios.ToListAsync();
        }

        // GET: api/Envios/5
        [HttpGet("{id}")]
        [Authorize(Policy = "PuedeVisualizarEnvios")]
        public async Task<ActionResult<Envio>> GetEnvio(int id)
        {
            var envio = await _context.Envios.FindAsync(id);

            if (envio == null)
            {
                return NotFound();
            }

            return envio;
        }

        // PUT: api/Envios/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Policy = "PuedeEditarEnvios")]
        public async Task<IActionResult> PutEnvio(int id, Envio envio)
        {
            if (id != envio.Id) return BadRequest();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Obtener el envío actual de la DB para comparar el estado previo
                // Usamos AsNoTracking para que no interfiera con la actualización posterior
                var envioPrevio = await _context.Envios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (envioPrevio == null) return NotFound();

                // 2. Aquí necesitamos una forma de saber el "Nuevo Estado" 
                // Nota: Como tu modelo Envio no tiene un campo 'Estado' directo (lo maneja el historial),
                // usualmente el frontend envía el nuevo estado en una propiedad extendida o 
                // se asume que si llamas al PUT es para actualizar el historial.

                // OPCIÓN RECOMENDADA: Si el frontend envía un estado en el cuerpo del JSON 
                // (asumiendo que agregaste un campo temporal o manejas un DTO)

                _context.Entry(envio).State = EntityState.Modified;

                // 3. Si el estado es diferente al último registrado, creamos historial
                // Aquí compararemos contra el último hito del historial si fuera necesario.

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                if (!EnvioExists(id)) return NotFound();
                else throw;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Error al actualizar el envío");
            } 
        }

        // POST: api/Envios
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Policy = "PuedeCrearEnvios")]
        public async Task<ActionResult<Envio>> PostEnvio(Envio envio)
        {
            // 1. Verificar si ya existe un envío para este pedido
            var envioExistente = await _context.Envios
                .Include(e => e.Historial)
                .Where(e => e.PedidoId == envio.PedidoId)
                .OrderByDescending(e => e.Id)
                .FirstOrDefaultAsync();

            if (envioExistente != null)
            {
                // Obtener el último estado del historial
                var ultimoEstado = envioExistente.Historial
                    .OrderByDescending(h => h.FechaHora)
                    .Select(h => h.Estado)
                    .FirstOrDefault();

                // Si el envío no ha fallado, bloqueamos la creación de uno nuevo
                if (ultimoEstado != "Fallido" && ultimoEstado != "Fallido/Devuelto")
                {
                    return BadRequest(new { mensaje = "Este pedido ya tiene un envío en curso o ya fue entregado." });
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Envios.Add(envio);
                await _context.SaveChangesAsync();

                // Crear hito inicial
                var hitoInicial = new HistorialEnvio
                {
                    EnvioId = envio.Id,
                    Estado = "Envío Generado / En Almacén",
                    FechaHora = DateTime.Now
                };
                _context.HistorialesEnvio.Add(hitoInicial);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction("GetEnvio", new { id = envio.Id }, envio);
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Error al procesar el envío");
            }
        }

        // DELETE: api/Envios/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "PuedeEliminarEnvios")]
        public async Task<IActionResult> DeleteEnvio(int id)
        {
            var envio = await _context.Envios.FindAsync(id);
            if (envio == null)
            {
                return NotFound();
            }

            _context.Envios.Remove(envio);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EnvioExists(int id)
        {
            return _context.Envios.Any(e => e.Id == id);
        }
    }
}
