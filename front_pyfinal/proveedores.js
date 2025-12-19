const API_URL = "https://localhost:7252/api"; 
const token = localStorage.getItem('token');

if (!token) window.location.href = 'index.html';

document.getElementById('usuarioNombre').innerText = localStorage.getItem('usuario') || "Usuario";

document.getElementById('btnCerrarSesion').onclick = () => {
    localStorage.clear();
    window.location.href = 'index.html';
};

window.onload = cargarProveedores;

// --- 1. CARGAR TABLA ---
async function cargarProveedores() {
    const res = await fetch(`${API_URL}/Proveedores`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });
    const provs = await res.json();
    const tbody = document.getElementById('tablaProveedores');
    tbody.innerHTML = '';

    provs.forEach(p => {
        tbody.innerHTML += `
            <tr>
                <td>${p.razonSocial}</td>
                <td>${p.ruc}</td>
                <td>${p.telefono}</td>
                <td>
                    <button class="btn btn-sm btn-warning" onclick="abrirEditar(${p.id})">Editar</button>
                    <button class="btn btn-sm btn-danger" onclick="eliminarProveedor(${p.id})">Eliminar</button>
                </td>
            </tr>`;
    });
}

// --- 2. PREPARAR CREACIÓN (LIMPIAR MODAL) ---
function prepararCreacion() {
    document.getElementById('provId').value = "";
    document.getElementById('formProveedor').reset();
    document.getElementById('tituloModal').innerText = "Nuevo Proveedor";
}

// --- 3. ABRIR EDITAR (LLENAR MODAL CON DATOS) ---
async function abrirEditar(id) {
    try {
        const res = await fetch(`${API_URL}/Proveedores/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!res.ok) throw new Error("No se pudo obtener el proveedor");
        
        const p = await res.json();

        // Llenamos los campos del formulario con los datos de la base de datos
        document.getElementById('provId').value = p.id;
        document.getElementById('provRazonSocial').value = p.razonSocial;
        document.getElementById('provRUC').value = p.ruc;
        document.getElementById('provTelefono').value = p.telefono;

        document.getElementById('tituloModal').innerText = "Editar Proveedor";

        // Mostramos el modal
        const modalElement = document.getElementById('modalProveedor');
        const modalInstance = new bootstrap.Modal(modalElement);
        modalInstance.show();

    } catch (error) {
        console.error(error);
        alert("Error al cargar los datos del proveedor");
    }
}

// --- 4. GUARDAR CAMBIOS (POST o PUT) ---
document.getElementById('formProveedor').onsubmit = async (e) => {
    e.preventDefault();
    const id = document.getElementById('provId').value;
    
    const proveedor = {
        id: id ? parseInt(id) : 0,
        razonSocial: document.getElementById('provRazonSocial').value,
        ruc: document.getElementById('provRUC').value,
        telefono: document.getElementById('provTelefono').value
    };

    const metodo = id ? 'PUT' : 'POST';
    const url = id ? `${API_URL}/Proveedores/${id}` : `${API_URL}/Proveedores`;

    try {
        const res = await fetch(url, {
            method: metodo,
            headers: { 
                'Content-Type': 'application/json', 
                'Authorization': `Bearer ${token}` 
            },
            body: JSON.stringify(proveedor)
        });

        if (res.ok || res.status === 204) {
            alert("Proveedor guardado correctamente");
            // Cerrar el modal correctamente
            const modalElement = document.getElementById('modalProveedor');
            const modalInstance = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);
            modalInstance.hide();
            
            cargarProveedores();
        } else {
            alert("Error al guardar el proveedor");
        }
    } catch (error) {
        console.error(error);
        alert("Error de conexión");
    }
};

// --- 5. ELIMINAR ---
async function eliminarProveedor(id) {
    if (confirm("¿Está seguro de eliminar este proveedor?")) {
        await fetch(`${API_URL}/Proveedores/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });
        cargarProveedores();
    }
}