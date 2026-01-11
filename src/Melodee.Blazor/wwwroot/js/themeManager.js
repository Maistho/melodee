/**
 * Melodee Theme Management
 * JavaScript interop for dynamic theme loading and application
 */

window.melodeeTheme = {
    currentThemeLink: null,

    /**
     * Load a theme CSS file dynamically
     * @param {string} themeCssPath - Path to the theme CSS file
     */
    loadTheme: function (themeCssPath) {
        // Remove existing theme link if present
        if (this.currentThemeLink) {
            this.currentThemeLink.remove();
        }

        // Create new link element for theme CSS
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = themeCssPath + '?v=' + Date.now(); // Cache busting
        link.id = 'melodee-theme-css';

        // Insert before other stylesheets to allow overrides
        const firstLink = document.querySelector('link[rel="stylesheet"]');
        if (firstLink) {
            firstLink.parentNode.insertBefore(link, firstLink);
        } else {
            document.head.appendChild(link);
        }

        this.currentThemeLink = link;

        // Wait for load to prevent FOUC
        return new Promise((resolve, reject) => {
            link.onload = () => resolve();
            link.onerror = () => reject(new Error('Failed to load theme CSS'));
        });
    },

    /**
     * Set font family CSS variable
     * @param {string} type - Type of font (base, heading, mono)
     * @param {string} fontFamily - CSS font-family value
     */
    setFontFamily: function (type, fontFamily) {
        const varName = `--md-font-family-${type}`;
        document.documentElement.style.setProperty(varName, fontFamily);
    },

    /**
     * Get computed CSS variable value
     * @param {string} varName - CSS variable name (e.g., '--md-primary')
     * @returns {string} The computed value
     */
    getCssVariable: function (varName) {
        return getComputedStyle(document.documentElement).getPropertyValue(varName).trim();
    },

    /**
     * Set CSS variable value
     * @param {string} varName - CSS variable name
     * @param {string} value - Value to set
     */
    setCssVariable: function (varName, value) {
        document.documentElement.style.setProperty(varName, value);
    },

    /**
     * Hide NavMenu items by their IDs
     * @param {string[]} itemIds - Array of NavMenu item IDs to hide
     */
    hideNavMenuItems: function (itemIds) {
        if (!itemIds || !Array.isArray(itemIds)) return;

        itemIds.forEach(id => {
            const element = document.querySelector(`[data-nav-item-id="${id}"]`);
            if (element) {
                element.style.display = 'none';
                element.setAttribute('aria-hidden', 'true');
            }
        });
    },

    /**
     * Show all NavMenu items (reset visibility)
     */
    showAllNavMenuItems: function () {
        const navItems = document.querySelectorAll('[data-nav-item-id]');
        navItems.forEach(element => {
            element.style.display = '';
            element.removeAttribute('aria-hidden');
        });
    },

    /**
     * Apply theme metadata to document (title, favicon, etc.)
     * @param {object} branding - Theme branding metadata
     */
    applyBranding: function (branding) {
        if (!branding) return;

        // Update favicon if specified
        if (branding.favicon) {
            let faviconLink = document.querySelector("link[rel~='icon']");
            if (!faviconLink) {
                faviconLink = document.createElement('link');
                faviconLink.rel = 'icon';
                document.head.appendChild(faviconLink);
            }
            faviconLink.href = branding.favicon;
        }
    },

    /**
     * Reset branding to original values
     */
    resetBranding: function () {
        // Reserved for future use (e.g. resetting favicon)
    }
}
    ;
