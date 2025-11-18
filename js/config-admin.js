/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/config-admin.js
 * ROL: Senior Software Developer
 * STARE: REFACTORIZAT (Curățat de logica de autentificare)
 *
 * MODIFICARE (Refactorizare):
 * - ELIMINAT: Logica redundantă de autentificare (verificare token, etc.).
 * - ELIMINAT: Listener-ul redundant pentru #logout-button.
 * - JUSTIFICARE: Noul script global 'nav.js' gestionează acum centralizat
 * autentificarea.
 * ===================================================================================
 */

document.addEventListener("DOMContentLoaded", () => {

    // --- NOTĂ: Blocul de autentificare a fost eliminat. ---

    // Elemente DOM
    const form = document.getElementById('user-form');
    const idInput = document.getElementById('user-id');
    const usernameInput = document.getElementById('username'); // Rămâne pentru username
    const passwordInput = document.getElementById('password');
    const rolesSelect = document.getElementById('roles');
    const tbody = document.getElementById('users-tbody');
    const submitBtn = form.querySelector('button[type="submit"]');
    const clearBtn = document.getElementById('user-clear-btn');

    // Obține token-ul o singură dată la încărcare
    const token = localStorage.getItem('token');

    /**
     * Funcția principală de inițializare a paginii
     */
    async function initializePage() {
        if (!token) {
            console.error("Token lipsă, autentificare eșuată.");
            return;
        }
        
        showLoading(true);
        await loadUsers();
        showLoading(false);
    }

    // --- LOGICĂ GENERICĂ API ---
    
    async function apiCall(url, options = {}) {
        options.headers = {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
            ...options.headers
        };
        
        try {
            const response = await fetch(url, options);
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || `Eroare ${response.status}`);
            }
            if (response.status === 204) { // No Content
                return null;
            }
            return await response.json();
        } catch (error) {
            console.error(`Eroare API la ${url}:`, error);
            showToast(`Eroare: ${error.message}`, 'error');
            throw error;
        }
    }

    // --- LOGICA PAGINEI ---

    /**
     * Încarcă lista de utilizatori
     */
    async function loadUsers() {
        try {
            // Presupunem că endpoint-ul este /api/admin/users
            const users = await apiCall('/api/admin/users');
            renderTable(users);
        } catch (error) {
            showToast("Eroare la încărcarea utilizatorilor.", 'error');
        }
    }

    /**
     * Afișează datele în tabel
     */
    function renderTable(users) {
        tbody.innerHTML = '';
        if (users.length === 0) {
            tbody.innerHTML = '<tr><td colspan="4" style="text-align: center;">Nu există utilizatori definiți.</td></tr>';
            return;
        }
        
        users.forEach(user => {
            const row = document.createElement('tr');
            const rolesText = user.roles ? user.roles.join(', ') : 'N/A';
            row.innerHTML = ` 
                <td>${escapeHTML(user.userName)}</td>
                <td>${escapeHTML(user.fullName)}</td>
                <td>${escapeHTML(rolesText)}</td>
                <td class="actions">
                    <button class="btn btn-edit" data-id="${user.id}"><i class="fa-solid fa-pencil"></i></button>
                    </td>
            `;
            row.dataset.user = JSON.stringify(user); // Stocăm datele
            tbody.appendChild(row);
        });
    }

    /**
     * Salvare (Adăugare sau Editare)
     */
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const id = idInput.value;
        const selectedRoles = Array.from(rolesSelect.selectedOptions).map(option => option.value);

        if (selectedRoles.length === 0) {
            showToast("Utilizatorul trebuie să aibă cel puțin un rol.", 'error');
            return;
        }
        
        const data = {
            userName: usernameInput.value,
            fullName: fullNameInput.value,
            password: passwordInput.value || null,
            roles: selectedRoles
        };

        // Validare parolă la creare
        if (!id && !data.password) {
            showToast("Parola este obligatorie la crearea unui utilizator nou.", 'error');
            return;
        }

        showLoading(true);
        try {
            if (id) { // Editare
                // Endpoint-ul pentru editare (PUT)
                await apiCall(`/api/admin/users/${id}`, { method: 'PUT', body: JSON.stringify(data) });
                showToast("Utilizator actualizat.", 'success');
            } else { // Adăugare
                // Endpoint-ul pentru creare (POST)
                await apiCall('/api/admin/users', { method: 'POST', body: JSON.stringify(data) });
                showToast("Utilizator adăugat.", 'success');
            }
            clearForm();
            await loadUsers(); // Reîncarcă utilizatorii
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    });

    /**
     * Click pe butoanele din tabel (Editare)
     */
    tbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('.btn-edit');

        if (editBtn) {
            const row = editBtn.closest('tr');
            const user = JSON.parse(row.dataset.user);
            if (user) { // Modelul User are Username și Role, nu userName, fullName, roles
                idInput.value = user.id;
                usernameInput.value = user.username; // Corectat: user.username
                
                // Resetează selecția rolurilor
                Array.from(rolesSelect.options).forEach(opt => { // Rolul este un singur string, nu un array
                    opt.selected = user.roles.includes(opt.value);
                });

                passwordInput.value = ''; // Golește parola
                passwordInput.placeholder = "Lăsați gol pentru a păstra parola actuală";
                
                submitBtn.textContent = 'Actualizează Utilizator';
                submitBtn.classList.replace('btn-primary', 'btn-success');
                clearBtn.classList.remove('hidden');
            }
        }
    });

    /**
     * Curățare formular
     */
    function clearForm() {
        form.reset();
        idInput.value = '';
        
        Array.from(rolesSelect.options).forEach(opt => {
            opt.selected = false; // Rolul este un singur string, nu un array
        });
        
        passwordInput.placeholder = "Minim 6 caractere";
        submitBtn.textContent = 'Adaugă Utilizator';
        submitBtn.classList.replace('btn-success', 'btn-primary');
        clearBtn.classList.add('hidden');
    }
    clearBtn.addEventListener('click', clearForm);

    // --- UTILITARE ---

    function escapeHTML(str) {
        if (str === null || str === undefined) return '';
        return str.toString()
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }
    
    function showToast(message, type = 'info') {
        let toast = document.getElementById('toast-notification');
        if (!toast) {
            toast = document.createElement('div');
            toast.id = 'toast-notification';
            document.body.appendChild(toast);
        }
        toast.textContent = message;
        toast.className = ''; // Reset
        toast.classList.add(type); // 'info', 'success', 'error', 'warn'
        toast.style.display = 'block';
        setTimeout(() => {
            toast.style.display = 'none';
        }, 3000);
    }

    function showLoading(isLoading) {
        const overlay = document.getElementById('loading-overlay');
        if (overlay) {
            overlay.classList.toggle('hidden', !isLoading);
        }
    }

    // --- Inițializare Pagină ---
    initializePage();
});