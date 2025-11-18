/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/config-general.js
 * ROL: Senior Software Developer
 * STARE: REFACTORIZAT (Curățat de logica de autentificare)
 *
 * MODIFICARE (Refactorizare):
 * - ELIMINAT: Logica redundantă de autentificare (verificare token,
 * parsare payload, update #username-display).
 * - ELIMINAT: Listener-ul redundant pentru #logout-button.
 * - JUSTIFICARE: Noul script global 'nav.js' gestionează acum centralizat
 * autentificarea. Acest script obține token-ul direct din localStorage
 * doar atunci când are nevoie de el pentru apelurile API.
 * ===================================================================================
 */

document.addEventListener("DOMContentLoaded", () => {
    
    // --- NOTĂ: Blocul de autentificare a fost eliminat. ---
    // nav.js gestionează acum verificarea token-ului și redirect-ul.

    // Elemente DOM Linii
    const linieForm = document.getElementById('linie-form');
    const linieIdInput = document.getElementById('linie-id');
    const linieNumeInput = document.getElementById('linie-nume');
    const linieDescriereInput = document.getElementById('linie-descriere');
    const linieTbody = document.getElementById('linii-tbody');
    const linieFormActions = document.getElementById('linie-form-actions');
    const linieSubmitBtn = linieForm.querySelector('button[type="submit"]');
    const linieClearBtn = document.getElementById('linie-clear-btn');

    // Elemente DOM Echipamente
    const echipamentForm = document.getElementById('echipament-form');
    const echipamentIdInput = document.getElementById('echipament-id');
    const echipamentLinieIdSelect = document.getElementById('echipament-linie-id');
    const echipamentNumeInput = document.getElementById('echipament-nume');
    const echipamentDescriereInput = document.getElementById('echipament-descriere');
    const echipamentTbody = document.getElementById('echipamente-tbody');
    const echipamentFormActions = document.getElementById('echipament-form-actions');
    const echipamentSubmitBtn = echipamentForm.querySelector('button[type="submit"]');
    const echipamentClearBtn = document.getElementById('echipament-clear-btn');

    // Elemente DOM Produse
    const produsForm = document.getElementById('produs-form');
    const produsIdInput = document.getElementById('produs-id');
    const produsCodInput = document.getElementById('produs-cod');
    const produsNumeInput = document.getElementById('produs-nume');
    const produsCicluTargetInput = document.getElementById('produs-ciclu-target');
    const produsTbody = document.getElementById('produse-tbody');
    const produsFormActions = document.getElementById('produs-form-actions');
    const produsSubmitBtn = produsForm.querySelector('button[type="submit"]');
    const produsClearBtn = document.getElementById('produs-clear-btn');

    // Stocare locală
    let liniiData = [];
    let echipamenteData = [];
    
    // Obține token-ul o singură dată la încărcare
    const token = localStorage.getItem('token');

    /**
     * Funcția principală de inițializare a paginii
     */
    async function initializePage() {
        if (!token) {
            console.error("Token lipsă, autentificare eșuată.");
            // nav.js ar trebui să gestioneze redirect-ul
            return;
        }
        showLoading(true);
        await loadAllData();
        showLoading(false);
    }    /**
     * Încarcă toate datele (linii, echipamente, produse) în paralel
     */
    async function loadAllData() {
        await Promise.all([
            loadLinii(),
            loadEchipamente(),
            loadProduse()
        ]);
        
        // După ce liniile și echipamentele sunt încărcate, populează tabelele
        renderLiniiTable();
        renderEchipamenteTable();
        // Nu e nevoie să apelăm renderProduseTable aici, loadProduse() o face deja
    }

    // --- LOGICĂ GENERICĂ API ---
    
    /**
     * Funcție generică pentru apeluri fetch
     * @param {string} url URL-ul API
     * @param {object} options Opțiunile fetch (method, headers, body)
     * @returns {Promise<any>} Rezultatul JSON
     */
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
            throw error; // Aruncă eroarea pentru a fi prinsă de funcția apelantă
        }
    }

    // --- LOGICĂ LINII ---

    async function loadLinii() {
        try {
            liniiData = await apiCall('/api/config/lines'); // Endpoint corect
            // Populează dropdown-ul pentru echipamente
            populateSelect(echipamentLinieIdSelect, liniiData, 'name', 'id', 'Selectați linia...');
        } catch (error) {
            showToast("Eroare la încărcarea liniilor.", 'error');
        }
    }

    function renderLiniiTable() {
        linieTbody.innerHTML = '';
        if (!liniiData || liniiData.length === 0) {
            linieTbody.innerHTML = '<tr><td colspan="3" style="text-align: center;">Nu există linii definite.</td></tr>';
            return;
        }
        
        liniiData.forEach(linie => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${escapeHTML(linie.name)}</td>
                <td>${escapeHTML(linie.scanIdentifier || '-')}</td>
                <td class="actions">
                    <button class="btn btn-edit" data-id="${linie.id}"><i class="fa-solid fa-pencil"></i></button>
                    <button class="btn btn-danger" data-id="${linie.id}"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            linieTbody.appendChild(row);
        });
    }

    linieForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = linieIdInput.value;
        const data = {
            name: linieNumeInput.value,
            scanIdentifier: linieDescriereInput.value, // Numele câmpului este 'scanIdentifier'
            hasLiveScanning: false // Valoare default, poate fi extins
        };

        showLoading(true);
        try {
            if (id) { // Editare
                await apiCall(`/api/config/lines/${id}`, { method: 'PUT', body: JSON.stringify({ id: parseInt(id), ...data}) });
                showToast("Linie actualizată.", 'success');
            } else { // Adăugare
                await apiCall('/api/config/lines', { method: 'POST', body: JSON.stringify(data) });
                showToast("Linie adăugată.", 'success');
            }
            clearLinieForm();
            await loadLinii(); // Reîncarcă liniile (și dropdown-ul de echipamente)
            renderLiniiTable();
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    });

    linieTbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('.btn-edit');
        const deleteBtn = e.target.closest('.btn-danger');

        if (editBtn) {
            const id = editBtn.dataset.id;
            const linie = liniiData.find(l => l.id == id);
            if (linie) {
                linieIdInput.value = linie.id;
                linieNumeInput.value = linie.name;
                linieDescriereInput.value = linie.scanIdentifier;
                linieSubmitBtn.textContent = 'Actualizează Linie';
                linieSubmitBtn.classList.replace('btn-primary', 'btn-success');
                linieClearBtn.classList.remove('hidden');
            }
        }

        if (deleteBtn) {
            const id = deleteBtn.dataset.id;
            if (confirm("Sunteți sigur că doriți să ștergeți această linie? Aceasta va șterge și echipamentele asociate!")) {
                handleDeleteLinie(id);
            }
        }
    });

    async function handleDeleteLinie(id) {
        showLoading(true);
        try {
            await apiCall(`/api/config/lines/${id}`, { method: 'DELETE' });
            showToast("Linie ștearsă.", 'success');
            await loadAllData(); // Reîncarcă tot
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    }

    function clearLinieForm() {
        linieForm.reset();
        linieIdInput.value = '';
        linieSubmitBtn.textContent = 'Adaugă Linie';
        linieSubmitBtn.classList.replace('btn-success', 'btn-primary');
        linieClearBtn.classList.add('hidden');
    }
    linieClearBtn.addEventListener('click', clearLinieForm);


    // --- LOGICĂ ECHIPAMENTE ---

    async function loadEchipamente() {
        try {
            echipamenteData = await apiCall('/api/config/equipments'); // Endpoint corect
        } catch (error) {
            showToast("Eroare la încărcarea echipamentelor.", 'error');
        }
    }

    function renderEchipamenteTable() {
        echipamentTbody.innerHTML = '';
        if (!echipamenteData || echipamenteData.length === 0) {
            echipamentTbody.innerHTML = '<tr><td colspan="3" style="text-align: center;">Nu există echipamente definite.</td></tr>';
            return;
        }

        echipamenteData.forEach(eq => {
            const linie = liniiData.find(l => l.id === eq.lineId);
            const linieNume = linie ? linie.name : 'N/A';
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${escapeHTML(eq.name)}</td>
                <td>${escapeHTML(linieNume)}</td>
                <td class="actions">
                    <button class="btn btn-edit" data-id="${eq.id}"><i class="fa-solid fa-pencil"></i></button>
                    <button class="btn btn-danger" data-id="${eq.id}"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            echipamentTbody.appendChild(row);
        });
    }

    echipamentForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = echipamentIdInput.value;
        const data = {
            lineId: parseInt(echipamentLinieIdSelect.value),
            name: echipamentNumeInput.value
        };

        if (isNaN(data.lineId)) {
            showToast("Vă rugăm selectați o linie.", 'error');
            return;
        }

        showLoading(true);
        try {
            if (id) { // Editare
                await apiCall(`/api/config/equipments/${id}`, { method: 'PUT', body: JSON.stringify({id: parseInt(id), ...data}) });
                showToast("Echipament actualizat.", 'success');
            } else { // Adăugare
                await apiCall('/api/config/equipments', { method: 'POST', body: JSON.stringify(data) });
                showToast("Echipament adăugat.", 'success');
            }
            clearEchipamentForm();
            await loadEchipamente();
            renderEchipamenteTable();
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    });

    echipamentTbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('.btn-edit');
        const deleteBtn = e.target.closest('.btn-danger');

        if (editBtn) {
            const id = editBtn.dataset.id;
            const echipament = echipamenteData.find(eq => eq.id == id);
            if (echipament) {
                echipamentIdInput.value = echipament.id;
                echipamentLinieIdSelect.value = echipament.lineId;
                echipamentNumeInput.value = echipament.name;
                echipamentSubmitBtn.textContent = 'Actualizează Echipament';
                echipamentSubmitBtn.classList.replace('btn-primary', 'btn-success');
                echipamentClearBtn.classList.remove('hidden');
            }
        }

        if (deleteBtn) {
            const id = deleteBtn.dataset.id;
            if (confirm("Sunteți sigur că doriți să ștergeți acest echipament?")) {
                handleDeleteEchipament(id);
            }
        }
    });

    async function handleDeleteEchipament(id) {
        showLoading(true);
        try {
            await apiCall(`/api/config/equipments/${id}`, { method: 'DELETE' });
            showToast("Echipament șters.", 'success');
            await loadEchipamente();
            renderEchipamenteTable();
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    }

    function clearEchipamentForm() {
        echipamentForm.reset();
        echipamentIdInput.value = '';
        echipamentSubmitBtn.textContent = 'Adaugă Echipament';
        echipamentSubmitBtn.classList.replace('btn-success', 'btn-primary');
        echipamentClearBtn.classList.add('hidden');
    }
    echipamentClearBtn.addEventListener('click', clearEchipamentForm);

    // --- LOGICĂ PRODUSE ---

    async function loadProduse() {
        try {
            const produse = await apiCall('/api/config/products'); // Endpoint corect
            renderProduseTable(produse);
        } catch (error) {
            showToast("Eroare la încărcarea produselor.", 'error');
        }
    }    function renderProduseTable(produse) {
        produsTbody.innerHTML = '';
        if (produse.length === 0) {
            produsTbody.innerHTML = '<tr><td colspan="3" style="text-align: center;">Nu există produse definite.</td></tr>';
            return;
        }

        produse.forEach(prod => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${escapeHTML(prod.name)}</td>
                <td>${prod.cycleTimeSeconds} sec</td>
                <td class="actions">
                    <button class="btn btn-edit" data-id="${prod.id}"><i class="fa-solid fa-pencil"></i></button>
                    <button class="btn btn-danger" data-id="${prod.id}"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            // Stocăm datele pe rând pentru editare facilă
            row.dataset.produs = JSON.stringify(prod);
            produsTbody.appendChild(row);
        });
    }

    produsForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = produsIdInput.value;
        const data = {
            name: produsNumeInput.value,
            cycleTimeSeconds: parseFloat(produsCicluTargetInput.value)
        };

        if (isNaN(data.cycleTimeSeconds) || data.cycleTimeSeconds <= 0) {
            showToast("Timpul de ciclu trebuie să fie un număr pozitiv.", 'error');
            return;
        }

        showLoading(true);
        try {
            if (id) { // Editare
                await apiCall(`/api/config/products/${id}`, { method: 'PUT', body: JSON.stringify({id: parseInt(id), ...data}) });
                showToast("Produs actualizat.", 'success');
            } else { // Adăugare
                await apiCall('/api/config/products', { method: 'POST', body: JSON.stringify(data) });
                showToast("Produs adăugat.", 'success');
            }
            clearProdusForm();
            await loadProduse();
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    });

    produsTbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('.btn-edit');
        const deleteBtn = e.target.closest('.btn-danger');        if (editBtn) {
            const row = editBtn.closest('tr');
            const produs = JSON.parse(row.dataset.produs);
            
            produsIdInput.value = produs.id;
            // Nu folosim câmpul cod, doar numele
            produsNumeInput.value = produs.name;
            produsCicluTargetInput.value = produs.cycleTimeSeconds;
            
            produsSubmitBtn.textContent = 'Actualizează Produs';
            produsSubmitBtn.classList.replace('btn-primary', 'btn-success');
            produsClearBtn.classList.remove('hidden');
        }

        if (deleteBtn) {
            const id = deleteBtn.dataset.id;
            if (confirm("Sunteți sigur că doriți să ștergeți acest produs?")) {
                handleDeleteProdus(id);
            }
        }
    });

    async function handleDeleteProdus(id) {
        showLoading(true);
        try {
            await apiCall(`/api/config/products/${id}`, { method: 'DELETE' });
            showToast("Produs șters.", 'success');
            await loadProduse();
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    }
    
    function clearProdusForm() {
        produsForm.reset();
        produsIdInput.value = '';
        produsSubmitBtn.textContent = 'Adaugă Produs';
        produsSubmitBtn.classList.replace('btn-success', 'btn-primary');
        produsClearBtn.classList.add('hidden');
    }
    produsClearBtn.addEventListener('click', clearProdusForm);

    // --- UTILITARE ---

    /**
     * Populează un element <select>
     */
    function populateSelect(selectElement, data, textKey, valueKey, defaultOptionText) {
        selectElement.innerHTML = `<option value="">${defaultOptionText}</option>`;
        data.forEach(item => {
            const option = document.createElement("option");
            option.value = item[valueKey];
            option.textContent = item[textKey];
            selectElement.appendChild(option);
        });
    }

    /**
     * Previne XSS (Cross-Site Scripting)
     */
    function escapeHTML(str) {
        if (str === null || str === undefined) {
            return '';
        }
        return str.toString()
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }
    
    // Funcțiile globale showLoading și showToast sunt definite în
    // interventii.js sau ar trebui mutate într-un utils.js global.
    // Le duplicăm aici pentru siguranță, în caz că nu sunt încărcate.
    
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