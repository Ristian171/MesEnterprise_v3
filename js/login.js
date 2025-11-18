/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/login.js
 * ROL: Senior Software Developer
 * STARE: REFACTORIZAT (Aliniat cu noul login.html)
 *
 * MODIFICARE (Refactorizare):
 * - MODIFICAT: Selectorul pentru mesaje de eroare pentru a se potrivi
 * cu noul .error-message din login.html.
 * - ADĂUGAT: Apeluri la funcția showLoading() (definită inline în login.html)
 * pentru feedback vizual la autentificare.
 * ===================================================================================
 */

document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.getElementById("login-form");
    const usernameInput = document.getElementById("username");
    const passwordInput = document.getElementById("password");
    const loginButton = document.getElementById("login-button");
    
    // (UI/UX Refactor): Folosim noul element pentru erori
    const errorMessage = document.getElementById("error-message");

    // Verifică dacă utilizatorul este deja logat (ex: apasă "back")
    if (localStorage.getItem('token')) {
        window.location.href = '/index.html'; // Redirect la pagina principală
    }

    loginForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        
        const username = usernameInput.value;
        const password = passwordInput.value;

        // (UI/UX Refactor): Afișează overlay-ul de încărcare
        if (typeof showLoading === 'function') {
            showLoading(true);
        }
        loginButton.disabled = true;
        errorMessage.classList.add('hidden'); // Ascunde eroarea precedentă

        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    username: username,
                    password: password
                })
            });

            if (response.ok) {
                const data = await response.json();
                localStorage.setItem('token', data.token);
                
                // Succes, redirect la pagina principală
                window.location.href = '/index.html';
                
            } else {
                // Afișează eroarea
                const errorData = await response.json();
                errorMessage.textContent = errorData.message || "Utilizator sau parolă invalidă.";
                errorMessage.classList.remove('hidden');
                
                if (typeof showLoading === 'function') {
                    showLoading(false);
                }
                loginButton.disabled = false;
            }
        } catch (error) {
            // Eroare de rețea
            errorMessage.textContent = "Eroare de conexiune. Verificați rețeaua.";
            errorMessage.classList.remove('hidden');
            
            if (typeof showLoading === 'function') {
                showLoading(false);
            }
            loginButton.disabled = false;
        }
    });
});