const API_URL = "https://localhost:7252/api"; 
const token = localStorage.getItem('token');

if (!token) window.location.href = 'index.html';

document.getElementById('usuarioNombre').innerText = localStorage.getItem('usuario') || "Usuario";

document.getElementById('btnCerrarSesion').onclick = () => {
    localStorage.clear();
    window.location.href = 'index.html';
};

window.onload = () => {
    cargarProveedores();
    cargarCompras();
};

async function cargarProveedores() {
    const res = await fetch(`${API_URL}/Proveedores`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });
    if (res.ok) {
        const data = await res.json();
        const select = document.getElementById('compraProveedor');
        select.innerHTML = '<option value="">Seleccione Proveedor</option>';
        data.forEach(p => select.innerHTML += `<option value="${p.id}">${p.razonSocial}</option>`);
    }
}

async function cargarCompras() {
    const res = await fetch(`${API_URL}/Compras`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });
    if (res.ok) {
        const compras = await res.json();
        const tbody = document.getElementById('tablaCompras');
        tbody.innerHTML = '';

        compras.forEach(c => {
            tbody.innerHTML += `
                <tr>
                    <td>${c.id}</td>
                    <td>${new Date(c.fecha).toLocaleDateString()}</td>
                    <td>${c.proveedorId}</td>
                    <td>S/ ${c.totalCompra.toFixed(2)}</td>
                    <td>
                        <button class="btn btn-sm btn-danger" onclick="eliminarCompra(${c.id})">🗑️</button>
                    </td>
                </tr>`;
        });
    }
}

document.getElementById('formCompra').onsubmit = async (e) => {
    e.preventDefault();
    const nuevaCompra = {
        proveedorId: parseInt(document.getElementById('compraProveedor').value),
        totalCompra: parseFloat(document.getElementById('compraTotal').value),
        fecha: new Date().toISOString()
    };

    const res = await fetch(`${API_URL}/Compras`, {
        method: 'POST',
        headers: { 
            'Content-Type': 'application/json', 
            'Authorization': `Bearer ${token}` 
        },
        body: JSON.stringify(nuevaCompra)
    });

    if (res.ok) {
        bootstrap.Modal.getInstance(document.getElementById('modalCompra')).hide();
        cargarCompras();
    }
};

async function eliminarCompra(id) {
    if (confirm("¿Eliminar registro de compra?")) {
        await fetch(`${API_URL}/Compras/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });
        cargarCompras();
    }
}