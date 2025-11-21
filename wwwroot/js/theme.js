/**
 * MES Enterprise - Theme Management
 * Handles dark/light theme switching with localStorage persistence
 */

(function() {
    'use strict';

    const THEME_STORAGE_KEY = 'mes-theme-preference';
    const THEME_DARK = 'dark';
    const THEME_LIGHT = 'light';

    /**
     * Get the current theme from localStorage or system preference
     */
    function getPreferredTheme() {
        const stored = localStorage.getItem(THEME_STORAGE_KEY);
        if (stored) {
            return stored;
        }

        // Check system preference
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return THEME_DARK;
        }

        return THEME_LIGHT;
    }

    /**
     * Apply theme to the document
     */
    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        
        // Update toggle checkbox state if it exists
        const toggleCheckbox = document.getElementById('theme-toggle-checkbox');
        if (toggleCheckbox) {
            toggleCheckbox.checked = (theme === THEME_DARK);
        }

        // Emit custom event for other scripts that might need to react
        window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme } }));
    }

    /**
     * Toggle between dark and light themes
     */
    function toggleTheme() {
        const currentTheme = document.documentElement.getAttribute('data-theme') || THEME_LIGHT;
        const newTheme = currentTheme === THEME_DARK ? THEME_LIGHT : THEME_DARK;
        
        localStorage.setItem(THEME_STORAGE_KEY, newTheme);
        applyTheme(newTheme);
        
        return newTheme;
    }

    /**
     * Initialize theme on page load
     */
    function initTheme() {
        const preferredTheme = getPreferredTheme();
        applyTheme(preferredTheme);
    }

    /**
     * Create and inject theme toggle UI into header
     */
    function createThemeToggle() {
        const header = document.querySelector('.app-header') || document.querySelector('header');
        if (!header) {
            console.warn('Theme toggle: Header element not found');
            return;
        }

        // Check if toggle already exists
        if (document.getElementById('theme-toggle-checkbox')) {
            return;
        }

        const toggleHTML = `
            <label class="theme-toggle" title="Comutare temă Dark/Light">
                <span class="theme-icon">
                    <i class="fa-solid fa-sun"></i>
                </span>
                <input type="checkbox" id="theme-toggle-checkbox" aria-label="Comutare temă">
                <span class="theme-toggle-slider"></span>
                <span class="theme-icon">
                    <i class="fa-solid fa-moon"></i>
                </span>
            </label>
        `;

        // Try to find a nav or utility section in header
        let targetContainer = header.querySelector('nav') || header.querySelector('.header-controls');
        if (!targetContainer) {
            // Create a controls container if it doesn't exist
            targetContainer = document.createElement('div');
            targetContainer.className = 'header-controls';
            targetContainer.style.cssText = 'display: flex; align-items: center; gap: 16px; margin-left: auto;';
            header.appendChild(targetContainer);
        }

        // Insert the toggle
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = toggleHTML;
        const toggleElement = tempDiv.firstElementChild;
        targetContainer.appendChild(toggleElement);

        // Attach event listener
        const checkbox = document.getElementById('theme-toggle-checkbox');
        if (checkbox) {
            checkbox.addEventListener('change', function() {
                toggleTheme();
            });
        }
    }

    /**
     * Listen for system theme changes
     */
    function watchSystemTheme() {
        if (window.matchMedia) {
            const darkModeQuery = window.matchMedia('(prefers-color-scheme: dark)');
            
            // Only auto-switch if user hasn't set a preference
            darkModeQuery.addEventListener('change', (e) => {
                if (!localStorage.getItem(THEME_STORAGE_KEY)) {
                    applyTheme(e.matches ? THEME_DARK : THEME_LIGHT);
                }
            });
        }
    }

    // Public API
    window.MESTheme = {
        toggle: toggleTheme,
        apply: applyTheme,
        getCurrent: function() {
            return document.documentElement.getAttribute('data-theme') || THEME_LIGHT;
        },
        init: initTheme
    };

    // Auto-initialize theme as early as possible
    initTheme();

    // Initialize theme toggle when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            createThemeToggle();
            watchSystemTheme();
        });
    } else {
        // DOM already loaded
        createThemeToggle();
        watchSystemTheme();
    }

    // Log theme initialization for debugging
    console.log('MES Theme System initialized:', window.MESTheme.getCurrent());
})();
