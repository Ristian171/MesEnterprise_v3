# Frontend Integration Guide - MES Enterprise

## Overview

This document provides guidance for frontend developers working with the MES Enterprise system. The frontend is built with vanilla JavaScript and HTML5, communicating with the backend via REST APIs.

## Project Structure

```
wwwroot/
├── css/
│   ├── branding.css          # Enterprise branding styles
│   └── (future styles)
├── js/
│   ├── auth.js               # Authentication utilities
│   ├── config.js             # Configuration management
│   ├── operator.js           # Operator dashboard logic  
│   ├── interventii.js        # Maintenance ticket management
│   ├── changeover.js         # Changeover interface
│   ├── edit.js               # Production log editing
│   ├── scan.js               # Scanner integration
│   ├── nav.js                # Navigation utilities
│   └── utils.js              # Common utilities
├── images/
│   └── (logo files)
├── login.html                # Authentication page
├── index.html                # Operator dashboard
├── config.html               # Configuration landing
├── config-*.html             # Configuration subpages
├── interventii.html          # Maintenance management
├── changeover.html           # Changeover interface
├── edit.html                 # Production log editing
├── scan.html                 # Scanner interface
├── help.html                 # Help documentation
└── style.css                 # Global styles
```

## API Integration

### Base URL
```javascript
const API_BASE_URL = window.location.origin; // Same origin
```

### Authentication

**Login:**
```javascript
POST /api/auth/login
Content-Type: application/json

{
  "username": "operator1",
  "password": "password123"
}

Response:
{
  "token": "eyJ...",
  "username": "operator1",
  "role": "Operator"
}
```

**Using Token:**
```javascript
fetch('/api/operator/state?lineId=1', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
})
```

**Token Storage:**
```javascript
// Store in sessionStorage (recommended)
sessionStorage.setItem('jwt_token', token);
sessionStorage.setItem('username', username);

// Retrieve
const token = sessionStorage.getItem('jwt_token');
```

### Core Endpoints

#### Operator Dashboard State
```javascript
GET /api/operator/state?lineId={lineId}
Authorization: Bearer {token}

Response:
{
  "line": { "id": 1, "name": "Line 1", ... },
  "currentProduct": { "id": 5, "name": "Product A", ... },
  "currentShift": { "id": 2, "name": "Shift 2", ... },
  "lineStatus": { "status": "Running", ... },
  "currentHour": "14:00",
  "availableTimeSlots": ["14:00", "15:00", ...],
  "currentTarget": 100,
  "currentGood": 85,
  "currentScrap": 5,
  "currentOee": 85.0,
  "needsJustification": false,
  "shiftLogs": [ ... ]
}
```

#### Production Log Submission
```javascript
POST /api/productionlogs
Authorization: Bearer {token}
Content-Type: application/json

{
  "lineId": 1,
  "productId": 5,
  "shiftId": 2,
  "logTime": "2025-11-17T14:00:00Z",
  "goodParts": 95,
  "scrapParts": 5,
  "nrftParts": 0,
  "downtimeMinutes": 0,
  "defecte": [
    { "defectCodeId": 3, "quantity": 3 },
    { "defectCodeId": 7, "quantity": 2 }
  ]
}
```

#### Operator Commands
```javascript
POST /api/operator/command
Authorization: Bearer {token}
Content-Type: application/json

{
  "lineId": 1,
  "command": "Start", // Start|Stop|Breakdown
  "productId": 5,
  "shiftId": 2
}
```

#### Maintenance Tickets
```javascript
GET /api/interventii?status=Open
Authorization: Bearer {token}

Response: [ ... tickets ... ]

POST /api/interventii
Authorization: Bearer {token}
Content-Type: application/json

{
  "lineId": 1,
  "problemaRaportataId": 2,
  "descriereProblema": "Machine stopped unexpectedly"
}
```

### Configuration Endpoints

#### Lines
```javascript
GET /api/config/lines
POST /api/config/lines
PUT /api/config/lines/{id}
DELETE /api/config/lines/{id}
```

#### Products
```javascript
GET /api/config/products
POST /api/config/products
PUT /api/config/products/{id}
DELETE /api/config/products/{id}
```

#### System Settings
```javascript
GET /api/config/settings
PUT /api/config/settings/{id}
```

### Enterprise Endpoints

#### Work Orders
```javascript
GET /api/planning/workorders?status=InProgress
GET /api/planning/workorders/active/{lineId}
POST /api/planning/workorders (Admin only)
```

#### Admin
```javascript
GET /api/admin/stats
POST /api/admin/optimize
POST /api/admin/backup
```

## Common Patterns

### API Call with Error Handling
```javascript
async function apiCall(endpoint, options = {}) {
    const token = sessionStorage.getItem('jwt_token');
    
    const defaultOptions = {
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    };
    
    try {
        const response = await fetch(endpoint, { ...defaultOptions, ...options });
        
        if (response.status === 401) {
            // Token expired, redirect to login
            window.location.href = '/login.html';
            return null;
        }
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'API call failed');
        }
        
        return await response.json();
    } catch (error) {
        console.error('API Error:', error);
        alert(`Error: ${error.message}`);
        return null;
    }
}
```

### Loading Dropdown from API
```javascript
async function loadProducts(selectElement) {
    const products = await apiCall('/api/config/products');
    if (products) {
        selectElement.innerHTML = '<option value="">Select Product</option>';
        products.forEach(p => {
            const option = document.createElement('option');
            option.value = p.id;
            option.textContent = p.name;
            selectElement.appendChild(option);
        });
    }
}
```

