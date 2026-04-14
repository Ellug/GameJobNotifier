using System.Text.Json;
using System.IO;
using GameJobNotifier.App.Infrastructure;
using GameJobNotifier.App.Models;
using GameJobNotifier.App.Services.Interfaces;

namespace GameJobNotifier.App.Services;

public sealed class JsonSettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim _gate = new(1, 1);
    private AppSettings? _cachedSettings;

    public async Task<AppSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_cachedSettings is not null)
            {
                return _cachedSettings;
            }

            AppPaths.EnsureCreated();

            if (!File.Exists(AppPaths.SettingsFile))
            {
                _cachedSettings = new AppSettings().Sanitize();
                await WriteUnsafeAsync(_cachedSettings, cancellationToken);
                return _cachedSettings;
            }

            await using var stream = File.OpenRead(AppPaths.SettingsFile);
            var loaded = await JsonSerializer.DeserializeAsync<AppSettings>(stream, SerializerOptions, cancellationToken);
            _cachedSettings = (loaded ?? new AppSettings()).Sanitize();

            return _cachedSettings;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        var sanitized = settings.Sanitize();

        await _gate.WaitAsync(cancellationToken);
        try
        {
            AppPaths.EnsureCreated();
            await WriteUnsafeAsync(sanitized, cancellationToken);
            _cachedSettings = sanitized;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static async Task WriteUnsafeAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(AppPaths.SettingsFile);
        await JsonSerializer.SerializeAsync(stream, settings, SerializerOptions, cancellationToken);
    }
}
