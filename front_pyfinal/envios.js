const API_URL = "https://localhost:7252/api";
const token = localStorage.getItem('token');

let idEnvioActual = null;

if (!token) window.location.href = 'index.html';

window.onload = () => {
    listarEnvios();
};
// 2. CERRAR SESIÓN
document.getElementById('btnCerrarSesion').addEventListener('click', () => {
    localStorage.clear();
    window.location.href = 'index.html';
});
// Función auxiliar para limpiar FORZADAMENTE los residuos de los modales
function limpiarModalResiduos() {
    document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
    document.body.classList.remove('modal-open');
    document.body.style.paddingRight = '';
}

// 1. CARGAR DATOS CON FILTROS (REPARTIDORES Y PEDIDOS)
async function cargarDatosFormulario() {
    const rolActual = localStorage.getItem('rol');
    const miId = localStorage.getItem('usuarioId');

    // 1. Cargar Pedidos
    const resP = await fetch(`${API_URL}/Pedidos`, { headers: { 'Authorization': `Bearer ${token}` } });
    const pedidos = await resP.json();
    const selP = document.getElementById('envioPedidoId');
    selP.innerHTML = '<option value="">Seleccione Pedido</option>';
    
    // Solo pedidos no cancelados
    pedidos.filter(p => p.estado !== "Cancelado").forEach(p => {
        selP.innerHTML += `<option value="${p.id}">Pedido #${p.id}</option>`;
    });

    // 2. Lógica de Repartidor Automática
    const selU = document.getElementById('envioRepartidorId');
    
    if (rolActual === "Repartidor") {
        // Verificamos que el ID exista
        if (!miId) {
            alert("Error: No se encontró tu ID de usuario. Por favor, cierra sesión y vuelve a entrar.");
            return;
        }
        selU.innerHTML = `<option value="${miId}" selected>Asignado a mí</option>`;
        selU.disabled = true; 
    } else {
        // Si eres Admin, cargar todos los repartidores
        const resU = await fetch(`${API_URL}/Usuarios`, { headers: { 'Authorization': `Bearer ${token}` } });
        const usuarios = await resU.json();
        selU.innerHTML = '<option value="">Seleccione Repartidor</option>';
        usuarios.filter(u => u.rol === "Repartidor").forEach(u => {
            selU.innerHTML += `<option value="${u.id}">${u.nombre}</option>`;
        });
        selU.disabled = false;
    }
}

// 2. LISTAR ENVÍOS
async function listarEnvios() {
    const res = await fetch(`${API_URL}/Envios`, { headers: { 'Authorization': `Bearer ${token}` } });
    const envios = await res.json();
    const tbody = document.getElementById('tablaEnvios');
    tbody.innerHTML = '';

    const resH = await fetch(`${API_URL}/HistorialEnvios`, { headers: { 'Authorization': `Bearer ${token}` } });
    const todosHistoriales = await resH.json();

    for (const e of envios) {
        const miHistorial = todosHistoriales
            .filter(h => h.envioId === e.id)
            .sort((a, b) => new Date(b.fechaHora) - new Date(a.fechaHora));
        
        const ultimoEstado = miHistorial.length > 0 ? miHistorial[0].estado : "Pendiente";

        tbody.innerHTML += `
            <tr>
                <td>${e.id}</td>
                <td>#${e.pedidoId}</td>
                <td>ID Repartidor: ${e.repartidorId}</td>
                <td>${e.direccionDestino}</td>
                <td><span class="badge bg-info text-dark">${ultimoEstado}</span></td>
                <td>
                    <button class="btn btn-sm btn-dark" onclick="verTrazabilidad(${e.id})">🔍 Historial</button>
                </td>
            </tr>`;
    }
}

// 3. POST: CREAR ENVÍO (CORREGIDO CIERRE DE MODAL)
// POST: CREAR ENVÍO (CORREGIDO PARA AUTO-ASIGNACIÓN)
document.getElementById('formNuevoEnvio').onsubmit = async (e) => {
    e.preventDefault();

    // Capturamos el ID. Si el select está deshabilitado (porque es repartidor), 
    // el valor se obtiene igual. Como respaldo, usamos el localStorage.
    const selectRepartidor = document.getElementById('envioRepartidorId');
    const repartidorIdFinal = selectRepartidor.value || localStorage.getItem('usuarioId');

    const nuevoEnvio = {
        pedidoId: parseInt(document.getElementById('envioPedidoId').value),
        repartidorId: parseInt(repartidorIdFinal), // ID corregido
        direccionDestino: document.getElementById('envioDireccion').value,
        fechaSalida: new Date().toISOString()
    };

    // Validación extra antes de enviar
    if (!nuevoEnvio.repartidorId) {
        alert("Error: No se ha podido identificar al repartidor.");
        return;
    }

    const res = await fetch(`${API_URL}/Envios`, {
        method: 'POST',
        headers: { 
            'Content-Type': 'application/json', 
            'Authorization': `Bearer ${token}` 
        },
        body: JSON.stringify(nuevoEnvio)
    });

    if (res.ok) {
        alert("Envío programado correctamente.");
        
        // Cierre seguro del modal
        const modalElement = document.getElementById('modalNuevoEnvio');
        const modalInstance = bootstrap.Modal.getInstance(modalElement);
        if(modalInstance) modalInstance.hide();
        
        limpiarModalResiduos(); // Función que ya tenemos para quitar lo opaco
        document.getElementById('formNuevoEnvio').reset();
        listarEnvios();
    } else {
        const error = await res.json();
        alert("⚠️ No se pudo crear: " + (error.mensaje || "Error desconocido"));
    }
};

// 4. VER TRAZABILIDAD
async function verTrazabilidad(id) {
    idEnvioActual = id;
    const res = await fetch(`${API_URL}/HistorialEnvios`, { headers: { 'Authorization': `Bearer ${token}` } });
    const data = await res.json();
    
    const hitos = data
        .filter(h => h.envioId === id)
        .sort((a, b) => new Date(b.fechaHora) - new Date(a.fechaHora));

    let html = '<div class="list-group list-group-flush">';
    hitos.forEach(h => {
        html += `
            <div class="list-group-item border-start border-4 border-primary mb-1">
                <small class="text-muted fw-bold">${new Date(h.fechaHora).toLocaleString()}</small><br>
                <span>${h.estado}</span>
            </div>`;
    });
    html += '</div>';
    document.getElementById('lineaTiempo').innerHTML = html;
    
    const modalHistorial = new bootstrap.Modal(document.getElementById('modalHistorial'));
    modalHistorial.show();
}

// 5. REGISTRAR NUEVO ESTADO (CORREGIDO CIERRE DE MODAL)
document.getElementById('btnActualizarEstado').onclick = async () => {
    const nuevoHito = {
        envioId: idEnvioActual,
        estado: document.getElementById('nuevoEstado').value,
        fechaHora: new Date().toISOString()
    };

    const res = await fetch(`${API_URL}/HistorialEnvios`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
        body: JSON.stringify(nuevoHito)
    });

    if (res.ok) {
        // Cierre seguro del modal
        const modalElement = document.getElementById('modalHistorial');
        const modalInstance = bootstrap.Modal.getInstance(modalElement);
        if(modalInstance) modalInstance.hide();
        
        limpiarModalResiduos(); // Elimina la capa opaca
        listarEnvios();
    } else {
        alert("Error al actualizar el estado.");
    }
};