/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/changeover.js
 * ROL: Senior Software Developer
 * STARE: REFACTORIZAT (Curățat de logica de autentificare)
 *
 * MODIFICARE (Refactorizare):
 * - ELIMINAT: Logica redundantă de autentificare (verificare token,
 * parsare payload, update #username-display).
 * - ELIMINAT: Listener-ul redundant pentru #logout-button.
 * - JUSTIFICARE: Noul script global 'nav.js' gestionează acum centralizat
 * autentificarea.
 * ===================================================================================
 */

document.addEventListener("DOMContentLoaded", () => {

    // Elemente DOM
    const form = document.getElementById('changeover-form');
    const linieSelect = document.getElementById('linie-id');
    const produsSelect = document.getElementById('produs-id');
    const linieStatusInfo = document.getElementById('linie-status-info');
    const startButton = document.getElementById('start-changeover-btn');

    // Stocare locală
    let liniiData = [];
    let produseData = [];
    let linieStatusCurent = null;

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
        await loadInitialData();
        showLoading(false);
    }

    /**
     * Încarcă datele inițiale (linii și produse)
     */
    async function loadInitialData() {
        try {
            await Promise.all([
                loadLinii(),
                loadProduse()
            ]);
            // Interfața este gata de utilizare
        } catch (error) {
            // Erorile sunt deja afișate
        }
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
     * Încarcă lista de linii pentru dropdown
     */
    async function loadLinii() {
        try {
            liniiData = await apiCall('/api/config/lines');
            populateSelect(linieSelect, liniiData, 'name', 'id', 'Selectați linia...');
        } catch (error) {
            showToast("Eroare la încărcarea liniilor.", 'error');
        }
    }

    /**
     * Încarcă lista de produse
     */
    async function loadProduse() {
        try {
            produseData = await apiCall('/api/config/products');
            // Nu populăm încă dropdown-ul de produse
        } catch (error) {
            showToast("Eroare la încărcarea produselor.", 'error');
        }
    }

    /**
     * Când se selectează o linie, verificăm starea acesteia
     */
    linieSelect.addEventListener('change', async () => {
        const linieId = linieSelect.value;
        if (!linieId) {
            resetProductSelect();
            linieStatusInfo.innerHTML = '';
            startButton.disabled = true;
            return;
        }

        showLoading(true);
        try {
            // 1. Verificăm starea liniei (presupunem un endpoint de status)
            // (MES Engineer): Acest endpoint e vital.
            const status = await apiCall(`/api/operator/status/${linieId}`);
            linieStatusCurent = status; // Salvăm starea curentă

            // 2. Afișăm feedback (UI/UX)
            let statusMessage = `Stare linie: <strong style="color: ${status.color};">${status.statusName}</strong>`;
            if (status.productName) {
                statusMessage += ` | Produs curent: <strong>${status.productName}</strong>`;
            }
            linieStatusInfo.innerHTML = statusMessage;

            // 3. Verificăm dacă putem face changeover
            // (MES Engineer): Putem face changeover doar dacă linia e oprită.
            if (status.statusName !== "Oprit") {
                linieStatusInfo.innerHTML += `<br><strong style="color: var(--color-danger);">Linia trebuie să fie OPRITĂ pentru a începe un changeover.</strong>`;
                resetProductSelect();
                startButton.disabled = true;
            } else {
                // Linia e oprită, populăm produsele
                populateProduseDisponibile(status.productId);
                startButton.disabled = false;
            }
            
        } catch (error) {
            linieStatusInfo.innerHTML = `<strong style="color: var(--color-danger);">Nu s-a putut obține starea liniei.</strong>`;
            resetProductSelect();
            startButton.disabled = true;
        } finally {
            showLoading(false);
        }
    });

    function resetProductSelect() {
        produsSelect.innerHTML = '<option value="">Selectați o linie mai întâi...</option>';
        produsSelect.disabled = true;
    }

    /**
     * Populează dropdown-ul de produse, excluzând produsul curent (UI/UX)
     */
    function populateProduseDisponibile(produsCurentId) {
        produsSelect.disabled = false;
        
        // Filtrăm produsele pentru a-l exclude pe cel curent
        const produseDisponibile = produseData.filter(p => p.id !== produsCurentId);
        
        populateSelect(produsSelect, produseDisponibile, 'name', 'id', 'Selectați produsul nou...');
        
        // Adăugăm și codul produsului pentru claritate
        produsSelect.querySelectorAll('option').forEach(opt => {
            if (opt.value) {
                const produs = produseData.find(p => p.id == opt.value);
                if (produs) {
                    opt.textContent = `(${produs.productCode}) ${produs.name}`;
                }
            }
        });
    }

    /**
     * Trimiterea formularului de changeover
     */
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const linieId = parseInt(linieSelect.value);
        const produsId = parseInt(produsSelect.value);

        if (isNaN(linieId) || isNaN(produsId)) {
            showToast("Vă rugăm selectați o linie și un produs valid.", 'error');
            return;
        }
        
        // Verificare dublă a stării (dacă e salvată)
        if (linieStatusCurent && linieStatusCurent.statusName !== "Oprit") {
             showToast("Eroare: Linia nu este în starea OPRIT.", 'error');
             return;
        }

        if (confirm(`Sunteți sigur că doriți să începeți changeover-ul pe ${linieSelect.options[linieSelect.selectedIndex].text} pentru produsul ${produsSelect.options[produsSelect.selectedIndex].text}?`)) {
            
            const data = {
                linieId: linieId,
                produsId: produsId
            };

            showLoading(true);
            try {
                // Apelăm API-ul de changeover (presupunem endpoint-ul)
                await apiCall('/api/operator/start-changeover', { method: 'POST', body: JSON.stringify(data) });
                
                showToast("Changeover început cu succes!", 'success');
                
                // Feedback (UI/UX): Redirecționăm utilizatorul la pagina principală
                // pentru a vedea noua stare a liniei.
                setTimeout(() => {
                    window.location.href = '/index.html';
                }, 1500);

            } catch (error) {
                // Eroarea e deja afișată de apiCall
                showLoading(false);
            }
        }
    });


    // --- UTILITARE ---

    function populateSelect(selectElement, data, textKey, valueKey, defaultOptionText) {
        selectElement.innerHTML = `<option value="">${defaultOptionText}</option>`;
        data.forEach(item => {
            const option = document.createElement("option");
            option.value = item[valueKey];
            option.textContent = item[textKey];
            selectElement.appendChild(option);
        });
    }

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