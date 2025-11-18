// Changeover Modal for Justification
// Include this in pages that need changeover justification

function showChangeoverJustificationModal(changeoverId, durationMinutes, targetMinutes) {
    const modal = document.getElementById('changeoverJustificationModal');
    if (!modal) {
        createChangeoverModal();
    }
    
    document.getElementById('changeoverDuration').textContent = durationMinutes;
    document.getElementById('changeoverTarget').textContent = targetMinutes;
    document.getElementById('changeoverExceeded').textContent = durationMinutes - targetMinutes;
    document.getElementById('changeoverId').value = changeoverId;
    
    document.getElementById('changeoverJustificationModal').classList.add('active');
}

function createChangeoverModal() {
    const modalHTML = `
        <div class="modal" id="changeoverJustificationModal">
            <div class="modal-content">
                <div class="modal-header">
                    <h2>⚠️ Changeover Depășit Target</h2>
                    <span class="close-modal" onclick="closeChangeoverModal()">✕</span>
                </div>
                <div class="modal-body">
                    <div class="warning-box">
                        <p><strong>Timpul de changeover a depășit targetul!</strong></p>
                        <div class="changeover-stats">
                            <div class="stat-item">
                                <span class="label">Durată Efectivă:</span>
                                <span class="value" id="changeoverDuration">0</span> minute
                            </div>
                            <div class="stat-item">
                                <span class="label">Target:</span>
                                <span class="value" id="changeoverTarget">0</span> minute
                            </div>
                            <div class="stat-item exceeded">
                                <span class="label">Depășire:</span>
                                <span class="value" id="changeoverExceeded">0</span> minute
                            </div>
                        </div>
                    </div>
                    
                    <form id="changeoverJustificationForm">
                        <input type="hidden" id="changeoverId">
                        
                        <div class="form-group">
                            <label for="justificationReason" class="required">Motivul Depășirii</label>
                            <select id="justificationReason" required>
                                <option value="">Selectați motivul...</option>
                                <option value="equipment_issue">Problemă echipament</option>
                                <option value="tooling_change">Schimbare scule</option>
                                <option value="quality_check">Verificare calitate</option>
                                <option value="training">Training operator nou</option>
                                <option value="material_issue">Problemă materiale</option>
                                <option value="cleaning_extended">Curățare extinsă</option>
                                <option value="other">Altele</option>
                            </select>
                        </div>
                        
                        <div class="form-group">
                            <label for="justificationDetails" class="required">Detalii</label>
                            <textarea id="justificationDetails" rows="4" required placeholder="Descrieți pe scurt ce a cauzat întârzierea..."></textarea>
                        </div>
                        
                        <div class="form-group">
                            <label for="correctiveAction">Acțiuni Corective Propuse</label>
                            <textarea id="correctiveAction" rows="3" placeholder="Ce poate fi îmbunătățit pentru viitor?"></textarea>
                        </div>
                        
                        <div class="form-actions">
                            <button type="button" class="btn-cancel" onclick="closeChangeoverModal()">Anulează</button>
                            <button type="submit" class="btn-save">💾 Salvează Justificarea</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    `;
    
    document.body.insertAdjacentHTML('beforeend', modalHTML);
    
    // Add event listener to form
    document.getElementById('changeoverJustificationForm').addEventListener('submit', handleChangeoverJustification);
}

async function handleChangeoverJustification(event) {
    event.preventDefault();
    
    const changeoverId = document.getElementById('changeoverId').value;
    const reason = document.getElementById('justificationReason').value;
    const details = document.getElementById('justificationDetails').value;
    const correctiveAction = document.getElementById('correctiveAction').value;
    
    try {
        // Create a technical downtime entry for the exceeded changeover
        const response = await fetch('/api/interventii', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                lineId: parseInt(sessionStorage.getItem('changeoverLineId')),
                equipmentId: null,
                productId: null,
                dataRaportareOperator: new Date().toISOString(),
                operatorNume: sessionStorage.getItem('username') || 'System',
                problemaRaportataId: null,
                defectiuneTextLiber: `CHANGEOVER DEPĂȘIT: ${reason} - ${details}`,
                defectiuneIdentificataId: null,
                rootCause: reason,
                correctiveAction: correctiveAction,
                preventiveAction: 'Monitorizare target changeover',
                durataMinute: parseInt(document.getElementById('changeoverExceeded').textContent),
                influenteazaProdusul: false,
                status: 'Închis',
                dataStartInterventie: new Date().toISOString(),
                dataStopInterventie: new Date().toISOString()
            })
        });
        
        if (response.ok) {
            alert('Justificare salvată cu succes!');
            closeChangeoverModal();
            // Optionally refresh the page or update UI
        } else {
            throw new Error('Failed to save justification');
        }
    } catch (error) {
        console.error('Error saving justification:', error);
        alert('Eroare la salvarea justificării. Vă rugăm încercați din nou.');
    }
}

function closeChangeoverModal() {
    document.getElementById('changeoverJustificationModal')?.classList.remove('active');
}

// Add CSS for changeover modal
const changeoverModalCSS = `
<style>
.warning-box {
    background: #fef3c7;
    border-left: 4px solid #f59e0b;
    padding: 20px;
    margin-bottom: 20px;
    border-radius: 4px;
}

.changeover-stats {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    gap: 15px;
    margin-top: 15px;
}

.stat-item {
    background: white;
    padding: 15px;
    border-radius: 6px;
    text-align: center;
}

.stat-item.exceeded {
    background: #fee2e2;
    border: 2px solid #ef4444;
}

.stat-item .label {
    display: block;
    font-size: 0.9em;
    color: #6b7280;
    margin-bottom: 5px;
}

.stat-item .value {
    display: block;
    font-size: 1.8em;
    font-weight: bold;
    color: #1f2937;
}

.stat-item.exceeded .value {
    color: #991b1b;
}
</style>
`;

// Inject CSS if not already present
if (!document.getElementById('changeover-modal-css')) {
    document.head.insertAdjacentHTML('beforeend', changeoverModalCSS.replace('<style>', '<style id="changeover-modal-css">'));
}
