# MesEnterprise_v3 - Implementation Guide

## 🎯 Overview

MesEnterprise_v3 este un sistem complet de Management Execuție Producție (MES) cu funcționalități extinse pentru:
- **Staționări Tehnice** (Technical Downtimes)
- **Spare Parts Management**
- **TPM** (Total Productive Maintenance)
- **Changeover Monitoring** cu target și alerts
- **Role-Based Access Control**
- **Excel Export** pentru toate modulele

---

## 📋 Modele Implementate

### Core Models (13 entități)
- **User** - Utilizatori sistem
- **Role** - Roluri (Admin, Operator, Tehnician, etc.)
- **Permission** - Permisiuni granulare
- **RolePermission** - Mapare rol-permisiune
- **Department** - Departamente
- **Line** - Linii producție (extins cu ChangeoverTargetMinutes, TargetOEEPercent)
- **Equipment** - Echipamente/Stații
- **Product** - Produse
- **BreakdownReason** - Motive oprire
- **Shift** - Schimburi
- **ShiftBreak** - Pauze schimb
- **PlannedDowntime** - Downtimeuri planificate
- **ObservatieOperator** - Observații operator

### Production Models (4 entități)
- **LineStatus** - Status curent linii
- **ProductionLog** - Loguri producție orare
- **ProductionLogDefect** - Alocări defecte
- **ChangeoverLog** - Istoric changeover

### Maintenance Models (5 entități)
- **InterventieTichet** - Tichete intervenții tehnice (14 câmpuri Access)
- **ProblemaRaportata** - Probleme configurabile
- **DefectiuneIdentificata** - Defecțiuni configurabile
- **ProblemaDefectiuneCorelatie** - Corelații problemă-defect
- **PreventiveMaintenancePlan** - Planuri TPM

### Inventory Models (4 entități)
- **SparePart** - Piese de schimb
- **SparePartUsage** - **NOU** - Tracking utilizare piese în intervenții
- **RawMaterial** - Materiale prime
- **ProductBOM** - BOM produse

### Quality Models (5 entități)
- **DefectCategory** - Categorii defecte
- **DefectCode** - Coduri defecte
- **QualityTest** - Teste calitate
- **ProductionLogQualityCheck** - Verificări calitate
- **MrbTicket** - Tichete MRB

### Other Models (6 entități)
- **AlertRule, AlertLog** - Alerte sistem
- **ExportJob, ExportTemplate** - Export templates
- **SystemSetting, StopOnDefectRule** - Configurări

---

## 🚀 Funcționalități Implementate

### 1. Staționări Tehnice (Technical Downtimes)

**UI:** `stationari-tehnice.html`
**JavaScript:** `js/stationari-tehnice.js`
**API:** Folosește `/api/interventii` (InterventieTichet)

#### Caracteristici:
- ✅ **14 câmpuri** conform Access:
  1. Data și ora (DataRaportareOperator)
  2. Linie (LineId)
  3. Stație/Echipament (EquipmentId)
  4. Produs (ProductId)
  5. Influențează produsul? (InfluenteazaProdusul)
  6. Durată (DurataMinute)
  7. Schimb (derivat din timestamp)
  8. Nume Operator (OperatorNume)
  9. Motivul staționării (ProblemaRaportataId)
  10. Descriere problemă (DefectiuneTextLiber)
  11. Defecțiune identificată (DefectiuneIdentificataId)
  12. Cauza probabilă (RootCause)
  13. Acțiuni luate (CorrectiveAction)
  14. Acțiuni preventive (PreventiveAction)

- ✅ **Timer live** pentru tracking durată intervenție
- ✅ **Auto-calculare** durată din start/stop times
- ✅ **Non-blocant** - nu blochează logarea producției
- ✅ **Filtrare avansată** istoric
- ✅ **Status badges** color-coded

#### Flux de utilizare:
1. Operator/Tehnician apasă "Start Intervenție Nouă"
2. Timer pornește automat
3. Completează toate câmpurile necesare
4. La salvare, durata se calculează automat
5. Intervenția apare în istoric cu status

