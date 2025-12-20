namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Equalizer band with frequency and gain.
/// </summary>
public record EqualizerBand(double Frequency, double Gain);

/// <summary>
/// Equalizer preset.
/// </summary>
public record EqualizerPreset(
    Guid Id,
    string Name,
    EqualizerBand[] Bands,
    bool IsDefault);

/// <summary>
/// Request to create or update an equalizer preset.
/// </summary>
public record CreateEqualizerPresetRequest(
    string Name,
    EqualizerBand[] Bands,
    bool IsDefault);

/// <summary>
/// Paginated response for equalizer presets.
/// </summary>
public record EqualizerPresetsPagedResponse(EqualizerPreset[] Presets, PaginationMetadata Meta);
