/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/edit.js
 * ROL: Senior Software Developer
 * STARE: REFACTORIZAT (Curățat de logica de autentificare + Adăugat Flatpickr)
 *
 * MODIFICARE (Refactorizare):
 * - ELIMINAT: Logica redundantă de autentificare (verificare token, etc.).
 * - ELIMINAT: Listener-ul redundant pentru #logout-button.
 * - ADĂUGAT: Inițializarea 'flatpickr' pentru câmpurile de dată.
 * - ADĂUGAT: Logică pentru gestionarea modalului de editare.
 * ===================================================================================
 */

document.addEventListener("DOMContentLoaded", () => {

    // --- NOTĂ: Blocul de autentificare a fost eliminat. ---

    // Elemente DOM Filtrare
    const filterLinie = document.getElementById('filter-linie');
    const filterDataStart = document.getElementById('filter-data-start');
    const filterDataStop = document.getElementById('filter-data-stop');
    const searchButton = document.getElementById('search-button');
    const tbody = document.getElementById('edit-tbody');

    // Elemente DOM Modal
    const modal = document.getElementById('edit-modal');
    const modalForm = document.getElementById('edit-modal-form');
    const modalCloseBtn = document.getElementById('edit-modal-close');
    const modalCancelBtn = document.getElementById('edit-modal-cancel');
    
    // Elemente DOM Formular Modal
    const editRecordId = document.getElementById('edit-record-id');
    const editTimestamp = document.getElementById('edit-timestamp');
    const editProduct = document.getElementById('edit-product');
    const editGood = document.getElementById('edit-good');
    const editScrap = document.getElementById('edit-scrap');
    const editNrft = document.getElementById('edit-nrft');

    // Stocare locală
    let liniiData = [];
    let currentData = []; // Stocăm datele căutate

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
        
        initializePickers();
        
        showLoading(true);
        await loadLinii();
        showLoading(false);
    }

    /**
     * Inițializează Flatpickr pentru câmpurile de dată
     */
    function initializePickers() {
        if (typeof flatpickr === 'undefined') {
            console.warn("Biblioteca Flatpickr nu este încărcată. Selectoarele de dată nu vor funcționa.");
            return;
        }
        
        const dateOptions = {
            dateFormat: "Y-m-d", // Format compatibil ISO
            allowInput: true
        };

        flatpickr(filterDataStart, dateOptions);
        flatpickr(filterDataStop, dateOptions);
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
            populateSelect(filterLinie, liniiData, 'name', 'id', 'Selectați linia...');
        } catch (error) {
            showToast("Eroare la încărcarea liniilor.", 'error');
        }
    }

    /**
     * Căutare înregistrări
     */
    searchButton.addEventListener('click', async () => {
        const linieId = filterLinie.value;
        const dataStart = filterDataStart.value;
        const dataStop = filterDataStop.value;

        if (!linieId || !dataStart || !dataStop) {
            showToast("Vă rugăm selectați linia și ambele date.", 'error');
            return;
        }

        showLoading(true);
        tbody.innerHTML = '<tr><td colspan="7" style="text-align: center;">Se încarcă...</td></tr>';
        
        try {
            // CORECȚIE: Endpoint-ul corect este /api/edit/logs-by-date
            const params = new URLSearchParams({
                lineId: linieId,
                date: dataStart // API-ul se așteaptă la un singur parametru 'date'
            });
            // Am presupus că se dorește filtrarea pentru o singură zi, conform API-ului.
            const data = await apiCall(`/api/edit/logs-by-date?${params.toString()}`);
            currentData = data; // Salvăm datele
            renderTable(data);

        } catch (error) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align: center; color: red;">Eroare la încărcarea datelor.</td></tr>';
        } finally {
            showLoading(false);
        }
    });

    /**
     * Afișează datele în tabel
     */
    function renderTable(data) {
        tbody.innerHTML = '';
        if (data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align: center;">Nu s-au găsit înregistrări.</td></tr>';
            return;
        }
        
        data.forEach(record => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${formatDateTime(record.timestamp)}</td> <!-- Afișează data și ora -->
                <td>${escapeHTML(record.shiftName)}</td>
                <td>${escapeHTML(record.productName)}</td>
                <td>${record.actualParts}</td>
                <td>${record.scrapParts}</td>
                <td>${record.nrftParts}</td>
                <td class="actions">
                    <button class="btn btn-edit" data-id="${record.id}"><i class="fa-solid fa-pencil"></i></button>
                </td>
            `;
            tbody.appendChild(row);
        });
    }

    /**
     * Click pe butoanele din tabel (Editare)
     */
    tbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('.btn-edit');
        if (editBtn) {
            const id = editBtn.dataset.id;
            const record = currentData.find(r => r.id == id);
            
            if (record) {
                openModal(record);
            }
        }
    });

    /**
     * Deschide și populează modalul de editare
     */
    function openModal(record) {
        editRecordId.value = record.id;
        editTimestamp.value = formatDateTime(record.timestamp);
        editProduct.value = escapeHTML(record.numeProdus);
        editGood.value = record.actualParts;
        editScrap.value = record.scrapParts;
        editNrft.value = record.nrftParts;
        
        modal.classList.remove('hidden');
    }

    /**
     * Închide modalul
     */
    function closeModal() {
        modal.classList.add('hidden');
        modalForm.reset();
        editRecordId.value = '';
    }

    modalCloseBtn.addEventListener('click', closeModal);
    modalCancelBtn.addEventListener('click', closeModal);

    /**
     * Salvarea datelor din modal
     */
    modalForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const id = editRecordId.value;
        const data = {
            actualParts: parseInt(editGood.value),
            scrapParts: parseInt(editScrap.value),
            nrftParts: parseInt(editNrft.value)
        };

        if (isNaN(data.good) || isNaN(data.scrap) || isNaN(data.nrft)) {
            showToast("Toate câmpurile trebuie să fie numere valide.", 'error');
            return;
        }

        showLoading(true);
        try {
            // CORECȚIE: Endpoint-ul corect este /api/edit/log/{id}
            await apiCall(`/api/edit/log/${id}`, { method: 'PUT', body: JSON.stringify(data) });
            showToast("Înregistrare salvată cu succes.", 'success');
            closeModal();
            // Re-executăm căutarea pentru a reîmprospăta datele
            searchButton.click();
            
        } catch (error) {
            // Eroarea e deja afișată
        } finally {
            showLoading(false); // Oprim loading-ul doar dacă nu re-căutăm
            // Dacă searchButton.click() rulează, el va opri loading-ul.
        }
    });


    // --- NOTĂ: Funcțiile utilitare au fost mutate în /js/utils.js ---

    // --- Inițializare Pagină ---
    initializePage();
});