### Form Submission
```javascript
async function submitForm(formData) {
    const result = await apiCall('/api/productionlogs', {
        method: 'POST',
        body: JSON.stringify(formData)
    });
    
    if (result) {
        alert('Success!');
        loadData(); // Refresh page data
    }
}
```

## UI Components

### Loading Indicator
```javascript
function showLoading(element) {
    element.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Loading...';
}

function hideLoading(element) {
    element.innerHTML = '';
}
```

### Toast Notifications
```javascript
function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.textContent = message;
    document.body.appendChild(toast);
    
    setTimeout(() => {
        toast.classList.add('fade-out');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}
```

### Data Table
```javascript
function renderTable(data, columns, tableBody) {
    tableBody.innerHTML = '';
    
    data.forEach(row => {
        const tr = document.createElement('tr');
        columns.forEach(col => {
            const td = document.createElement('td');
            td.textContent = row[col.field];
            tr.appendChild(td);
        });
        tableBody.appendChild(tr);
    });
}
```

## Styling Guidelines

### Using Branding CSS
```html
<link rel="stylesheet" href="/css/branding.css">

<button class="mes-btn-primary">Primary Action</button>
<div class="mes-card">Card content</div>
<span class="mes-badge mes-badge-success">Active</span>
```

### Color Variables
```css
:root {
    --brand-primary: #2563eb;
    --brand-secondary: #1e40af;
    --brand-accent: #3b82f6;
    --brand-success: #10b981;
    --brand-warning: #f59e0b;
    --brand-danger: #ef4444;
}
```

## Real-Time Updates

### Polling Pattern
```javascript
let pollInterval;

function startPolling() {
    pollInterval = setInterval(async () => {
        const state = await apiCall('/api/operator/state?lineId=1');
        updateUI(state);
    }, 5000); // Every 5 seconds
}

function stopPolling() {
    if (pollInterval) {
        clearInterval(pollInterval);
    }
}

// Start on page load
document.addEventListener('DOMContentLoaded', startPolling);

// Stop on page unload
window.addEventListener('beforeunload', stopPolling);
```

## Module Toggle Integration

### Checking Module Availability
```javascript
async function isModuleEnabled(moduleName) {
    const settings = await apiCall('/api/config/settings');
    const moduleSetting = settings.find(s => s.key === `${moduleName}_Module_Enabled`);
    return moduleSetting?.value === 'true';
}

// Hide menu items for disabled modules
async function initializeMenu() {
    const planningEnabled = await isModuleEnabled('Planning');
    if (!planningEnabled) {
        document.getElementById('planning-menu').style.display = 'none';
    }
}
```

## Error Handling

### Common HTTP Status Codes
- **200 OK**: Success
- **201 Created**: Resource created
- **400 Bad Request**: Invalid input
- **401 Unauthorized**: Token missing or invalid
- **403 Forbidden**: Insufficient permissions or module disabled
- **404 Not Found**: Resource doesn't exist
- **500 Internal Server Error**: Server error

### Display User-Friendly Errors
```javascript
function handleApiError(error, response) {
    if (response.status === 403) {
        return 'This feature is not available. Contact your administrator.';
    }
    if (response.status === 400) {
        return 'Please check your input and try again.';
    }
    return 'An unexpected error occurred. Please try again later.';
}
```

## Testing

### Manual Testing Checklist
- [ ] Login with valid credentials
- [ ] Login with invalid credentials (should fail)
- [ ] Navigate to each page
- [ ] Submit forms with valid data
- [ ] Submit forms with invalid data (should show errors)
- [ ] Check dropdown population
- [ ] Test real-time updates
- [ ] Test on different screen sizes
- [ ] Test with different user roles
- [ ] Test with modules disabled

### Browser Compatibility
- Chrome 90+
- Firefox 88+
- Edge 90+
- Safari 14+

## Performance Tips

### Minimize API Calls
```javascript
// BAD: Multiple calls
const products = await apiCall('/api/config/products');
const lines = await apiCall('/api/config/lines');
const shifts = await apiCall('/api/config/shifts');

// GOOD: Single call or cache results
let cachedProducts = null;
async function getProducts() {
    if (!cachedProducts) {
        cachedProducts = await apiCall('/api/config/products');
    }
    return cachedProducts;
}
```

### Debounce Search Inputs
```javascript
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func(...args), wait);
    };
}

const searchInput = document.getElementById('search');
searchInput.addEventListener('input', debounce(async (e) => {
    const results = await apiCall(`/api/search?q=${e.target.value}`);
    displayResults(results);
}, 300));
```

## Future Enhancements

### Planned Features
- [ ] SignalR for real-time updates (replace polling)
- [ ] Progressive Web App (PWA) support
- [ ] Offline mode with sync
- [ ] Mobile-optimized layouts
- [ ] Dark mode toggle
- [ ] Multi-language support (i18n)
- [ ] Advanced charting (Chart.js integration)
- [ ] File upload for documents
- [ ] Voice commands for hands-free operation

## Troubleshooting

### Common Issues

**Q: API calls return 401 even with valid token**
A: Token may have expired. Clear sessionStorage and login again.

**Q: Dropdown doesn't populate**
A: Check browser console for API errors. Verify endpoint returns data.

**Q: Changes don't appear immediately**
A: Implement real-time polling or refresh the page.

**Q: Module-specific page shows 403**
A: Module is disabled in SystemSettings. Enable via Admin interface.

## Resources

- API Documentation: `/swagger` (Development mode)
- Technical Docs: `DOCUMENTATIE_TEHNICA.md`
- Database Manual: `MANUAL_BAZA_DE_DATE.md`
- Changelog: `CHANGELOG.md`

## Support

For frontend-specific issues or feature requests, contact the development team.

---

© 2025 M.E.S - Made by Joja Cristian
*Last Updated: 2025-11-17*
