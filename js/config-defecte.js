/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/config-defecte.js
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
    
    // --- NOTĂ: Blocul de autentificare a fost eliminat. ---

    // Elemente DOM Motive Staționare
    const motivForm = document.getElementById('motiv-form');
    const motivIdInput = document.getElementById('motiv-id');
    const motivCodInput = document.getElementById('motiv-cod');
    const motivNumeInput = document.getElementById('motiv-nume');
    const motivTipSelect = document.getElementById('motiv-tip');
    const motivTbody = document.getElementById('motive-tbody');
    const motivSubmitBtn = motivForm.querySelector('button[type="submit"]');
    const motivClearBtn = document.getElementById('motiv-clear-btn');

    // Elemente DOM Coduri Defect
    const defectForm = document.getElementById('defect-form');
    const defectIdInput = document.getElementById('defect-id');
    const defectCodInput = document.getElementById('defect-cod');
    const defectNumeInput = document.getElementById('defect-nume');
    const defectTipSelect = document.getElementById('defect-tip');
    const defectTbody = document.getElementById('defecte-tbody');
    const defectSubmitBtn = defectForm.querySelector('button[type="submit"]');
    const defectClearBtn = document.getElementById('defect-clear-btn');

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
     * Încarcă toate datele (motive și defecte) în paralel
     */
    async function loadAllData() {
        await Promise.all([
            loadMotive(),
            loadDefecte()
        ]);
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

    // --- LOGICĂ MOTIVE STAȚIONARE (DOWNTIME) ---

    async function loadMotive() {
        try {
            const motive = await apiCall('/api/config/breakdownreasons');
            renderMotiveTable(motive);
        } catch (error) {
            showToast("Eroare la încărcarea motivelor de staționare.", 'error');
        }
    }

    function renderMotiveTable(motive) {
        motivTbody.innerHTML = '';
        if (motive.length === 0) {
            motivTbody.innerHTML = '<tr><td colspan="4" style="text-align: center;">Nu există motive definite.</td></tr>';
            return;
        }
        
        motive.forEach(motiv => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${escapeHTML(motiv.name)}</td>
                <td class="actions">
                    <button class="btn btn-edit" data-id="${motiv.id}"><i class="fa-solid fa-pencil"></i></button>
                    <button class="btn btn-danger" data-id="${motiv.id}"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            row.dataset.motiv = JSON.stringify(motiv); // Stocăm datele
            motivTbody.appendChild(row);
        });
    }

    motivForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = motivIdInput.value;
        const data = {
            name: motivNumeInput.value
        };

        showLoading(true);
        try {
            if (id) { // Editare
                await apiCall(`/api/config/breakdownreasons/${id}`, { method: 'PUT', body: JSON.stringify({id: parseInt(id), ...data}) });
                showToast("Motiv actualizat.", 'success');
            } else { // Adăugare
                await apiCall('/api/config/breakdownreasons', { method: 'POST', body: JSON.stringify(data) });
                showToast("Motiv adăugat.", 'success');
            }
            clearMotivForm();
            await loadMotive();
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    });

    motivTbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('.btn-edit');
        const deleteBtn = e.target.closest('.btn-danger');

        if (editBtn) {
            const row = editBtn.closest('tr');
            const motiv = JSON.parse(row.dataset.motiv);
            if (motiv) {
                motivIdInput.value = motiv.id;
                motivNumeInput.value = motiv.name;
                motivSubmitBtn.textContent = 'Actualizează Motiv';
                motivSubmitBtn.classList.replace('btn-primary', 'btn-success');
                motivClearBtn.classList.remove('hidden');
            }
        }

        if (deleteBtn) {
            const id = deleteBtn.dataset.id;
            if (confirm("Sunteți sigur că doriți să ștergeți acest motiv de staționare?")) {
                handleDeleteMotiv(id);
            }
        }
    });

    async function handleDeleteMotiv(id) {
        showLoading(true);
        try {
            await apiCall(`/api/config/breakdownreasons/${id}`, { method: 'DELETE' });
            showToast("Motiv șters.", 'success');
            await loadMotive();
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    }

    function clearMotivForm() {
        motivForm.reset();
        motivIdInput.value = '';
        motivSubmitBtn.textContent = 'Adaugă Motiv';
        motivSubmitBtn.classList.replace('btn-success', 'btn-primary');
        motivClearBtn.classList.add('hidden');
    }
    motivClearBtn.addEventListener('click', clearMotivForm);


    // --- LOGICĂ CODURI DEFECT (SCRAP) ---

    async function loadDefecte() {
        try {
            const defecte = await apiCall('/api/config/defectcategories');
            renderDefecteTable(defecte);
        } catch (error) {
            showToast("Eroare la încărcarea codurilor de defect.", 'error');
        }
    }

    function renderDefecteTable(defecte) {
        defectTbody.innerHTML = '';
        if (defecte.length === 0) {
            defectTbody.innerHTML = '<tr><td colspan="4" style="text-align: center;">Nu există coduri de defect definite.</td></tr>';
            return;
        }

        defecte.forEach(defect => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${escapeHTML(defect.name)}</td>
                <td class="actions">
                    <button class="btn btn-edit" data-id="${defect.id}"><i class="fa-solid fa-pencil"></i></button>
                    <button class="btn btn-danger" data-id="${defect.id}"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            row.dataset.defect = JSON.stringify(defect); // Stocăm datele
            defectTbody.appendChild(row);
        });
    }

    defectForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = defectIdInput.value;
        const data = {
            name: defectNumeInput.value
        };

        showLoading(true);
        try {
            if (id) { // Editare
                await apiCall(`/api/config/defectcategories/${id}`, { method: 'PUT', body: JSON.stringify({id: parseInt(id), ...data}) });
                showToast("Defect actualizat.", 'success');
            } else { // Adăugare
                await apiCall('/api/config/defectcategories', { method: 'POST', body: JSON.stringify(data) });
                showToast("Defect adăugat.", 'success');
            }
            clearDefectForm();
            await loadDefecte();
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    });

    defectTbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('.btn-edit');
        const deleteBtn = e.target.closest('.btn-danger');

        if (editBtn) {
            const row = editBtn.closest('tr');
            const defect = JSON.parse(row.dataset.defect);
            if (defect) {
                defectIdInput.value = defect.id;
                defectNumeInput.value = defect.name;
                
                defectSubmitBtn.textContent = 'Actualizează Defect';
                defectSubmitBtn.classList.replace('btn-primary', 'btn-success');
                defectClearBtn.classList.remove('hidden');
            }
        }

        if (deleteBtn) {
            const id = deleteBtn.dataset.id;
            if (confirm("Sunteți sigur că doriți să ștergeți acest cod de defect?")) {
                handleDeleteDefect(id);
            }
        }
    });

    async function handleDeleteDefect(id) {
        showLoading(true);
        try {
            await apiCall(`/api/config/defectcategories/${id}`, { method: 'DELETE' });
            showToast("Defect șters.", 'success');
            await loadDefecte();
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
        }
    }

    function clearDefectForm() {
        defectForm.reset();
        defectIdInput.value = '';
        defectSubmitBtn.textContent = 'Adaugă Defect';
        defectSubmitBtn.classList.replace('btn-success', 'btn-primary');
        defectClearBtn.classList.add('hidden');
    }
    defectClearBtn.addEventListener('click', clearDefectForm);

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