// Staționări Tehnice - JavaScript
const API_BASE = '/api';
let interventionStartTime = null;
let timerInterval = null;

// Initialize page
document.addEventListener('DOMContentLoaded', function() {
    loadDropdowns();
    setupFormHandlers();
    setCurrentDateTime();
});

function setCurrentDateTime() {
    const now = new Date();
    const localDateTime = new Date(now.getTime() - now.getTimezoneOffset() * 60000)
        .toISOString()
        .slice(0, 16);
    document.getElementById('dataOra').value = localDateTime;
}

async function loadDropdowns() {
    try {
        // Load lines
        const linesResponse = await fetch(`${API_BASE}/config/lines`);
        const lines = await linesResponse.json();
        const linieSelect = document.getElementById('linie');
        const filterLinieSelect = document.getElementById('filterLinie');
        
        lines.forEach(line => {
            const option = new Option(line.name, line.id);
            linieSelect.add(option.cloneNode(true));
            filterLinieSelect.add(option);
        });

        // Load products
        const productsResponse = await fetch(`${API_BASE}/config/products`);
        const products = await productsResponse.json();
        const produsSelect = document.getElementById('produs');
        
        products.forEach(product => {
            produsSelect.add(new Option(product.name, product.id));
        });

        // Load shifts
        const shiftsResponse = await fetch(`${API_BASE}/config/shifts`);
        const shifts = await shiftsResponse.json();
        const schimbSelect = document.getElementById('schimb');
        
        shifts.forEach(shift => {
            schimbSelect.add(new Option(shift.name, shift.id));
        });

        // Load problems
        const problemsResponse = await fetch(`${API_BASE}/config/problems-raportate`);
        const problems = await problemsResponse.json();
        const problemaSelect = document.getElementById('problema');
        
        problems.forEach(problem => {
            problemaSelect.add(new Option(problem.nume, problem.id));
        });

        // Load defects
        const defectsResponse = await fetch(`${API_BASE}/config/defectiuni-identificate`);
        const defects = await defectsResponse.json();
        const defectiuneSelect = document.getElementById('defectiune');
        
        defects.forEach(defect => {
            defectiuneSelect.add(new Option(defect.nume, defect.id));
        });

    } catch (error) {
        console.error('Error loading dropdowns:', error);
        showNotification('Eroare la încărcarea datelor de configurare', 'error');
    }
}

async function loadEquipments() {
    const lineId = document.getElementById('linie').value;
    if (!lineId) return;

    try {
        const response = await fetch(`${API_BASE}/config/equipments?lineId=${lineId}`);
        const equipments = await response.json();
        const echipamentSelect = document.getElementById('echipament');
        
        // Clear existing options
        echipamentSelect.innerHTML = '<option value="">Selectați echipamentul...</option>';
        
        equipments.forEach(equipment => {
            echipamentSelect.add(new Option(equipment.name, equipment.id));
        });
    } catch (error) {
        console.error('Error loading equipments:', error);
    }
}

function setupFormHandlers() {
    const form = document.getElementById('technicalDowntimeForm');
    form.addEventListener('submit', handleFormSubmit);

    // Auto-calculate duration when start/stop times change
    document.getElementById('dataStart').addEventListener('change', calculateDuration);
    document.getElementById('dataStop').addEventListener('change', calculateDuration);
}

function calculateDuration() {
    const startInput = document.getElementById('dataStart');
    const stopInput = document.getElementById('dataStop');
    const durationInput = document.getElementById('durata');

    if (startInput.value && stopInput.value) {
        const start = new Date(startInput.value);
        const stop = new Date(stopInput.value);
        const diffMinutes = Math.floor((stop - start) / 60000);
        
        if (diffMinutes > 0) {
            durationInput.value = diffMinutes;
        }
    }
}

function startNewIntervention() {
    const formContainer = document.getElementById('interventionForm');
    const historyContainer = document.getElementById('historyContainer');
    
    formContainer.classList.add('active');
    historyContainer.classList.remove('active');
    
    // Start timer
    interventionStartTime = new Date();
    document.getElementById('timerDisplay').style.display = 'block';
    startTimer();
    
    // Set start time
    const localDateTime = new Date(interventionStartTime.getTime() - interventionStartTime.getTimezoneOffset() * 60000)
        .toISOString()
        .slice(0, 16);
    document.getElementById('dataStart').value = localDateTime;
}

