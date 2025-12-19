const API_URL = "https://localhost:7252/api";
const token = localStorage.getItem('token');

// Verificar sesión
if (!token) window.location.href = 'index.html';

window.onload = () => {
    cargarHistorialCompleto();
};

async function cargarHistorialCompleto() {
    try {
        const res = await fetch(`${API_URL}/HistorialEnvios`, {
            headers: { 
                'Authorization': `Bearer ${token}`,
                'Accept': 'application/json'
            }
        });

        if (res.ok) {
            const historiales = await res.json();
            const tbody = document.getElementById('tablaHistorialGlobal');
            tbody.innerHTML = '';

            // Ordenar por fecha (más reciente primero)
            historiales.sort((a, b) => new Date(b.fechaHora) - new Date(a.fechaHora));

            historiales.forEach(h => {
                // Formatear la fecha de forma legible
                const fechaFormateada = new Date(h.fechaHora).toLocaleString('es-PE', {
                    year: 'numeric',
                    month: '2-digit',
                    day: '2-digit',
                    hour: '2-digit',
                    minute: '2-digit',
                    second: '2-digit'
                });

                // Definir color según el estado
                let badgeClass = "bg-secondary";
                if (h.estado.includes("Entregado")) badgeClass = "bg-success";
                if (h.estado.includes("En camino") || h.estado.includes("Ruta")) badgeClass = "bg-warning text-dark";
                if (h.estado.includes("Generado")) badgeClass = "bg-info text-dark";
                if (h.estado.includes("Fallido")) badgeClass = "bg-danger";

                tbody.innerHTML += `
                    <tr>
                        <td>${h.id}</td>
                        <td><span class="fw-bold">Envío #${h.envioId}</span></td>
                        <td><span class="badge ${badgeClass}">${h.estado}</span></td>
                        <td>${fechaFormateada}</td>
                    </tr>`;
            });
        } else {
            console.error("Error al obtener el historial");
        }
    } catch (error) {
        console.error("Error de conexión:", error);
    }
}