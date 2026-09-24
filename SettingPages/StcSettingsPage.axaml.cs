using System.Collections.ObjectModel;
using Avalonia.Controls;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using UMP.DlyStc.Plugin.Cld.Shared;

namespace UMP.DlyStc.Plugin.Cld.SettingPages;

[SettingsPageInfo("stc.master", "每日多言 设置", "\uE34C", "\uE34D")]
public partial class StcSettingsPage : SettingsPageBase
{
    public StcSettingsPage()
    {
        Config = GlobalConstants.PluginConfig!;

        RefreshProviderItems();

        InitializeComponent();
    }

    public StcPluginConfig Config { get; }

    public ObservableCollection<StcProviderSettingsItem> ProviderItems { get; } = [];

    void RefreshProviderItems()
    {
        Config.EnsureProviderSettings(StcHandler.Providers);

        ProviderItems.Clear();
        
        foreach (IStcProvider provider in StcHandler.Providers)
        {
            ProviderItems.Add(new StcProviderSettingsItem(
                provider,
                Config.ProviderSettings[provider.Id]));
        }
    }
}

public sealed class StcProviderSettingsItem(IStcProvider provider, StcProviderConfig configuration)
{
    public string Id { get; } = provider.Id;
    public string DisplayName { get; } = provider.DisplayName;
    public string Description { get; } = provider.Description;
    public StcProviderConfig Configuration { get; } = configuration;
    public Control? SettingsControl { get; } = (provider as IStcProviderSettingsFactory)?.CreateSettingsControl(configuration);
    public bool HasSettings { get => SettingsControl != null; }
}