const API_URL = "https://localhost:7252/api"; 
const token = localStorage.getItem('token');

if (!token) window.location.href = 'index.html';

document.getElementById('usuarioNombre').innerText = localStorage.getItem('usuario') || "Usuario";

document.getElementById('btnCerrarSesion').onclick = () => {
    localStorage.clear();
    window.location.href = 'index.html';
};

window.onload = cargarCategorias;

async function cargarCategorias() {
    const res = await fetch(`${API_URL}/Categorias`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });
    const cats = await res.json();
    const tbody = document.getElementById('tablaCategorias');
    tbody.innerHTML = '';

    cats.forEach(c => {
        tbody.innerHTML += `
            <tr>
                <td>${c.id}</td>
                <td>${c.nombre}</td>
                <td>
                    <button class="btn btn-sm btn-warning" onclick="abrirEditar(${c.id})">Editar</button>
                    <button class="btn btn-sm btn-danger" onclick="eliminarCategoria(${c.id})">Eliminar</button>
                </td>
            </tr>`;
    });
}

function prepararCreacion() {
    document.getElementById('catId').value = "";
    document.getElementById('formCategoria').reset();
    document.getElementById('tituloModal').innerText = "Nueva Categoría";
}

async function abrirEditar(id) {
    const res = await fetch(`${API_URL}/Categorias/${id}`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });
    const c = await res.json();
    document.getElementById('catId').value = c.id;
    document.getElementById('catNombre').value = c.nombre;
    document.getElementById('tituloModal').innerText = "Editar Categoría";
    new bootstrap.Modal(document.getElementById('modalCategoria')).show();
}

document.getElementById('formCategoria').onsubmit = async (e) => {
    e.preventDefault();
    const id = document.getElementById('catId').value;
    const categoria = {
        id: id ? parseInt(id) : 0,
        nombre: document.getElementById('catNombre').value
    };

    const url = id ? `${API_URL}/Categorias/${id}` : `${API_URL}/Categorias`;
    const metodo = id ? 'PUT' : 'POST';

    const res = await fetch(url, {
        method: metodo,
        headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
        body: JSON.stringify(categoria)
    });

    if (res.ok || res.status === 204) {
        bootstrap.Modal.getInstance(document.getElementById('modalCategoria')).hide();
        cargarCategorias();
    }
};

async function eliminarCategoria(id) {
    if (confirm("¿Eliminar categoría?")) {
        await fetch(`${API_URL}/Categorias/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });
        cargarCategorias();
    }
}