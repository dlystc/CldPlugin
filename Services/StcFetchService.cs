using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UMP.DlyStc.Plugin.Cld.Shared;

namespace UMP.DlyStc.Plugin.Cld.Services;

public interface IStcFetchService
{
    void RegisterComponent(IStcDataReceiver component);
    void UnregisterComponent(IStcDataReceiver component);
}

public class StcFetchService : IHostedService, IStcFetchService
{
    readonly ILogger<StcFetchService> _logger;
    readonly List<IStcDataReceiver> _components = [];
    int _currentIndex;
    Timer? _timer;

    public StcFetchService(ILogger<StcFetchService> logger)
    {
        _logger = logger;
        
        if (GlobalConstants.PluginConfig is { } config)
        {
            config.PropertyChanged += OnConfigPropertyChanged;
        }
    }

    void OnConfigPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(StcPluginConfig.FetchIntervalSeconds))
        {
            lock (_components)
            {
                if (_timer != null)
                {
                    StartFetchTimer();
                }
            }
        }
    }

    public void RegisterComponent(IStcDataReceiver component)
    {
        lock (_components)
        {
            if (_components.Contains(component)) return;
            _components.Add(component);

            if (_timer == null)
            {
                StartFetchTimer();
            }
        }
    }

    public void UnregisterComponent(IStcDataReceiver component)
    {
        lock (_components)
        {
            _components.Remove(component);
        }
    }

    void StartFetchTimer()
    {
        var config = GlobalConstants.PluginConfig;

        double interval = config?.FetchIntervalSeconds ?? 30.0;
        if (interval <= 0) return;

        _timer?.Dispose();
        _timer = new Timer(async _ => await FetchAndPushAsync(), null,
            TimeSpan.FromSeconds(interval), TimeSpan.FromSeconds(interval));
    }

    public async Task FetchAndPushAsync()
    {
        List<IStcDataReceiver> snapshot;
        int startIndex;

        lock (_components)
        {
            if (_components.Count == 0) return;

            snapshot = [.. _components];

            startIndex = _currentIndex % snapshot.Count;
        }

        var config = GlobalConstants.PluginConfig;
        if (config == null) return;

        try
        {
            var data = await StcHandler.GetAsync(config.ProviderSettings, config.LengthLimitation);

            if (data != null && (config.LengthLimitation == 0 || data.Content.Length <= config.LengthLimitation))
            {
                for (int i = 0; i < snapshot.Count; i++)
                {
                    int idx = (startIndex + i) % snapshot.Count;

                    if (snapshot[idx].PushData(data))
                    {
                        AdvanceIndex(idx + 1);

                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Stc fetch failed");

            return;
        }
    }

    void AdvanceIndex(int? next = null)
    {
        lock (_components)
        {
            if (_components.Count == 0) return;

            _currentIndex = (next ?? (_currentIndex + 1)) % _components.Count;
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        _timer = null;

        return Task.CompletedTask;
    }
}
