// TPM Planning - JavaScript
const API_BASE = '/api';
let currentMonth = new Date();
let currentEditId = null;

document.addEventListener('DOMContentLoaded', function() {
    loadStatistics();
    loadPlans();
    loadDropdowns();
    setupFormHandlers();
    renderCalendar();
});

function switchTab(tabName) {
    document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));
    
    event.target.classList.add('active');
    document.getElementById(tabName + 'Tab').classList.add('active');
    
    if (tabName === 'calendar') renderCalendar();
    if (tabName === 'execute') loadExecuteTasks();
    if (tabName === 'history') loadHistory();
}

async function loadStatistics() {
    try {
        const response = await fetch(`${API_BASE}/tpm/statistics`);
        const stats = await response.json();
        
        document.getElementById('activePlans').textContent = stats.activePlans || 0;
        document.getElementById('todayTasks').textContent = stats.todayTasks || 0;
        document.getElementById('overdueTasks').textContent = stats.overdueTasks || 0;
        document.getElementById('completedMonth').textContent = stats.completedMonth || 0;
    } catch (error) {
        console.error('Error loading statistics:', error);
    }
}

async function loadPlans() {
    try {
        const response = await fetch(`${API_BASE}/tpm/plans`);
        const plans = await response.json();
        
        const container = document.getElementById('plansList');
        container.innerHTML = '';
        
        plans.forEach(plan => {
            const card = document.createElement('div');
            card.className = 'plan-card';
            
            const statusClass = plan.nextDueDate ? 
                (new Date(plan.nextDueDate) < new Date() ? 'status-overdue' : 'status-upcoming') : 
                'status-active';
            
            card.innerHTML = `
                <div class="plan-header">
                    <div>
                        <div class="plan-title">${plan.name}</div>
                        <div class="plan-meta">
                            <span>🏭 ${plan.lineName || 'Toate liniile'}</span>
                            <span>⚙️ ${plan.equipmentName || 'Toate echipamentele'}</span>
                            <span class="frequency-badge freq-${plan.frequencyType.toLowerCase()}">${plan.frequencyValue} ${plan.frequencyType}</span>
                        </div>
                    </div>
                    <span class="status-badge ${statusClass}">${plan.isActive ? 'Activ' : 'Inactiv'}</span>
                </div>
                <div>${plan.description || ''}</div>
                <div style="margin-top: 15px;">
                    <strong>Următoarea executie:</strong> ${plan.nextDueDate ? new Date(plan.nextDueDate).toLocaleDateString('ro-RO') : '-'}
                </div>
                <div style="margin-top: 10px; display: flex; gap: 10px;">
                    <button class="btn-icon btn-edit" onclick="editPlan(${plan.id})">✏️ Editează</button>
                    <button class="btn-execute" onclick="executePlan(${plan.id})">▶️ Execută Acum</button>
                </div>
            `;
            
            container.appendChild(card);
        });
    } catch (error) {
        console.error('Error loading plans:', error);
    }
}

async function loadDropdowns() {
    try {
        const linesResponse = await fetch(`${API_BASE}/config/lines`);
        const lines = await linesResponse.json();
        const lineSelect = document.getElementById('planLine');
        
        lines.forEach(line => {
            lineSelect.add(new Option(line.name, line.id));
        });
    } catch (error) {
        console.error('Error loading dropdowns:', error);
    }
}

async function loadPlanEquipments() {
    const lineId = document.getElementById('planLine').value;
    const equipmentSelect = document.getElementById('planEquipment');
    
    equipmentSelect.innerHTML = '<option value="">Selectați echipamentul...</option>';
    
    if (!lineId) return;
    
    try {
        const response = await fetch(`${API_BASE}/config/equipments?lineId=${lineId}`);
        const equipments = await response.json();
        
        equipments.forEach(equipment => {
            equipmentSelect.add(new Option(equipment.name, equipment.id));
        });
    } catch (error) {
        console.error('Error loading equipments:', error);
    }
}

function setupFormHandlers() {
    const form = document.getElementById('planForm');
    form.addEventListener('submit', handlePlanFormSubmit);
}

