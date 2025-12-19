const API_URL = "https://localhost:7252/api"; // TU PUERTO
const token = localStorage.getItem('token');

// Mapas para guardar los nombres (ID -> Nombre)
let mapaCategorias = {};
let mapaProveedores = {};

// 1. SEGURIDAD
if (!token) {
    window.location.href = 'index.html';
}

document.getElementById('btnCerrarSesion').addEventListener('click', () => {
    localStorage.clear();
    window.location.href = 'index.html';
});

// 2. INICIALIZACIÓN
document.addEventListener("DOMContentLoaded", async () => {
    // Primero cargamos las listas auxiliares (Categorias y Proveedores)
    await cargarListasAuxiliares();
    // Luego cargamos los productos
    cargarProductos();
});

// 3. CARGAR CATEGORÍAS Y PROVEEDORES
async function cargarListasAuxiliares() {
    try {
        // --- Categorías ---
        const resCat = await fetch(`${API_URL}/Categorias`, { headers: { 'Authorization': `Bearer ${token}` } });
        if (resCat.ok) {
            const categorias = await resCat.json();
            const select = document.getElementById('prodCategoria');
            select.innerHTML = '<option value="">Seleccione Categoría</option>';
            
            categorias.forEach(c => {
                // Llenamos el Select
                select.innerHTML += `<option value="${c.id}">${c.nombre}</option>`;
                // Guardamos en el mapa para usarlo en la tabla luego
                mapaCategorias[c.id] = c.nombre;
            });
        }

        // --- Proveedores ---
        const resProv = await fetch(`${API_URL}/Proveedores`, { headers: { 'Authorization': `Bearer ${token}` } });
        if (resProv.ok) {
            const proveedores = await resProv.json();
            const select = document.getElementById('prodProveedor');
            select.innerHTML = '<option value="">Seleccione Proveedor</option>';

            proveedores.forEach(p => {
                // Llenamos el Select
                select.innerHTML += `<option value="${p.id}">${p.razonSocial}</option>`;
                // Guardamos en el mapa
                mapaProveedores[p.id] = p.razonSocial;
            });
        }

    } catch (error) {
        console.error("Error cargando listas:", error);
    }
}

// 4. CRUD PRODUCTOS
async function cargarProductos() {
    try {
        const response = await fetch(`${API_URL}/Productos`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        if (!response.ok) throw new Error("Error al obtener productos");
        
        const productos = await response.json();
        const tbody = document.getElementById('tablaProductos');
        tbody.innerHTML = '';

        productos.forEach(p => {
            // Usamos los mapas para obtener el nombre, o mostramos "Sin datos"
            const nombreCat = mapaCategorias[p.categoriaId] || '<span class="text-muted">Desconocido</span>';
            const nombreProv = mapaProveedores[p.proveedorId] || '<span class="text-muted">Desconocido</span>';

            tbody.innerHTML += `
                <tr>
                    <td>${p.codigo}</td>
                    <td><strong>${p.nombre}</strong></td>
                    <td><span class="badge bg-info text-dark">${nombreCat}</span></td>
                    <td>${nombreProv}</td>
                    <td>S/ ${p.precioVenta.toFixed(2)}</td>
                    <td>${p.stock}</td>
                    <td>
                        <button class="btn btn-sm btn-warning" onclick="editarProducto(${p.id})">✏️</button>
                        <button class="btn btn-sm btn-danger" onclick="eliminarProducto(${p.id})">🗑️</button>
                    </td>
                </tr>
            `;
        });
    } catch (error) {
        console.error(error);
        document.getElementById('tablaProductos').innerHTML = '<tr><td colspan="7" class="text-danger text-center">Error al cargar datos. Revisa la consola.</td></tr>';
    }
}

// --- MODAL Y FORMULARIO ---
let esEdicion = false;
const modal = new bootstrap.Modal(document.getElementById('modalProducto'));

function abrirModalCrear() {
    esEdicion = false;
    document.getElementById('formProducto').reset();
    document.getElementById('tituloModal').innerText = "Nuevo Producto";
    document.getElementById('prodId').value = "";
    modal.show();
}

window.editarProducto = async (id) => {
    esEdicion = true;
    document.getElementById('tituloModal').innerText = "Editar Producto";

    // Pedimos el producto individual a la API
    const res = await fetch(`${API_URL}/Productos/${id}`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });
    const p = await res.json();

    // Llenamos el formulario
    document.getElementById('prodId').value = p.id;
    document.getElementById('prodCodigo').value = p.codigo;
    document.getElementById('prodNombre').value = p.nombre;
    document.getElementById('prodPrecio').value = p.precioVenta;
    document.getElementById('prodStock').value = p.stock;
    document.getElementById('prodCategoria').value = p.categoriaId;
    document.getElementById('prodProveedor').value = p.proveedorId;

    modal.show();
};

document.getElementById('formProducto').addEventListener('submit', async (e) => {
    e.preventDefault();

    const id = document.getElementById('prodId').value;
    
    const producto = {
        id: id ? parseInt(id) : 0,
        codigo: document.getElementById('prodCodigo').value,
        nombre: document.getElementById('prodNombre').value,
        precioVenta: parseFloat(document.getElementById('prodPrecio').value),
        stock: parseInt(document.getElementById('prodStock').value),
        categoriaId: parseInt(document.getElementById('prodCategoria').value),
        proveedorId: parseInt(document.getElementById('prodProveedor').value)
    };

    const metodo = esEdicion ? 'PUT' : 'POST';
    const url = esEdicion ? `${API_URL}/Productos/${id}` : `${API_URL}/Productos`;

    try {
        const res = await fetch(url, {
            method: metodo,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(producto)
        });

        if (res.ok || res.status === 204) {
            alert("Operación exitosa");
            modal.hide();
            cargarProductos();
        } else {
            const error = await res.json();
            alert("Error: " + JSON.stringify(error));
        }
    } catch (err) {
        console.error(err);
        alert("Error de conexión");
    }
});

// --- ELIMINAR ---
window.eliminarProducto = async (id) => {
    if (!confirm("¿Estás seguro de eliminar este producto?")) return;

    try {
        const res = await fetch(`${API_URL}/Productos/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (res.ok) {
            cargarProductos();
        } else {
            alert("No se pudo eliminar (verifica permisos o dependencias)");
        }
    } catch (error) {
        console.error(error);
    }
};