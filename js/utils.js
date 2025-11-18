/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/utils.js
 * ROL: Senior Software Developer
 * SCOP: Centralizarea funcțiilor utilitare JavaScript.
 *
 * MODIFICARE (Refactorizare):
 * - CREAT: Acest fișier a fost creat pentru a elimina duplicarea codului.
 * - CENTRALIZAT: Funcțiile showToast, showLoading, escapeHTML, populateSelect,
 *   formatDateTime și formatTime sunt acum definite într-un singur loc.
 * - JUSTIFICARE: Previne necesitatea de a modifica aceeași funcție în 10+
 *   fișiere diferite. Îmbunătățește mentenabilitatea și reduce riscul de erori.
 * ===================================================================================
 */

/**
 * Afișează un mesaj toast.
 * @param {string} message Mesajul de afișat.
 * @param {'info' | 'success' | 'error' | 'warn'} type Tipul de mesaj.
 */
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

/**
 * Afișează sau ascunde overlay-ul de încărcare.
 * @param {boolean} isLoading True pentru a afișa, false pentru a ascunde.
 */
function showLoading(isLoading) {
    const overlay = document.getElementById('loading-overlay');
    if (overlay) {
        overlay.classList.toggle('hidden', !isLoading);
    }
}

/**
 * Previne XSS (Cross-Site Scripting) prin "escaparea" caracterelor HTML.
 * @param {string} str String-ul de escapat.
 * @returns {string} String-ul escapat.
 */
function escapeHTML(str) {
    if (str === null || str === undefined) return '';
    return str.toString()
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

/**
 * Populează un element <select> cu opțiuni dintr-un array de date.
 * @param {HTMLSelectElement} selectElement Elementul select.
 * @param {Array<object>} data Array-ul de obiecte.
 * @param {string} textKey Cheia pentru textul opțiunii.
 * @param {string} valueKey Cheia pentru valoarea opțiunii.
 * @param {string} defaultOptionText Textul pentru opțiunea default.
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

function formatDateTime(isoString) {
    if (!isoString) return "N/A";
    try {
        const date = new Date(isoString);
        const options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', hour12: false };
        return new Intl.DateTimeFormat('ro-RO', options).format(date).replace(',', '');
    } catch (e) {
        return isoString;
    }
}

function formatTime(timeSpanString) {
    if (!timeSpanString) return "";
    return timeSpanString.substring(0, 5); // Ia doar HH:mm
}