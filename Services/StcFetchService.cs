using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UMP.DlyStc.Plugin.Cld.Shared;

namespace UMP.DlyStc.Plugin.Cld.Services;

public interface IStcFetchService {
    void RegisterComponent(IStcDataReceiver component);
    void UnregisterComponent(IStcDataReceiver component);
}

public class StcFetchService : IHostedService, IStcFetchService {
    readonly ILogger<StcFetchService> _logger;
    readonly List<IStcDataReceiver> _components = [];
    int _currentIndex;
    Timer? _timer;

    public StcFetchService(ILogger<StcFetchService> logger) {
        _logger = logger;
        // Re-apply timer when config changes
        if (GlobalConstants.PluginConfig is { } config) {
            config.PropertyChanged += OnConfigPropertyChanged;
        }
    }

    void OnConfigPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(StcPluginConfig.FetchIntervalSeconds)) {
            lock (_components) {
                if (_timer != null) {
                    StartFetchTimer();
                }
            }
        }
    }

    public void RegisterComponent(IStcDataReceiver component) {
        lock (_components) {
            if (_components.Contains(component)) return;
            _components.Add(component);
            // Start timer if this is the first component
            if (_timer == null) {
                StartFetchTimer();
            }
        }
    }

    public void UnregisterComponent(IStcDataReceiver component) {
        lock (_components) {
            _components.Remove(component);
        }
    }

    void StartFetchTimer() {
        var config = GlobalConstants.PluginConfig;
        double interval = config?.FetchIntervalSeconds ?? 30.0;
        if (interval <= 0) return;
        _timer?.Dispose();
        _timer = new Timer(async _ => await FetchAndPushAsync(), null,
            TimeSpan.FromSeconds(interval), TimeSpan.FromSeconds(interval));
    }

    public async Task FetchAndPushAsync() {
        // Take a snapshot under lock to get the current component list and start index
        List<IStcDataReceiver> snapshot;
        int startIndex;
        lock (_components) {
            if (_components.Count == 0) return;
            snapshot = [.._components];
            startIndex = _currentIndex % snapshot.Count;
        }

        var config = GlobalConstants.PluginConfig;
        if (config == null) return;

        // Fetch one sentence
        StcData? data;
        try {
            data = await StcHandler.GetAsync(
                config.ProviderSettings,
                config.LengthLimitation);
        } catch (Exception ex) {
            // Upstream error → skip this cycle, don't update any component
            _logger.LogDebug(ex, "Stc fetch failed, skipping cycle");
            return;
        }

        // No valid data from any provider → skip (keep previous content on all components)
        if (data == null) {
            _logger.LogDebug("No valid data fetched, skipping cycle");
            return;
        }

        // Try each component in round-robin order
            for (int i = 0; i < snapshot.Count; i++) {
                int idx = (startIndex + i) % snapshot.Count;
                if (snapshot[idx].PushData(data)) {
                    // Component accepted → advance index for next cycle
                    lock (_components) {
                        if (_components.Count > 0) {
                            _currentIndex = (idx + 1) % _components.Count;
                        }
                    }
                    return;
                }
            }

        // All components rejected → discard silently
        _logger.LogDebug("All components rejected the fetched data");
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        // Timer starts when the first component registers
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        _timer?.Dispose();
        _timer = null;
        return Task.CompletedTask;
    }
}