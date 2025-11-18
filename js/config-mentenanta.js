/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/config-mentenanta.js
 * ROL: Senior Software Developer
 * STARE: REFACTORIZAT (Curățat de logica de autentificare)
 *
 * MODIFICARE (Refactorizare):
 * - ELIMINAT: Logica redundantă de autentificare (verificare token, etc.).
 * - ELIMINAT: Listener-ul redundant pentru #logout-button.
 * - ADĂUGAT: Logică pentru a gestiona toate cele 3 carduri:
 * 1. CRUD pentru Probleme
 * 2. CRUD pentru Defecțiuni
 * 3. Logică de încărcare și salvare pentru Corelări (Problemă -> Defecțiuni)
 * ===================================================================================
 */

document.addEventListener("DOMContentLoaded", () => {
    
    // Elemente DOM Probleme
    const problemaForm = document.getElementById('problema-form');
    const problemaIdInput = document.getElementById('problema-id');
    const problemaNumeInput = document.getElementById('problema-nume');
    const problemaTbody = document.getElementById('probleme-tbody');
    const problemaSubmitBtn = problemaForm.querySelector('button[type="submit"]');
    const problemaClearBtn = document.getElementById('problema-clear-btn');

    // Elemente DOM Defecțiuni
    const defectiuneForm = document.getElementById('defectiune-form');
    const defectiuneIdInput = document.getElementById('defectiune-id');
    const defectiuneNumeInput = document.getElementById('defectiune-nume');
    const defectiuneEsteAltulInput = document.getElementById('defectiune-este-altul');
    const defectiuneTbody = document.getElementById('defectiuni-tbody');
    const defectiuneSubmitBtn = defectiuneForm.querySelector('button[type="submit"]');
    const defectiuneClearBtn = document.getElementById('defectiune-clear-btn');

    // Elemente DOM Corelări
    const corelatieForm = document.getElementById('corelatie-form');
    const corelatieProblemaSelect = document.getElementById('corelatie-problema-id');
    const corelatieDefectiuniContainer = document.getElementById('corelatie-defectiuni-container');

    // Stocare locală
    let problemeData = [];
    let defectiuniData = [];

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
     * Încarcă toate datele (probleme și defecțiuni)
     */
    async function loadAllData() {
        showLoading(true);
        try {
            await Promise.all([
                loadProbleme(),
                loadDefectiuni()
            ]);
            // După ce s-au încărcat, populăm tabelele și dropdown-ul
            renderProblemeTable();
            renderDefectiuniTable();
            populateSelect(corelatieProblemaSelect, problemeData, 'nume', 'id', 'Selectați o problemă...');
        } catch (error) {
            // Erorile sunt deja afișate
        } finally {
            showLoading(false);
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

    // --- LOGICĂ PROBLEME RAPORTATE ---

    async function loadProbleme() {
        try {
            // Endpoint-ul trebuie să corespundă cu cel folosit în interventii.js
            problemeData = await apiCall('/api/config/mentenanta/probleme');
        } catch (error) {
            showToast("Eroare la încărcarea problemelor.", 'error');
        }
    }

    function renderProblemeTable() {
        problemaTbody.innerHTML = '';
        if (problemeData.length === 0) {
            problemaTbody.innerHTML = '<tr><td colspan="2" style="text-align: center;">Nu există probleme definite.</td></tr>';
            return;
        }
        
        problemeData.forEach(item => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${escapeHTML(item.nume)}</td>
                <td class="actions">
                    <button class="btn btn-edit" data-id="${item.id}"><i class="fa-solid fa-pencil"></i></button>
                    <button class="btn btn-danger" data-id="${item.id}"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            row.dataset.item = JSON.stringify(item); // Stocăm datele
            problemaTbody.appendChild(row);
        });
    }

    problemaForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = problemaIdInput.value;
        const data = {
            nume: problemaNumeInput.value
        };

        showLoading(true);
        try {
            if (id) { // Editare
                await apiCall(`/api/config/mentenanta/probleme/${id}`, { method: 'PUT', body: JSON.stringify({ id: parseInt(id), ...data }) });
                showToast("Problemă actualizată.", 'success');
            } else { // Adăugare
                await apiCall('/api/config/mentenanta/probleme', { method: 'POST', body: JSON.stringify(data) });
                showToast("Problemă adăugată.", 'success');
            }
            clearProblemaForm();
            await loadProbleme(); // Reîncarcă
            renderProblemeTable();
            populateSelect(corelatieProblemaSelect, problemeData, 'nume', 'id', 'Selectați o problemă...'); // Reîncarcă și dropdown-ul
        } catch (error) {
            // Eroarea e deja afișată
        } finally {
            showLoading(false);
        }
    });

    problemaTbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('.btn-edit');
        const deleteBtn = e.target.closest('.btn-danger');

        if (editBtn) {
            const row = editBtn.closest('tr');
            const item = JSON.parse(row.dataset.item);
            if (item) {
                problemaIdInput.value = item.id;
                problemaNumeInput.value = item.nume;
                problemaSubmitBtn.textContent = 'Actualizează Problemă';
                problemaSubmitBtn.classList.replace('btn-primary', 'btn-success');
                problemaClearBtn.classList.remove('hidden');
            }
        }

        if (deleteBtn) {
            const id = deleteBtn.dataset.id;
            if (confirm("Sunteți sigur că doriți să ștergeți această problemă? Aceasta va șterge și corelările asociate!")) {
                handleDeleteProblema(id);
            }
        }
    });

    async function handleDeleteProblema(id) {
        showLoading(true);
        try {
            await apiCall(`/api/config/mentenanta/probleme/${id}`, { method: 'DELETE' });
            showToast("Problemă ștearsă.", 'success');
            await loadAllData(); // Reîncarcă tot
        } catch (error) {
            // Eroarea e deja afișată
        } finally {
            showLoading(false);
        }
    }

    function clearProblemaForm() {
        problemaForm.reset();
        problemaIdInput.value = '';
        problemaSubmitBtn.textContent = 'Adaugă Problemă';
        problemaSubmitBtn.classList.replace('btn-success', 'btn-primary');
        problemaClearBtn.classList.add('hidden');
    }
    problemaClearBtn.addEventListener('click', clearProblemaForm);


    // --- LOGICĂ DEFECȚIUNI IDENTIFICATE ---

    async function loadDefectiuni() {
        try {
            defectiuniData = await apiCall('/api/config/mentenanta/defectiuni');
        } catch (error) {
            showToast("Eroare la încărcarea defecțiunilor.", 'error');
        }
    }

    function renderDefectiuniTable() {
        defectiuneTbody.innerHTML = '';
        if (defectiuniData.length === 0) {
            defectiuneTbody.innerHTML = '<tr><td colspan="3" style="text-align: center;">Nu există defecțiuni definite.</td></tr>';
            return;
        }

        defectiuniData.forEach(item => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${escapeHTML(item.nume)}</td>
                <td>${item.esteOptiuneAltul ? 'Da' : 'Nu'}</td>
                <td class="actions">
                    <button class="btn btn-edit" data-id="${item.id}"><i class="fa-solid fa-pencil"></i></button>
                    <button class="btn btn-danger" data-id="${item.id}"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            row.dataset.item = JSON.stringify(item); // Stocăm datele
            defectiuneTbody.appendChild(row);
        });
    }

    defectiuneForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = defectiuneIdInput.value;
        const data = {
            nume: defectiuneNumeInput.value,
            esteOptiuneAltul: defectiuneEsteAltulInput.checked
        };

        showLoading(true);
        try {
            if (id) { // Editare
                await apiCall(`/api/config/mentenanta/defectiuni/${id}`, { method: 'PUT', body: JSON.stringify({ id: parseInt(id), ...data }) });
                showToast("Defecțiune actualizată.", 'success');
            } else { // Adăugare
                await apiCall('/api/config/mentenanta/defectiuni', { method: 'POST', body: JSON.stringify(data) });
                showToast("Defecțiune adăugată.", 'success');
            }
            clearDefectiuneForm();
            await loadDefectiuni();
            renderDefectiuniTable();
        } catch (error) {
            // Eroarea e deja afișată
        } finally {
            showLoading(false);
        }
    });

    defectiuneTbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('.btn-edit');
        const deleteBtn = e.target.closest('.btn-danger');

        if (editBtn) {
            const row = editBtn.closest('tr');
            const item = JSON.parse(row.dataset.item);
            if (item) {
                defectiuneIdInput.value = item.id;
                defectiuneNumeInput.value = item.nume;
                defectiuneEsteAltulInput.checked = item.esteOptiuneAltul;
                
                defectiuneSubmitBtn.textContent = 'Actualizează Defecțiune';
                defectiuneSubmitBtn.classList.replace('btn-primary', 'btn-success');
                defectiuneClearBtn.classList.remove('hidden');
            }
        }

        if (deleteBtn) {
            const id = deleteBtn.dataset.id;
            if (confirm("Sunteți sigur că doriți să ștergeți această defecțiune? Aceasta va șterge și corelările asociate!")) {
                handleDeleteDefectiune(id);
            }
        }
    });

    async function handleDeleteDefectiune(id) {
        showLoading(true);
        try {
            await apiCall(`/api/config/mentenanta/defectiuni/${id}`, { method: 'DELETE' });
            showToast("Defecțiune ștearsă.", 'success');
            await loadAllData(); // Reîncarcă tot
        } catch (error) {
            // Eroarea e deja afișată
        } finally {
            showLoading(false);
        }
    }

    function clearDefectiuneForm() {
        defectiuneForm.reset();
        defectiuneIdInput.value = '';
        defectiuneEsteAltulInput.checked = false;
        defectiuneSubmitBtn.textContent = 'Adaugă Defecțiune';
        defectiuneSubmitBtn.classList.replace('btn-success', 'btn-primary');
        defectiuneClearBtn.classList.add('hidden');
    }
    defectiuneClearBtn.addEventListener('click', clearDefectiuneForm);


    // --- LOGICĂ CORELĂRI ---

    // Când se selectează o problemă, încărcăm defecțiunile asociate
    corelatieProblemaSelect.addEventListener('change', async (e) => {
        const problemaId = e.target.value;
        if (!problemaId) {
            corelatieDefectiuniContainer.innerHTML = '<p>Selectați o problemă mai întâi...</p>';
            return;
        }

        showLoading(true);
        corelatieDefectiuniContainer.innerHTML = '';

        try {
            // 1. Luăm corelațiile existente pentru această problemă
            const corelatiiExistente = await apiCall(`/api/config/mentenanta/corelatii/${problemaId}`);
            const defectiuniAsociateIds = corelatiiExistente.map(c => c.defectiuneIdentificataId);

            // 2. Afișăm TOATE defecțiunile ca checkbox-uri
            if(defectiuniData.length === 0) {
                 corelatieDefectiuniContainer.innerHTML = '<p>Nu există defecțiuni definite în sistem.</p>';
                 return;
            }
            
            defectiuniData.forEach(defectiune => {
                const isChecked = defectiuniAsociateIds.includes(defectiune.id);
                
                const group = document.createElement('div');
                group.classList.add('checkbox-group');
                group.innerHTML = `
                    <input type="checkbox" id="defectiune-check-${defectiune.id}" value="${defectiune.id}" ${isChecked ? 'checked' : ''}>
                    <label for="defectiune-check-${defectiune.id}">${escapeHTML(defectiune.nume)}</label>
                `;
                corelatieDefectiuniContainer.appendChild(group);
            });

        } catch (error) {
            corelatieDefectiuniContainer.innerHTML = '<p style="color: red;">Eroare la încărcarea corelărilor.</p>';
        } finally {
            showLoading(false);
        }
    });

    // Salvarea corelărilor
    corelatieForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const problemaId = parseInt(corelatieProblemaSelect.value);
        if (isNaN(problemaId)) {
            showToast("Vă rugăm selectați o problemă.", 'error');
            return;
        }

        // Găsim toate ID-urile bifate
        const checkedBoxes = corelatieDefectiuniContainer.querySelectorAll('input[type="checkbox"]:checked');
        const defectiuneIds = Array.from(checkedBoxes).map(cb => parseInt(cb.value));
        
        const data = {
            problemaRaportataId: problemaId,
            defectiuniIds: defectiuneIds
        };

        showLoading(true);
        try {
            // API-ul de corelatii (POST) ar trebui să șteargă și să adauge noile corelări
            await apiCall('/api/config/mentenanta/corelatii', { method: 'POST', body: JSON.stringify(data) });
            showToast("Corelările au fost salvate cu succes.", 'success');
        } catch (error) {
             // Eroarea e deja afișată
        } finally {
            showLoading(false);
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