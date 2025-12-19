using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using pyfinal.Data;
using pyfinal.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace pyfinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController: ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        /*[HttpPost("registro")]
        public async Task<ActionResult<Usuario>> Registro(Usuario usuario)
        {
            // 1. Validar si el email ya existe
            if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
            {
                return BadRequest("El correo electrónico ya está registrado");
            }

            // 2. Hashear la contraseña (proviene de PasswordHash del JSON)
            // Usamos contrasena como nombre de variable corto
            string contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);

            // 3. Validar el Rol de Logística
            var rolesValidos = new[] { "Admin", "Vendedor", "Repartidor" };
            string rolFinal = usuario.Rol;

            if (string.IsNullOrWhiteSpace(rolFinal) || !rolesValidos.Contains(rolFinal))
            {
                rolFinal = "Vendedor"; // Rol por defecto
            }

            // 4. Crear el nuevo objeto para la base de datos
            var user = new Usuario
            {
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                PasswordHash = contrasena, // Guardamos el hash generado
                Rol = rolFinal
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Usuario registrado exitosamente" });
        }
        */

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto) 
        {
            // 2. Usas 'loginDto' (minúscula) para leer el email
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            // 3. Usas 'loginDto' (minúscula) para leer la contraseña
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.PasswordHash, user.PasswordHash))
            {
                return Unauthorized("Credenciales inválidas");
            }

            string token = GenerateJwtToken(user);

            return Ok(new
            {
                token = token,
                nombre = user.Nombre,
                rol = user.Rol,
                id = user.Id
            });
        }



        private string GenerateJwtToken(Usuario user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Rol),
        new Claim("Nombre", user.Nombre)
    };

            // --- Módulo de Proveedores ---
            bool puedeCrearProveedores = false;
            bool puedeEditarProveedores = false;
            bool puedeEliminarProveedores = false;
            bool puedeVisualizarProveedores = false;


            // --- Módulo de Productos y Categorias---
            bool puedeGestionarCategorias = false; //Crear/Editar/Eliminar categorías
            bool puedeCrearProductos = false;
            bool puedeEditarProductos = false;
            bool puedeEliminarProductos = false;
            bool puedeVisualizarProductos = false;

            // --- Módulo de Pedidos Y DetallesP---
            bool puedeCrearPedidos = false;
            bool puedeEditarPedidos = false;
            bool puedeEliminarPedidos = false;
            bool puedeVisualizarPedidos = false;
            bool puedeGestionarDetallesPedido = false; //Crear/Editar/Eliminar detalles de pedido

            //---Modulo de Compras y DetallesC---
            bool puedeCrearCompras = false;
            bool puedeEditarCompras = false;
            bool puedeEliminarCompras = false;
            bool puedeVisualizarCompras = false;
            bool puedeGestionarDetallesCompra = false; //Crear/Editar/Eliminar detalles de compra

            // --- Módulo de Envíos (Específico) ---
            bool puedeCrearEnvios = false;
            bool puedeEditarEnvios = false;
            bool puedeEliminarEnvios = false;
            bool puedeVisualizarEnvios = false;
            bool puedeActualizarEstadoEnvio = false; // Permiso especial para el Repartidor

            // --- NUEVO: Módulo de Gestión de Usuarios (RRHH) ---
            bool puedeCrearUsuarios = false;
            bool puedeEditarUsuarios = false;
            bool puedeEliminarUsuarios = false;
            bool puedeVisualizarUsuarios = false;

            switch (user.Rol)
            {
                case "Admin":
                    // El administrador tiene control total sobre todos los módulos
                    puedeCrearProveedores = puedeEditarProveedores = puedeEliminarProveedores = puedeVisualizarProveedores = true;

                    puedeGestionarCategorias = true;
                    puedeCrearProductos = puedeEditarProductos = puedeEliminarProductos = puedeVisualizarProductos = true;

                    puedeCrearPedidos = puedeEditarPedidos = puedeEliminarPedidos = puedeVisualizarPedidos = true;
                    puedeGestionarDetallesPedido = true;

                    puedeCrearCompras = puedeEditarCompras = puedeEliminarCompras = puedeVisualizarCompras = true;
                    puedeGestionarDetallesCompra = true;

                    puedeCrearEnvios = puedeEditarEnvios = puedeEliminarEnvios = puedeVisualizarEnvios = true;
                    puedeActualizarEstadoEnvio = true;

                    // Permisos de Usuario para el Admin
                    puedeCrearUsuarios = true;
                    puedeEditarUsuarios = true;
                    puedeEliminarUsuarios = true;
                    puedeVisualizarUsuarios = true;

                    break;

                case "Vendedor":
                    // El vendedor se enfoca en clientes y ventas (Pedidos)
                    puedeVisualizarProveedores = true;

                    puedeVisualizarProductos = true; // Para consultar stock
                    puedeGestionarCategorias = false; // No crea categorías

                    puedeCrearPedidos = true;
                    puedeEditarPedidos = true;
                    puedeVisualizarPedidos = true;
                    puedeGestionarDetallesPedido = true; // Para agregar productos al carrito

                    puedeVisualizarCompras = false; // No ve costos de proveedores
                    puedeVisualizarEnvios = true;   // Para dar seguimiento al cliente
                    break;

                case "Repartidor":
                    // El repartidor solo ve lo que tiene que entregar y cambia el estado
                    puedeVisualizarProductos = false;
                    puedeVisualizarProveedores = false;
                    puedeVisualizarPedidos = true; // Para ver qué contiene el paquete

                    puedeCrearEnvios = true;
                    puedeEditarEnvios = true;
                    puedeVisualizarEnvios = true;
                    puedeActualizarEstadoEnvio = true; // Este es su permiso principal para el Historial

                    // El resto se mantiene en false por defecto
                    break;
            }

            // Usuarios
            claims.Add(new Claim("PuedeCrearUsuarios", puedeCrearUsuarios.ToString()));
            claims.Add(new Claim("PuedeEditarUsuarios", puedeEditarUsuarios.ToString()));
            claims.Add(new Claim("PuedeEliminarUsuarios", puedeEliminarUsuarios.ToString()));
            claims.Add(new Claim("PuedeVisualizarUsuarios", puedeVisualizarUsuarios.ToString()));

            claims.Add(new Claim("PuedeCrearProveedores", puedeCrearProveedores.ToString()));
            claims.Add(new Claim("PuedeEditarProveedores", puedeEditarProveedores.ToString()));
            claims.Add(new Claim("PuedeEliminarProveedores", puedeEliminarProveedores.ToString()));
            claims.Add(new Claim("PuedeVisualizarProveedores", puedeVisualizarProveedores.ToString()));

            claims.Add(new Claim("PuedeGestionarCategorias", puedeGestionarCategorias.ToString()));
            claims.Add(new Claim("PuedeCrearProductos", puedeCrearProductos.ToString()));
            claims.Add(new Claim("PuedeEditarProductos", puedeEditarProductos.ToString()));
            claims.Add(new Claim("PuedeEliminarProductos", puedeEliminarProductos.ToString()));
            claims.Add(new Claim("PuedeVisualizarProductos", puedeVisualizarProductos.ToString()));

            claims.Add(new Claim("PuedeCrearPedidos", puedeCrearPedidos.ToString()));
            claims.Add(new Claim("PuedeEditarPedidos", puedeEditarPedidos.ToString()));
            claims.Add(new Claim("PuedeEliminarPedidos", puedeEliminarPedidos.ToString()));
            claims.Add(new Claim("PuedeVisualizarPedidos", puedeVisualizarPedidos.ToString()));
            claims.Add(new Claim("PuedeGestionarDetallesPedido", puedeGestionarDetallesPedido.ToString()));

            claims.Add(new Claim("PuedeCrearCompras", puedeCrearCompras.ToString()));
            claims.Add(new Claim("PuedeEditarCompras", puedeEditarCompras.ToString()));
            claims.Add(new Claim("PuedeEliminarCompras", puedeEliminarCompras.ToString()));
            claims.Add(new Claim("PuedeVisualizarCompras", puedeVisualizarCompras.ToString()));
            claims.Add(new Claim("PuedeGestionarDetallesCompra", puedeGestionarDetallesCompra.ToString()));

            claims.Add(new Claim("PuedeCrearEnvios", puedeCrearEnvios.ToString()));
            claims.Add(new Claim("PuedeEditarEnvios", puedeEditarEnvios.ToString()));
            claims.Add(new Claim("PuedeEliminarEnvios", puedeEliminarEnvios.ToString()));
            claims.Add(new Claim("PuedeVisualizarEnvios", puedeVisualizarEnvios.ToString()));
            claims.Add(new Claim("PuedeActualizarEstadoEnvio", puedeActualizarEstadoEnvio.ToString()));




            // Configuración del Token (Igual a tu taller)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
