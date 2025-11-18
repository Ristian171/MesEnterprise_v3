/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/interventii.js
 * ROL: Senior Software Developer
 * STARE: REFACTORIZAT (Curățat de logica de autentificare)
 *
 * MODIFICARE (Refactorizare):
 * - ELIMINAT: Logica redundantă de autentificare (verificare token,
 * parsare payload, update #username-display).
 * - ELIMINAT: Listener-ul redundant pentru #logout-button.
 * - JUSTIFICARE: Noul script global 'nav.js' gestionează acum centralizat
 * autentificarea, redirect-ul, afișarea numelui de utilizator/rol
 * și funcționalitatea de delogare pentru TOATĂ aplicația.
 * Acest script se concentrează acum doar pe logica paginii de intervenții.
 * ===================================================================================
 */

document.addEventListener("DOMContentLoaded", () => {
    
    // --- NOTĂ: Blocul de autentificare a fost eliminat. ---
    // nav.js gestionează acum verificarea token-ului și redirect-ul.

    // --- Definire Elemente DOM ---

    // Filtre
    const filterLinie = document.getElementById("filter-linie");
    const filterProdus = document.getElementById("filter-produs");
    const filterEchipament = document.getElementById("filter-echipament");
    const filterData = document.getElementById("filter-data"); 

    const searchButton = document.getElementById("search-button");

    // Lista Tichete
    const ticheteTbody = document.getElementById("tichete-tbody");

    // Formular
    const interventieForm = document.getElementById("interventie-form");
    const hiddenTichetId = document.getElementById("tichet-id");
    
    // Formular - Info
    const infoLinie = document.getElementById("info-linie");
    const infoEchipament = document.getElementById("info-echipament");
    const infoOperator = document.getElementById("info-operator");
    const infoDataRaportare = document.getElementById("info-data-raportare");

    // Formular - Timp
    const startDateInput = document.getElementById("start-data");
    const startTimeInput = document.getElementById("start-ora");
    const stopDateInput = document.getElementById("stop-data");
    const stopTimeInput = document.getElementById("stop-ora");

    // Formular - Detalii
    const problemaSelect = document.getElementById("problema-raportata");
    const defectiuneSelect = document.getElementById("defectiune-identificata");
    const altulGroup = document.getElementById("altul-group");
    const defectiuneTextLiber = document.getElementById("defectiune-text-liber");

    // Formular - QS
    const influenteazaProdusulSelect = document.getElementById("influenteaza-produsul");
    const qsAlert = document.getElementById("qs-alert");

    // Formular - Acțiuni
    const saveInProgressButton = document.getElementById("save-inprogress-button");
    const saveCloseButton = document.getElementById("save-close-button");

    // Stocare date configurare
    let allProbleme = [];
    let allDefectiuni = [];
    let allCorelatii = [];

    // --- Funcții de Încărcare Date ---

    /**
     * Funcția principală de inițializare
     */
    async function initializePage() {
        showLoading(true);
        // Obține token-ul (necesar pentru headerele API)
        const token = localStorage.getItem('token');
        if (!token) {
            // nav.js ar fi trebuit deja să redirecționeze, dar ca o siguranță:
            console.error("Token lipsă, autentificare eșuată.");
            showLoading(false);
            return;
        }

        // Rulează în paralel încărcarea filtrelor, configurărilor și tichetelor
        await Promise.all([
            loadFiltreData(token),
            loadMentenantaConfig(token),
            searchTichete(token) // Încarcă tichetele deschise la început
        ]);
        
        // Inițializăm pickerele după ce pagina e gata
        initializePickers();
        
        showLoading(false);
    }
    
    /**
     * Inițializare Date/Time Pickers (flatpickr)
     */
    function initializePickers() {
        if (typeof flatpickr === 'undefined') {
            console.warn("Biblioteca Flatpickr nu este încărcată. Selectoarele de dată/oră nu vor funcționa.");
            return;
        }
        
        const dateOptions = {
            dateFormat: "Y-m-d", // Format compatibil ISO
            allowInput: true,
        };
        
        const timeOptions = {
            enableTime: true,
            noCalendar: true,
            dateFormat: "H:i", // Format 24h
            time_24hr: true,
            allowInput: true
        };
        
        // Aplicare pe câmpurile din formular
        flatpickr(startDateInput, dateOptions);
        flatpickr(startTimeInput, timeOptions);
        flatpickr(stopDateInput, dateOptions);
        flatpickr(stopTimeInput, timeOptions);
        
        // Aplicare pe filtrul din header
        flatpickr(filterData, dateOptions);
    }


    /**
     * Încarcă datele pentru dropdown-urile de filtrare (Linii, Produse, Echipamente)
     */
    async function loadFiltreData(token) {
        try {
            const [liniiRes, produseRes, echipamenteRes] = await Promise.all([
                fetch('/api/config/lines', { headers: { 'Authorization': `Bearer ${token}` } }),
                fetch('/api/config/products', { headers: { 'Authorization': `Bearer ${token}` } }),
                fetch('/api/config/equipments', { headers: { 'Authorization': `Bearer ${token}` } })
            ]);

            if (liniiRes.ok) {
                const linii = await liniiRes.json();
                populateSelect(filterLinie, linii, "name", "id", "Toate Liniile");
            }
            if (produseRes.ok) {
                const produse = await produseRes.json();
                populateSelect(filterProdus, produse, "name", "id", "Toate Produsele");
            }
            if (echipamenteRes.ok) {
                const echipamente = await echipamenteRes.json();
                populateSelect(filterEchipament, echipamente, "name", "id", "Toate Echipamentele");
            }
        } catch (error) {
            console.error("Eroare la încărcarea datelor de filtrare:", error);
            showToast("Eroare la încărcarea filtrelor.", "error");
        }
    }

    /**
     * Încarcă datele de configurare pentru formular (Probleme, Defecțiuni, Corelări)
     */
    async function loadMentenantaConfig(token) {
        try {
            const [problemeRes, defectiuniRes, corelatiiRes] = await Promise.all([
                fetch('/api/config/mentenanta/probleme', { headers: { 'Authorization': `Bearer ${token}` } }),
                fetch('/api/config/mentenanta/defectiuni', { headers: { 'Authorization': `Bearer ${token}` } }),
                fetch('/api/config/mentenanta/corelatii', { headers: { 'Authorization': `Bearer ${token}` } })
            ]);

            if (problemeRes.ok) {
                allProbleme = await problemeRes.json();
                populateSelect(problemaSelect, allProbleme, "nume", "id", "Selectați problema...");
            }
            if (defectiuniRes.ok) {
                allDefectiuni = await defectiuniRes.json();
                // Nu populăm defectiunile încă, ele depind de corelări
            }
            if (corelatiiRes.ok) {
                allCorelatii = await corelatiiRes.json();
            }
        } catch (error) {
            console.error("Eroare la încărcarea configurării de mentenanță:", error);
            showToast("Eroare la încărcarea configurării.", "error");
        }
    }

    /**
     * Caută tichetele pe baza filtrelor și populează tabelul
     */
    async function searchTichete(token) {
        showLoading(true);
        
        // Asigură-te că avem un token valid
        const currentToken = token || localStorage.getItem('token');
        if (!currentToken) {
             showToast("Sesiune expirată. Vă rugăm să vă relogați.", "error");
             showLoading(false);
             return;
        }
        
        // Construire URL cu filtre (server-side filtering)
        const params = new URLSearchParams();
        if (filterLinie.value) params.append("linieId", filterLinie.value);
        if (filterProdus.value) params.append("produsId", filterProdus.value);
        if (filterEchipament.value) params.append("equipmentId", filterEchipament.value); 
        if (filterData.value) params.append("dataStart", filterData.value); // API-ul filtrează după dataStart
        
        try {
            const response = await fetch(`/api/interventii/deschise?${params.toString()}`, {
                headers: { 'Authorization': `Bearer ${currentToken}` }
            });

            if (!response.ok) {
                // Aici era "bug-ul": API-ul returnează 401/403 pentru roluri nepermise
                throw new Error(`Eroare ${response.status} la căutarea tichetelor.`);
            }

            const tichete = await response.json();
            populateTicheteTable(tichete);

        } catch (error) {
            console.error("Eroare la căutarea tichetelor:", error);
            showToast("Eroare la încărcarea tichetelor. Verificați permisiunile.", "error");
            ticheteTbody.innerHTML = `<tr><td colspan="5" style="text-align: center; color: red;">Eroare la încărcarea tichetelor. (Acces refuzat sau eroare de rețea)</td></tr>`;
        } finally {
            showLoading(false);
        }
    }

    // --- Funcții de Populare UI ---

    /**
     * Populează un element <select> dintr-un array de date
     */
    function populateSelect(selectElement, data, textKey, valueKey, defaultOptionText) {
        selectElement.innerHTML = `<option value="">${defaultOptionText}</option>`;
        data.forEach(item => {
            const option = document.createElement("option");
            option.value = item[valueKey];
            option.textContent = item[textKey];
            // Stocăm întreaga dată pe opțiune dacă e nevoie
            option.dataset.fullData = JSON.stringify(item);
            selectElement.appendChild(option);
        });
    }

    /**
     * Populează tabelul de tichete
     */
    function populateTicheteTable(tichete) {
        ticheteTbody.innerHTML = ""; // Curăță tabelul
        
        if (tichete.length === 0) {
            ticheteTbody.innerHTML = `<tr><td colspan="5" style="text-align: center;">Niciun tichet deschis găsit.</td></tr>`;
            return;
        }

        tichete.forEach(tichet => {
            const row = document.createElement("tr");
            row.dataset.tichetId = tichet.id; // Stocăm ID-ul pe rând
            row.innerHTML = `
                <td>${tichet.unicIdTicket.substring(0, 8)}...</td>
                <td>${formatDateTime(tichet.dataRaportareOperator)}</td>
                <td>${tichet.operatorNume}</td>
                <td>${tichet.linie}</td>
                <td>${tichet.echipament}</td>
            `;
            ticheteTbody.appendChild(row);
        });
    }

    /**
     * Încarcă detaliile unui tichet specific în formular
     * @param {number} tichetId - ID-ul tichetului selectat
     */
    async function loadTichetDetails(tichetId) {
        showLoading(true);
        resetForm();
        const token = localStorage.getItem('token');

        try {
            const response = await fetch(`/api/interventii/${tichetId}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (!response.ok) {
                throw new Error("Tichetul nu a putut fi încărcat.");
            }

            const tichet = await response.json();

            // Populare formular
            hiddenTichetId.value = tichet.id;
            infoLinie.value = tichet.line.name;
            infoEchipament.value = tichet.equipment.name;
            infoOperator.value = tichet.operatorNume;
            infoDataRaportare.value = formatDateTime(tichet.dataRaportareOperator);

            // Populare date intervenție dacă există deja (ex: un tichet "În Lucru")
            if (tichet.dataStartInterventie) {
                const start = new Date(tichet.dataStartInterventie);
                startDateInput._flatpickr.setDate(start, true);
                startTimeInput._flatpickr.setDate(start.toTimeString().substring(0, 5), true, "H:i");
            }
            if (tichet.dataStopInterventie) {
                const stop = new Date(tichet.dataStopInterventie);
                stopDateInput._flatpickr.setDate(stop, true);
                stopTimeInput._flatpickr.setDate(stop.toTimeString().substring(0, 5), true, "H:i");
            }
            
            // Populare dropdown-uri și logică corelare
            if (tichet.problemaRaportataId) {
                problemaSelect.value = tichet.problemaRaportataId;
                filterDefectiuniCorelate(); 
                if (tichet.defectiuneIdentificataId) {
                    defectiuneSelect.value = tichet.defectiuneIdentificataId;
                    checkDefectiuneAltul(); 
                }
            } else {
                populateSelect(defectiuneSelect, allDefectiuni, "nume", "id", "Selectați defecțiunea...");
            }
            
            if(tichet.defectiuneTextLiber) {
                defectiuneTextLiber.value = tichet.defectiuneTextLiber;
            }

            influenteazaProdusulSelect.value = tichet.influenteazaProdusul.toString();
            checkQsAlert(); // Arată alerta QS dacă e cazul

        } catch (error) {
            console.error("Eroare la încărcarea detaliilor tichetului:", error);
            showToast(error.message, "error");
        } finally {
            showLoading(false);
        }
    }

    // --- Logică Formular ---

    /**
     * Resetează formularul la starea inițială
     */
    function resetForm() {
        interventieForm.reset();
        hiddenTichetId.value = "";
        infoLinie.value = "";
        infoEchipament.value = "";
        infoOperator.value = "";
        infoDataRaportare.value = "";
        qsAlert.classList.add("hidden");
        altulGroup.classList.add("hidden");
        
        if (startDateInput._flatpickr) startDateInput._flatpickr.clear();
        if (startTimeInput._flatpickr) startTimeInput._flatpickr.clear();
        if (stopDateInput._flatpickr) stopDateInput._flatpickr.clear();
        if (stopTimeInput._flatpickr) stopTimeInput._flatpickr.clear();
        
        // Resetează evidențierea rândurilor
        ticheteTbody.querySelectorAll('tr').forEach(r => r.classList.remove('selected'));
    }

    /**
     * (UX) Filtrează dropdown-ul de defecțiuni pe baza problemei selectate
     */
    function filterDefectiuniCorelate() {
        const problemaId = parseInt(problemaSelect.value);
        
        if (!problemaId || allCorelatii.length === 0 || allDefectiuni.length === 0) {
            populateSelect(defectiuneSelect, allDefectiuni, "nume", "id", "Selectați defecțiunea...");
            return;
        }
        
        const corelatedDefectIds = allCorelatii
            .filter(c => c.problemaRaportataId === problemaId)
            .map(c => c.defectiuneIdentificataId);
        
        if (corelatedDefectIds.length === 0) {
             populateSelect(defectiuneSelect, allDefectiuni, "nume", "id", "Selectați defecțiunea...");
             return;
        }

        const filteredDefectiuni = allDefectiuni.filter(d => corelatedDefectIds.includes(d.id));
        populateSelect(defectiuneSelect, filteredDefectiuni, "nume", "id", "Selectați defecțiunea (filtrat)...");
    }

    /**
     * (UI) Verifică dacă trebuie afișat câmpul "Altele"
     */
    function checkDefectiuneAltul() {
        const selectedOption = defectiuneSelect.options[defectiuneSelect.selectedIndex];
        if (selectedOption && selectedOption.text.toLowerCase().includes("altul")) {
            altulGroup.classList.remove("hidden");
        } else {
            altulGroup.classList.add("hidden");
            defectiuneTextLiber.value = ""; // Curăță textul dacă se ascunde
        }
    }

    /**
     * (UI) Verifică dacă trebuie afișată alerta QS
     */
    function checkQsAlert() {
        if (influenteazaProdusulSelect.value === "true") {
            qsAlert.classList.remove("hidden");
        } else {
            qsAlert.classList.add("hidden");
        }
    }

    /**
     * (UX) Setează data și ora curentă în câmpurile țintă
     */
    function setDateTimeNow(e) {
        const targetDateId = e.target.dataset.targetDate;
        const targetTimeId = e.target.dataset.targetTime;
        
        const now = new Date();
        
        const dateInput = document.getElementById(targetDateId);
        const timeInput = document.getElementById(targetTimeId);

        if (dateInput._flatpickr) {
            dateInput._flatpickr.setDate(now, true); 
        } else {
            dateInput.value = now.toISOString().split('T')[0];
        }
        
        if (timeInput._flatpickr) {
            timeInput._flatpickr.setDate(now, true, "H:i"); 
        } else {
            timeInput.value = now.toTimeString().split(' ')[0].substring(0, 5);
        }
    }

    /**
     * Funcția principală de salvare a intervenției
     * @param {'InLucru' | 'Rezolvat'} status - Statusul cu care se salvează tichetul
     */
    async function saveInterventie(status) {
        const tichetId = hiddenTichetId.value;
        if (!tichetId) {
            showToast("Vă rugăm selectați un tichet din listă.", "error");
            return;
        }

        if (!startDateInput.value || !startTimeInput.value || !problemaSelect.value || !defectiuneSelect.value) {
            showToast("Completați cel puțin Data Start, Ora Start, Problema și Defecțiunea.", "error");
            return;
        }
        
        const dataStartInterventie = `${startDateInput.value}T${startTimeInput.value}:00`;
        
        let dataStopInterventie = null;
        if (stopDateInput.value && stopTimeInput.value) {
             dataStopInterventie = `${stopDateInput.value}T${stopTimeInput.value}:00`;
        } else if (status === 'Rezolvat') {
            showToast("Data Stop și Ora Stop sunt obligatorii pentru a închide tichetul.", "error");
            return;
        }
        
        const requestBody = {
            dataStartInterventie: dataStartInterventie,
            dataStopInterventie: dataStopInterventie,
            problemaRaportataId: parseInt(problemaSelect.value),
            defectiuneIdentificataId: parseInt(defectiuneSelect.value) || null,
            defectiuneTextLiber: defectiuneTextLiber.value || null,
            influenteazaProdusul: influenteazaProdusulSelect.value === "true",
            status: status
        };

        const token = localStorage.getItem('token');
        showLoading(true);
        try {
            const response = await fetch(`/api/interventii/${tichetId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(requestBody)
            });

            if (!response.ok) {
                const err = await response.json();
                throw new Error(err.message || "Eroare la salvarea intervenției.");
            }

            showToast("Intervenție salvată cu succes.", "success");
            
            if (status === 'Rezolvat') {
                resetForm();
                await searchTichete(token);
            }
            
        } catch (error) {
            console.error("Eroare la salvare:", error);
            showToast(error.message, "error");
        } finally {
            showLoading(false);
        }
    }

    // --- Listenere Evenimente ---

    // Filtrare
    searchButton.addEventListener("click", () => searchTichete(localStorage.getItem('token')));

    // Selectare Tichet
    ticheteTbody.addEventListener("click", (e) => {
        const row = e.target.closest('tr');
        if (row && row.dataset.tichetId) {
            ticheteTbody.querySelectorAll('tr').forEach(r => r.classList.remove('selected'));
            row.classList.add('selected');
            loadTichetDetails(row.dataset.tichetId);
        }
    });

    // Logică Formular
    problemaSelect.addEventListener("change", filterDefectiuniCorelate);
    defectiuneSelect.addEventListener("change", checkDefectiuneAltul);
    influenteazaProdusulSelect.addEventListener("change", checkQsAlert);
    
    // Butoane "Acum"
    document.querySelectorAll('.btn-now').forEach(btn => {
        btn.addEventListener("click", setDateTimeNow);
    });

    // Butoane Salvare
    saveInProgressButton.addEventListener("click", (e) => {
        e.preventDefault();
        saveInterventie('InLucru');
    });
    
    saveCloseButton.addEventListener("click", (e) => {
        e.preventDefault();
        saveInterventie('Rezolvat');

    });
    
    // --- NOTĂ: Listener-ul de Logout a fost eliminat. 'nav.js' se ocupă de el. ---

    // --- Inițializare Pagină ---
    initializePage();
});

// --- NOTĂ: Funcțiile utilitare (showToast, showLoading, formatDateTime) au fost mutate în /js/utils.js ---