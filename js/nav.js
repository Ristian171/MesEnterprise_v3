/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/nav.js
 * ROL: UI/UX Designer
 * SCOP: Piesa centrală a noului UI.
 * - Injectează un antet de navigare standardizat pe toate paginile.
 * - Gestionează autentificarea și afișarea meniului în funcție de rol.
 * - Gestionează delogarea.
 *
 * MODIFICARE (Senior Dev):
 * - CORECTAT PARSARE JWT: Numele de utilizator este în claim-ul "name"
 * (din ClaimTypes.Name), nu "DisplayName".
 * ===================================================================================
 */

document.addEventListener("DOMContentLoaded", () => {
    // 1. Definim placeholder-ul unde va fi injectat antetul
    const headerPlaceholder = document.getElementById("header-placeholder");
    
    // Nu facem nimic pe pagina de login
    if (!headerPlaceholder) {
        return;
    }

    // 2. Verificare Autentificare
    const token = localStorage.getItem('token');
    if (!token) {
        // Dacă nu e token și nu suntem pe pagina de login, redirect
        if (window.location.pathname !== "/login.html") {
            window.location.href = '/login.html';
        }
        return;
    }

    let username = "Utilizator";
    let role = "Operator"; // Default

    try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        
        // ======================= ÎNCEPUT CORECȚIE =======================
        // Claim-ul generat de TokenService.cs este "name" (din ClaimTypes.Name)
        username = payload.name; // << CORECTAT
        // ======================= SFÂRȘIT CORECȚIE =======================

        // Rolul poate fi un string sau un array, normalizăm
        // Căutăm cheia C# "ClaimTypes.Role" (schema URI)
        const rolePayload = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

        if (Array.isArray(rolePayload)) {
            // Prioritizăm rolurile: Admin > Technician > Operator
            if (rolePayload.includes("Admin")) role = "Admin";
            else if (rolePayload.includes("Technician")) role = "Technician";
            else if (rolePayload.includes("Operator")) role = "Operator";
        } else if (rolePayload) { 
            role = rolePayload;
        }
    } catch (e) {
        console.error("Token invalid:", e);
        localStorage.removeItem('token');
        window.location.href = '/login.html';
        return;
    }

    // 3. Definim Link-urile de Navigare pe bază de Rol
    const navLinks = {
        Operator: [
            { href: "/index.html", text: "Producție" },
            { href: "/changeover.html", text: "Changeover" }
        ],
        Technician: [
            { href: "/index.html", text: "Producție" },
            { href: "/changeover.html", text: "Changeover" },
            { href: "/interventii.html", text: "Mentenanță" }
        ],
        Admin: [
            { href: "/index.html", text: "Producție" },
            { href: "/changeover.html", text: "Changeover" },
            { href: "/interventii.html", text: "Mentenanță" },
            { href: "/edit.html", text: "Editare Date" },
            { href: "/config.html", text: "Configurare" }
        ]
    };

    // 4. Stabilim link-urile pentru rolul curent
    const links = navLinks[role] || navLinks.Operator;
    const currentPath = window.location.pathname;

    let navHtml = "";
    links.forEach(link => {
        // MODIFICARE: Verificăm dacă link-ul începe cu href-ul, pentru a activa și sub-paginile
        const isActive = (currentPath === "/" && link.href === "/index.html") || 
                         (currentPath !== "/" && currentPath.startsWith(link.href));
        navHtml += `<a href="${link.href}" class="${isActive ? 'active' : ''}">${link.text}</a>`;
    });

    // 5. Creăm HTML-ul pentru antet
    const headerHtml = `
        <header>
            <h1>MesSimplu</h1>
            <nav>
                ${navHtml}
            </nav>
            <div id="user-info">
                <span id="username-display">${username} (${role})</span>
                <button id="logout-button" title="Delogare">
                    <i class="fa-solid fa-right-from-bracket"></i>
                </button>
            </div>
        </header>
    `;

    // 6. Injectăm antetul în pagină
    headerPlaceholder.innerHTML = headerHtml;

    // 7. Adăugăm funcționalitatea de delogare
    const logoutButton = document.getElementById("logout-button");
    if (logoutButton) {
        logoutButton.addEventListener("click", () => {
            localStorage.removeItem('token');
            window.location.href = '/login.html';
        });
    }

    // 8. Stocăm global rolul pentru scripturile de pe pagină (ex: ascundere butoane)
    window.CURRENT_USER_ROLE = role;
});