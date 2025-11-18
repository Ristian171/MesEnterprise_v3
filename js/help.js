/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/help.js
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
    // nav.js se ocupă de verificarea token-ului și redirect.
    // nav.js se ocupă de afișarea numelui de utilizator.
    // nav.js se ocupă de butonul de delogare.

    // Pagina de Ajutor este statică și nu necesită alt JavaScript
    // specific în acest moment.
});

// --- Funcțiile globale (showLoading, showToast) ---
// Definite în alte scripturi, dar disponibile global dacă e nevoie.

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