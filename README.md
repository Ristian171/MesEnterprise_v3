# MES Enterprise v3

Sistema de Management al Execuției Producției (Manufacturing Execution System) pentru monitorizarea și controlul în timp real al liniilor de producție.

## 🚀 Funcționalități Principale

### Core MES
- **Monitorizare Producție în Timp Real**: Dashboard operator cu KPI-uri live (OEE, piese bune, scrap, target)
- **Gestionare Linii și Produse**: Configurare completă a liniilor de producție și produselor
- **Schimburi și Pauze**: Sistem de schimburi cu detectare automată și gestionare pauze
- **Logare Producție**: Înregistrare manuală sau automată a producției pe intervale orare
- **Changeover Management**: Urmărire și optimizare schimbări de produs
- **Raportare Avarii**: Sistem de tichete pentru intervenții de mentenanță
- **Observații Operator**: Jurnal de observații pentru fiecare linie

### Live Scan (NOU)
- **Scanare Automată**: Integrare cu stații de scanare cod de bare
- **Înregistrare Automată Piese Bune**: Fiecare scan incrementează automat contorul de piese bune
- **Control Live Scan**: Operatorul poate activa/dezactiva modulul Live Scan din pagina principală
- **Indicator Stație Scan**: 3 stări vizuale
  - 🟢 **Stație scan activă**: Live Scan pornit, piese înregistrate automat
  - 🟠 **Stație scan disponibilă (Live Scan dezactivat)**: Stație configurată dar Live Scan oprit
  - ⚪ **Stație scan neconfigurată**: Fără configurare scan pentru această linie
- **Identificare Unică**: Fiecare stație de scan are un identificator unic (ex: SCAN-L1)
- **Configurare Simplă**: Setup one-time pe tabletă cu salvare în localStorage

### OEE Target și Justificări (NOU)
- **Target OEE per Linie**: Fiecare linie poate avea un target de eficiență configurat (ex: 85%)
- **Detectare Automată**: Sistemul detectează când OEE < Target la sfârșitul intervalului
- **Justificări Inteligente**: Verifică dacă există justificări suficiente (downtime logged sau defecte alocate)
- **Prompt Operator**: Dacă nu există justificări, operatorul primește un prompt să explice motivul
- **Raportare**: Toate justificările sunt salvate și disponibile pentru analiză

### Alte Funcționalități
- **Autentificare și Autorizare**: Sistem RBAC (Role-Based Access Control)
- **Export Date**: Export Excel/CSV pentru rapoarte
- **Alerte și Notificări**: Sistem configurable de alerte
- **Istoric Complet**: Urmărire completă a tuturor evenimentelor
- **Mentenanță Preventivă**: Planificare și urmărire intervenții preventive
- **Gestionare Inventar**: Piese de schimb și materii prime

## 📋 Cerințe

- .NET 8.0 SDK
- PostgreSQL 12+
- Browser modern (Chrome, Firefox, Edge)

## 🔧 Instalare și Configurare

### 1. Clonare Repo
```bash
git clone https://github.com/Ristian171/MesEnterprise_v3.git
cd MesEnterprise_v3
```

### 2. Configurare Bază de Date
Editați `appsettings.json` cu conexiunea la PostgreSQL:
```json
{
  "ConnectionStrings": {
    "MesDatabase": "Host=localhost;Database=mes_enterprise;Username=your_user;Password=your_password"
  }
}
```

### 3. Rulare Migrații
```bash
dotnet ef database update
```

### 4. Pornire Aplicație
```bash
dotnet run
```

Aplicația va fi disponibilă la `http://localhost:5000`

## 📖 Utilizare

### Setup Inițial (Admin)

#### 1. Configurare Linie cu Live Scan
- Navigați la **Configurare → Linii**
- Creați sau editați o linie
- Bifați **"Activează Scanare Live"** (Has Live Scanning)
- Setați **Scan Identifier** (ex: SCAN-L1) - cod unic pentru stația de scan
- Setați **OEE Target** (ex: 85.0) - target de eficiență pentru linie

#### 2. Configurare Produse
- Navigați la **Configurare → Produse**
- Adăugați produse cu **Timp Ciclu** (secunde/piesă)

#### 3. Configurare Schimburi
- Navigați la **Configurare → Schimburi**
- Definiți schimburile cu ore de start/sfârșit

### Configurare Stație Scan (One-Time)

1. Accesați `/scan.html` pe dispozitivul dedicat scanării
2. La prima accesare, veți fi întrebați pentru **identificatorul stației**
3. Introduceți codul configurat de admin (ex: SCAN-L1)
4. Identificatorul se salvează și nu mai trebuie introdus

### Operare Zilnică (Operator)

#### Start Producție
1. Accesați pagina principală (`/index.html`)
2. Selectați **Linie** și **Produs**
3. Apăsați **"START PRODUCȚIE"**

