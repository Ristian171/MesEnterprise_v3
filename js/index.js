/*
 * ===================================================================================
 * FIȘIER: MesSimplu/wwwroot/js/index.js
 * STARE: MODIFICAT (Flux de logare unificat + Corecție Stare Client + Corecție Sesiune)
 *
 * MODIFICARE (Senior Dev):
 * - ELIMINAT: Formularul separat `log-downtime-form`.
 * - ELIMINAT: Butonul și modalul separat `defect-alloc-button`.
 * - MODIFICAT: `handleLogSubmit` (butonul Salvează Log) face acum următorul flux:
 * 1. Dacă (Scrap + NRFT == 0), salvează direct.
 * 2. Dacă (Scrap + NRFT > 0), deschide noul modal unificat
 * `#log-details-modal`.
 * - ADĂUGAT: Logică pentru a gestiona noul modal, care cere AMBELE:
 * - Minutele de staționare (opțional).
 * - Alocarea defectelor (obligatoriu, trebuie să corespundă sumei).
 * - ADĂUGAT: Funcția `saveProductionLog()` care este apelată de ambele
 * fluxuri (logare simplă sau logare cu detalii).
 * - CORECȚIE STARE: Adăugat `logDataPendingSave` pentru a preveni
 * modificarea datelor în formularul principal în timp ce modalul este deschis.
 * - CORECȚIE SESIUNE: Eliminat conceptul de "sesiune server". Toate apelurile
 * API trimit acum `lineId` și `productId` ca parametri de interogare.
 * ===================================================================================
 */

