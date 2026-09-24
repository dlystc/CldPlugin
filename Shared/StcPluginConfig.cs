using System.ComponentModel;
using ClassIsland.Shared.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UMP.DlyStc.Plugin.Cld.Shared;

public class StcPluginConfig : ObservableObject {
    public StcPluginConfig() {
        PropertyChanged += OnPropertyChangedSave;
    }

    // --- Persistence ---

    static string ConfigPath => System.IO.Path.Combine(
        GlobalConstants.PluginConfigFolder!, "StcPlugin.json");

    public static StcPluginConfig Load() {
        if (!File.Exists(ConfigPath)) {
            var config = new StcPluginConfig();
            config.ForceSave();
            return config;
        }

        try {
            return ConfigureFileHelper.LoadConfig<StcPluginConfig>(ConfigPath);
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Failed to load StcPluginConfig: {ex.Message}");
            var config = new StcPluginConfig();
            config.ForceSave();
            return config;
        }
    }

    void OnPropertyChangedSave(object? sender, PropertyChangedEventArgs e) {
        Save();
    }

    public void ForceSave() {
        try {
            ConfigureFileHelper.SaveConfig(ConfigPath, this);
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Failed to force save StcPluginConfig: {ex.Message}");
        }
    }

    void Save() {
        try {
            ConfigureFileHelper.SaveConfig(ConfigPath, this);
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Failed to save StcPluginConfig: {ex.Message}");
        }
    }

    // --- Fetch settings ---

    bool _isTelemetryActivated = true;
    public bool IsTelemetryActivated {
        get => _isTelemetryActivated;
        set => SetProperty(ref _isTelemetryActivated, value);
    }

    double _fetchIntervalSeconds = 30.0;
    public double FetchIntervalSeconds {
        get => _fetchIntervalSeconds;
        set => SetProperty(ref _fetchIntervalSeconds, Math.Max(1, value));
    }

    int _lengthLimitation;
    public int LengthLimitation {
        get => _lengthLimitation;
        set => SetProperty(ref _lengthLimitation, Math.Max(0, value));
    }

    Dictionary<string, StcProviderConfig> _providerSettings =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, StcProviderConfig> ProviderSettings {
        get => _providerSettings;
        set => SetProperty(
            ref _providerSettings,
            value == null
                ? new Dictionary<string, StcProviderConfig>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, StcProviderConfig>(value, StringComparer.OrdinalIgnoreCase));
    }

    public void EnsureProviderSettings(IEnumerable<IStcProvider> providers) {
        bool changed = false;
        foreach (var provider in providers) {
            if (_providerSettings.ContainsKey(provider.Id)) continue;
            _providerSettings[provider.Id] = new StcProviderConfig {
                IsEnabled = provider.IsEnabledByDefault,
                Weight = provider.DefaultWeight
            };
            changed = true;
        }

        if (changed) {
            OnPropertyChanged(nameof(ProviderSettings));
        }
    }
}