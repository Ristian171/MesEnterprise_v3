/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/config-export.js
 * ROL: Senior Software Developer
 * STARE: REFACTORIZAT (Curățat + Adăugat Flatpickr + Logică Export)
 *
 * MODIFICARE (Refactorizare):
 * - ELIMINAT: Logica redundantă de autentificare (verificare token, etc.).
 * - ELIMINAT: Listener-ul redundant pentru #logout-button.
 * - ADĂUGAT: Inițializarea 'flatpickr' pentru câmpurile de dată/oră.
 * - ADĂUGAT: Logică pentru a încărca liniile.
 * - ADĂUGAT: Logică pentru a apela API-ul de export și a descărca fișierul blob.
 * ===================================================================================
 */

document.addEventListener("DOMContentLoaded", () => {

    // --- NOTĂ: Blocul de autentificare a fost eliminat. ---

    // Elemente DOM
    const form = document.getElementById('export-form');
    const tipSelect = document.getElementById('export-tip');
    const linieSelect = document.getElementById('export-linie');
    const dataStartInput = document.getElementById('export-data-start');
    const dataStopInput = document.getElementById('export-data-stop');
    const exportButton = document.getElementById('export-button');

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
     * Inițializează Flatpickr pentru câmpurile de dată și oră
     */
    function initializePickers() {
        if (typeof flatpickr === 'undefined') {
            console.warn("Biblioteca Flatpickr nu este încărcată. Selectoarele nu vor funcționa.");
            return;
        }
        
        // (MES Engineer UI/UX): Exporturile necesită precizie de timp
        const dateTimeOptions = {
            enableTime: true,
            dateFormat: "Y-m-d H:i", // Format compatibil ISO cu ora
            time_24hr: true,
            allowInput: true
        };

        flatpickr(dataStartInput, dateTimeOptions);
        flatpickr(dataStopInput, dateTimeOptions);
    }

    // --- LOGICĂ GENERICĂ API (Doar pentru JSON) ---
    
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
            if (response.status === 204) return null;
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
            const linii = await apiCall('/api/config/lines');
            // Nu folosim populateSelect deoarece avem nevoie de opțiunea "Toate Liniile"
            linieSelect.innerHTML = `
                <option value="">Selectați linia...</option>
                <option value="all">Toate Liniile</option>
            `;
            linii.forEach(item => {
                const option = document.createElement("option");
                option.value = item.id;
                option.textContent = item.name;
                linieSelect.appendChild(option);
            });

        } catch (error) {
            showToast("Eroare la încărcarea liniilor.", 'error');
        }
    }

    /**
     * Gestionarea trimiterii formularului de export
     */
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const tipRaport = tipSelect.value;
        const linieId = linieSelect.value;
        const dataStart = dataStartInput.value;
        const dataStop = dataStopInput.value;

        if (!tipRaport || !linieId || !dataStart || !dataStop) {
            showToast("Vă rugăm completați toate câmpurile.", 'error');
            return;
        }

        // (MES Engineer): Convertim orele locale în ISO string (UTC) pentru API
        const dataStartISO = new Date(dataStart).toISOString();
        const dataStopISO = new Date(dataStop).toISOString();

        const params = new URLSearchParams({
            linieId: linieId, // API-ul ar trebui să gestioneze "all"
            dataStart: dataStartISO,
            dataStop: dataStopISO
        });
        
        const url = `/api/export/${tipRaport}?${params.toString()}`;

        showLoading(true);
        exportButton.disabled = true;

        try {
            // (Dev): Tranzacțiile de fișiere nu folosesc apiCall,
            // deoarece ne așteptăm la un blob, nu JSON.
            const response = await fetch(url, {
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (!response.ok) {
                // Încercăm să citim eroarea ca JSON
                const errorData = await response.json().catch(() => null);
                const message = errorData?.message || `Eroare ${response.status} la generarea raportului.`;
                throw new Error(message);
            }

            // Extragem numele fișierului din header (dacă API-ul îl setează)
            const disposition = response.headers.get('content-disposition');
            let fileName = `${tipRaport}_${new Date().toISOString().split('T')[0]}.xlsx`;
            if (disposition && disposition.indexOf('attachment') !== -1) {
                const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                const matches = filenameRegex.exec(disposition);
                if (matches != null && matches[1]) {
                    fileName = matches[1].replace(/['"]/g, '');
                }
            }

            const blob = await response.blob();
            
            // (UI/UX): Creăm un link "fals" pentru a declanșa descărcarea
            const downloadUrl = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.style.display = 'none';
            a.href = downloadUrl;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(downloadUrl);
            a.remove();
            
            showToast("Raportul a fost descărcat.", 'success');

        } catch (error) {
            showToast(`Eroare: ${error.message}`, 'error');
        } finally {
            showLoading(false);
            exportButton.disabled = false;
        }
    });

    // --- UTILITARE ---
    
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