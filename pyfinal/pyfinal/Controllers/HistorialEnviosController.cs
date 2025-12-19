using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class HistorialEnviosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HistorialEnviosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/HistorialEnvios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HistorialEnvio>>> GetHistorialesEnvio()
        {
            return await _context.HistorialesEnvio.ToListAsync();
        }

        // GET: api/HistorialEnvios/5

        [HttpGet("{id}")]
        [Authorize(Policy = "PuedeVisualizarEnvios")]
        public async Task<ActionResult<HistorialEnvio>> GetHistorialEnvio(int id)
        {
            var historialEnvio = await _context.HistorialesEnvio.FindAsync(id);

            if (historialEnvio == null)
            {
                return NotFound();
            }

            return historialEnvio;
        }

        // PUT: api/HistorialEnvios/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Policy = "PuedeVisualizarEnvios")]
        public async Task<IActionResult> PutHistorialEnvio(int id, HistorialEnvio historialEnvio)
        {
            if (id != historialEnvio.Id)
            {
                return BadRequest();
            }

            _context.Entry(historialEnvio).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HistorialEnvioExists(id))
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

        // POST: api/HistorialEnvios
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Policy = "PuedeActualizarEstadoEnvio")]
        public async Task<ActionResult<HistorialEnvio>> PostHistorialEnvio(HistorialEnvio historialEnvio)
        {
            _context.HistorialesEnvio.Add(historialEnvio);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHistorialEnvio", new { id = historialEnvio.Id }, historialEnvio);
        }

        // DELETE: api/HistorialEnvios/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHistorialEnvio(int id)
        {
            var historialEnvio = await _context.HistorialesEnvio.FindAsync(id);
            if (historialEnvio == null)
            {
                return NotFound();
            }

            _context.HistorialesEnvio.Remove(historialEnvio);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HistorialEnvioExists(int id)
        {
            return _context.HistorialesEnvio.Any(e => e.Id == id);
        }
    }
}
