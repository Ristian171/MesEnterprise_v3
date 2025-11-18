/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/config-setari.js
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
    const form = document.getElementById('setari-form');
    const goodPartsLoggingModeSelect = document.getElementById('good-parts-logging-mode');
    const downtimeScrapLoggingModeSelect = document.getElementById('downtime-scrap-logging-mode');
    const justificationThresholdPercentInput = document.getElementById('justification-threshold-percent');
    const requireJustificationCheckbox = document.getElementById('require-justification');
    const autoCloseTicketMinutesInput = document.getElementById('auto-close-ticket-minutes');

    // Obține token-ul o singură dată la încărcare
    const token = localStorage.getItem('token');

    // Numele setărilor (chei) așa cum sunt definite în API
    const settingsKeys = {
        GoodPartsLoggingMode: "GoodPartsLoggingMode",
        DowntimeScrapLoggingMode: "DowntimeScrapLoggingMode",
        JustificationThresholdPercent: "JustificationThresholdPercent",
        RequireJustification: "RequireJustification",
        AutoCloseTicketMinutes: "AutoCloseTicketMinutes"
    };

    /**
     * Funcția principală de inițializare a paginii
     */
    async function initializePage() {
        if (!token) {
            console.error("Token lipsă, autentificare eșuată.");
            return;
        }
        
        showLoading(true);
        await loadSettings();
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
     * Încarcă setările globale
     */
    async function loadSettings() {
        try {
            // Presupunem că endpoint-ul este /api/config/settings și returnează un array de { key, value }
            // CORECȚIE: API-ul returnează un obiect AppSettingsDto, nu un array de { key, value }
            const settings = await apiCall('/api/config/settings'); 
            
            goodPartsLoggingModeSelect.value = settings.goodPartsLoggingMode || "Overwrite";
            downtimeScrapLoggingModeSelect.value = settings.downtimeScrapLoggingMode || "Overwrite";
            justificationThresholdPercentInput.value = settings.justificationThresholdPercent || 85;
            requireJustificationCheckbox.checked = settings.requireJustification;
            autoCloseTicketMinutesInput.value = settings.autoCloseTicketMinutes || 0;

        } catch (error) {
            showToast("Eroare la încărcarea setărilor.", 'error');
        }
    }

    /**
     * Salvarea setărilor
     */
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        // Validăm valorile
        const justificationThreshold = parseInt(justificationThresholdPercentInput.value);
        const autoCloseMinutes = parseInt(autoCloseTicketMinutesInput.value);

        if (isNaN(justificationThreshold) || justificationThreshold < 0 || justificationThreshold > 100) {
            showToast("Pragul de justificare trebuie să fie un număr între 0 și 100.", 'error');
            return;
        }
        if (isNaN(autoCloseMinutes) || autoCloseMinutes < 0) {
            showToast("Minutele pentru închiderea automată a tichetelor trebuie să fie un număr pozitiv sau zero.", 'error');
            return;
        }

        // CORECȚIE: Construim payload-ul ca obiect AppSettingsDto
        const settingsPayload = {
            goodPartsLoggingMode: goodPartsLoggingModeSelect.value,
            downtimeScrapLoggingMode: downtimeScrapLoggingModeSelect.value,
            justificationThresholdPercent: justificationThreshold,
            requireJustification: requireJustificationCheckbox.checked,
            autoCloseTicketMinutes: autoCloseMinutes
        };

        showLoading(true);
        try {
            // CORECȚIE: Endpoint-ul de salvare este PUT și acceptă obiectul DTO
            await apiCall('/api/config/settings', { method: 'PUT', body: JSON.stringify(settingsPayload) });
            showToast("Setările au fost salvate cu succes.", 'success');
        } catch (error) {
            // Eroarea e deja afișată de apiCall
        } finally {
            showLoading(false);
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