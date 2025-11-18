/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/config-schimburi.js
 * ROL: Senior Software Developer
 * STARE: REFACTORIZAT (Curățat + Adăugat Flatpickr)
 *
 * MODIFICARE (Refactorizare):
 * - ELIMINAT: Logica redundantă de autentificare (verificare token,
 * parsare payload, update #username-display).
 * - ELIMINAT: Listener-ul redundant pentru #logout-button.
 * - ADĂUGAT: Inițializarea 'flatpickr' pentru câmpurile de oră
 * (schimb-start, schimb-end, pauza-start, pauza-end)
 * pentru o experiență UI/UX modernă.
 * ===================================================================================
 */

document.addEventListener("DOMContentLoaded", () => {

    // --- NOTĂ: Blocul de autentificare a fost eliminat. ---
    // nav.js gestionează acum verificarea token-ului și redirect-ul.

    // Elemente DOM Schimburi
    const schimbForm = document.getElementById('schimb-form');
    const schimbIdInput = document.getElementById('schimb-id');
    const schimbNumeInput = document.getElementById('schimb-nume');
    const schimbStartInput = document.getElementById('schimb-start');
    const schimbEndInput = document.getElementById('schimb-end');
    const schimbTbody = document.getElementById('schimburi-tbody');
    const schimbSubmitBtn = schimbForm.querySelector('button[type="submit"]');
    const schimbClearBtn = document.getElementById('schimb-clear-btn');

    // Elemente DOM Pauze
    const pauzaForm = document.getElementById('pauza-form');
    const pauzaIdInput = document.getElementById('pauza-id');
    const pauzaSchimbIdSelect = document.getElementById('pauza-schimb-id');
    const pauzaNumeInput = document.getElementById('pauza-nume');
    const pauzaStartInput = document.getElementById('pauza-start');
    const pauzaEndInput = document.getElementById('pauza-end');
    const pauzaTbody = document.getElementById('pauze-tbody');
    const pauzaSubmitBtn = pauzaForm.querySelector('button[type="submit"]');
    const pauzaClearBtn = document.getElementById('pauza-clear-btn');

    // Stocare locală
    let schimburiData = [];
    let pauzeData = [];

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
        
        // Inițializează selectoarele de oră
        initializeTimePickers();
        
        showLoading(true);
        await loadAllData();
        showLoading(false);
    }

    /**
     * Inițializează Flatpickr pentru câmpurile de oră
     */
    function initializeTimePickers() {
        if (typeof flatpickr === 'undefined') {
            console.warn("Biblioteca Flatpickr nu este încărcată. Selectoarele de oră nu vor funcționa.");
            return;
        }
        
        const timeOptions = {
            enableTime: true,
            noCalendar: true,
            dateFormat: "H:i", // Format 24h
            time_24hr: true,
            allowInput: true
        };

        flatpickr(schimbStartInput, timeOptions);
        flatpickr(schimbEndInput, timeOptions);
        flatpickr(pauzaStartInput, timeOptions);
        flatpickr(pauzaEndInput, timeOptions);
    }

    /**
     * Încarcă toate datele (schimburi și pauze) în paralel
     */
    async function loadAllData() {
        // Încarcă schimburile mai întâi, apoi pauzele
        try {
            await loadSchimburi();
            await loadPauze();
            
            // Populează tabelele
            renderSchimburiTable();
            renderPauzeTable();
        } catch (error) {
             // Erorile sunt deja afișate de funcțiile de încărcare
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

    // --- LOGICĂ SCHIMBURI ---

    async function loadSchimburi() {
        try {
            schimburiData = await apiCall('/api/config/shifts');
            // Populează dropdown-ul pentru pauze
            populateSelect(pauzaSchimbIdSelect, schimburiData, 'name', 'id', 'Selectați schimbul...');
        } catch (error) {
            showToast("Eroare la încărcarea schimburilor.", 'error');
        }
    }

    function renderSchimburiTable() {
        schimbTbody.innerHTML = '';
        if (schimburiData.length === 0) {
            schimbTbody.innerHTML = '<tr><td colspan="4" style="text-align: center;">Nu există schimburi definite.</td></tr>';
            return;
        }
        
        schimburiData.forEach(schimb => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${escapeHTML(schimb.name)}</td>
                <td>${formatTime(schimb.startTime)}</td>
                <td>${formatTime(schimb.endTime)}</td>
                <td class="actions">
                    <button class="btn btn-edit" data-id="${schimb.id}"><i class="fa-solid fa-pencil"></i></button>
                    <button class="btn btn-danger" data-id="${schimb.id}"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            row.dataset.schimb = JSON.stringify(schimb); // Stocăm datele
            schimbTbody.appendChild(row);
        });
    }

    schimbForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = schimbIdInput.value;
        const data = {
            name: schimbNumeInput.value,
            startTime: schimbStartInput.value, // "HH:mm"
            endTime: schimbEndInput.value      // "HH:mm"
        };

        showLoading(true);
        try {
            if (id) { // Editare
                await apiCall(`/api/config/shifts/${id}`, { method: 'PUT', body: JSON.stringify(data) });
                showToast("Schimb actualizat.", 'success');
            } else { // Adăugare
                await apiCall('/api/config/shifts', { method: 'POST', body: JSON.stringify(data) });
                showToast("Schimb adăugat.", 'success');
            }
            clearSchimbForm();
            await loadSchimburi(); // Reîncarcă schimburile (și dropdown-ul de pauze)
            renderSchimburiTable();
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    });

    schimbTbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('.btn-edit');
        const deleteBtn = e.target.closest('.btn-danger');

        if (editBtn) {
            const row = editBtn.closest('tr');
            const schimb = JSON.parse(row.dataset.schimb);
            if (schimb) {
                schimbIdInput.value = schimb.id;
                schimbNumeInput.value = schimb.name;
                schimbStartInput.value = formatTime(schimb.startTime);
                schimbEndInput.value = formatTime(schimb.endTime);
                schimbSubmitBtn.textContent = 'Actualizează Schimb';
                schimbSubmitBtn.classList.replace('btn-primary', 'btn-success');
                schimbClearBtn.classList.remove('hidden');
            }
        }

        if (deleteBtn) {
            const id = deleteBtn.dataset.id;
            if (confirm("Sunteți sigur că doriți să ștergeți acest schimb? Aceasta va șterge și pauzele asociate!")) {
                handleDeleteSchimb(id);
            }
        }
    });

    async function handleDeleteSchimb(id) {
        showLoading(true);
        try {
            await apiCall(`/api/config/shifts/${id}`, { method: 'DELETE' });
            showToast("Schimb șters.", 'success');
            await loadAllData(); // Reîncarcă tot
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    }

    function clearSchimbForm() {
        schimbForm.reset();
        schimbIdInput.value = '';
        schimbSubmitBtn.textContent = 'Adaugă Schimb';
        schimbSubmitBtn.classList.replace('btn-success', 'btn-primary');
        schimbClearBtn.classList.add('hidden');
    }
    schimbClearBtn.addEventListener('click', clearSchimbForm);


    // --- LOGICĂ PAUZE ---

    async function loadPauze() {
        try {
            pauzeData = await apiCall('/api/config/shifts/breaks'); // Endpoint-ul corect este sub /shifts
        } catch (error) {
            showToast("Eroare la încărcarea pauzelor.", 'error');
        }
    }

    function renderPauzeTable() {
        pauzaTbody.innerHTML = '';
        if (pauzeData.length === 0) {
            pauzaTbody.innerHTML = '<tr><td colspan="5" style="text-align: center;">Nu există pauze definite.</td></tr>';
            return;
        }

        pauzeData.forEach(pauza => {
            const schimb = schimburiData.find(s => s.id === pauza.shiftId);
            const schimbNume = schimb ? schimb.name : 'N/A';
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${escapeHTML(pauza.name)}</td>
                <td>${escapeHTML(schimbNume)}</td>
                <td>${formatTime(pauza.breakTime)}</td>
                <td>${pauza.durationMinutes} min</td>
                <td class="actions">
                    <button class="btn btn-edit" data-id="${pauza.id}"><i class="fa-solid fa-pencil"></i></button>
                    <button class="btn btn-danger" data-id="${pauza.id}"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            row.dataset.pauza = JSON.stringify(pauza); // Stocăm datele
            pauzaTbody.appendChild(row);
        });
    }

    pauzaForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = pauzaIdInput.value;
        const data = {
            shiftId: parseInt(pauzaSchimbIdSelect.value),
            name: pauzaNumeInput.value,
            breakTime: pauzaStartInput.value, // "HH:mm"
            durationMinutes: parseInt(pauzaEndInput.value) // Acum este durata
        };

        if (isNaN(data.shiftDefinitionId)) {
            showToast("Vă rugăm selectați un schimb.", 'error');
            return;
        }

        showLoading(true);
        try {
            if (id) { // Editare
                await apiCall(`/api/config/breaks/${id}`, { method: 'PUT', body: JSON.stringify({id: parseInt(id), ...data}) });
                showToast("Pauză actualizată.", 'success');
            } else { // Adăugare
                await apiCall(`/api/config/shifts/${data.shiftId}/breaks`, { method: 'POST', body: JSON.stringify(data) });
                showToast("Pauză adăugată.", 'success');
            }
            clearPauzaForm();
            await loadPauze();
            renderPauzeTable();
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    });

    pauzaTbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('.btn-edit');
        const deleteBtn = e.target.closest('.btn-danger');

        if (editBtn) {
            const row = editBtn.closest('tr');
            const pauza = JSON.parse(row.dataset.pauza);
            if (pauza) {
                pauzaIdInput.value = pauza.id;
                pauzaSchimbIdSelect.value = pauza.shiftId;
                pauzaNumeInput.value = pauza.name;
                pauzaStartInput.value = formatTime(pauza.breakTime);
                pauzaEndInput.value = pauza.durationMinutes;
                
                pauzaSubmitBtn.textContent = 'Actualizează Pauză';
                pauzaSubmitBtn.classList.replace('btn-primary', 'btn-success');
                pauzaClearBtn.classList.remove('hidden');
            }
        }

        if (deleteBtn) {
            const id = deleteBtn.dataset.id;
            if (confirm("Sunteți sigur că doriți să ștergeți această pauză?")) {
                handleDeletePauza(id);
            }
        }
    });

    async function handleDeletePauza(id) {
        showLoading(true);
        try {
            await apiCall(`/api/config/breaks/${id}`, { method: 'DELETE' });
            showToast("Pauză ștearsă.", 'success');
            await loadPauze();
            renderPauzeTable();
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    }

    function clearPauzaForm() {
        pauzaForm.reset();
        pauzaIdInput.value = '';
        pauzaSubmitBtn.textContent = 'Adaugă Pauză';
        pauzaSubmitBtn.classList.replace('btn-success', 'btn-primary');
        pauzaClearBtn.classList.add('hidden');
    }
    pauzaClearBtn.addEventListener('click', clearPauzaForm);

    // --- UTILITARE ---

    /**
     * Formatează un string 'TimeSpan' (ex: "08:00:00") în 'HH:mm'
     */
    function formatTime(timeSpanString) {
        if (!timeSpanString) return "";
        return timeSpanString.substring(0, 5); // Ia doar HH:mm
    }

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
    
    // Funcțiile globale showLoading și showToast
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