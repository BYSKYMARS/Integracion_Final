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
    public class ComprasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComprasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Compras
        [HttpGet]
        [Authorize(Policy = "PuedeVisualizarCompras")]
        public async Task<ActionResult<IEnumerable<Compra>>> GetCompras()
        {
            return await _context.Compras.ToListAsync();
        }

        // GET: api/Compras/5
        [HttpGet("{id}")]
        [Authorize(Policy = "PuedeVisualizarCompras")]
        public async Task<ActionResult<Compra>> GetCompra(int id)
        {
            var compra = await _context.Compras.FindAsync(id);

            if (compra == null)
            {
                return NotFound();
            }

            return compra;
        }

        // PUT: api/Compras/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Policy = "PuedeEditarCompras")]
        public async Task<IActionResult> PutCompra(int id, Compra compra)
        {
            if (id != compra.Id)
            {
                return BadRequest();
            }

            _context.Entry(compra).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompraExists(id))
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

        // POST: api/Compras
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Policy = "PuedeCrearCompras")]
        public async Task<ActionResult<Compra>> PostCompra(Compra compra)
        {
            _context.Compras.Add(compra);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCompra", new { id = compra.Id }, compra);
        }

        // DELETE: api/Compras/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "PuedeEliminarCompras")]
        public async Task<IActionResult> DeleteCompra(int id)
        {
            // Usamos una transacción para asegurar que el stock y la compra se actualicen juntos
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Buscamos la compra incluyendo sus detalles
                var compra = await _context.Compras
                    .Include(c => c.DetallesC)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (compra == null)
                {
                    return NotFound();
                }

                // 2. REVERSIÓN DE STOCK
                // Por cada producto en el detalle, restamos la cantidad del stock (porque la compra se cancela)
                foreach (var detalle in compra.DetallesC)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        producto.Stock -= detalle.Cantidad;
                    }
                }

                // 3. Eliminar la compra (EF borrará los detalles en cascada si está configurado, 
                // o puedes borrarlos manualmente aquí)
                _context.Compras.Remove(compra);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al eliminar la compra: {ex.Message}");
            }
        }

        private bool CompraExists(int id)
        {
            return _context.Compras.Any(e => e.Id == id);
        }
    }
}
