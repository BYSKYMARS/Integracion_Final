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
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PedidosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Pedidos
        [HttpGet]
        [Authorize(Policy = "PuedeVisualizarPedidos")]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
        {
            return await _context.Pedidos.ToListAsync();
        }

        // GET: api/Pedidos/5
        [HttpGet("{id}")]
        [Authorize(Policy = "PuedeVisualizarPedidos")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);

            if (pedido == null)
            {
                return NotFound();
            }

            return pedido;
        }

        // PUT: api/Pedidos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Policy = "PuedeEditarPedidos")]
        public async Task<IActionResult> PutPedido(int id, Pedido pedido)
        {
            if (id != pedido.Id) return BadRequest();

            // Consultamos el estado actual en la DB sin rastrearlo
            var pedidoActual = await _context.Pedidos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

            if (pedidoActual == null) return NotFound();

            // REGLA DE NEGOCIO: Si ya está Pagado, no permitir cambios
            if (pedidoActual.Estado == "Pagado")
            {
                return BadRequest(new { mensaje = "El pedido ya está Pagado y no puede ser modificado." });
            }

            _context.Entry(pedido).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Pedidos.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // POST: api/Pedidos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Policy = "PuedeCrearPedidos")]
        public async Task<ActionResult<Pedido>> PostPedido(Pedido pedido)
        {
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPedido", new { id = pedido.Id }, pedido);
        }

        // DELETE: api/Pedidos/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "PuedeEliminarPedidos")]
        public async Task<IActionResult> DeletePedido(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Buscamos el pedido usando el nombre exacto de la propiedad: DetallesP
                var pedido = await _context.Pedidos
                    .Include(p => p.DetallesP)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (pedido == null) return NotFound();

                // 2. REGLA DE NEGOCIO: No borrar si ya está Pagado
                if (pedido.Estado == "Pagado")
                {
                    return BadRequest(new { mensaje = "No se puede eliminar un pedido con estado Pagado." });
                }

                // 3. REVERSIÓN DE STOCK
                // Al borrar una venta, el producto debe REGRESAR al inventario
                foreach (var detalle in pedido.DetallesP)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        producto.Stock += detalle.Cantidad; // Sumamos de vuelta
                    }
                }

                _context.Pedidos.Remove(pedido);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al eliminar el pedido: {ex.Message}");
            }
        }

        private bool PedidoExists(int id)
        {
            return _context.Pedidos.Any(e => e.Id == id);
        }
    }
}
