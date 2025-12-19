const API_URL = "https://integracionfinal-production.up.railway.app/api"; 
const loginForm = document.getElementById('loginForm');
const mensajeError = document.getElementById('mensajeError');

loginForm.addEventListener('submit', async (e) => {
    e.preventDefault(); 

    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;

    const datosLogin = {
        Email: email,
        PasswordHash: password 
    };

    try {
        const response = await fetch(`${API_URL}/Auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(datosLogin)
        });

        if (response.ok) {
            const data = await response.json();
            
            // --- CAMBIO CLAVE: Guardar usuarioId y Rol ---
            localStorage.setItem('token', data.token);
            localStorage.setItem('usuario', data.nombre);
            localStorage.setItem('rol', data.rol);
            localStorage.setItem('usuarioId', data.id); // Guardamos el ID único del usuario

            alert(`Bienvenido ${data.nombre}`);
            window.location.href = 'dashboard.html'; 
        } else {
            mostrarError("Correo o contraseña incorrectos");
        }

    } catch (error) {
        console.error("Error de conexión:", error);
        mostrarError("No se pudo conectar con el servidor.");
    }
});

function mostrarError(mensaje) {
    mensajeError.textContent = mensaje;
    mensajeError.classList.remove('d-none');
}