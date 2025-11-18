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
        
        // Load or prompt for scan identifier
        let identifier = localStorage.getItem('scanIdentifier');
        if (!identifier) {
            identifier = prompt('Introduceți codul identificator al stației de scan (ex: SCAN-L1):');
            if (identifier && identifier.trim()) {
                localStorage.setItem('scanIdentifier', identifier.trim());
            } else {
                alert('Identificator necesar pentru funcționarea stației de scan.');
                return;
            }
        }
        
        // Display current identifier
        const identifierDisplay = document.createElement('p');
        identifierDisplay.style.marginBottom = 'var(--spacing-m)';
        identifierDisplay.innerHTML = `<strong>Stație:</strong> ${escapeHTML(identifier)} 
            <button onclick="localStorage.removeItem('scanIdentifier'); location.reload();" 
                    style="margin-left: var(--spacing-s); font-size: 0.9rem;">
                Schimbă
            </button>`;
        document.querySelector('.config-card').insertBefore(
            identifierDisplay, 
            document.getElementById('scan-form')
        );
        
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

        const identifier = localStorage.getItem('scanIdentifier');
        if (!identifier) {
            alert('Identificator lipsă. Reîncărcați pagina pentru a configura.');
            return;
        }

        showLoading(true);
        resultOutput.innerHTML = '<p style="text-align: center;">Se procesează...</p>';
        
        try {
            const data = {
                identifier: identifier,
                scanData: scanValue
            };
            
            // Call the new endpoint
            const result = await apiCall('/api/production/scan', { method: 'POST', body: JSON.stringify(data) });

            // (UI/UX) Afișare feedback pozitiv
            let resultHtml = `
                <p style="color: var(--color-success); font-weight: 600; text-align: center; font-size: 1.2rem;">
                    <i class="fa-solid fa-check-circle"></i> Scanare reușită!
                </p>
                <p><strong>Acțiune:</strong> ${escapeHTML(result.action || 'processed')}</p>
                <p><strong>Detalii:</strong> ${escapeHTML(result.message || 'Scanare procesată cu succes')}</p>
            `;
            
            if (result.lineName) {
                resultHtml += `<p><strong>Linie:</strong> ${escapeHTML(result.lineName)}</p>`;
            }
            if (result.goodParts !== undefined) {
                resultHtml += `<p><strong>Piese bune (ora curentă):</strong> ${result.goodParts} / ${result.targetParts || '?'}</p>`;
            }
            
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