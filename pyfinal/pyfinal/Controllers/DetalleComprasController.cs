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
    public class DetalleComprasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DetalleComprasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DetalleCompras
        [HttpGet]
        [Authorize(Policy = "PuedeGestionarDetallesCompra")]
        public async Task<ActionResult<IEnumerable<DetalleCompra>>> GetDetallesCompra()
        {
            return await _context.DetallesCompra.ToListAsync();
        }

        // GET: api/DetalleCompras/5
        [HttpGet("{id}")]
        [Authorize(Policy = "PuedeGestionarDetallesCompra")]
        public async Task<ActionResult<DetalleCompra>> GetDetalleCompra(int id)
        {
            var detalleCompra = await _context.DetallesCompra.FindAsync(id);

            if (detalleCompra == null)
            {
                return NotFound();
            }

            return detalleCompra;
        }

        // PUT: api/DetalleCompras/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Policy = "PuedeGestionarDetallesCompra")]
        public async Task<IActionResult> PutDetalleCompra(int id, DetalleCompra detalleCompra)
        {
            if (id != detalleCompra.Id)
            {
                return BadRequest();
            }

            _context.Entry(detalleCompra).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DetalleCompraExists(id))
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

        // POST: api/DetalleCompras
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Policy = "PuedeGestionarDetallesCompra")]
        public async Task<ActionResult<DetalleCompra>> PostDetalleCompra(DetalleCompra detalleCompra)
        {
            // 1. Iniciar una transacción para asegurar que o se guarda todo o nada
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Buscar el producto para actualizar stock
                var producto = await _context.Productos.FindAsync(detalleCompra.ProductoId);
                if (producto == null)
                    return NotFound(new { mensaje = "El producto no existe." });

                // 3. Buscar la compra (cabecera) para actualizar el total
                var compra = await _context.Compras.FindAsync(detalleCompra.CompraId);
                if (compra == null)
                    return NotFound(new { mensaje = "La cabecera de compra no existe." });

                // 4. LÓGICA DE NEGOCIO
                // Sumamos al stock del producto
                producto.Stock += detalleCompra.Cantidad;

                // Sumamos al total de la compra: (Cantidad * PrecioCosto)
                compra.TotalCompra += (detalleCompra.Cantidad * detalleCompra.PrecioCosto);

                // 5. Guardar el detalle
                _context.DetallesCompra.Add(detalleCompra);

                // Guardar todos los cambios en la DB
                await _context.SaveChangesAsync();

                // Confirmar la transacción
                await transaction.CommitAsync();

                return CreatedAtAction("GetDetalleCompra", new { id = detalleCompra.Id }, detalleCompra);
            }
            catch (Exception ex)
            {
                // Si algo falla, se deshacen los cambios automáticamente
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // DELETE: api/DetalleCompras/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "PuedeGestionarDetallesCompra")]
        public async Task<IActionResult> DeleteDetalleCompra(int id)
        {
            // 1. Iniciar transacción
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Buscar el detalle que se desea eliminar
                var detalleCompra = await _context.DetallesCompra.FindAsync(id);
                if (detalleCompra == null)
                {
                    return NotFound();
                }

                // 3. Buscar Producto y Compra para revertir valores
                var producto = await _context.Productos.FindAsync(detalleCompra.ProductoId);
                var compra = await _context.Compras.FindAsync(detalleCompra.CompraId);

                // 4. LÓGICA DE REVERSIÓN
                if (producto != null)
                {
                    // Restamos del stock lo que se había "comprado" en este item
                    producto.Stock -= detalleCompra.Cantidad;
                }

                if (compra != null)
                {
                    // Restamos del total de la compra el subtotal de este item
                    compra.TotalCompra -= (detalleCompra.Cantidad * detalleCompra.PrecioCosto);

                    // Opcional: Asegurarse de que el total no sea negativo por errores de redondeo
                    if (compra.TotalCompra < 0) compra.TotalCompra = 0;
                }

                // 5. Eliminar el registro
                _context.DetallesCompra.Remove(detalleCompra);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al eliminar: {ex.Message}");
            }
        }

        private bool DetalleCompraExists(int id)
        {
            return _context.DetallesCompra.Any(e => e.Id == id);
        }
    }
}