---

### 2. Spare Parts Management

**UI:** `spare-parts.html`
**JavaScript:** `js/spare-parts.js`
**API:** `/api/spare-parts`
**Model:** `SparePartUsage` (nou creat)

#### Caracteristici:
- ✅ **Dashboard statistici**: Total piese, Stoc scăzut, Valoare inventar
- ✅ **CRUD complet** pentru piese
- ✅ **Tracking utilizare** în intervenții
- ✅ **Istoric complet** per piesă
- ✅ **Alerte stoc scăzut** automate
- ✅ **Status badges** (OK/Scăzut/Critic)
- ✅ **Filtrare și căutare** avansată

#### Endpoints:
```
GET    /api/spare-parts                 - Lista piese
GET    /api/spare-parts/{id}            - Detalii piesă
POST   /api/spare-parts                 - Creare piesă nouă
PUT    /api/spare-parts/{id}            - Actualizare piesă
DELETE /api/spare-parts/{id}            - Dezactivare piesă
GET    /api/spare-parts/statistics      - Statistici inventar
GET    /api/spare-parts/{id}/usage-history - Istoric utilizare
POST   /api/spare-parts/use             - Înregistrare utilizare
GET    /api/spare-parts/low-stock       - Piese cu stoc scăzut
```

#### Integrare cu Intervenții:
Când se folosește o piesă în intervenție:
1. Se selectează piesa din inventar
2. Se introduce cantitatea
3. Stocul se scade automat
4. Se creează `SparePartUsage` legat de `InterventieTichetId`
5. Cost total se calculează automat

---

### 3. TPM (Total Productive Maintenance)

**UI:** `tpm-planning.html`
**JavaScript:** `js/tpm-planning.js`
**API:** `/api/tpm`

#### Caracteristici:
- ✅ **Planificare mentenanță** preventivă
- ✅ **Calendar vizual** cu task-uri
- ✅ **Frecvențe configurabile** (zilnic/săptămânal/lunar/anual)
- ✅ **Check-lists** pentru fiecare plan
- ✅ **Tracking execuție** automat
- ✅ **Dashboard statistici**: Active, Astăzi, Întârziate, Completate

#### Tabs:
1. **Planuri** - Gestionare planuri TPM
2. **Calendar** - Vizualizare calendar cu task-uri
3. **Execuție** - Task-uri de executat
4. **Istoric** - Istoric execuții

#### Endpoints:
```
GET  /api/tpm/statistics         - Statistici TPM
GET  /api/tpm/plans               - Lista planuri
GET  /api/tpm/plans/{id}          - Detalii plan
POST /api/tpm/plans               - Creare plan
PUT  /api/tpm/plans/{id}          - Actualizare plan
POST /api/tpm/execute/{id}        - Înregistrare execuție
```

#### Auto-calculation:
- Următoarea scadență se calculează automat bazat pe frecvență
- La execuție, se recalculează următoarea dată
- Alertele pentru task-uri întârziate

---

### 4. Changeover cu Target și Prompt

**Extended:** Line model cu `ChangeoverTargetMinutes`
**API:** `/api/changeover` (extins)

#### Caracteristici:
- ✅ **Target configurat** per linie
- ✅ **Monitorizare automată** durată
- ✅ **Prompt automat** când target depășit
- ✅ **Integrare OEE** corectă

#### Flux:
1. Inginer configurează `ChangeoverTargetMinutes` în Line
2. Tehnician start changeover (`POST /api/changeover/start`)
3. Tehnician complete changeover (`POST /api/changeover/complete`)
4. Sistem compară durată cu target
5. Dacă depășit:
   - Response include `ExceededTarget: true` și `RequiresJustification: true`
   - Frontend deschide modal pentru justificare
   - Se salvează în InterventieTichet cu tip special

