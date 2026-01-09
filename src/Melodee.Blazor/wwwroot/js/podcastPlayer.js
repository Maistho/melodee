let audio = null;
let dotNetHelper = null;

export function initializeAudio(helper) {
    if (!helper) {
        console.error("Helper object is required");
        return false;
    }

    dotNetHelper = helper;

    try {
        if (!audio) {
            audio = new Audio();

            // Set up event listeners
            audio.addEventListener('timeupdate', handleTimeUpdate);
            audio.addEventListener('ended', handleEpisodeEnded);
            audio.addEventListener('error', handleAudioError);
            audio.addEventListener('loadedmetadata', handleMetadataLoaded);
        }
        return true;
    } catch (error) {
        console.error("Failed to initialize audio:", error);
        return false;
    }
}

function handleTimeUpdate() {
    if (dotNetHelper) {
        try {
            // Ensure values are valid numbers before sending to .NET
            const currentTime = isNaN(audio.currentTime) ? 0 : audio.currentTime;
            const duration = isNaN(audio.duration) ? 0 : audio.duration;

            dotNetHelper.invokeMethodAsync('OnTimeUpdate', currentTime, duration);
        } catch (error) {
            console.error("Error in timeupdate handler:", error);
        }
    }
}

function handleMetadataLoaded() {
    if (dotNetHelper) {
        try {
            const duration = isNaN(audio.duration) ? 0 : audio.duration;
            dotNetHelper.invokeMethodAsync('OnMetadataLoaded', duration);
        } catch (error) {
            console.error("Error in loadedmetadata handler:", error);
        }
    }
}

function handleEpisodeEnded() {
    if (dotNetHelper) {
        try {
            dotNetHelper.invokeMethodAsync('OnEpisodeEnded');
        } catch (error) {
            console.error("Error in ended handler:", error);
        }
    }
}

function handleAudioError(event) {
    if (dotNetHelper) {
        try {
            dotNetHelper.invokeMethodAsync('OnAudioError', audio.error?.code || -1);
        } catch (error) {
            console.error("Error in audio error handler:", error);
        }
    }
}

// Function to clean up resources when no longer needed
export function cleanupAudio() {
    if (audio) {
        audio.removeEventListener('timeupdate', handleTimeUpdate);
        audio.removeEventListener('ended', handleEpisodeEnded);
        audio.removeEventListener('error', handleAudioError);
        audio.removeEventListener('loadedmetadata', handleMetadataLoaded);
        audio.pause();
        audio = null;
    }
    dotNetHelper = null;
}

export function loadEpisode(src, startPosition = 0) {
    if (audio) {
        audio.src = src;
        audio.load();
        
        // Set start position once metadata is loaded
        if (startPosition > 0) {
            audio.addEventListener('loadedmetadata', function setStartPosition() {
                if (!isNaN(audio.duration) && startPosition < audio.duration) {
                    audio.currentTime = startPosition;
                }
                audio.removeEventListener('loadedmetadata', setStartPosition);
            }, { once: true });
        }
        
        return true;
    }
    return false;
}

export function playEpisode() {
    if (audio) {
        audio.play();
        return true;
    }
    return false;
}

export function pauseEpisode() {
    if (audio) {
        audio.pause();
        return true;
    }
    return false;
}

export function stopEpisode() {
    if (audio) {
        audio.pause();
        audio.currentTime = 0;
        return true;
    }
    return false;
}

export function seekTo(timeInSeconds) {
    if (audio && !isNaN(audio.duration) && timeInSeconds >= 0 && timeInSeconds <= audio.duration) {
        audio.currentTime = timeInSeconds;
        return true;
    }
    return false;
}

export function setVolume(volume) {
    if (audio) {
        audio.volume = Math.max(0, Math.min(1, volume));
        audio.muted = false;
        return true;
    }
    return false;
}

export function setMute(muted) {
    if (audio) {
        audio.muted = muted;
        return true;
    }
    return false;
}

export function getCurrentTime() {
    if (audio) {
        return isNaN(audio.currentTime) ? 0 : audio.currentTime;
    }
    return 0;
}

export function getDuration() {
    if (audio) {
        return isNaN(audio.duration) ? 0 : audio.duration;
    }
    return 0;
}

export function getIsPlaying() {
    if (audio) {
        return !audio.paused && !audio.ended && audio.currentTime > 0 && audio.readyState > 2;
    }
    return false;
}
