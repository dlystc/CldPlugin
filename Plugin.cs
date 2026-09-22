using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UMP.DlyStc.Plugin.Cld.Services;
using UMP.DlyStc.Plugin.Cld.Components;
using UMP.DlyStc.Plugin.Cld.SettingPages;
using UMP.DlyStc.Plugin.Cld.Shared;

namespace UMP.DlyStc.Plugin.Cld;

[PluginEntrance]
public class Plugin : PluginBase {
    public override void Initialize(HostBuilderContext context, IServiceCollection services) {
        // Initialize config folder
        GlobalConstants.PluginConfigFolder = PluginConfigFolder;
        
        // Register Stc providers (must be before config initialization)
        StcHandler.RegisterProvider(new DlystcStcProvider());
        StcHandler.RegisterProvider(new HitokotoStcProvider());
        StcHandler.RegisterProvider(new JinrishiciStcProvider());
        StcHandler.RegisterProvider(new SainticStcProvider());
        
        // Load plugin-level config and ensure provider entries exist
        GlobalConstants.PluginConfig = StcPluginConfig.Load();
        GlobalConstants.PluginConfig.EnsureProviderSettings(StcHandler.Providers);

        // Initialize Sentry for crash reporting (Release only, opt-in)
        if (GlobalConstants.PluginConfig.IsTelemetryActivated) {
#if !DEBUG
            SentrySdk.Init(o => {
                o.Dsn = "https://64f230f98e840b2f01b6505d2cfdf6bf@o4511653575983104.ingest.us.sentry.io/4512129885667328";
                o.Release = Info.Manifest.Version;
                o.AutoSessionTracking = true;
                o.TracesSampleRate = 0.01;
                o.ProfilesSampleRate = 0.01;
                // o.EnableLogs = true;
            });
#endif
        }
        
        // Register StcFetchService as a singleton so it can be resolved by type
        services.AddSingleton<StcFetchService>();
        // Register as hosted service - the generic host will call StartAsync/StopAsync
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<StcFetchService>());
        // Expose as IStcFetchService for components to inject
        services.AddSingleton<IStcFetchService>(sp => sp.GetRequiredService<StcFetchService>());
        
        // Register Stc component with its settings control
        services.AddComponent<Stc,StcSettings>();
        
        // Register settings page
        services.AddSettingsPageGroup("stc", "\uE34C", "每日多言")
            .AddSettingsPage<StcSettingsPage>();
    }
}
