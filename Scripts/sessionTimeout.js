/**
 * Session timeout handler for the Online Pastry Shop
 * This script manages the session timeout warning and automatic logout
 */

// Default session timeout configuration (will be overridden by the values in Site.Master)
let sessionTimeoutConfig = window.sessionTimeoutConfig || {
    warningTime: 60, // Time in seconds to show warning before timeout
    redirectUrl: '/Pages/Login.aspx?timeout=true', // URL to redirect to after timeout
    checkInterval: 30, // Check interval in seconds
    timer: null, // Timer reference
    modalVisible: false, // Flag to track if the warning modal is visible
    countdownTimer: null // Reference to the countdown timer
};

// Initialize the session timeout handler
function initSessionTimeout() {
    console.log('Initializing session timeout handler');

    // Check if we need to start the timer
    if (typeof isAuthenticated !== 'undefined' && isAuthenticated) {
        startSessionTimer();
    }
}

// Start the session timer
function startSessionTimer() {
    // Clear any existing timer
    if (sessionTimeoutConfig.timer) {
        clearInterval(sessionTimeoutConfig.timer);
    }

    // Set up the interval to check remaining time
    sessionTimeoutConfig.timer = setInterval(checkSessionTimeout, sessionTimeoutConfig.checkInterval * 1000);
    console.log('Session timer started');
}

// Check the session timeout
function checkSessionTimeout() {
    // Make an AJAX call to get the remaining session time
    fetch('/Pages/GetSessionTimeout.aspx')
        .then(response => response.json())
        .then(data => {
            // Check if we need to show the warning
            if (data.secondsRemaining <= sessionTimeoutConfig.warningTime && !sessionTimeoutConfig.modalVisible) {
                showTimeoutWarning(data.secondsRemaining);
            } else if (data.secondsRemaining <= 0) {
                // Session has timed out, redirect to login page
                window.location.href = sessionTimeoutConfig.redirectUrl;
            }
        })
        .catch(error => {
            console.error('Session timeout check failed:', error);
        });
}

// Show the timeout warning
function showTimeoutWarning(secondsRemaining) {
    console.log('Showing session timeout warning');
    sessionTimeoutConfig.modalVisible = true;

    // Get the existing modal
    let modal = document.getElementById('session-timeout-modal');
    if (modal) {
        // Update the countdown
        const countdownElement = document.getElementById('timeout-countdown');
        if (countdownElement) {
            countdownElement.textContent = secondsRemaining;
        }

        // Add event listeners if they don't exist
        const extendButton = document.getElementById('extend-session-btn');
        if (extendButton && !extendButton.hasEventListener) {
            extendButton.addEventListener('click', extendSession);
            extendButton.hasEventListener = true;
        }

        const logoutButton = document.getElementById('logout-now-btn');
        if (logoutButton && !logoutButton.hasEventListener) {
            logoutButton.addEventListener('click', logoutNow);
            logoutButton.hasEventListener = true;
        }

        // Show the modal
        modal.classList.remove('hidden');

        // Start the countdown
        startCountdown(secondsRemaining);
    } else {
        console.error('Session timeout modal not found in the DOM');
    }
}

// Start the countdown timer
function startCountdown(seconds) {
    const countdownElement = document.getElementById('timeout-countdown');
    if (!countdownElement) return;

    let remainingSeconds = seconds;
    const countdownTimer = setInterval(() => {
        remainingSeconds--;
        countdownElement.textContent = remainingSeconds;

        if (remainingSeconds <= 0) {
            clearInterval(countdownTimer);
            window.location.href = sessionTimeoutConfig.redirectUrl;
        }
    }, 1000);

    // Store the timer reference
    sessionTimeoutConfig.countdownTimer = countdownTimer;
}

// Extend the session
function extendSession() {
    // Make an AJAX call to extend the session
    fetch('/Pages/ExtendSession.aspx')
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                hideTimeoutWarning();
                // Show success toast if the showToast function exists
                if (typeof window.showToast === 'function') {
                    window.showToast('Your session has been extended.', 'success');
                }
                // Restart the session timer
                startSessionTimer();
            } else {
                // Something went wrong, redirect to login
                window.location.href = sessionTimeoutConfig.redirectUrl;
            }
        })
        .catch(error => {
            console.error('Failed to extend session:', error);
            // On error, redirect to login
            window.location.href = sessionTimeoutConfig.redirectUrl;
        });
}

// Log out immediately
function logoutNow() {
    window.location.href = '/Pages/Login.aspx?logout=1';
}

// Hide the timeout warning
function hideTimeoutWarning() {
    sessionTimeoutConfig.modalVisible = false;
    const modal = document.getElementById('session-timeout-modal');
    if (modal) {
        // Clear the countdown timer
        clearInterval(sessionTimeoutConfig.countdownTimer);
        // Hide the modal
        modal.classList.add('hidden');
    }
}

// Initialize on document ready
document.addEventListener('DOMContentLoaded', initSessionTimeout); 