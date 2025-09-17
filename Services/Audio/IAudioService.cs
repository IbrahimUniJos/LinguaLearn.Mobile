using LinguaLearn.Mobile.Models.Common;

namespace LinguaLearn.Mobile.Services.Audio;

/// <summary>
/// Service for audio playback and recording functionality
/// </summary>
public interface IAudioService
{
    // Audio Playback
    Task<ServiceResult<bool>> PlayAudioAsync(string audioUrl, CancellationToken ct = default);
    Task<ServiceResult<bool>> PlayLocalAudioAsync(string filePath, CancellationToken ct = default);
    Task<ServiceResult<bool>> StopAudioAsync(CancellationToken ct = default);
    Task<ServiceResult<bool>> PauseAudioAsync(CancellationToken ct = default);
    Task<ServiceResult<bool>> ResumeAudioAsync(CancellationToken ct = default);
    
    // Audio Recording
    Task<ServiceResult<bool>> StartRecordingAsync(CancellationToken ct = default);
    Task<ServiceResult<string>> StopRecordingAsync(CancellationToken ct = default); // Returns file path
    Task<ServiceResult<bool>> CancelRecordingAsync(CancellationToken ct = default);
    
    // Audio Properties
    Task<ServiceResult<TimeSpan>> GetAudioDurationAsync(string audioUrl, CancellationToken ct = default);
    Task<ServiceResult<TimeSpan>> GetCurrentPositionAsync(CancellationToken ct = default);
    Task<ServiceResult<bool>> SetPositionAsync(TimeSpan position, CancellationToken ct = default);
    
    // Audio State
    bool IsPlaying { get; }
    bool IsRecording { get; }
    bool IsPaused { get; }
    
    // Events
    event EventHandler<AudioPlaybackEventArgs>? PlaybackStateChanged;
    event EventHandler<AudioRecordingEventArgs>? RecordingStateChanged;
    event EventHandler<AudioPositionEventArgs>? PositionChanged;
    
    // Audio Processing
    Task<ServiceResult<byte[]>> ConvertAudioToWavAsync(string inputPath, CancellationToken ct = default);
    Task<ServiceResult<string>> SaveAudioToFileAsync(byte[] audioData, string fileName, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeleteAudioFileAsync(string filePath, CancellationToken ct = default);
    
    // Volume Control
    Task<ServiceResult<bool>> SetVolumeAsync(float volume, CancellationToken ct = default); // 0.0 to 1.0
    Task<ServiceResult<float>> GetVolumeAsync(CancellationToken ct = default);
}

/// <summary>
/// Audio playback state enumeration
/// </summary>
public enum AudioPlaybackState
{
    Stopped,
    Playing,
    Paused,
    Loading,
    Error
}

/// <summary>
/// Audio recording state enumeration
/// </summary>
public enum AudioRecordingState
{
    Stopped,
    Recording,
    Paused,
    Error
}

/// <summary>
/// Event args for audio playback state changes
/// </summary>
public class AudioPlaybackEventArgs : EventArgs
{
    public AudioPlaybackState State { get; set; }
    public string? AudioUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Event args for audio recording state changes
/// </summary>
public class AudioRecordingEventArgs : EventArgs
{
    public AudioRecordingState State { get; set; }
    public string? FilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Event args for audio position changes
/// </summary>
public class AudioPositionEventArgs : EventArgs
{
    public TimeSpan Position { get; set; }
    public TimeSpan Duration { get; set; }
    public double Progress => Duration.TotalSeconds > 0 ? Position.TotalSeconds / Duration.TotalSeconds : 0;
}