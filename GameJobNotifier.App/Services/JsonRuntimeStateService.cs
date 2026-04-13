using System.Text.Json;
using System.IO;
using GameJobNotifier.App.Infrastructure;
using GameJobNotifier.App.Models;
using GameJobNotifier.App.Services.Interfaces;

namespace GameJobNotifier.App.Services;

public sealed class JsonRuntimeStateService : IRuntimeStateService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim _gate = new(1, 1);
    private RuntimeState? _cachedState;

    public async Task<RuntimeState> GetAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_cachedState is not null)
            {
                return _cachedState;
            }

            AppPaths.EnsureCreated();

            if (!File.Exists(AppPaths.RuntimeStateFile))
            {
                _cachedState = new RuntimeState();
                await WriteUnsafeAsync(_cachedState, cancellationToken);
                return _cachedState;
            }

            await using var stream = File.OpenRead(AppPaths.RuntimeStateFile);
            _cachedState = await JsonSerializer.DeserializeAsync<RuntimeState>(stream, SerializerOptions, cancellationToken)
                           ?? new RuntimeState();
            return _cachedState;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(RuntimeState state, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            AppPaths.EnsureCreated();
            await WriteUnsafeAsync(state, cancellationToken);
            _cachedState = state;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static async Task WriteUnsafeAsync(RuntimeState state, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(AppPaths.RuntimeStateFile);
        await JsonSerializer.SerializeAsync(stream, state, SerializerOptions, cancellationToken);
    }
}