#### Endpoints noi:
```
POST /api/changeover/complete
Body: { "ChangeoverId": 123 }
Response: {
  "DurationMinutes": 45,
  "ExceededTarget": true,
  "TargetMinutes": 30,
  "RequiresJustification": true
}
```

---

### 5. Export Excel

**API:** `/api/export`
**Endpoints:** `ExcelExportEndpoints.cs`
**Library:** NPOI (XSSF pentru .xlsx)

#### Exports disponibile:
```
GET /api/export/stationari-tehnice?startDate=...&endDate=...&lineId=...
GET /api/export/spare-parts-usage?startDate=...&endDate=...
GET /api/export/tpm-plans
GET /api/export/changeover?startDate=...&endDate=...&lineId=...
```

#### Caracteristici:
- ✅ Filtrare pe toate criteriile relevante
- ✅ Auto-size columns
- ✅ Header styling (bold)
- ✅ Format .xlsx modern
- ✅ Timestamp în nume fișier

#### Utilizare în UI:
```javascript
// Adaugă buton în pagină:
<button onclick="exportToExcel()">📊 Export Excel</button>

// JavaScript:
function exportToExcel() {
    const params = new URLSearchParams({
        startDate: startDateInput.value,
        endDate: endDateInput.value,
        lineId: lineIdInput.value
    });
    window.location.href = `/api/export/stationari-tehnice?${params}`;
}
```

---

## 🎨 Design UI/UX

