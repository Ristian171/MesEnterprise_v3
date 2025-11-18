/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/config-reguli.js
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

    // Elemente DOM
    const form = document.getElementById('regula-form');
    const idInput = document.getElementById('regula-id');
    const linieSelect = document.getElementById('linie-id'); // LineId
    const produsSelect = document.getElementById('produs-id'); // ProductId (nou)
    const maxConsecutiveNrftInput = document.getElementById('max-consecutive-nrft'); // MaxConsecutiveNRFT
    const maxConsecutiveScrapInput = document.getElementById('max-consecutive-scrap'); // MaxConsecutiveScrap
    const regulaActivaSelect = document.getElementById('regula-activa');
    const tbody = document.getElementById('reguli-tbody');
    const submitBtn = form.querySelector('button[type="submit"]');
    const clearBtn = document.getElementById('regula-clear-btn');

    // Stocare locală
    let liniiData = [];
    let produseData = []; // NOU: pentru dropdown-ul de produse
    let reguliData = [];

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
        await loadAllData();
        showLoading(false);
    }

    /**
     * Încarcă toate datele (linii și regulile setate)
     */
    async function loadAllData() {
        try {
            await loadLinii();
            await loadProduse(); // NOU: Încarcă produsele
            await loadReguli();
            renderTable();
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
            liniiData = await apiCall('/api/config/lines'); // Endpoint corect
            populateSelect(linieSelect, liniiData, 'name', 'id', 'Selectați linia...');
        } catch (error) {
            showToast("Eroare la încărcarea liniilor.", 'error');
        }
    }

    /**
     * NOU: Încarcă lista de produse pentru dropdown
     */
    async function loadProduse() {
        try {
            produseData = await apiCall('/api/config/products'); // Endpoint corect
            populateSelect(produsSelect, produseData, 'name', 'id', 'Toate produsele...');
        } catch (error) {
            showToast("Eroare la încărcarea produselor.", 'error');
        }
    }

    /**
     * Încarcă regulile existente
     */
    async function loadReguli() {
        try {
            // CORECȚIE: Endpoint-ul corect este /api/config/stopondefectrules
            reguliData = await apiCall('/api/config/stopondefectrules');
        } catch (error) {
            showToast("Eroare la încărcarea regulilor.", 'error');
        }
    }

    function getProductName(productId) {
        const produs = produseData.find(p => p.id === productId);
        return produs ? produs.name : 'Toate Produsele';
    }

    /**
     * Afișează datele în tabel
     */
    function renderTable() {
        tbody.innerHTML = '';
        if (reguliData.length === 0) {
            tbody.innerHTML = '<tr><td colspan="4" style="text-align: center;">Nu există reguli automate definite.</td></tr>';
            return;
        }
        reguliData.forEach(regula => {
            const linie = liniiData.find(l => l.id === regula.lineId);
            const produsNume = getProductName(regula.productId); // NOU: Numele produsului
            if (!linie) return; // Skip dacă linia nu (mai) există
            const row = document.createElement('tr');
            // Regula StopOnDefect nu are isActive. Afișăm detaliile regulii.
            row.innerHTML = `
                <td>${escapeHTML(linie.name)}</td>
                <td>${escapeHTML(produsNume)}</td>
                <td>Oprește linia dacă apar ${regula.defectCountThreshold ?? regula.maxConsecutiveNRFT} defecte în ${regula.timeWindowMinutes ?? 'N/A'} minute.</td>
                <td class="actions">
                    <button class="btn btn-edit" data-id="${regula.id}"><i class="fa-solid fa-pencil"></i></button>
                    <button class="btn btn-danger" data-id="${regula.id}"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            row.dataset.regula = JSON.stringify(regula); // Stocăm datele
            tbody.appendChild(row);
        });
    }

    /**
     * Salvare (Adăugare sau Editare)
     */
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = idInput.value;
        const data = {
            lineId: parseInt(linieSelect.value), // LineId
            productId: produsSelect.value ? parseInt(produsSelect.value) : null, // ProductId (poate fi null)
            maxConsecutiveNRFT: parseInt(maxConsecutiveNrftInput.value), // MaxConsecutiveNRFT
            maxConsecutiveScrap: parseInt(maxConsecutiveScrapInput.value), // MaxConsecutiveScrap
            maxNrftPerHour: 0 // Nu avem un câmp direct, setăm la 0 sau adăugăm un câmp nou
        };

        if (isNaN(data.lineId)) { // Validare LineId
            showToast("Vă rugăm selectați o linie.", 'error');
            return;
        }
        if (isNaN(data.maxConsecutiveNRFT) || data.maxConsecutiveNRFT < 0) { // Validare MaxConsecutiveNRFT
            showToast("Nr. consecutiv NRFT trebuie să fie un număr pozitiv sau zero.", 'error');
            return;
        }
        if (isNaN(data.maxConsecutiveScrap) || data.maxConsecutiveScrap < 0) { // Validare MaxConsecutiveScrap
            showToast("Nr. consecutiv Scrap trebuie să fie un număr pozitiv sau zero.", 'error');
            return;
        }
        // Adăugăm validare pentru MaxNrftPerHour dacă adăugăm un câmp
        if (data.maxConsecutiveNRFT === 0 && data.maxConsecutiveScrap === 0) {
            showToast("Trebuie să setați cel puțin o regulă (NRFT sau Scrap).", 'error');
            return;
        }

        showLoading(true);
        try {
            if (id) { // Editare
                // CORECȚIE: Endpoint-ul corect este /api/config/stopondefectrules/{id}
                await apiCall(`/api/config/stopondefectrules/${id}`, { method: 'PUT', body: JSON.stringify({ id: parseInt(id), ...data }) });
                showToast("Regulă actualizată.", 'success');
            } else { // Adăugare
                // CORECȚIE: Endpoint-ul corect este /api/config/stopondefectrules
                await apiCall('/api/config/stopondefectrules', { method: 'POST', body: JSON.stringify(data) });
                showToast("Regulă adăugată.", 'success');
            }
            clearForm();
            await loadReguli(); // Reîncarcă regulile
            renderTable(); // Re-afișează tabelul
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    });

    /**
     * Click pe butoanele din tabel (Editare / Ștergere)
     */
    tbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('.btn-edit');
        const deleteBtn = e.target.closest('.btn-danger');

        if (editBtn) {
            const row = editBtn.closest('tr');
            const regula = JSON.parse(row.dataset.regula);
            if (regula) {
                idInput.value = regula.id;
                linieSelect.value = regula.lineId; // LineId
                produsSelect.value = regula.productId || ''; // ProductId
                maxConsecutiveNrftInput.value = regula.maxConsecutiveNRFT; // MaxConsecutiveNRFT
                maxConsecutiveScrapInput.value = regula.maxConsecutiveScrap; // MaxConsecutiveScrap
                // regulaActivaSelect.value = regula.isActive.toString(); // Nu are corespondent
                
                submitBtn.textContent = 'Actualizează Regulă';
                submitBtn.classList.replace('btn-primary', 'btn-success');
                clearBtn.classList.remove('hidden');
                
                linieSelect.disabled = true; // Nu permitem schimbarea liniei/produsului la editare
            }
        }

        if (deleteBtn) {
            const id = deleteBtn.dataset.id;
            if (confirm("Sunteți sigur că doriți să ștergeți această regulă?")) {
                handleDelete(id);
            }
        }
    });

    /**
     * Ștergere
     */
    async function handleDelete(id) {
        showLoading(true);
        try {
            // CORECȚIE: Endpoint-ul corect este /api/config/stopondefectrules/{id}
            await apiCall(`/api/config/stopondefectrules/${id}`, { method: 'DELETE' });
            showToast("Regulă ștearsă.", 'success');
            await loadReguli();
            renderTable();
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    }

    /**
     * Curățare formular
     */
    function clearForm() {
        form.reset();
        idInput.value = '';
        linieSelect.disabled = false; // Permitem schimbarea liniei
        produsSelect.disabled = false; // Permitem schimbarea produsului
        // regulaActivaSelect.value = 'true'; // Nu are corespondent
        submitBtn.textContent = 'Setează Regulă';
        submitBtn.classList.replace('btn-success', 'btn-primary');
        clearBtn.classList.add('hidden');
    }
    clearBtn.addEventListener('click', clearForm);

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