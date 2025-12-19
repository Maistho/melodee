/**
 * Google Sign-In JavaScript interop for Melodee Blazor
 * Handles Google Identity Services (GIS) library integration
 */

// Google Sign-In configuration state
let googleInitialized = false;
let googleClientId = null;
let dotNetReference = null;

/**
 * Initialize Google Sign-In with the provided client ID
 * @param {string} clientId - Google OAuth client ID
 * @param {object} dotNetRef - Reference to the Blazor component
 */
window.initializeGoogleSignIn = function (clientId, dotNetRef) {
    if (!clientId) {
        console.warn('Google Sign-In: No client ID provided');
        return;
    }
    
    googleClientId = clientId;
    dotNetReference = dotNetRef;
    
    // Load the Google Identity Services library if not already loaded
    if (!window.google || !window.google.accounts) {
        const script = document.createElement('script');
        script.src = 'https://accounts.google.com/gsi/client';
        script.async = true;
        script.defer = true;
        script.onload = function () {
            initializeGoogleClient();
        };
        script.onerror = function () {
            console.error('Failed to load Google Sign-In script');
            if (dotNetReference) {
                dotNetReference.invokeMethodAsync('OnGoogleSignInError', 'Failed to load Google Sign-In');
            }
        };
        document.head.appendChild(script);
    } else {
        initializeGoogleClient();
    }
};

/**
 * Initialize the Google client after the library is loaded
 */
function initializeGoogleClient() {
    if (!googleClientId) {
        console.warn('Google Sign-In: No client ID configured');
        return;
    }
    
    try {
        google.accounts.id.initialize({
            client_id: googleClientId,
            callback: handleGoogleCredentialResponse,
            auto_select: false,
            cancel_on_tap_outside: true
        });
        googleInitialized = true;
    } catch (error) {
        console.error('Failed to initialize Google Sign-In:', error);
        if (dotNetReference) {
            dotNetReference.invokeMethodAsync('OnGoogleSignInError', 'Failed to initialize Google Sign-In');
        }
    }
}

/**
 * Handle the credential response from Google Sign-In
 * @param {object} response - The credential response containing the ID token
 */
function handleGoogleCredentialResponse(response) {
    if (response.credential && dotNetReference) {
        dotNetReference.invokeMethodAsync('OnGoogleSignInSuccess', response.credential);
    } else if (dotNetReference) {
        dotNetReference.invokeMethodAsync('OnGoogleSignInError', 'No credential received from Google');
    }
}

/**
 * Prompt the Google Sign-In popup
 */
window.promptGoogleSignIn = function () {
    if (!googleInitialized) {
        console.warn('Google Sign-In not initialized');
        if (dotNetReference) {
            dotNetReference.invokeMethodAsync('OnGoogleSignInError', 'Google Sign-In not initialized');
        }
        return;
    }
    
    try {
        // Use the popup flow
        google.accounts.id.prompt((notification) => {
            if (notification.isNotDisplayed()) {
                // Fall back to button click method if popup is blocked
                console.log('Google Sign-In popup was not displayed, reason:', notification.getNotDisplayedReason());
                // Try rendering a button as fallback
                renderGoogleButton('google-signin-button-fallback');
            } else if (notification.isSkippedMoment()) {
                console.log('Google Sign-In was skipped, reason:', notification.getSkippedReason());
            } else if (notification.isDismissedMoment()) {
                console.log('Google Sign-In was dismissed, reason:', notification.getDismissedReason());
                if (notification.getDismissedReason() === 'credential_returned') {
                    // Success - credential was returned
                } else if (dotNetReference) {
                    dotNetReference.invokeMethodAsync('OnGoogleSignInCancelled');
                }
            }
        });
    } catch (error) {
        console.error('Error prompting Google Sign-In:', error);
        if (dotNetReference) {
            dotNetReference.invokeMethodAsync('OnGoogleSignInError', 'Failed to prompt Google Sign-In');
        }
    }
};

/**
 * Render a Google Sign-In button in the specified container
 * @param {string} containerId - The ID of the container element
 */
window.renderGoogleButton = function (containerId) {
    if (!googleInitialized) {
        console.warn('Google Sign-In not initialized');
        return;
    }
    
    const container = document.getElementById(containerId);
    if (container) {
        google.accounts.id.renderButton(container, {
            theme: 'outline',
            size: 'large',
            type: 'standard',
            text: 'continue_with',
            shape: 'rectangular',
            logo_alignment: 'left',
            width: container.offsetWidth || 300
        });
    }
};

/**
 * Check if Google Sign-In is available/initialized
 * @returns {boolean} True if Google Sign-In is ready
 */
window.isGoogleSignInReady = function () {
    return googleInitialized;
};

/**
 * Sign out from Google (revoke the token)
 */
window.googleSignOut = function () {
    if (window.google && window.google.accounts && window.google.accounts.id) {
        google.accounts.id.disableAutoSelect();
    }
};

/**
 * Cleanup Google Sign-In resources
 */
window.cleanupGoogleSignIn = function () {
    dotNetReference = null;
    // Don't reset googleInitialized as the script is still loaded
};