#### Utilizare Live Scan
- **Indicator Scan**: Vizibil în pagina operator, arată starea stației de scan
- **Toggle Live Scan**: Buton pentru activare/dezactivare
- **Când Live Scan este activ**:
  - Fiecare scan de la stația configurată incrementează automat piesele bune
  - Câmpul "Piese Bune" devine read-only
  - Se afișează mesaj informativ: "Mod Live Scan activ – piesele bune sunt înregistrate automat din scan"

#### Logare Manuală
Chiar dacă Live Scan este activ, operatorul trebuie să logheze:
- **Piese Scrap**: Piese defecte irecuperabile
- **Piese NRFT**: Piese ce necesită reprelucrare
- **Downtime**: Minute de staționare cu motiv

#### Justificări OEE
Dacă OEE < Target și nu există justificări suficiente:
- Apare automat un **modal de justificare**
- Operatorul trebuie să explice de ce nu s-a atins targetul
- Justificarea se salvează permanent

## 🏗️ Arhitectură

### Backend
- **ASP.NET Core 8.0**: Minimal APIs
- **Entity Framework Core**: ORM cu PostgreSQL
- **PostgreSQL**: Bază de date relațională
- **Serilog**: Logging
- **JWT**: Autentificare
- **Background Services**: Procese automatizate (alerte, verificări, backup)

### Frontend
- **HTML5 + CSS3**: UI static responsive
- **Vanilla JavaScript**: Fără framework-uri externe
- **Fetch API**: Comunicare cu backend
- **LocalStorage**: Persistență date client-side

### Structură Proiect
```
MesEnterprise_v3/
├── MesEnterprise/Models/      # Modele de date (entități)
├── DTOs/                       # Data Transfer Objects
├── Endpoints/                  # API endpoints
├── Services/                   # Business logic și background services
├── Data/                       # DbContext și configurări EF Core
├── Migrations/                 # Migrații bază de date
├── wwwroot/                    # Fișiere statice (nu există în acest proiect)
├── js/                         # JavaScript pentru frontend
├── *.html                      # Pagini HTML
└── style.css                   # Stiluri globale
```

## 🔌 API Endpoints Principale

### Live Scan
- `POST /api/production/scan` - Înregistrare scan (body: {Identifier, ScanData})
- `PUT /api/line/{lineId}/live-scan` - Enable/disable Live Scan (body: {Enabled})
- `GET /api/line/status-by-identifier/{identifier}` - Status linie după identifier scan

### Operator
- `GET /api/operator/state` - Status complet linie (KPIs, istoric, Live Scan info)
- `POST /api/operator/command` - Comenzi operator (start, stop)
- `POST /api/productionlogs` - Logare producție
- `POST /api/operator/justify-oee` - Justificare OEE (body: {ProductionLogId, Reason})
- `GET /api/operator/pending-justifications` - Justificări pendinte pentru linie

### Configurare (Admin)
- `GET/POST/PUT/DELETE /api/config/lines` - Gestionare linii
- `GET/POST/PUT/DELETE /api/config/products` - Gestionare produse
- `GET/POST/PUT/DELETE /api/config/shifts` - Gestionare schimburi

## 📄 Documentație

### Flow Complet
Accesați `/flow.html` pentru documentație detaliată despre:
- Configurare inițială (Admin)
- Setup stație scan (Tehnician)
- Operare zilnică (Operator)
- Live Scan - funcționare și stări
- OEE Target și justificări
- Best practices

## 🔒 Securitate

- Autentificare JWT obligatorie pentru toate endpoint-urile (exceptând /login)
- Role-Based Access Control (RBAC): Operator, Technician, Admin
- Password hashing cu BCrypt
- Validare input pe server
- SQL injection prevention prin EF Core parametrizat
- XSS prevention prin escaping în frontend

## 🛠️ Dezvoltare

### Build
```bash
dotnet build
```

### Run Development
```bash
dotnet run --environment Development
```

### Creare Migrație
```bash
dotnet ef migrations add NouaMigratie
```

### Aplicare Migrație
```bash
dotnet ef database update
```

## 📊 Monitorizare și Log-uri

- **Serilog**: Logging în consolă și fișiere
- **PostgreSQL Sink**: Log-uri salvate în baza de date
- **Background Services**: Log-uri pentru procese automate
- Fișiere log în directorul `logs/`

## 🤝 Contribuții

Acest proiect este dezvoltat și menținut intern. Pentru întrebări sau sugestii, contactați echipa de dezvoltare.

## 📝 Licență

Proprietary - Toate drepturile rezervate

## 📞 Contact

- **Dezvoltator**: Ristian171
- **GitHub**: https://github.com/Ristian171/MesEnterprise_v3

---

**Versiune**: 3.0  
**Ultima actualizare**: Noiembrie 2024  
**Status**: Production Ready