### Culori și Teme
- **Staționări Tehnice**: Gradient violet/purple (#667eea → #764ba2)
- **Spare Parts**: Gradient orange (#f59e0b → #d97706)
- **TPM**: Gradient pink/purple (#ec4899 → #8b5cf6)
- **Success**: Green (#10b981)
- **Warning**: Yellow (#f59e0b)
- **Error**: Red (#ef4444)

### Componente UI
- ✅ **Gradient headers** cu shadow effects
- ✅ **Cards** cu hover animations
- ✅ **Badges** color-coded pentru status
- ✅ **Modals** elegante pentru editare
- ✅ **Forms** pe 2 coloane responsive
- ✅ **Tables** moderne cu hover
- ✅ **Buttons** cu icoane emoji
- ✅ **Timers** live pentru tracking

### Responsive Design
- Mobile-first approach
- Grid layouts cu auto-fit
- Media queries pentru < 768px
- Touch-friendly buttons
- Scroll horizontal pentru tables

---

## 👥 Roluri și Permisiuni

### Roluri Planificate:
1. **Admin** - Acces complet
2. **Manager** - Vizualizare rapoarte, configurări read-only
3. **Inginer** - Configurări, TPM planning, targets
4. **Tehnician Mentenanță** - TPM execuție, spare parts
5. **Tehnician** - Intervenții tehnice
6. **TeamLeader** - Monitoring echipă, rapoarte
7. **Operator** - Logare producție, raportare probleme

### Implementare RBAC:
Models există: `Role`, `Permission`, `RolePermission`

**TODO:**
- Seeding roluri în `DatabaseInitializer`
- UI configurare permisiuni (`/admin/roles`)
- Middleware verificare permisiuni pe endpoints
- Frontend hiding/showing based on roles

---

## 📊 Rapoarte și Analytics

### Rapoarte Disponibile:
1. **Staționări Tehnice**
   - Filtrate: perioadă, linie, stație, problemă
   - Metrici: frecvență, durată medie, cost

2. **Spare Parts**
   - Utilizare per piesă
   - Cost total pe perioadă
   - Forecast restock

3. **TPM**
   - Compliance rate
   - Task-uri întârziate
   - Trend execuții

4. **Changeover**
   - Timp real vs target
   - Justificări depășiri
   - Trend îmbunătățire

### Export Excel:
Toate rapoartele pot fi exportate în Excel cu filtrele aplicate.

---

## 🔧 Configurabilitate

### Liste Configurabile:
Toate se gestionează din `/config.html`:

1. **Linii** - Nume, Identifier, ChangeoverTarget, TargetOEE
2. **Echipamente** - Per linie
3. **Produse** - Cycle time, BOM
4. **Probleme Raportate** - Liste custom
5. **Defecțiuni Identificate** - Liste custom
6. **Corelații** - Problemă-Defecțiune mapping
7. **Schimburi** - Ore, Pauze
8. **Defect Codes** - Categorii și coduri

### System Settings:
- Justification required
- Threshold OEE percent
- Auto-close ticket minutes
- Logging modes

---

## 🚦 Fluxuri Complete

### Flux 1: Operator Raportează Problemă
```
1. Operator → "Start Intervenție" în stationari-tehnice.html
2. Timer start automat
3. Completează formular (14 câmpuri)
4. Salvare → POST /api/interventii
5. Tehnician vede în listă cu status "Deschis"
6. Tehnician preia → Status "În Lucru"
7. Tehnician adaugă spare parts folosite
8. Tehnician închide → Status "Închis"
9. Sistem actualizează statistici
```

### Flux 2: Changeover cu Depășire Target
```
1. Inginer configurează ChangeoverTargetMinutes=30 în Line
2. Tehnician → Start Changeover
3. LineStatus → "Changeover"
4. După 45 minute → Complete Changeover
5. API detectează 45 > 30
6. Response: RequiresJustification=true
7. Frontend deschide modal justificare
8. Tehnician scrie motiv
9. Salvare ca InterventieTichet tip "Changeover depășit"
10. Raport include ambele: changeover log + justificare
```

### Flux 3: TPM Execution
```
1. Inginer creează plan TPM: "Lubrifiere săptămânală"
2. Frecvență: 1 Week
3. Sistem calculează NextDueDate
4. Tehnician vede în tab "Execuție" 
5. Tehnician → "Execută Acum"
6. Marchează check-list items
7. Poate adăuga spare parts folosite
8. Salvare → POST /api/tpm/execute/{id}
9. Sistem recalculează NextDueDate (+1 săptămână)
10. Istoric se actualizează
```

---

## 📝 Best Practices

### Backend:
- Folosește `async/await` consistent
- Include related entities cu `.Include()`
- Validare input în endpoints
- Return proper HTTP status codes
- Log errors cu Serilog

### Frontend:
- Async/await pentru API calls
- Error handling cu try/catch
- Loading states pentru UX
- Debounce pentru search inputs
- Cache API responses când posibil

### Database:
- Indexes pe foreign keys
- Nullable fields pentru opționale
- Timestamps (UTC) pentru tracking
- Soft delete (IsActive flag)
- Audit trails pentru modificări

---

## 🐛 Troubleshooting

### Build Errors:
```bash
# Clean și rebuild
dotnet clean
dotnet build
```

### Missing Models:
Toate modelele sunt în `/Models` subdirectoare.
Verifică namespace-urile sunt corecte.

### API Errors:
Verifică în browser console (F12) exact ce returnează API-ul.
Status codes:
- 200: OK
- 201: Created
- 400: Bad Request (validare)
- 401: Unauthorized (auth)
- 404: Not Found
- 500: Server Error (check logs)

---

## 🚀 Next Steps

### Finalizare:
1. ✅ Build și test toate endpoints
2. ✅ Verificare UI/UX pe toate paginile
3. ⏳ Seeding roluri și permisiuni
4. ⏳ UI configurare roluri în Admin
5. ⏳ Integrare autentificare cu toate paginile noi
6. ⏳ Screenshots pentru documentație
7. ⏳ User training materials

### Features Viitoare:
- Mobile app pentru operator
- Real-time notifications (SignalR)
- Advanced analytics cu charts
- Predictive maintenance AI
- Integration cu ERP

---

## 📞 Support

Pentru întrebări sau probleme:
- Verifică această documentație
- Check logs în `/logs`
- Review commit history pentru changes
- Contact: [Your contact info]

---

**Ultima actualizare:** 2025-01-18
**Versiune:** 3.0
**Status:** Production Ready pentru Module 1-5
