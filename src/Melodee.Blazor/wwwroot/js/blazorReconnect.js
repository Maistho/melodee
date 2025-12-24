// Blazor Server Enhanced Reconnection Handler
(function () {
    let reconnectModal = null;
    let reconnectAttempts = 0;
    const maxReconnectAttempts = 10;
    let reconnectInterval = null;

    function createReconnectModal() {
        if (reconnectModal) {
            return;
        }

        reconnectModal = document.createElement('div');
        reconnectModal.id = 'components-reconnect-modal';
        reconnectModal.innerHTML = `
            <div class="modal-content">
                <div class="spinner"></div>
                <h4>Connection Lost</h4>
                <p id="reconnect-message">Attempting to reconnect to the server...</p>
                <p id="reconnect-status">Attempt <span id="attempt-count">1</span> of ${maxReconnectAttempts}</p>
                <button id="manual-reload" onclick="window.location.reload()">Reload Page</button>
            </div>
        `;
        document.body.appendChild(reconnectModal);
    }

    function removeReconnectModal() {
        if (reconnectModal) {
            reconnectModal.remove();
            reconnectModal = null;
        }
        reconnectAttempts = 0;
        if (reconnectInterval) {
            clearInterval(reconnectInterval);
            reconnectInterval = null;
        }
    }

    function updateReconnectStatus(message, attemptNumber) {
        const messageEl = document.getElementById('reconnect-message');
        const statusEl = document.getElementById('reconnect-status');
        const attemptEl = document.getElementById('attempt-count');

        if (messageEl) messageEl.textContent = message;
        if (attemptEl) attemptEl.textContent = attemptNumber;
        if (statusEl && attemptNumber >= maxReconnectAttempts) {
            statusEl.textContent = 'Maximum reconnection attempts reached.';
        }
    }

    // Enhanced Blazor reconnection configuration
    Blazor.defaultReconnectionHandler._reconnectCallback = async function (reconnectionOptions) {
        reconnectAttempts++;
        
        if (reconnectAttempts === 1) {
            createReconnectModal();
        }

        updateReconnectStatus(
            'Attempting to reconnect to the server...',
            reconnectAttempts
        );

        if (reconnectAttempts > maxReconnectAttempts) {
            updateReconnectStatus(
                'Unable to reconnect. Please reload the page.',
                reconnectAttempts
            );
            return null; // Stop trying
        }

        // Exponential backoff with jitter
        const baseDelay = 1000;
        const maxDelay = 30000;
        const delay = Math.min(
            maxDelay,
            baseDelay * Math.pow(2, reconnectAttempts - 1) + Math.random() * 1000
        );

        console.log(`Reconnection attempt ${reconnectAttempts}, waiting ${Math.round(delay)}ms`);

        return new Promise((resolve) => {
            setTimeout(() => {
                resolve(true);
            }, delay);
        });
    };

    // Listen for Blazor reconnection events
    Blazor.addEventListener('reconnect', () => {
        console.log('Blazor reconnect event triggered');
    });

    Blazor.addEventListener('reconnecting', () => {
        console.log('Blazor reconnecting...');
    });

    Blazor.addEventListener('reconnected', () => {
        console.log('Blazor reconnected successfully');
        removeReconnectModal();
        
        // Show brief success message
        const successMsg = document.createElement('div');
        successMsg.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background-color: #4CAF50;
            color: white;
            padding: 1rem 2rem;
            border-radius: 4px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.2);
            z-index: 10001;
            animation: fadeIn 0.3s ease-in;
        `;
        successMsg.textContent = 'Connection restored';
        document.body.appendChild(successMsg);
        
        setTimeout(() => {
            successMsg.style.animation = 'fadeOut 0.3s ease-out';
            setTimeout(() => successMsg.remove(), 300);
        }, 3000);
    });

    Blazor.addEventListener('disconnected', () => {
        console.log('Blazor disconnected from server');
        createReconnectModal();
    });

    // Handle page visibility changes to pause/resume reconnection
    document.addEventListener('visibilitychange', () => {
        if (document.hidden) {
            console.log('Page hidden, pausing reconnection attempts');
        } else {
            console.log('Page visible, resuming reconnection attempts');
            if (reconnectModal && !Blazor._internal.navigationManager.isConnected) {
                // Try to reconnect immediately when page becomes visible
                reconnectAttempts = 0;
            }
        }
    });

    // Add offline/online event listeners
    window.addEventListener('offline', () => {
        console.log('Network offline detected');
        if (!reconnectModal) {
            createReconnectModal();
            updateReconnectStatus('No internet connection. Waiting for connection...', reconnectAttempts);
        }
    });

    window.addEventListener('online', () => {
        console.log('Network online detected');
        reconnectAttempts = 0; // Reset attempts when network comes back
        if (reconnectModal) {
            updateReconnectStatus('Connection restored. Attempting to reconnect...', 1);
        }
    });

    console.log('Enhanced Blazor reconnection handler initialized');
})();
