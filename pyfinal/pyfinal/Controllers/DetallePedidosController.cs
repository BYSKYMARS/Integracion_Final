using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using pyfinal.Data;
using pyfinal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyfinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "PuedeGestionarDetallesPedido")]
    public class DetallePedidosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DetallePedidosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DetallePedidos
        [HttpGet]
        [Authorize(Policy = "PuedeGestionarDetallesPedido")]
        public async Task<ActionResult<IEnumerable<DetallePedido>>> GetDetallesPedido()
        {
            return await _context.DetallesPedido.ToListAsync();
        }

        // GET: api/DetallePedidos/5
        [HttpGet("{id}")]
        [Authorize(Policy = "PuedeGestionarDetallesPedido")]
        public async Task<ActionResult<DetallePedido>> GetDetallePedido(int id)
        {
            var detallePedido = await _context.DetallesPedido.FindAsync(id);

            if (detallePedido == null)
            {
                return NotFound();
            }

            return detallePedido;
        }

        // PUT: api/DetallePedidos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Policy = "PuedeGestionarDetallesPedido")]
        public async Task<IActionResult> PutDetallePedido(int id, DetallePedido detallePedido)
        {
            if (id != detallePedido.Id)
            {
                return BadRequest();
            }

            _context.Entry(detallePedido).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DetallePedidoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/DetallePedidos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Policy = "PuedeGestionarDetallesPedido")]
        public async Task<ActionResult<DetallePedido>> PostDetallePedido(DetallePedido detallePedido)
        {
            // Detectar proveedor InMemory (no soporta transacciones)
            var providerName = _context.Database.ProviderName ?? string.Empty;
            var isInMemory = providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase);

            IDbContextTransaction? transaction = null;
            if (!isInMemory)
            {
                transaction = await _context.Database.BeginTransactionAsync();
            }

            try
            {
                var producto = await _context.Productos.FindAsync(detallePedido.ProductoId);
                var pedido = await _context.Pedidos.FindAsync(detallePedido.PedidoId);

                if (producto == null || pedido == null)
                    return NotFound(new { mensaje = "Producto o Pedido no encontrado." });

                if (producto.Stock < detallePedido.Cantidad)
                    return BadRequest(new { mensaje = $"Stock insuficiente. Solo quedan {producto.Stock}." });


                // REGLA DE ORO: Si el pedido está pagado, no se toca nada.
                if (pedido != null && pedido.Estado == "Pagado")
                {
                    return BadRequest(new { mensaje = "No puedes agregar productos a un pedido que ya ha sido Pagado." });
                }

                // 1. Restar Stock
                producto.Stock -= detallePedido.Cantidad;

                // 2. Actualizar Total de la Cabecera
                pedido!.Total += (detallePedido.Cantidad * detallePedido.PrecioUnitario);

                // 3. Guardar el Detalle
                _context.DetallesPedido.Add(detallePedido);

                await _context.SaveChangesAsync();

                if (transaction != null)
                    await transaction.CommitAsync();

                return CreatedAtAction("GetDetallePedido", new { id = detallePedido.Id }, detallePedido);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();

                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        // DELETE: api/DetallePedidos/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "PuedeGestionarDetallesPedido")]
        public async Task<IActionResult> DeleteDetallePedido(int id)
        {
            // Detectar proveedor InMemory (no soporta transacciones)
            var providerName = _context.Database.ProviderName ?? string.Empty;
            var isInMemory = providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase);

            IDbContextTransaction? transaction = null;
            if (!isInMemory)
            {
                transaction = await _context.Database.BeginTransactionAsync();
            }

            try
            {
                var detallePedido = await _context.DetallesPedido.FindAsync(id);
                if (detallePedido == null) return NotFound();

                var producto = await _context.Productos.FindAsync(detallePedido.ProductoId);
                var pedido = await _context.Pedidos.FindAsync(detallePedido.PedidoId);

                // REGLA DE ORO: No se puede devolver stock de un pedido ya cobrado.
                if (pedido != null && pedido.Estado == "Pagado")
                {
                    return BadRequest(new { mensaje = "No se puede eliminar items ni devolver stock de un pedido Pagado." });
                }

                // LÓGICA DE REVERSIÓN
                if (producto != null)
                {
                    // Devolvemos el stock al almacén
                    producto.Stock += detallePedido.Cantidad;
                }

                if (pedido != null)
                {
                    // Restamos el monto del total del pedido
                    pedido.Total -= (detallePedido.Cantidad * detallePedido.PrecioUnitario);
                    if (pedido.Total < 0) pedido.Total = 0;
                }

                _context.DetallesPedido.Remove(detallePedido);

                await _context.SaveChangesAsync();

                if (transaction != null)
                    await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();

                return StatusCode(500, $"Error al revertir: {ex.Message}");
            }
        }

        private bool DetallePedidoExists(int id)
        {
            return _context.DetallesPedido.Any(e => e.Id == id);
        }
    }
}
