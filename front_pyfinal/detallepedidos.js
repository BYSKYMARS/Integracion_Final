const API_URL = "https://integracionfinal-production.up.railway.app/api"; 
const token = localStorage.getItem('token');

if (!token) window.location.href = 'index.html';

document.getElementById('usuarioNombre').innerText = localStorage.getItem('usuario') || "Usuario";

document.getElementById('btnCerrarSesion').onclick = () => {
    localStorage.clear();
    window.location.href = 'index.html';
};

window.onload = () => {
    cargarPedidos();
    cargarProductos();
    cargarDetalles();
};

async function cargarPedidos() {
    const res = await fetch(`${API_URL}/Pedidos`, { headers: { 'Authorization': `Bearer ${token}` } });
    const data = await res.json();
    const select = document.getElementById('detPedId');
    select.innerHTML = '<option value="">Seleccione el Pedido</option>';
    data.forEach(p => select.innerHTML += `<option value="${p.id}">Pedido #${p.id} - Total: ${p.total}</option>`);
}

async function cargarProductos() {
    const res = await fetch(`${API_URL}/Productos`, { headers: { 'Authorization': `Bearer ${token}` } });
    const data = await res.json();
    const select = document.getElementById('detPedProductoId');
    select.innerHTML = '<option value="">Seleccione Producto</option>';
    data.forEach(p => select.innerHTML += `<option value="${p.id}">${p.nombre} (Disp: ${p.stock})</option>`);
}

async function cargarDetalles() {
    const res = await fetch(`${API_URL}/DetallePedidos`, { headers: { 'Authorization': `Bearer ${token}` } });
    const detalles = await res.json();
    const tbody = document.getElementById('tablaDetallesPedidos');
    tbody.innerHTML = '';

    detalles.forEach(d => {
        tbody.innerHTML += `
            <tr>
                <td>${d.pedidoId}</td>
                <td>${d.productoId}</td>
                <td>${d.cantidad}</td>
                <td>S/ ${d.precioUnitario.toFixed(2)}</td>
                <td>S/ ${(d.cantidad * d.precioUnitario).toFixed(2)}</td>
                <td>
                    <button class="btn btn-sm btn-danger" onclick="eliminarDetalle(${d.id})">🗑️</button>
                </td>
            </tr>`;
    });
}

document.getElementById('formDetallePedido').onsubmit = async (e) => {
    e.preventDefault();
    const detalle = {
        cantidad: parseInt(document.getElementById('detPedCantidad').value),
        precioUnitario: parseFloat(document.getElementById('detPedPrecio').value),
        pedidoId: parseInt(document.getElementById('detPedId').value),
        productoId: parseInt(document.getElementById('detPedProductoId').value)
    };

    const res = await fetch(`${API_URL}/DetallePedidos`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
        body: JSON.stringify(detalle)
    });

    if (res.ok) {
        alert("Venta registrada. El stock ha sido descontado.");
        bootstrap.Modal.getInstance(document.getElementById('modalDetallePedido')).hide();
        cargarDetalles();
    } else {
        const error = await res.json();
        alert("Error: " + (error.mensaje || "No se pudo registrar la venta"));
    }
};
// Variable global para guardar los productos cargados
let listaProductosLocal = [];

async function cargarProductos() {
    const res = await fetch(`${API_URL}/Productos`, { headers: { 'Authorization': `Bearer ${token}` } });
    listaProductosLocal = await res.json(); // Guardamos los productos aquí
    
    const select = document.getElementById('detPedProductoId');
    select.innerHTML = '<option value="">Seleccione Producto</option>';
    listaProductosLocal.forEach(p => {
        select.innerHTML += `<option value="${p.id}">${p.nombre} (Stock: ${p.stock})</option>`;
    });
}

// EVENTO: Cuando el usuario selecciona un producto del menú desplegable
document.getElementById('detPedProductoId').addEventListener('change', (e) => {
    const productoId = e.target.value;
    
    // Buscamos el producto en nuestra lista local
    const productoSeleccionado = listaProductosLocal.find(p => p.id == productoId);
    
    if (productoSeleccionado) {
        // "Jalamos" el precio automáticamente al input
        document.getElementById('detPedPrecio').value = productoSeleccionado.precioVenta;
    } else {
        document.getElementById('detPedPrecio').value = "";
    }
});

async function eliminarDetalle(id) {
    if (confirm("¿Seguro que desea eliminar este item? El stock se devolverá automáticamente.")) {
        const res = await fetch(`${API_URL}/DetallePedidos/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (res.ok) {
            alert("Eliminado y stock restaurado");
            cargarDetalles();
            if (typeof cargarPedidos === 'function') cargarPedidos(); // Actualiza el total en el select
        } else {
            alert("Error al eliminar");
        }
    }
}