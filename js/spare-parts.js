// Spare Parts Management - JavaScript
const API_BASE = '/api';
let currentEditId = null;

document.addEventListener('DOMContentLoaded', function() {
    loadStatistics();
    loadSpareParts();
    setupFormHandlers();
    setupSearchFilters();
});

async function loadStatistics() {
    try {
        const response = await fetch(`${API_BASE}/spare-parts/statistics`);
        const stats = await response.json();
        
        document.getElementById('totalParts').textContent = stats.totalParts || 0;
        document.getElementById('lowStockParts').textContent = stats.lowStockParts || 0;
        document.getElementById('inventoryValue').textContent = 
            (stats.inventoryValue || 0).toFixed(2) + ' RON';
        document.getElementById('pendingOrders').textContent = stats.pendingOrders || 0;
    } catch (error) {
        console.error('Error loading statistics:', error);
    }
}

async function loadSpareParts() {
    try {
        const response = await fetch(`${API_BASE}/spare-parts`);
        const parts = await response.json();
        
        const tbody = document.getElementById('partsTableBody');
        tbody.innerHTML = '';
        
        parts.forEach(part => {
            const stockStatus = getStockStatus(part.quantityInStock, part.minimumStock);
            const row = tbody.insertRow();
            row.innerHTML = `
                <td><strong>${part.partNumber}</strong></td>
                <td>${part.name}</td>
                <td>${part.description || '-'}</td>
                <td>${part.location || '-'}</td>
                <td>${part.quantityInStock}</td>
                <td>${part.minimumStock}</td>
                <td><span class="stock-badge stock-${stockStatus.class}">${stockStatus.text}</span></td>
                <td>${part.unitCost.toFixed(2)} RON</td>
                <td>
                    <div class="action-buttons">
                        <button class="btn-icon btn-edit" onclick="editPart(${part.id})" title="Editează">
                            ✏️
                        </button>
                        <button class="btn-icon btn-history" onclick="viewHistory(${part.id})" title="Istoric">
                            📊
                        </button>
                    </div>
                </td>
            `;
        });
    } catch (error) {
        console.error('Error loading spare parts:', error);
        showNotification('Eroare la încărcarea pieselor', 'error');
    }
}

function getStockStatus(current, minimum) {
    const ratio = current / minimum;
    if (ratio >= 1.5) {
        return { class: 'ok', text: 'OK' };
    } else if (ratio >= 1) {
        return { class: 'low', text: 'Scăzut' };
    } else {
        return { class: 'critical', text: 'Critic' };
    }
}

function setupFormHandlers() {
    const partForm = document.getElementById('partForm');
    partForm.addEventListener('submit', handlePartFormSubmit);
}

function setupSearchFilters() {
    const searchInput = document.getElementById('searchPart');
    const categoryFilter = document.getElementById('filterCategory');
    const stockFilter = document.getElementById('filterStock');
    
    searchInput.addEventListener('input', applyFilters);
    categoryFilter.addEventListener('change', applyFilters);
    stockFilter.addEventListener('change', applyFilters);
}

function applyFilters() {
    // Implementation for filtering
    loadSpareParts();
}

function openAddPartModal() {
    document.getElementById('modalTitle').textContent = 'Adaugă Piesă Nouă';
    document.getElementById('partForm').reset();
    currentEditId = null;
    document.getElementById('partModal').classList.add('active');
}

async function editPart(id) {
    try {
        const response = await fetch(`${API_BASE}/spare-parts/${id}`);
        const part = await response.json();
        
        document.getElementById('modalTitle').textContent = 'Editează Piesă';
        document.getElementById('partNumber').value = part.partNumber;
        document.getElementById('partName').value = part.name;
        document.getElementById('partDescription').value = part.description || '';
        document.getElementById('partLocation').value = part.location || '';
        document.getElementById('partStock').value = part.quantityInStock;
        document.getElementById('partMinStock').value = part.minimumStock;
        document.getElementById('partCost').value = part.unitCost;
        
        currentEditId = id;
        document.getElementById('partModal').classList.add('active');
    } catch (error) {
        console.error('Error loading part:', error);
        showNotification('Eroare la încărcarea piesei', 'error');
    }
}

async function handlePartFormSubmit(event) {
    event.preventDefault();
    
    const formData = {
        partNumber: document.getElementById('partNumber').value,
        name: document.getElementById('partName').value,
        description: document.getElementById('partDescription').value,
        location: document.getElementById('partLocation').value,
        quantityInStock: parseInt(document.getElementById('partStock').value),
        minimumStock: parseInt(document.getElementById('partMinStock').value),
        unitCost: parseFloat(document.getElementById('partCost').value),
        isActive: true
    };

    try {
        const url = currentEditId 
            ? `${API_BASE}/spare-parts/${currentEditId}`
            : `${API_BASE}/spare-parts`;
        
        const method = currentEditId ? 'PUT' : 'POST';
        
        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        });

        if (response.ok) {
            showNotification('Piesa a fost salvată cu succes!', 'success');
            closePartModal();
            loadSpareParts();
            loadStatistics();
        } else {
            throw new Error('Failed to save part');
        }
    } catch (error) {
        console.error('Error saving part:', error);
        showNotification('Eroare la salvarea piesei', 'error');
    }
}

function closePartModal() {
    document.getElementById('partModal').classList.remove('active');
    currentEditId = null;
}

async function viewHistory(partId) {
    try {
        const response = await fetch(`${API_BASE}/spare-parts/${partId}/usage-history`);
        const history = await response.json();
        
        const historyContent = document.getElementById('historyContent');
        historyContent.innerHTML = `
            <table class="parts-table">
                <thead>
                    <tr>
                        <th>Data</th>
                        <th>Intervenție</th>
                        <th>Cantitate</th>
                        <th>Utilizator</th>
                        <th>Note</th>
                    </tr>
                </thead>
                <tbody>
                    ${history.map(h => `
                        <tr>
                            <td>${new Date(h.usedAt).toLocaleString('ro-RO')}</td>
                            <td>${h.interventionId || '-'}</td>
                            <td>${h.quantityUsed}</td>
                            <td>${h.userName || '-'}</td>
                            <td>${h.notes || '-'}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        `;
        
        document.getElementById('historyModal').classList.add('active');
    } catch (error) {
        console.error('Error loading history:', error);
        showNotification('Eroare la încărcarea istoricului', 'error');
    }
}

function closeHistoryModal() {
    document.getElementById('historyModal').classList.remove('active');
}

function showNotification(message, type = 'info') {
    alert(message);
}

function exportToExcel() {
    const params = new URLSearchParams();
    
    // Add date range if available
    const startDate = prompt('Data start (YYYY-MM-DD) sau Enter pentru toate:');
    const endDate = prompt('Data sfârșit (YYYY-MM-DD) sau Enter pentru toate:');
    
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    
    window.location.href = `${API_BASE}/export/spare-parts-usage?${params.toString()}`;
    showNotification('Export în curs...', 'info');
}
