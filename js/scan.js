/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/scan.js
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

    // Elemente DOM
    const form = document.getElementById('scan-form');
    const scanInput = document.getElementById('scan-input');
    const resultOutput = document.getElementById('scan-result-output');

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
        
        // (UI/UX) Focalizează automat pe câmpul de scanare
        scanInput.focus();
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
     * Procesarea scanării
     */
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const scanValue = scanInput.value;
        if (!scanValue.trim()) {
            return;
        }

        showLoading(true);
        resultOutput.innerHTML = '<p style="text-align: center;">Se procesează...</p>';
        
        try {
            const data = {
                scanData: scanValue
            };
            
            // Presupunem endpoint-ul
            const result = await apiCall('/api/scan', { method: 'POST', body: JSON.stringify(data) });

            // (UI/UX) Afișare feedback pozitiv
            let resultHtml = `
                <p style="color: var(--color-success); font-weight: 600; text-align: center; font-size: 1.2rem;">
                    <i class="fa-solid fa-check-circle"></i> Scanare reușită!
                </p>
                <p><strong>Acțiune:</strong> ${escapeHTML(result.action)}</p>
                <p><strong>Detalii:</strong> ${escapeHTML(result.message)}</p>
            `;
            resultOutput.innerHTML = resultHtml;
            
            showToast("Scanare procesată.", 'success');

        } catch (error) {
            // (UI/UX) Afișare feedback negativ
            let resultHtml = `
                <p style="color: var(--color-danger); font-weight: 600; text-align: center; font-size: 1.2rem;">
                    <i class="fa-solid fa-times-circle"></i> Eroare Scanare!
                </p>
                <p><strong>Cod:</strong> ${escapeHTML(scanValue)}</p>
                <p><strong>Motiv:</strong> ${escapeHTML(error.message)}</p>
            `;
            resultOutput.innerHTML = resultHtml;
        } finally {
            showLoading(false);
            // (UI/UX) Resetează și refocalizează câmpul
            scanInput.value = '';
            scanInput.focus();
        }
    });

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