const API_URL = "https://integracionfinal-production.up.railway.app/api"; 
const token = localStorage.getItem('token');
const userRol = localStorage.getItem('rol');
const currentUserId = localStorage.getItem('usuarioId');

if (!token) window.location.href = 'index.html';

const elNombre = document.getElementById('usuarioNombre');
if (elNombre) elNombre.innerText = localStorage.getItem('usuario') || "Usuario";

const btnCerrar = document.getElementById('btnCerrarSesion');
if (btnCerrar) {
    btnCerrar.onclick = () => {
        localStorage.clear();
        window.location.href = 'index.html';
    };
}

// INICIALIZACIÓN
window.onload = () => {
    cargarUsuarios();
    cargarPedidos();
    inicializarEventos();
};

// Función auxiliar para evitar pantalla opaca
function limpiarModalResiduos() {
    document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
    document.body.classList.remove('modal-open');
    document.body.style.paddingRight = '';
}

// 1. CARGAR USUARIOS (LÓGICA DE ROLES)
async function cargarUsuarios() {
    const selectCrear = document.getElementById('pedidoUsuario');
    const selectEdit = document.getElementById('editPedidoUsuario');

    // CASO VENDEDOR: Se asigna a sí mismo
    if (userRol === 'Vendedor') {
        const miNombre = localStorage.getItem('usuario') || "Yo";
        const htmlOption = `<option value="${currentUserId}" selected>${miNombre} (Tú)</option>`;
        
        if (selectCrear) {
            selectCrear.innerHTML = htmlOption;
            selectCrear.disabled = true;
        }
        if (selectEdit) {
            selectEdit.innerHTML = htmlOption;
            selectEdit.disabled = true;
        }
        return; 
    }

    // CASO ADMIN: Carga lista de vendedores
    try {
        const res = await fetch(`${API_URL}/Usuarios`, { headers: { 'Authorization': `Bearer ${token}` } });
        if (res.ok) {
            const data = await res.json();
            const vendedores = data.filter(u => u.rol === 'Vendedor');
            
            let options = '<option value="">Seleccione Vendedor</option>';
            vendedores.forEach(u => {
                options += `<option value="${u.id}">${u.nombre}</option>`;
            });

            if (selectCrear) {
                selectCrear.innerHTML = options;
                selectCrear.disabled = false;
            }
            if (selectEdit) {
                selectEdit.innerHTML = options;
                selectEdit.disabled = false;
            }
        }
    } catch (error) {
        console.error("Error al cargar usuarios:", error);
    }
}

// 2. LISTAR PEDIDOS
async function cargarPedidos() {
    try {
        const res = await fetch(`${API_URL}/Pedidos`, { headers: { 'Authorization': `Bearer ${token}` } });
        if (res.ok) {
            const pedidos = await res.json();
            const tbody = document.getElementById('tablaPedidos');
            if (!tbody) return;
            
            tbody.innerHTML = '';
            pedidos.forEach(p => {
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
    } catch (error) {
        console.error("Error al cargar pedidos:", error);
    }
}

// 3. CONFIGURAR EVENTOS
function inicializarEventos() {
    // CREAR PEDIDO
    const formCrear = document.getElementById('formPedido');
    if (formCrear) {
        formCrear.onsubmit = async (e) => {
            e.preventDefault();
            
            let usuarioSeleccionado = document.getElementById('pedidoUsuario').value;
            if (!usuarioSeleccionado && userRol === 'Vendedor') usuarioSeleccionado = currentUserId;

            if (!usuarioSeleccionado) {
                alert("Error: No se ha identificado al usuario vendedor.");
                return;
            }

            const nuevoPedido = {
                usuarioId: parseInt(usuarioSeleccionado),
                total: 0,
                estado: "Pendiente",
                fecha: new Date().toISOString()
            };

            try {
                const res = await fetch(`${API_URL}/Pedidos`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
                    body: JSON.stringify(nuevoPedido)
                });

                if (res.ok) {
                    alert("Pedido creado. Ahora agrega los productos en 'Ver Items'");
                    
                    // Cierre seguro del modal
                    const modalElement = document.getElementById('modalPedido');
                    const modalInstance = bootstrap.Modal.getInstance(modalElement);
                    if(modalInstance) modalInstance.hide();
                    
                    limpiarModalResiduos(); // Limpieza forzada
                    formCrear.reset();
                    cargarUsuarios(); 
                    cargarPedidos();
                } else {
                    alert("Error al crear el pedido.");
                }
            } catch (error) {
                alert("Error de conexión.");
            }
        };
    }

    // EDITAR PEDIDO
    const formEditar = document.getElementById('formEditarPedido');
    if (formEditar) {
        formEditar.onsubmit = async (e) => {
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
                
                // Cierre seguro del modal
                const modalElement = document.getElementById('modalEditarPedido');
                const modalInstance = bootstrap.Modal.getInstance(modalElement);
                if(modalInstance) modalInstance.hide();
                
                limpiarModalResiduos(); // Limpieza forzada
                cargarPedidos();
            } else {
                const error = await res.json();
                alert("Error: " + (error.mensaje || "No se pudo editar. El pedido podría estar Pagado."));
            }
        };
    }
}

// FUNCIONES GLOBALES
window.abrirEditarPedido = async (id) => {
    const res = await fetch(`${API_URL}/Pedidos/${id}`, { headers: { 'Authorization': `Bearer ${token}` } });
    if (res.ok) {
        const p = await res.json();
        document.getElementById('editPedidoId').value = p.id;
        document.getElementById('editPedidoUsuario').value = p.usuarioId;
        document.getElementById('editPedidoEstado').value = p.estado;
        document.getElementById('editPedidoTotal').value = p.total;

        new bootstrap.Modal(document.getElementById('modalEditarPedido')).show();
    }
};

window.eliminarPedido = async (id) => {
    if (confirm("¿Seguro que deseas eliminar este pedido? El stock será devuelto al almacén.")) {
        const res = await fetch(`${API_URL}/Pedidos/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (res.ok) {
            alert("Pedido eliminado y stock devuelto");
            cargarPedidos();
        } else {
            try {
                const error = await res.json();
                alert("Error: " + (error.mensaje || "No se pudo eliminar"));
            } catch (e) {
                alert("No se pudo eliminar. Probablemente el pedido ya está Pagado.");
            }
        }
    }
};