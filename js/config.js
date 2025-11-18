/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/config.js
 * ROL: Senior Software Developer
 * STARE: REFACTORIZAT (Curățat de logica de autentificare)
 *
 * MODIFICARE (Refactorizare):
 * - ELIMINAT: Logica redundantă de autentificare (verificare token,
 * parsare payload, update #username-display).
 * - ELIMINAT: Listener-ul redundant pentru #logout-button.
 * - JUSTIFICARE: Noul script global 'nav.js' gestionează acum centralizat
 * autentificarea. Pagina config.html oricum nu are logică
 * specifică în afara celei din nav.js, dar curățăm acest fișier
 * pentru a preveni erorile.
 * ===================================================================================
 */

document.addEventListener("DOMContentLoaded", () => {
    
    // --- NOTĂ: Blocul de autentificare a fost eliminat. ---
    // nav.js se ocupă de verificarea token-ului și redirect.
    // nav.js se ocupă de afișarea numelui de utilizator.
    // nav.js se ocupă de butonul de delogare.

    // Acest fișier (config.js) poate fi folosit în viitor pentru 
    // a ascunde/afișa anumite carduri din hub pe bază de roluri
    // suplimentare, dacă logica din nav.js nu este suficientă.
    
    // De exemplu, dacă doar anumiți Admini pot vedea "Administrare":
    // const userRole = window.CURRENT_USER_ROLE;
    // if (userRole && userRole === "Admin") {
    //     // Poate o logică viitoare aici
    // }

});

// --- Funcțiile globale (showLoading, showToast) ---
// Acestea sunt definite în alte scripturi (ex: index.js)
// și sunt disponibile global dacă este necesar,
// deși această pagină specifică (config.html) nu le folosește.

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