function startTimer() {
    if (timerInterval) clearInterval(timerInterval);
    
    timerInterval = setInterval(() => {
        const now = new Date();
        const elapsed = Math.floor((now - interventionStartTime) / 1000);
        
        const hours = Math.floor(elapsed / 3600);
        const minutes = Math.floor((elapsed % 3600) / 60);
        const seconds = elapsed % 60;
        
        const timeString = `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
        document.getElementById('elapsedTime').textContent = timeString;
    }, 1000);
}

function stopTimer() {
    if (timerInterval) {
        clearInterval(timerInterval);
        timerInterval = null;
    }
    document.getElementById('timerDisplay').style.display = 'none';
}

async function handleFormSubmit(event) {
    event.preventDefault();
    
    const formData = {
        dataRaportareOperator: document.getElementById('dataOra').value,
        lineId: parseInt(document.getElementById('linie').value),
        equipmentId: parseInt(document.getElementById('echipament').value),
        productId: document.getElementById('produs').value ? parseInt(document.getElementById('produs').value) : null,
        operatorNume: document.getElementById('operator').value,
        problemaRaportataId: document.getElementById('problema').value ? parseInt(document.getElementById('problema').value) : null,
        defectiuneIdentificataId: document.getElementById('defectiune').value ? parseInt(document.getElementById('defectiune').value) : null,
        dataStartInterventie: document.getElementById('dataStart').value || null,
        dataStopInterventie: document.getElementById('dataStop').value || null,
        durataMinute: document.getElementById('durata').value ? parseInt(document.getElementById('durata').value) : null,
        influenteazaProdusul: document.getElementById('influenteazaProdusul').checked,
        defectiuneTextLiber: document.getElementById('descriereProblema').value,
        rootCause: document.getElementById('cauzaProbabila').value,
        correctiveAction: document.getElementById('actiuniLuate').value,
        preventiveAction: document.getElementById('actiuniPreventive').value,
        status: 'Inchis'
    };

    try {
        const response = await fetch(`${API_BASE}/interventii`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        });

        if (response.ok) {
            showNotification('Staționare tehnică salvată cu succes!', 'success');
            stopTimer();
            resetForm();
            // Optionally reload history
            if (document.getElementById('historyContainer').classList.contains('active')) {
                loadHistory();
            }
        } else {
            throw new Error('Failed to save');
        }
    } catch (error) {
        console.error('Error saving intervention:', error);
        showNotification('Eroare la salvarea staționării tehnice', 'error');
    }
}

function resetForm() {
    document.getElementById('technicalDowntimeForm').reset();
    setCurrentDateTime();
    interventionStartTime = null;
}

function cancelForm() {
    if (confirm('Sigur doriți să anulați înregistrarea?')) {
        stopTimer();
        resetForm();
        document.getElementById('interventionForm').classList.remove('active');
    }
}

function toggleHistory() {
    const formContainer = document.getElementById('interventionForm');
    const historyContainer = document.getElementById('historyContainer');
    
    if (historyContainer.classList.contains('active')) {
        historyContainer.classList.remove('active');
    } else {
        formContainer.classList.remove('active');
        historyContainer.classList.add('active');
        loadHistory();
    }
}

async function loadHistory() {
    try {
        const response = await fetch(`${API_BASE}/interventii/deschise`);
        const interventions = await response.json();
        
        const tbody = document.getElementById('historyTableBody');
        tbody.innerHTML = '';
        
        interventions.forEach(intervention => {
            const row = tbody.insertRow();
            row.innerHTML = `
                <td>${new Date(intervention.dataRaportareOperator).toLocaleString('ro-RO')}</td>
                <td>${intervention.linie || '-'}</td>
                <td>${intervention.echipament || '-'}</td>
                <td>${intervention.problema || '-'}</td>
                <td>${intervention.operatorNume || '-'}</td>
                <td>${intervention.durataMinute || '-'}</td>
                <td><span class="status-badge status-${intervention.status.toLowerCase().replace(' ', '')}">${intervention.status}</span></td>
                <td>
                    <button onclick="viewIntervention(${intervention.id})" class="btn-save" style="padding: 6px 12px; font-size: 0.9em;">
                        👁️ Detalii
                    </button>
                </td>
            `;
        });
    } catch (error) {
        console.error('Error loading history:', error);
        showNotification('Eroare la încărcarea istoricului', 'error');
    }
}

function applyFilters() {
    // Implementation for filtering
    loadHistory();
}

function viewIntervention(id) {
    // Implementation to view intervention details
    window.location.href = `interventii.html?id=${id}`;
}

function showNotification(message, type = 'info') {
    // Simple notification
    alert(message);
}
