// AQUI TAMBIEN EL PUERTO 7252
const API_URL = "https://integracionfinal-production.up.railway.app/api"; 

// 1. VERIFICAR SEGURIDAD
const token = localStorage.getItem('token');

if (!token) {
    window.location.href = 'index.html'; // Si no hay token, fuera
}

document.getElementById('usuarioNombre').innerText = localStorage.getItem('usuario') || "Usuario";

// 2. CERRAR SESIÓN
document.getElementById('btnCerrarSesion').addEventListener('click', () => {
    localStorage.clear();
    window.location.href = 'index.html';
});

// 3. CARGAR USUARIOS
async function cargarUsuarios() {
    try {
        // Petición GET al puerto 7252
        const response = await fetch(`${API_URL}/Usuarios`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}` // <--- IMPORTANTE: El Token viaja aquí
            }
        });

        if (response.status === 401) {
            alert("Tu sesión ha expirado.");
            localStorage.clear();
            window.location.href = 'index.html';
            return;
        }

        if (response.ok) {
            const usuarios = await response.json();
            renderizarTabla(usuarios);
        } else {
            console.error("Error al obtener usuarios:", response.statusText);
        }

    } catch (error) {
        console.error("Error cargando usuarios:", error);
    }
}

function renderizarTabla(usuarios) {
    const tbody = document.getElementById('tablaUsuarios');
    tbody.innerHTML = ''; 

    usuarios.forEach(u => {
        // TRUCO: Agregamos onclick="eliminarUsuario(ID)"
        const fila = `
            <tr>
                <td>${u.id}</td>
                <td>${u.nombre}</td>
                <td>${u.email}</td>
                <td>${u.rol}</td>
                <td>
                    <button class="btn btn-sm btn-warning" onclick="abrirModalEditar(${u.id})">Editar</button>
                    <button class="btn btn-sm btn-danger" onclick="eliminarUsuario(${u.id})">Eliminar</button>
                </td>
            </tr>
        `;
        tbody.innerHTML += fila;
    });
}

cargarUsuarios();

// --- LÓGICA PARA CREAR USUARIO ---

const formRegistro = document.getElementById('formRegistro');

formRegistro.addEventListener('submit', async (e) => {
    e.preventDefault(); // Evita recargar la página

    // 1. Capturar datos del formulario
    const nuevoUsuario = {
        nombre: document.getElementById('regNombre').value,
        email: document.getElementById('regEmail').value,
        passwordHash: document.getElementById('regPassword').value, // Tu API lo hasheará
        rol: document.getElementById('regRol').value
    };

    try {
        // 2. Enviar petición POST protegida
        const response = await fetch(`${API_URL}/Usuarios`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}` // <--- ¡Sin esto te daría 401!
            },
            body: JSON.stringify(nuevoUsuario)
        });

        // 3. Verificar resultado
        if (response.ok) {
            alert("Usuario creado exitosamente");
            
            // Cerrar el modal (truco de Bootstrap)
            const modalElement = document.getElementById('modalRegistro');
            const modalInstance = bootstrap.Modal.getInstance(modalElement);
            modalInstance.hide();
            
            // Limpiar formulario
            formRegistro.reset();

            // Recargar la tabla para ver al nuevo
            cargarUsuarios(); 
        } else {
            const errorData = await response.json();
            alert("Error: " + JSON.stringify(errorData));
        }

    } catch (error) {
        console.error("Error:", error);
        alert("Error de conexión al crear usuario.");
    }
});

// --- LÓGICA PARA ELIMINAR USUARIO ---
async function eliminarUsuario(id) {
    // 1. Confirmación de seguridad
    if (!confirm(`¿Estás seguro de que deseas eliminar al usuario con ID ${id}?`)) {
        return; // Si dice que no, no hacemos nada
    }

    try {
        // 2. Enviar petición DELETE
        const response = await fetch(`${API_URL}/Usuarios/${id}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}` // ¡Siempre el token!
            }
        });

        // 3. Verificar resultado
        if (response.ok) {
            alert("Usuario eliminado correctamente.");
            cargarUsuarios(); // Recargamos la tabla para que desaparezca
        } else {
            alert("No se pudo eliminar. Verifica que tengas permisos o que el ID exista.");
        }

    } catch (error) {
        console.error("Error:", error);
        alert("Error de conexión al eliminar.");
    }
}

// --- LÓGICA PARA EDITAR USUARIO ---

// 1. Función que llama el botón "Editar" de la tabla
async function abrirModalEditar(id) {
    try {
        // Pedimos los datos actuales del usuario al backend
        const response = await fetch(`${API_URL}/Usuarios/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (!response.ok) throw new Error("No se pudo obtener el usuario");

        const u = await response.json();

        // Llenamos el modal con sus datos
        document.getElementById('editId').value = u.id; // Guardamos el ID
        document.getElementById('editNombre').value = u.nombre;
        document.getElementById('editEmail').value = u.email;
        document.getElementById('editRol').value = u.rol;

        document.getElementById('editPassword').value = "";

        // Abrimos el modal usando Bootstrap
        const modalElement = document.getElementById('modalEditar');
        const modalInstance = new bootstrap.Modal(modalElement);
        modalInstance.show();

    } catch (error) {
        console.error(error);
        alert("Error al cargar datos del usuario.");
    }
}

// 2. Evento para Guardar los cambios (PUT)
document.getElementById('formEditar').addEventListener('submit', async (e) => {
    e.preventDefault();

    const id = document.getElementById('editId').value;
    const nuevaPassword = document.getElementById('editPassword').value; // Leer valor

    const usuarioEditado = {
        id: parseInt(id), // El ID debe ser número
        nombre: document.getElementById('editNombre').value,
        email: document.getElementById('editEmail').value,
        rol: document.getElementById('editRol').value
        // No enviamos passwordHash para que el backend mantenga la vieja
    };

    if (nuevaPassword) {
        usuarioEditado.passwordHash = nuevaPassword;
    }

    try {
        const response = await fetch(`${API_URL}/Usuarios/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(usuarioEditado)
        });

        if (response.ok || response.status === 204) {
            alert("Usuario actualizado correctamente");
            
            // Cerrar modal y recargar
            const modalElement = document.getElementById('modalEditar');
            const modalInstance = bootstrap.Modal.getInstance(modalElement); 
            modalInstance.hide();
            
            cargarUsuarios(); // Refrescar la tabla
        } else {
            alert("Error al actualizar. Verifica los datos.");
        }

    } catch (error) {
        console.error(error);
        alert("Error de conexión al actualizar.");
    }
});