const API_URL = "https://integracionfinal-production.up.railway.app/api"; 
const token = localStorage.getItem('token');

if (!token) window.location.href = 'index.html';

document.getElementById('usuarioNombre').innerText = localStorage.getItem('usuario') || "Usuario";

document.getElementById('btnCerrarSesion').onclick = () => {
    localStorage.clear();
    window.location.href = 'index.html';
};

window.onload = () => {
    cargarUsuarios(); // Carga usuarios para el modal de Crear
    cargarPedidos();
};

// 1. CARGAR USUARIOS PARA LOS SELECTS
async function cargarUsuarios() {
    const res = await fetch(`${API_URL}/Usuarios`, { headers: { 'Authorization': `Bearer ${token}` } });
    const data = await res.json();
    
    // Llenar select de creación
    const selectCrear = document.getElementById('pedidoUsuario');
    if(selectCrear) {
        selectCrear.innerHTML = '<option value="">Seleccione Cliente</option>';
        data.forEach(u => selectCrear.innerHTML += `<option value="${u.id}">${u.nombre}</option>`);
    }

    // Llenar select de edición
    const selectEdit = document.getElementById('editPedidoUsuario');
    if(selectEdit) {
        selectEdit.innerHTML = '<option value="">Seleccione Cliente</option>';
        data.forEach(u => selectEdit.innerHTML += `<option value="${u.id}">${u.nombre}</option>`);
    }
}

// 2. LISTAR PEDIDOS
async function cargarPedidos() {
    const res = await fetch(`${API_URL}/Pedidos`, { headers: { 'Authorization': `Bearer ${token}` } });
    if (res.ok) {
        const pedidos = await res.json();
        const tbody = document.getElementById('tablaPedidos');
        tbody.innerHTML = '';
        pedidos.forEach(p => {
            // Color del badge según estado
            const badgeClass = p.estado === 'Pagado' ? 'bg-success' : (p.estado === 'Cancelado' ? 'bg-danger' : 'bg-info');
            
            tbody.innerHTML += `
                <tr>
                    <td>${p.id}</td>
                    <td>${new Date(p.fecha).toLocaleDateString()}</td>
                    <td><span class="badge ${badgeClass}">${p.estado}</span></td>
                    <td>S/ ${p.total.toFixed(2)}</td>
                    <td>
                        <a href="detallepedidos.html?id=${p.id}" class="btn btn-sm btn-dark">📦 Items</a>
                        <button class="btn btn-sm btn-warning" onclick="abrirEditarPedido(${p.id})">✏️</button>
                        <button class="btn btn-sm btn-danger" onclick="eliminarPedido(${p.id})">🗑️</button>
                    </td>
                </tr>`;
        });
    }
}

// 3. CREAR NUEVO PEDIDO (CABECERA EN 0)
document.getElementById('formPedido').onsubmit = async (e) => {
    e.preventDefault();
    const nuevoPedido = {
        usuarioId: parseInt(document.getElementById('pedidoUsuario').value),
        total: 0,
        estado: "Pendiente",
        fecha: new Date().toISOString()
    };

    const res = await fetch(`${API_URL}/Pedidos`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
        body: JSON.stringify(nuevoPedido)
    });

    if (res.ok) {
        alert("Pedido creado. Ahora agrega los productos en 'Ver Items'");
        bootstrap.Modal.getInstance(document.getElementById('modalPedido')).hide();
        document.getElementById('formPedido').reset();
        cargarPedidos();
    }
};

// 4. ABRIR MODAL EDITAR Y CARGAR DATOS
async function abrirEditarPedido(id) {
    const res = await fetch(`${API_URL}/Pedidos/${id}`, { headers: { 'Authorization': `Bearer ${token}` } });
    if (res.ok) {
        const p = await res.json();
        document.getElementById('editPedidoId').value = p.id;
        document.getElementById('editPedidoUsuario').value = p.usuarioId;
        document.getElementById('editPedidoEstado').value = p.estado;
        document.getElementById('editPedidoTotal').value = p.total;

        new bootstrap.Modal(document.getElementById('modalEditarPedido')).show();
    }
}

// 5. GUARDAR CAMBIOS (PUT)
document.getElementById('formEditarPedido').onsubmit = async (e) => {
    e.preventDefault();
    const id = document.getElementById('editPedidoId').value;
    const pedidoEditado = {
        id: parseInt(id),
        usuarioId: parseInt(document.getElementById('editPedidoUsuario').value),
        estado: document.getElementById('editPedidoEstado').value,
        total: parseFloat(document.getElementById('editPedidoTotal').value),
        fecha: new Date().toISOString()
    };

    const res = await fetch(`${API_URL}/Pedidos/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
        body: JSON.stringify(pedidoEditado)
    });

    if (res.ok) {
        alert("Estado actualizado correctamente");
        bootstrap.Modal.getInstance(document.getElementById('modalEditarPedido')).hide();
        cargarPedidos();
    } else {
        const error = await res.json();
        alert("Error: " + (error.mensaje || "No se pudo editar. El pedido podría estar Pagado."));
    }
};

// 6. ELIMINAR PEDIDO
async function eliminarPedido(id) {
    if (confirm("¿Seguro que deseas eliminar este pedido? El stock será devuelto al almacén.")) {
        const res = await fetch(`${API_URL}/Pedidos/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (res.ok) {
            alert("Pedido eliminado y stock devuelto");
            cargarPedidos();
        } else {
            // Manejo de error cuando el pedido está Pagado
            try {
                const error = await res.json();
                alert("Error: " + (error.mensaje || "No se pudo eliminar"));
            } catch (e) {
                alert("No se puede eliminar un pedido que ya ha sido pagado.");
            }
        }
    }
}