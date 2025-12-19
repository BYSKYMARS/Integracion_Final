const API_URL = "https://localhost:7252/api"; 
const token = localStorage.getItem('token');

// Variable global para almacenar productos y sus precios
let productosCatalogo = [];

if (!token) window.location.href = 'index.html';

document.getElementById('usuarioNombre').innerText = localStorage.getItem('usuario') || "Usuario";

const btnLogout = document.getElementById('btnCerrarSesion');
if (btnLogout) {
    btnLogout.onclick = () => {
        localStorage.clear();
        window.location.href = 'index.html';
    };
}

window.onload = () => {
    cargarCompras();
    cargarProductos();
    cargarDetalles();
};

// --- 1. CARGAR COMPRAS (CABECERAS) ---
async function cargarCompras() {
    const res = await fetch(`${API_URL}/Compras`, { headers: { 'Authorization': `Bearer ${token}` } });
    const data = await res.json();
    const select = document.getElementById('detCompraId');
    select.innerHTML = '<option value="">Seleccione la Compra</option>';
    data.forEach(c => select.innerHTML += `<option value="${c.id}">Compra #${c.id} - Total actual: S/ ${c.totalCompra}</option>`);
}

// --- 2. CARGAR PRODUCTOS Y GUARDARLOS LOCALMENTE ---
async function cargarProductos() {
    const res = await fetch(`${API_URL}/Productos`, { headers: { 'Authorization': `Bearer ${token}` } });
    productosCatalogo = await res.json(); 
    
    const select = document.getElementById('detProductoId');
    select.innerHTML = '<option value="">Seleccione Producto para Abastecer</option>';
    productosCatalogo.forEach(p => {
        select.innerHTML += `<option value="${p.id}">${p.nombre} (Stock: ${p.stock})</option>`;
    });
}

// --- 3. LOGICA DE PRECIO AUTOMÁTICO (EVENTO CHANGE) ---
document.getElementById('detProductoId').addEventListener('change', (e) => {
    const productoId = e.target.value;
    // Buscamos el producto en nuestra variable global
    const productoMatch = productosCatalogo.find(p => p.id == productoId);
    
    const inputCosto = document.getElementById('detCosto');
    if (productoMatch) {
        // Usamos el precio registrado en productos como costo de entrada
        inputCosto.value = productoMatch.precioVenta; 
    } else {
        inputCosto.value = "";
    }
});

// --- 4. CARGAR TABLA DE DETALLES ---
async function cargarDetalles() {
    const res = await fetch(`${API_URL}/DetalleCompras`, { headers: { 'Authorization': `Bearer ${token}` } });
    const detalles = await res.json();
    const tbody = document.getElementById('tablaDetalles');
    tbody.innerHTML = '';

    detalles.forEach(d => {
        tbody.innerHTML += `
            <tr>
                <td>Compra #${d.compraId}</td>
                <td>ID Producto: ${d.productoId}</td>
                <td>${d.cantidad}</td>
                <td>S/ ${d.precioCosto.toFixed(2)}</td>
                <td>S/ ${(d.cantidad * d.precioCosto).toFixed(2)}</td>
                <td>
                    <button class="btn btn-sm btn-danger" onclick="eliminarDetalle(${d.id})">🗑️</button>
                </td>
            </tr>`;
    });
}

// --- 5. GUARDAR DETALLE ---
document.getElementById('formDetalle').onsubmit = async (e) => {
    e.preventDefault();
    const detalle = {
        cantidad: parseInt(document.getElementById('detCantidad').value),
        precioCosto: parseFloat(document.getElementById('detCosto').value),
        compraId: parseInt(document.getElementById('detCompraId').value),
        productoId: parseInt(document.getElementById('detProductoId').value)
    };

    const res = await fetch(`${API_URL}/DetalleCompras`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
        body: JSON.stringify(detalle)
    });

    if (res.ok) {
        alert("Item agregado. El stock y el total de la compra se han actualizado.");
        bootstrap.Modal.getInstance(document.getElementById('modalDetalle')).hide();
        // Limpiar el formulario para la próxima entrada
        document.getElementById('formDetalle').reset();
        cargarDetalles();
        cargarCompras(); // Recargamos compras para ver el nuevo total en el select
    } else {
        alert("Error al guardar el detalle.");
    }
};

async function eliminarDetalle(id) {
    if (confirm("¿Eliminar este item de la compra?")) {
        await fetch(`${API_URL}/DetalleCompras/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });
        cargarDetalles();
    }
}