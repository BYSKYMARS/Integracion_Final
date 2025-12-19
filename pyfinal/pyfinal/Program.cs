using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using pyfinal.Data;

namespace pyfinal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //base de datos
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        

            // Configuración de CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("PermitirTodo",
                    policy => policy.AllowAnyOrigin()
                                    .AllowAnyMethod()
                                    .AllowAnyHeader());
            });

            // Configuración de Autenticación JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        //Valida el emisor, asegura que el token proviene de un emisor confiable
                        ValidateIssuer = true,
                        //Valida la audiencia, el token fue generado para esta aplicación y no para otra
                        ValidateAudience = true,
                        //Valida el tiempo de vida del token
                        ValidateLifetime = true,
                        //valida la firma del emisor
                        ValidateIssuerSigningKey = true,
                        //define quién es el emisor
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        //define quién será la audiencia
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        //Se proporciona la clave secreta que se va a usar para validar la firma
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });


            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "LOGÍSTICA B2B API",
                    Version = "v1",
                    Description = "API REST para gestión de suministros y envíos con JWT"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Ingresa tu token JWT aquí. Formato: Bearer {token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();


            builder.Services.AddAuthorization(options =>
            {

                // --- POLÍTICAS DE USUARIOS (RRHH) ---
                options.AddPolicy("PuedeCrearUsuarios", policy => policy.RequireClaim("PuedeCrearUsuarios", "True"));
                options.AddPolicy("PuedeEditarUsuarios", policy => policy.RequireClaim("PuedeEditarUsuarios", "True"));
                options.AddPolicy("PuedeEliminarUsuarios", policy => policy.RequireClaim("PuedeEliminarUsuarios", "True"));
                options.AddPolicy("PuedeVisualizarUsuarios", policy => policy.RequireClaim("PuedeVisualizarUsuarios", "True"));

                // --- POLÍTICAS DE CATEGORÍAS ---
                options.AddPolicy("PuedeGestionarCategorias", policy =>
                    policy.RequireClaim("PuedeGestionarCategorias", "True"));

                // --- POLÍTICAS DE PRODUCTOS ---
                options.AddPolicy("PuedeCrearProductos", policy => policy.RequireClaim("PuedeCrearProductos", "True"));
                options.AddPolicy("PuedeEditarProductos", policy => policy.RequireClaim("PuedeEditarProductos", "True"));
                options.AddPolicy("PuedeEliminarProductos", policy => policy.RequireClaim("PuedeEliminarProductos", "True"));
                options.AddPolicy("PuedeVisualizarProductos", policy => policy.RequireClaim("PuedeVisualizarProductos", "True"));

                // --- POLÍTICAS DE PEDIDOS (VENTAS) ---
                options.AddPolicy("PuedeCrearPedidos", policy => policy.RequireClaim("PuedeCrearPedidos", "True"));
                options.AddPolicy("PuedeEditarPedidos", policy => policy.RequireClaim("PuedeEditarPedidos", "True"));
                options.AddPolicy("PuedeEliminarPedidos", policy => policy.RequireClaim("PuedeEliminarPedidos", "True"));
                options.AddPolicy("PuedeVisualizarPedidos", policy => policy.RequireClaim("PuedeVisualizarPedidos", "True"));
                options.AddPolicy("PuedeGestionarDetallesPedido", policy => policy.RequireClaim("PuedeGestionarDetallesPedido", "True"));

                // --- POLÍTICAS DE COMPRAS (ABASTECIMIENTO) ---
                options.AddPolicy("PuedeCrearCompras", policy => policy.RequireClaim("PuedeCrearCompras", "True"));
                options.AddPolicy("PuedeEditarCompras", policy => policy.RequireClaim("PuedeEditarCompras", "True"));
                options.AddPolicy("PuedeEliminarCompras", policy => policy.RequireClaim("PuedeEliminarCompras", "True"));
                options.AddPolicy("PuedeVisualizarCompras", policy => policy.RequireClaim("PuedeVisualizarCompras", "True"));
                options.AddPolicy("PuedeGestionarDetallesCompra", policy => policy.RequireClaim("PuedeGestionarDetallesCompra", "True"));

                // --- POLÍTICAS DE ENVÍOS ---
                options.AddPolicy("PuedeCrearEnvios", policy => policy.RequireClaim("PuedeCrearEnvios", "True"));
                options.AddPolicy("PuedeEditarEnvios", policy => policy.RequireClaim("PuedeEditarEnvios", "True"));
                options.AddPolicy("PuedeEliminarEnvios", policy => policy.RequireClaim("PuedeEliminarEnvios", "True"));
                options.AddPolicy("PuedeVisualizarEnvios", policy => policy.RequireClaim("PuedeVisualizarEnvios", "True"));
                options.AddPolicy("PuedeActualizarEstadoEnvio", policy => policy.RequireClaim("PuedeActualizarEstadoEnvio", "True"));

                // --- POLÍTICAS DE PROVEEDORES ---
                options.AddPolicy("PuedeCrearProveedores", policy => policy.RequireClaim("PuedeCrearProveedores", "True"));
                options.AddPolicy("PuedeEditarProveedores", policy => policy.RequireClaim("PuedeEditarProveedores", "True"));
                options.AddPolicy("PuedeEliminarProveedores", policy => policy.RequireClaim("PuedeEliminarProveedores", "True"));
                options.AddPolicy("PuedeVisualizarProveedores", policy => policy.RequireClaim("PuedeVisualizarProveedores", "True"));
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();


            app.UseAuthentication();
            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    // Llama a la clase que acabamos de crear
                    DbInitializer.Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Ocurrió un error al sembrar la base de datos.");
                }
            }

            app.UseCors("PermitirTodo");

            app.MapControllers();

            app.Run();
        }
    }
}