document.addEventListener("DOMContentLoaded", () => {

    // --- Elemente DOM Principale ---
    const lineSelect = document.getElementById("line-select");
    const productSelect = document.getElementById("product-select"); 
    const shiftDisplay = document.getElementById("shift-display");

    // --- Elemente DOM Butoane Acțiune ---
    const startButton = document.getElementById("start-button");
    const stopButton = document.getElementById("stop-button");
    const changeoverButton = document.getElementById("changeover-button");
    const downtimeButton = document.getElementById("downtime-button");

    // --- Elemente DOM Logare Producție ---
    const logForm = document.getElementById("log-form");
    const logTimeSelect = document.getElementById("log-time-select"); 
    const logGoodInput = document.getElementById("log-good");
    const logScrapInput = document.getElementById("log-scrap");
    const logNrftInput = document.getElementById("log-nrft");
    const logSubmitButton = document.getElementById("log-submit-button");

    // --- Elemente DOM Observații ---
    const observationForm = document.getElementById("observation-form");
    const observationTextarea = document.getElementById("operator-observation");
    const observationSubmit = document.getElementById("observation-submit");

    // --- Elemente DOM Istoric ---
    const historyTbody = document.getElementById("history-tbody");
    const refreshHistoryBtn = document.getElementById("refresh-history-btn");

    // --- Elemente DOM KPI ---
    const oeeDisplay = document.getElementById("oee-hourly").querySelector(".value");
    const goodDisplay = document.getElementById("good-parts-hourly").querySelector(".value");
    const scrapDisplay = document.getElementById("scrap-parts-hourly").querySelector(".value");
    const targetDisplay = document.getElementById("target-hourly").querySelector(".value");
    const targetRealtimeDisplay = document.getElementById("target-realtime").querySelector(".value");
    const timeRemainingDisplay = document.getElementById("time-remaining-hourly").querySelector(".value");

    // --- Elemente DOM Modal Avarie ---
    const downtimeModal = document.getElementById("downtime-modal");
    const downtimeModalForm = document.getElementById("downtime-modal-form");
    const downtimeModalClose = document.getElementById("downtime-modal-close");
    const downtimeModalCancel = document.getElementById("downtime-modal-cancel");
    const downtimeModalEquipmentSelect = document.getElementById("downtime-modal-equipment");
    const downtimeModalProblemSelect = document.getElementById("downtime-modal-problem");
    
    // --- Elemente DOM Modal Detalii Logare (NOU UNIFICAT) ---
    const logDetailsModal = document.getElementById("log-details-modal");
    const logDetailsModalClose = document.getElementById("log-details-modal-close");
    const logDetailsModalCancel = document.getElementById("log-details-modal-cancel");
    const logDetailsModalSave = document.getElementById("log-details-modal-save");
    // Downtime in modal
    const modalDowntimeReason = document.getElementById("modal-downtime-reason");
    const modalDowntimeMinutes = document.getElementById("modal-downtime-minutes");
    // Defecte in modal
    const defectAllocForm = document.getElementById("defect-alloc-form");
    const defectCategorySelect = document.getElementById("defect-category-select");
    const defectCodeSelect = document.getElementById("defect-code-select");
    const defectQuantityInput = document.getElementById("defect-quantity-input");
    const defectTotalToAllocate = document.getElementById("defect-total-to-allocate");
    const defectTotalAllocated = document.getElementById("defect-total-allocated");
    const defectAllocationTbody = document.getElementById("defect-allocation-tbody");

    // --- Elemente DOM Live Scan ---
    const liveScanSection = document.getElementById("live-scan-section");
    const scanStatusText = document.getElementById("scan-status-text");
    const toggleLiveScanButton = document.getElementById("toggle-live-scan");
    const liveScanInfo = document.getElementById("live-scan-info");

    // --- Elemente DOM OEE Justification Modal ---
    const oeeJustificationModal = document.getElementById("oee-justification-modal");
    const oeeJustificationModalClose = document.getElementById("oee-justification-modal-close");
    const oeeJustificationModalSubmit = document.getElementById("oee-justification-modal-submit");
    const oeeJustificationReason = document.getElementById("oee-justification-reason");

    // --- Stare Aplicație ---
    let token = localStorage.getItem('token');
    let currentLineId = null;
    let currentProductId = null;
    let currentStatus = null; 
    let allDowntimeReasons = [];
    let allEquipment = [];
    let allProblems = [];
    let kpiRefreshInterval = null; 
    
    // Stare pentru alocare defecte
    let allDefectCategories = [];
    let allDefectCodes = [];
    let currentDefectAllocations = []; // Format: [{ categoryName, defectName, defectCodeId, quantity }]
    
    // CORECȚIE STARE CLIENT: Stochează datele logării cât timp modalul este deschis
    let logDataPendingSave = null;
    
    // Stare pentru justificarea OEE
    let pendingJustificationLog = null;

    // --- Inițializare ---

    async function initializePage() {
        if (!token) {
            console.error("Token lipsă, autentificare eșuată.");
            return;
        }

        showLoading(true);
        
        try {
            // Am adăugat /api/operator/defect-config la încărcarea inițială
            const [config, defectConfig] = await Promise.all([
                Promise.all([
                    loadLinii(),
                    loadAllProducts(), 
                    loadDowntimeReasons(), // Folosit acum în modal
                    loadMaintenanceConfig()
                ]),
                loadDefectConfig() // NOU
            ]);
        } catch (error) {
            console.warn("Eroare la încărcarea configurațiilor.", error);
        }
        
        const savedLineId = localStorage.getItem('selectedLineId');
        const savedProductId = localStorage.getItem('selectedProductId');

        if (savedLineId && lineSelect.querySelector(`option[value="${savedLineId}"]`)) {
            lineSelect.value = savedLineId;
            currentLineId = savedLineId;
        }
        if (savedProductId && productSelect.querySelector(`option[value="${savedProductId}"]`)) {
            productSelect.value = savedProductId;
            currentProductId = savedProductId;
        }
        
        if (currentLineId && currentProductId) {
            await startUserSessionAndLoadState();
        } else {
            updateUIfromStatus(null);
        }

        setupEventListeners();
        showLoading(false);
    }

    function setupEventListeners() {
        lineSelect.addEventListener("change", handleSessionSelectionChange);
        productSelect.addEventListener("change", handleSessionSelectionChange);
        
        startButton.addEventListener("click", () => handleCommand("start"));
        stopButton.addEventListener("click", () => handleCommand("stop"));
        changeoverButton.addEventListener("click", () => {
            window.location.href = '/changeover.html';
        });
        downtimeButton.addEventListener("click", openDowntimeModal);
        
        logForm.addEventListener("submit", handleLogSubmit); // Fluxul principal de logare
        observationForm.addEventListener("submit", handleObservationSubmit);
        downtimeModalForm.addEventListener("submit", handleDowntimeModalSubmit);
        
        refreshHistoryBtn.addEventListener("click", () => loadHistory());
        downtimeModalClose.addEventListener("click", closeModal);
        downtimeModalCancel.addEventListener("click", closeModal);
        
        // Listenere noi pentru modalul unificat
        logDetailsModalClose.addEventListener("click", closeLogDetailsModal);
        logDetailsModalCancel.addEventListener("click", closeLogDetailsModal);
        logDetailsModalSave.addEventListener("click", handleSaveLogDetails);
        defectCategorySelect.addEventListener("change", populateDefectCodes);
        defectAllocForm.addEventListener("submit", handleAddDefectAllocation);
        defectAllocationTbody.addEventListener("click", handleDeleteDefectAllocation);
        
        // Live Scan toggle
        toggleLiveScanButton.addEventListener("click", toggleLiveScan);
        
        // OEE Justification modal
        oeeJustificationModalClose.addEventListener("click", closeJustificationModal);
        oeeJustificationModalSubmit.addEventListener("click", submitJustification);
    }

    // --- Încărcare Date Configurare ---

    async function loadLinii() {
        try {
            // ================== ÎNCEPUT CORECȚIE BUG ÎNCĂRCARE PAGINĂ ==================
            // Endpoint-ul a fost mutat în OperatorApi pentru a permite accesul
            // utilizatorilor fără rol de Admin.
            const linii = await apiCall('/api/operator/lines');
            // ================== SFÂRȘIT CORECȚIE BUG ÎNCĂRCARE PAGINĂ ==================
            populateSelect(lineSelect, linii, 'name', 'id', 'Selectați o linie...');
        } catch (error) {
            showToast("Eroare la încărcarea liniilor.", 'error');
            throw error;
        }
    }

    async function loadAllProducts() {
        try {
            // ================== ÎNCEPUT CORECȚIE BUG ÎNCĂRCARE PAGINĂ ==================
            // Endpoint-ul a fost mutat în OperatorApi pentru a permite accesul
            // utilizatorilor fără rol de Admin.
            const produse = await apiCall('/api/operator/products');
            // ================== SFÂRȘIT CORECȚIE BUG ÎNCĂRCARE PAGINĂ ==================
            populateSelect(productSelect, produse, 'name', 'id', 'Selectați un produs...');
        } catch (error) {
            showToast("Eroare la încărcarea produselor.", 'error');
            throw error;
        }
    }

    async function loadDowntimeReasons() {
        try {
            allDowntimeReasons = await apiCall('/api/config/breakdownreasons');
            // Populăm dropdown-ul din noul modal
            populateSelect(modalDowntimeReason, allDowntimeReasons, 'name', 'id', 'Fără staționare');
        } catch (error) {
            showToast("Eroare la încărcarea motivelor de oprire.", 'error');
            throw error;
        }
    }

    async function loadMaintenanceConfig() {
        try {
            const [equipmentRes, problemsRes] = await Promise.all([
                apiCall('/api/config/equipments'),
                apiCall('/api/config/mentenanta/probleme')
            ]);
            allEquipment = equipmentRes;
            allProblems = problemsRes;
            populateSelect(downtimeModalProblemSelect, allProblems, 'nume', 'id', 'Selectați problema...');
        } catch (error) {
            showToast("Eroare la încărcarea configurării de mentenanță.", 'error');
            throw error;
        }
    }
    
    // NOU: Încarcă ambele categorii și coduri de defecte
    async function loadDefectConfig() {
        try {
            const data = await apiCall('/api/operator/defect-config');
            allDefectCategories = data.categories;
            allDefectCodes = data.codes;
            
            populateSelect(defectCategorySelect, allDefectCategories, 'name', 'id', 'Selectați categoria...');
        } catch (error) {
            showToast("Eroare la încărcarea configurării de defecte.", 'error');
            throw error;
        }
    }
    

    // --- Logică Principală (Stare Linie) ---

    // CORECȚIE SESIUNE: Funcție helper pentru a adăuga parametrii
    function getSessionParams() {
        if (!currentLineId || !currentProductId) {
            showToast("Sesiunea linie/produs nu este setată.", "error");
            return null;
        }
        return new URLSearchParams({
            lineId: currentLineId,
            productId: currentProductId
        });
    }

    async function handleSessionSelectionChange() {
        const newLinieId = lineSelect.value;
        const newProdusId = productSelect.value;

        if (!newLinieId || !newProdusId) {
            return;
        }

        if (newLinieId === currentLineId && newProdusId === currentProductId) {
            return;
        }
        
        currentLineId = newLinieId;
        currentProductId = newProdusId;
        
        localStorage.setItem('selectedLineId', currentLineId);
        localStorage.setItem('selectedProductId', currentProductId);
        
        showLoading(true);
        await startUserSessionAndLoadState();
        showLoading(false);
    }
    
    async function startUserSessionAndLoadState() {
        if (!currentLineId || !currentProductId) {
            showToast("Vă rugăm selectați o linie și un produs.", "warn");
            return;
        }
        
        try {
            // Acest apel API actualizează starea serverului (dacă e necesar)
            await apiCall('/api/operator/session', {
                method: 'POST',
                body: JSON.stringify({
                    lineId: parseInt(currentLineId),
                    productId: parseInt(currentProductId)
                })
            });
            
            // Acum, încărcăm starea bazată pe sesiunea noastră
            await getLineStatus();
            
        } catch (error) {
            showToast("Eroare la pornirea sesiunii. Încercați din nou.", 'error');
            updateUIfromStatus(null);
        }
    }


    async function getLineStatus() {
        // CORECȚIE SESIUNE: Trimitem parametrii
        const params = getSessionParams();
        if (!params) {
             updateUIfromStatus(null);
             return;
        }

        try {
            const status = await apiCall(`/api/operator/state?${params.toString()}`);
            currentStatus = status; 
            updateUIfromStatus(status);
            
            if (kpiRefreshInterval) clearInterval(kpiRefreshInterval);
            kpiRefreshInterval = setInterval(() => {
                if (currentLineId && currentProductId) {
                    getLineStatus();
                }
            }, 30000);
            
            // Check for pending justifications after loading status
            await checkPendingJustifications();

        } catch (error) {
            showToast("Eroare la obținerea stării liniei.", 'error');
            updateUIfromStatus(null);
        }
    }

    async function handleCommand(command) {
        // CORECȚIE SESIUNE: Trimitem parametrii
        const params = getSessionParams();
        if (!params) return;

        showLoading(true);
        try {
            const newStatusResponse = await apiCall(`/api/operator/command?${params.toString()}`, { 
                method: 'POST', 
                body: JSON.stringify({ command: command }) 
            });
            
            await getLineStatus();
            showToast(`Comanda "${command}" executată.`, 'success');
            
        } catch (error) {
            // Eroarea e deja afișată
        } finally {
            showLoading(false);
        }
    }

    // --- Live Scan Functions ---
    
    function updateLiveScanUI(status) {
        if (!status || !status.scanIdentifier) {
            // No scan identifier configured - hide section
            liveScanSection.style.display = 'none';
            return;
        }
        
        liveScanSection.style.display = 'block';
        
        if (!status.liveScanAvailable) {
            // Stație scan neconfigurată
            scanStatusText.textContent = 'Stație scan neconfigurată';
            scanStatusText.style.color = 'var(--color-text-muted)';
            toggleLiveScanButton.style.display = 'none';
            liveScanInfo.style.display = 'none';
            logGoodInput.disabled = false;
        } else if (status.liveScanEnabled) {
            // Stație scan activă
            scanStatusText.textContent = 'Stație scan activă';
            scanStatusText.style.color = 'var(--color-success)';
            toggleLiveScanButton.style.display = 'inline-block';
            toggleLiveScanButton.textContent = 'Dezactivează Live Scan';
            toggleLiveScanButton.className = 'btn btn-secondary';
            liveScanInfo.style.display = 'block';
            // Make good parts readonly when Live Scan is active
            logGoodInput.disabled = true;
            logGoodInput.title = 'Câmp dezactivat - Live Scan activ';
        } else {
            // Stație scan disponibilă (Live Scan dezactivat)
            scanStatusText.textContent = 'Stație scan disponibilă (Live Scan dezactivat)';
            scanStatusText.style.color = 'var(--color-warning)';
            toggleLiveScanButton.style.display = 'inline-block';
            toggleLiveScanButton.textContent = 'Activează Live Scan';
            toggleLiveScanButton.className = 'btn btn-primary';
            liveScanInfo.style.display = 'none';
            logGoodInput.disabled = false;
            logGoodInput.title = '';
        }
    }
    
    async function toggleLiveScan() {
        if (!currentLineId) return;
        
        showLoading(true);
        try {
            const newState = !currentStatus.liveScanEnabled;
            await apiCall(`/api/line/${currentLineId}/live-scan`, {
                method: 'PUT',
                body: JSON.stringify({ enabled: newState })
            });
            
            // Refresh status
            await getLineStatus();
            showToast(newState ? 'Live Scan activat' : 'Live Scan dezactivat', 'success');
        } catch (error) {
            // Error already shown
        } finally {
            showLoading(false);
        }
    }
    
    // --- OEE Justification Functions ---
    
    async function checkPendingJustifications() {
        if (!currentLineId) return;
        
        try {
            const pending = await apiCall(`/api/operator/pending-justifications?lineId=${currentLineId}`);
            
            if (pending && pending.length > 0) {
                // Show modal for the first pending justification
                showJustificationModal(pending[0]);
            }
        } catch (error) {
            console.error("Error checking pending justifications:", error);
            // Don't show toast - this is a background check
        }
    }
    
    function showJustificationModal(log) {
        pendingJustificationLog = log;
        
        // Populate modal with log details
        const timestamp = new Date(log.timestamp);
        document.getElementById('justif-timestamp').textContent = timestamp.toLocaleString('ro-RO');
        document.getElementById('justif-interval').textContent = log.hourInterval;
        document.getElementById('justif-actual').textContent = log.actualParts;
        document.getElementById('justif-target').textContent = log.targetParts;
        document.getElementById('justif-oee').textContent = log.oee.toFixed(1);
        
        oeeJustificationReason.value = '';
        oeeJustificationModal.classList.remove('hidden');
    }
    
    function closeJustificationModal() {
        oeeJustificationModal.classList.add('hidden');
        pendingJustificationLog = null;
        oeeJustificationReason.value = '';
    }
    
    async function submitJustification() {
        if (!pendingJustificationLog) return;
        
        const reason = oeeJustificationReason.value.trim();
        if (!reason) {
            showToast('Vă rugăm introduceți un motiv pentru justificare.', 'warn');
            return;
        }
        
        showLoading(true);
        try {
            await apiCall('/api/operator/justify-oee', {
                method: 'POST',
                body: JSON.stringify({
                    productionLogId: pendingJustificationLog.id,
                    reason: reason
                })
            });
            
            showToast('Justificare salvată cu succes.', 'success');
            closeJustificationModal();
            
            // Check if there are more pending justifications
            await checkPendingJustifications();
        } catch (error) {
            // Error already shown
        } finally {
            showLoading(false);
        }
    }

    // --- Actualizare UI ---

    function updateUIfromStatus(status) {
        // --- Formularul de downtime a fost eliminat din fluxul UI principal ---
        const logDowntimeForm = document.getElementById("log-downtime-form");
        if (logDowntimeForm) {
            logDowntimeForm.classList.add('hidden');
        }
        // --- ---

        if (!status) {
            shiftDisplay.textContent = "...";
            logTimeSelect.innerHTML = '<option value="">Selectați linia/produsul...</option>';
            logTimeSelect.disabled = true;
            
            startButton.disabled = true;
            stopButton.disabled = true;
            changeoverButton.disabled = true;
            downtimeButton.disabled = true;
            logSubmitButton.disabled = true;
            
            oeeDisplay.textContent = "0%";
            goodDisplay.textContent = "0";
            scrapDisplay.textContent = "0";
            targetDisplay.textContent = "0";
            targetRealtimeDisplay.textContent = "0";
            timeRemainingDisplay.textContent = "00:00";
            historyTbody.innerHTML = '<tr><td colspan="4" style="text-align: center;">Selectați o linie și un produs.</td></tr>';
            
            return;
        }

        shiftDisplay.textContent = status.shiftName || "N/A";
        
        populateTimeSelect(status.availableTimeSlots);
        
        oeeDisplay.textContent = `${status.currentHourOEE.toFixed(0)}%`;
        goodDisplay.textContent = status.currentHourGoodParts;
        scrapDisplay.textContent = status.currentHourScrap;
        targetDisplay.textContent = status.currentHourTarget;
        targetRealtimeDisplay.textContent = status.realTimeTarget;
        timeRemainingDisplay.textContent = status.timeRemaining || "00:00";
        
        // Update Live Scan UI
        updateLiveScanUI(status);
        
        loadHistory();
        
        // Actualizare Stare Butoane
        startButton.disabled = true;
        stopButton.disabled = true;
        changeoverButton.disabled = false;
        downtimeButton.disabled = true;
        logSubmitButton.disabled = true;
        logTimeSelect.disabled = true;

        switch (status.lineStatus) {
            case "Stopped": 
                startButton.disabled = false;
                downtimeButton.disabled = false; 
                break;
                
            case "Running":
                stopButton.disabled = false;
                downtimeButton.disabled = false;
                logSubmitButton.disabled = false;
                logTimeSelect.disabled = false;
                break;
                
            case "Changeover":
                break;
                
            case "Breakdown":
                break;
        }
    }
    
    function populateTimeSelect(timeSlots) {
        logTimeSelect.innerHTML = ""; 
        
        if (!timeSlots || timeSlots.length === 0) {
            logTimeSelect.innerHTML = '<option value="">Fără intervale disponibile</option>';
            logTimeSelect.disabled = true;
            return;
        }
        
        let hasAvailableSlot = false;
        timeSlots.forEach(slot => {
            const option = document.createElement("option");
            option.value = slot.value; 
            option.textContent = slot.text;
            option.disabled = !slot.isAvailable;
            logTimeSelect.appendChild(option);
            
            if (slot.isAvailable) {
                hasAvailableSlot = true;
            }
        });
        
        const lastAvailable = timeSlots.filter(s => s.isAvailable).pop();
        if (lastAvailable) {
            logTimeSelect.value = lastAvailable.value;
        }
        
        logTimeSelect.disabled = !hasAvailableSlot;
        
        if (currentStatus && currentStatus.lineStatus === "Running") {
             logSubmitButton.disabled = !hasAvailableSlot;
        } else {
             logSubmitButton.disabled = true;
        }
    }

    async function loadHistory() {
        // CORECȚIE SESIUNE: Trimitem parametrii
        const params = getSessionParams();
        if (!params) return;

        try {
            const historyData = await apiCall(`/api/operator/history-preview?${params.toString()}`);
            historyTbody.innerHTML = "";
            
            if (historyData.length === 0) {
                historyTbody.innerHTML = '<tr><td colspan="4" style="text-align: center;">Nicio logare în acest schimb.</td></tr>';
                return;
            }
            
            historyData.forEach(log => {
                const row = document.createElement('tr');
                const localTime = new Date(log.timestamp).toLocaleTimeString('ro-RO', { hour: '2-digit', minute: '2-digit' });
                
                if (log.type === 'log') {
                    row.innerHTML = `
                        <td>${localTime} (Log)</td>
                        <td>${log.good}</td>
                        <td class="${log.scrap > 0 ? 'bad' : ''}">${log.scrap}</td>
                        <td class="${log.nrft > 0 ? 'warn' : ''}">${log.nrft}</td>
                    `;
                } else { 
                     row.innerHTML = `
                        <td>${localTime} (Obs)</td>
                        <td colspan="3" style="text-align: left;"><i>${escapeHTML(log.text)}</i></td>
                    `;
                }
                historyTbody.appendChild(row);
            });
            
        } catch (error) {
            console.error("Eroare la reîmprospătare istoric:", error);
            historyTbody.innerHTML = '<tr><td colspan="4" style="text-align: center; color: red;">Eroare.</td></tr>';
        }
    }

    // --- Logică Formulare ---

    // NOU: Funcție centralizată de salvare
    async function saveProductionLog(data) {
        // CORECȚIE SESIUNE: Trimitem parametrii
        const params = getSessionParams();
        if (!params) return;

        logSubmitButton.disabled = true;
        showLoading(true);

        try {
            await apiCall(`/api/productionlogs?${params.toString()}`, { 
                method: 'POST', 
                body: JSON.stringify(data) 
            });
            showToast("Logare salvată.", 'success');
            
            // Resetare formular principal și alocări
            logGoodInput.value = 0;
            logScrapInput.value = 0;
            logNrftInput.value = 0;
            logDataPendingSave = null; // CORECȚIE STARE
            currentDefectAllocations = [];
            closeLogDetailsModal();
            
            await getLineStatus(); // Reîmprospătare stare
            
        } catch (error) {
            // Eroarea e deja afișată
        } finally {
            logSubmitButton.disabled = false;
            showLoading(false);
        }
    }


    async function handleLogSubmit(e) {
        e.preventDefault();
        
        const logTime = logTimeSelect.value;
        if (!logTime) {
            showToast("Vă rugăm selectați un interval orar valid.", "error");
            return;
        }
        
        const goodParts = parseInt(logGoodInput.value) || 0;
        const scrapParts = parseInt(logScrapInput.value) || 0;
        const nrftParts = parseInt(logNrftInput.value) || 0;
        const totalDefects = scrapParts + nrftParts;

        if (goodParts === 0 && totalDefects === 0) {
            showToast("Introduceți cel puțin o piesă (bună, scrap sau nrft).", 'warn');
            return;
        }
        
        // Resetăm alocările curente
        currentDefectAllocations = [];
        renderDefectAllocationList();

        // CORECȚIE STARE: Salvăm datele *înainte* de a deschide modalul
        logDataPendingSave = {
            logTime: logTime, 
            goodParts: goodParts,
            scrapParts: scrapParts,
            nrftParts: nrftParts
        };

        // Flux nou:
        if (totalDefects > 0) {
            // Dacă avem defecte, deschidem modalul pentru a cere detalii
            openLogDetailsModal(totalDefects);
        } else {
            // Dacă nu avem defecte, salvăm direct
            const data = {
                ...logDataPendingSave,
                declaredDowntimeReasonId: null,
                declaredDowntimeMinutes: 0,
                defecte: [] 
            };
            await saveProductionLog(data);
        }
    }
    
    // Eliminat: handleDowntimeReasonSubmit (funcționalitate mutată în modal)

    async function handleObservationSubmit(e) {
        e.preventDefault();
        const text = observationTextarea.value;
        if (!text.trim()) {
            showToast("Introduceți o observație.", 'warn');
            return;
        }
        
        // CORECȚIE SESIUNE: Trimitem parametrii
        const params = getSessionParams();
        if (!params) return;
        
        const data = {
            text: text 
        };
        
        observationSubmit.disabled = true;
        showLoading(true);
        
        try {
            await apiCall(`/api/operator/observatie?${params.toString()}`, { 
                method: 'POST', 
                body: JSON.stringify(data) 
            });
            showToast("Observație salvată.", 'success');
            observationTextarea.value = ""; 
            loadHistory(); 
        } catch (error) {
            // Eroarea e deja afișată
        } finally {
            observationSubmit.disabled = false;
            showLoading(false);
        }
    }

    // --- Logică Modal Avarie (Neschimbată, doar apelul API) ---

    function openDowntimeModal() {
        if (!currentLineId) return;
        const lineEquipment = allEquipment.filter(eq => eq.lineId == currentLineId);
        populateSelect(downtimeModalEquipmentSelect, lineEquipment, 'name', 'id', 'Selectați stația...');
        downtimeModal.classList.remove('hidden');
    }

    function closeModal() {
        downtimeModal.classList.add('hidden');
        downtimeModalForm.reset();
    }

    async function handleDowntimeModalSubmit(e) {
        e.preventDefault();
        
        // CORECȚIE SESIUNE: Trimitem parametrii
        const params = getSessionParams();
        if (!params) return;
        
        const equipmentId = parseInt(downtimeModalEquipmentSelect.value);
        const problemaId = parseInt(downtimeModalProblemSelect.value);

        if (isNaN(equipmentId) || isNaN(problemaId)) {
            showToast("Vă rugăm selectați echipamentul ȘI problema.", 'error');
            return;
        }
        
        const data = {
            equipmentId: equipmentId,
            problemaId: problemaId
        };
        
        showLoading(true);
        try {
            await apiCall(`/api/operator/create-ticket?${params.toString()}`, { 
                method: 'POST', 
                body: JSON.stringify(data) 
            });
            
            showToast("Avarie raportată! Linia este oprită și tichetul a fost creat.", 'success');
            closeModal();
            
            await getLineStatus();
            
        } catch (error) {
            // Eroarea e deja afișată
        } finally {
            showLoading(false);
        }
    }

    // --- NOU: Logică Modal Detalii Logare (Unificat) ---

    function openLogDetailsModal(totalDefects) {
        defectTotalToAllocate.textContent = totalDefects;
        // Resetăm starea modalului
        currentDefectAllocations = [];
        renderDefectAllocationList();
        modalDowntimeReason.value = "";
        modalDowntimeMinutes.value = 0;
        
        logDetailsModal.classList.remove('hidden');
    }
    
    function closeLogDetailsModal() {
        logDetailsModal.classList.add('hidden');
        defectAllocForm.reset();
        defectCodeSelect.innerHTML = '<option value="">Alege categoria...</option>';
        defectCodeSelect.disabled = true;
        logDataPendingSave = null; // CORECȚIE STARE: Anulează logarea
    }

    function populateDefectCodes() {
        const categoryId = defectCategorySelect.value;
        if (!categoryId) {
            defectCodeSelect.innerHTML = '<option value="">Alege categoria...</option>';
            defectCodeSelect.disabled = true;
            return;
        }
        
        const codes = allDefectCodes.filter(c => c.defectCategoryId == categoryId);
        populateSelect(defectCodeSelect, codes, 'name', 'id', 'Selectați defectul...');
        defectCodeSelect.disabled = false;
    }

    function handleAddDefectAllocation(e) {
        e.preventDefault();
        
        const categoryId = defectCategorySelect.value;
        const defectCodeId = parseInt(defectCodeSelect.value);
        const quantity = parseInt(defectQuantityInput.value);

        if (isNaN(defectCodeId) || isNaN(quantity) || quantity <= 0) {
            showToast("Vă rugăm selectați un defect și o cantitate validă.", 'error');
            return;
        }
        
        const allocatedTotal = currentDefectAllocations.reduce((sum, alloc) => sum + alloc.quantity, 0);
        const toAllocateTotal = parseInt(defectTotalToAllocate.textContent);
        
        if (allocatedTotal + quantity > toAllocateTotal) {
            showToast(`Eroare: Nu puteți aloca ${quantity} piese. Mai aveți de alocat ${toAllocateTotal - allocatedTotal} piese.`, 'error');
            return;
        }

        const categoryName = allDefectCategories.find(c => c.id == categoryId)?.name || 'N/A';
        const defectName = allDefectCodes.find(c => c.id == defectCodeId)?.name || 'N/A';

        const existingAlloc = currentDefectAllocations.find(a => a.defectCodeId === defectCodeId);
        if (existingAlloc) {
            existingAlloc.quantity += quantity;
        } else {
            currentDefectAllocations.push({
                categoryName: categoryName,
                defectName: defectName,
                defectCodeId: defectCodeId,
                quantity: quantity
            });
        }
        
        renderDefectAllocationList();
        defectAllocForm.reset();
        defectCodeSelect.innerHTML = '<option value="">Alege categoria...</option>';
        defectCodeSelect.disabled = true;
    }
    
    function handleDeleteDefectAllocation(e) {
        const deleteBtn = e.target.closest('.btn-danger');
        if (!deleteBtn) return;
        
        const defectId = parseInt(deleteBtn.dataset.id);
        currentDefectAllocations = currentDefectAllocations.filter(a => a.defectCodeId !== defectId);
        renderDefectAllocationList();
    }

    function renderDefectAllocationList() {
        defectAllocationTbody.innerHTML = '';
        if (currentDefectAllocations.length === 0) {
            defectAllocationTbody.innerHTML = '<tr><td colspan="4" style="text-align: center;">Niciun defect alocat.</td></tr>';
            defectTotalAllocated.textContent = 0;
            defectTotalAllocated.style.color = 'var(--color-danger)'; // Roșu dacă e 0
            return;
        }
        
        let allocatedTotal = 0;
        currentDefectAllocations.forEach(alloc => {
            allocatedTotal += alloc.quantity;
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${escapeHTML(alloc.categoryName)}</td>
                <td>${escapeHTML(alloc.defectName)}</td>
                <td>${alloc.quantity}</td>
                <td class="actions">
                    <button class="btn btn-danger" data-id="${alloc.defectCodeId}"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            defectAllocationTbody.appendChild(row);
        });
        
        defectTotalAllocated.textContent = allocatedTotal;
        const toAllocateTotal = parseInt(defectTotalToAllocate.textContent);
        if (allocatedTotal === toAllocateTotal) {
            defectTotalAllocated.style.color = 'var(--color-success)';
        } else {
            defectTotalAllocated.style.color = 'var(--color-danger)';
        }
    }
    
    // Butonul de Salvare din noul modal
    async function handleSaveLogDetails() {
        // CORECȚIE STARE: Verificăm dacă datele de salvare există
        if (!logDataPendingSave) {
            showToast("Eroare: Datele logării s-au pierdut. Încercați anularea și relogarea.", 'error');
            return;
        }

        const toAllocateTotal = logDataPendingSave.scrapParts + logDataPendingSave.nrftParts;
        const allocatedTotal = currentDefectAllocations.reduce((sum, alloc) => sum + alloc.quantity, 0);

        if (allocatedTotal !== toAllocateTotal) {
            // ===================================================================================
            // NOTĂ: Aceasta este logica principală. Suma defectelor alocate (ex: 2 zgârieturi + 1 pată)
            // TREBUIE să fie egală cu suma pieselor Scrap + NRFT introduse inițial (ex: 3).
            // ===================================================================================
            showToast(`Eroare: Trebuie să alocați exact ${toAllocateTotal} piese defecte. Ați alocat ${allocatedTotal}.`, 'error');
            return;
        }
        
        // Colectăm datele din modal
        const downtimeReasonId = parseInt(modalDowntimeReason.value) || null;
        const downtimeMinutes = parseInt(modalDowntimeMinutes.value) || 0;

        if (downtimeMinutes > 0 && !downtimeReasonId) {
            showToast("Vă rugăm selectați un motiv pentru staționarea declarată.", 'error');
            return;
        }

        // CORECȚIE STARE: Combinăm datele stocate cu cele din modal
        const data = {
            ...logDataPendingSave, // Conține logTime, goodParts, scrapParts, nrftParts
            declaredDowntimeReasonId: downtimeReasonId,
            declaredDowntimeMinutes: downtimeMinutes,
            defecte: currentDefectAllocations.map(a => ({
                defectCodeId: a.defectCodeId,
                quantity: a.quantity
            }))
        };
        
        // Apelăm funcția centralizată de salvare
        await saveProductionLog(data);
    }

    // --- NOTĂ: Funcțiile utilitare (showToast, showLoading, etc.) au fost mutate în /js/utils.js ---

    async function apiCall(url, options = {}) {
        token = localStorage.getItem('token'); 
        options.headers = {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
            ...options.headers
        };
        
        try {
            const response = await fetch(url, options);
            if (!response.ok) {
                if (response.status === 401) {
                    showToast("Sesiune expirată. Vă rugăm să vă relogați.", 'error');
                    localStorage.removeItem('token');
                    window.location.href = '/login.html';
                }
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || `Eroare ${response.status}`);
            }
            if (response.status === 204) {
                return null;
            }
            const text = await response.text();
            return text ? JSON.parse(text) : null;
            
        } catch (error) {
            if (error.message.includes("Unexpected end of JSON input")) {
                return null; 
            }
            console.error(`Eroare API la ${url}:`, error);
            showToast(`Eroare: ${error.message}`, 'error');
            throw error;
        }
    }

    // --- Pornire Aplicație ---
    initializePage();
});