async function handlePlanFormSubmit(event) {
    event.preventDefault();
    
    const formData = {
        name: document.getElementById('planName').value,
        description: document.getElementById('planDescription').value,
        lineId: document.getElementById('planLine').value ? parseInt(document.getElementById('planLine').value) : null,
        equipmentId: document.getElementById('planEquipment').value ? parseInt(document.getElementById('planEquipment').value) : null,
        frequencyType: document.getElementById('planFrequencyType').value,
        frequencyValue: parseInt(document.getElementById('planFrequencyValue').value),
        checklist: document.getElementById('planChecklist').value,
        isActive: true
    };

    try {
        const url = currentEditId ? `${API_BASE}/tpm/plans/${currentEditId}` : `${API_BASE}/tpm/plans`;
        const method = currentEditId ? 'PUT' : 'POST';
        
        const response = await fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(formData)
        });

        if (response.ok) {
            alert('Plan salvat cu succes!');
            closePlanModal();
            loadPlans();
            loadStatistics();
        } else {
            throw new Error('Failed to save plan');
        }
    } catch (error) {
        console.error('Error saving plan:', error);
        alert('Eroare la salvarea planului');
    }
}

function openPlanModal() {
    document.getElementById('planModalTitle').textContent = 'Plan de Mentenanță Nou';
    document.getElementById('planForm').reset();
    currentEditId = null;
    document.getElementById('planModal').classList.add('active');
}

async function editPlan(id) {
    try {
        const response = await fetch(`${API_BASE}/tpm/plans/${id}`);
        const plan = await response.json();
        
        document.getElementById('planModalTitle').textContent = 'Editează Plan';
        document.getElementById('planName').value = plan.name;
        document.getElementById('planDescription').value = plan.description || '';
        document.getElementById('planLine').value = plan.lineId || '';
        await loadPlanEquipments();
        document.getElementById('planEquipment').value = plan.equipmentId || '';
        document.getElementById('planFrequencyType').value = plan.frequencyType;
        document.getElementById('planFrequencyValue').value = plan.frequencyValue;
        document.getElementById('planChecklist').value = plan.checklist || '';
        
        currentEditId = id;
        document.getElementById('planModal').classList.add('active');
    } catch (error) {
        console.error('Error loading plan:', error);
    }
}

function closePlanModal() {
    document.getElementById('planModal').classList.remove('active');
    currentEditId = null;
}

async function executePlan(planId) {
    if (!confirm('Doriți să marcați această mentenanță ca executată?')) return;
    
    try {
        const response = await fetch(`${API_BASE}/tpm/execute/${planId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                executedAt: new Date().toISOString(),
                notes: ''
            })
        });

        if (response.ok) {
            alert('Execuție înregistrată cu succes!');
            loadPlans();
            loadStatistics();
        }
    } catch (error) {
        console.error('Error executing plan:', error);
        alert('Eroare la înregistrarea execuției');
    }
}

function renderCalendar() {
    const grid = document.getElementById('calendarGrid');
    const header = document.getElementById('currentMonth');
    
    const year = currentMonth.getFullYear();
    const month = currentMonth.getMonth();
    
    header.textContent = currentMonth.toLocaleDateString('ro-RO', { month: 'long', year: 'numeric' });
    
    const firstDay = new Date(year, month, 1).getDay();
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    
    grid.innerHTML = '';
    
    // Add day headers
    ['L', 'M', 'M', 'J', 'V', 'S', 'D'].forEach(day => {
        const header = document.createElement('div');
        header.style.textAlign = 'center';
        header.style.fontWeight = 'bold';
        header.textContent = day;
        grid.appendChild(header);
    });
    
    // Add empty cells before first day
    for (let i = 0; i < (firstDay === 0 ? 6 : firstDay - 1); i++) {
        grid.appendChild(document.createElement('div'));
    }
    
    // Add days
    for (let day = 1; day <= daysInMonth; day++) {
        const dayDiv = document.createElement('div');
        dayDiv.className = 'calendar-day';
        dayDiv.textContent = day;
        // TODO: Mark days with scheduled tasks
        grid.appendChild(dayDiv);
    }
}

function previousMonth() {
    currentMonth.setMonth(currentMonth.getMonth() - 1);
    renderCalendar();
}

function nextMonth() {
    currentMonth.setMonth(currentMonth.getMonth() + 1);
    renderCalendar();
}

async function loadExecuteTasks() {
    // Implementation for execute tab
}

async function loadHistory() {
    // Implementation for history tab
}
