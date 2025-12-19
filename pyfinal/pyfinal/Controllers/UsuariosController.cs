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
using BCrypt.Net;

namespace pyfinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Candado General
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet]
        [Authorize(Policy = "PuedeVisualizarUsuarios")]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        [Authorize(Policy = "PuedeVisualizarUsuarios")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        [Authorize(Policy = "PuedeEditarUsuarios")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest("El ID no coincide");
            }

            var usuarioExistente = await _context.Usuarios.FindAsync(id);
            if (usuarioExistente == null)
            {
                return NotFound();
            }

            // Actualizar datos
            usuarioExistente.Nombre = usuario.Nombre;
            usuarioExistente.Email = usuario.Email;
            usuarioExistente.Rol = usuario.Rol;

            // Solo actualizamos la contraseña si el admin escribió una nueva
            if (!string.IsNullOrWhiteSpace(usuario.PasswordHash) && usuario.PasswordHash.Length >= 6)
            {
                // Evitamos doble hasheo
                if (!usuario.PasswordHash.StartsWith("$2a$"))
                {
                    usuarioExistente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // -----------------------------------------------------------
        // CAMBIO AQUÍ: Ahora el método se llama "RegistrarUsuario"
        // -----------------------------------------------------------
        // POST: api/Usuarios
        [HttpPost] // La URL sigue siendo api/Usuarios (Estándar REST)
        [Authorize(Policy = "PuedeCrearUsuarios")]
        public async Task<ActionResult<Usuario>> RegistrarUsuario(Usuario usuario)
        {
            // 1. Validar duplicados
            if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
            {
                return BadRequest("El correo electrónico ya está registrado.");
            }

            // 2. Validar contraseña
            if (string.IsNullOrEmpty(usuario.PasswordHash) || usuario.PasswordHash.Length < 6)
            {
                return BadRequest("La contraseña es obligatoria y debe tener al menos 6 caracteres.");
            }

            // 3. Hashear
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);

            // 4. Validar Rol
            var rolesValidos = new[] { "Admin", "Vendedor", "Repartidor" };
            if (!rolesValidos.Contains(usuario.Rol))
            {
                usuario.Rol = "Vendedor";
            }

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.Id }, usuario);
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "PuedeEliminarUsuarios")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}