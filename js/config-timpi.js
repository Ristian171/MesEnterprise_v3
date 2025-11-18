/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/config-timpi.js
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
    const form = document.getElementById('timp-planificat-form');
    const idInput = document.getElementById('timp-planificat-id');
    const produsSelect = document.getElementById('produs-id');
    const minuteInput = document.getElementById('timp-minute');
    const tbody = document.getElementById('timpi-planificati-tbody');
    const submitBtn = form.querySelector('button[type="submit"]');
    const clearBtn = document.getElementById('timp-planificat-clear-btn');

    // Stocare locală
    let produseData = [];
    let timpiPlanificatiData = [];

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
     * Încarcă toate datele (produse și timpii setați)
     */
    async function loadAllData() {
        try {
            // Încărcăm produsele mai întâi pentru a popula dropdown-ul
            await loadProduse();
            // Apoi încărcăm setările de timp existente
            await loadTimpiPlanificati();
            
            // Renderizăm tabelul
            renderTable();
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

    // --- LOGICA PAGINEI ---

    /**
     * Încarcă lista de produse pentru dropdown
     */
    async function loadProduse() {
        try {
            produseData = await apiCall('/api/config/products');
            populateSelect(produsSelect, produseData, 'name', 'id', 'Selectați produsul...');
            // Adaugă și codul produsului pentru claritate (UI/UX)
            produsSelect.querySelectorAll('option').forEach(opt => {
                if (opt.value && produseData.length > 0) { // Verificăm dacă există produse
                    const produs = produseData.find(p => p.id == opt.value);
                    if (produs) {
                        opt.textContent = `(${produs.productCode}) ${produs.name}`;
                    }
                }
            });
        } catch (error) {
            showToast("Eroare la încărcarea produselor.", 'error');
        }
    }

    /**
     * Încarcă timpii planificați existenți
     */
    async function loadTimpiPlanificati() {
        try {
            // Presupunem că endpoint-ul este /api/config/planneddowntime
            // Acesta ar trebui să returneze o listă de obiecte
            // { id, productId, plannedMinutes }
            timpiPlanificatiData = await apiCall('/api/config/planneddowntime'); // Endpoint corect
        } catch (error) {
            showToast("Eroare la încărcarea timpilor planificați.", 'error');
        }
    }

    /**
     * Afișează datele în tabel
     */
    function renderTable() {
        tbody.innerHTML = '';
        if (timpiPlanificatiData.length === 0) {
            tbody.innerHTML = '<tr><td colspan="4" style="text-align: center;">Nu există timpi planificați definiți.</td></tr>';
            return;
        }
        
        timpiPlanificatiData.forEach(timp => {
            const produs = produseData.find(p => p.id === timp.productId);
            if (!produs) return; // Skip dacă produsul nu (mai) există (sau nu a fost încărcat)

            const row = document.createElement('tr');
            row.innerHTML = ` 
                <td>${escapeHTML(produs.productCode)}</td>
                <td>${escapeHTML(produs.name)}</td>
                <td>${timp.plannedMinutes} minute</td>
                <td class="actions">
                    <button class="btn btn-edit" data-id="${timp.id}"><i class="fa-solid fa-pencil"></i></button>
                    <button class="btn btn-danger" data-id="${timp.id}"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            row.dataset.timp = JSON.stringify(timp); // Stocăm datele
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
            productId: parseInt(produsSelect.value),
            minutesPerHour: parseInt(minuteInput.value) // CORECȚIE: minutesPerHour, nu plannedMinutes
        }; 

        if (isNaN(data.productId)) {
            showToast("Vă rugăm selectați un produs.", 'error');
            return;
        }
        if (isNaN(data.plannedMinutes) || data.plannedMinutes < 0) {
            showToast("Numărul de minute trebuie să fie pozitiv.", 'error');
            return;
        }

        showLoading(true);
        try {
            if (id) { // Editare
                // CORECȚIE: Endpoint-ul POST gestionează upsert-ul. Trimitem ID-ul în body.
                await apiCall('/api/config/planneddowntime', { method: 'POST', body: JSON.stringify({ id: parseInt(id), ...data }) });
                showToast("Timp planificat actualizat.", 'success');
            } else { // Adăugare
                await apiCall('/api/config/planneddowntime', { method: 'POST', body: JSON.stringify(data) });
                showToast("Timp planificat adăugat.", 'success');
            }
            clearForm();
            await loadTimpiPlanificati(); // Reîncarcă timpii
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
            const timp = JSON.parse(row.dataset.timp);
            if (timp) {
                idInput.value = timp.id;
                produsSelect.value = timp.productId;
                minuteInput.value = timp.minutesPerHour; // CORECȚIE: minutesPerHour
                
                submitBtn.textContent = 'Actualizează Timp';
                submitBtn.classList.replace('btn-primary', 'btn-success');
                clearBtn.classList.remove('hidden');
                
                produsSelect.disabled = true; // Nu permitem schimbarea produsului la editare
            }
        }

        if (deleteBtn) {
            const id = deleteBtn.dataset.id;
            if (confirm("Sunteți sigur că doriți să ștergeți această setare de timp planificat?")) {
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
            await apiCall(`/api/config/planneddowntime/${id}`, { method: 'DELETE' });
            showToast("Setare timp șters.", 'success');
            await loadTimpiPlanificati();
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
        produsSelect.disabled = false;
        submitBtn.textContent = 'Setează Timp';
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