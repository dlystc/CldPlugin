using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UMP.DlyStc.Plugin.Cld.Shared;

namespace UMP.DlyStc.Plugin.Cld.Shared;

public static class StcHandler {
    static readonly Lock ProvidersLock = new Lock();
    static readonly Dictionary<string,IStcProvider> RegisteredProviders = new Dictionary<string,IStcProvider>(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<IStcProvider> Providers {
        get {
            lock (ProvidersLock) {
                return RegisteredProviders.Values.ToArray();
            }
        }
    }

    public static bool RegisterProvider(IStcProvider provider) {
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(provider.Id)) {
            throw new ArgumentException("名句来源必须提供非空 Id。",nameof(provider));
        }
        if (provider.DefaultWeight < 0) {
            throw new ArgumentException("名句来源的默认权重不能小于 0。",nameof(provider));
        }

        lock (ProvidersLock) {
            return RegisteredProviders.TryAdd(provider.Id,provider);
        }
    }

    public static bool UnregisterProvider(string providerId) {
        lock (ProvidersLock) {
            return RegisteredProviders.Remove(providerId);
        }
    }

    public static async Task<StcData?> GetAsync(
            IReadOnlyDictionary<string,StcProviderConfig> providerConfigs,
            int lengthLimitation = 0,
            CancellationToken cancellationToken = default) {
            List<(IStcProvider Provider,StcProviderConfig Config)> candidates = Providers
                .Select(provider => (
                    Provider: provider,
                    Config: providerConfigs.TryGetValue(provider.Id,out StcProviderConfig? config)
                        ? config
                        : new StcProviderConfig {
                            IsEnabled = provider.IsEnabledByDefault,
                            Weight = provider.DefaultWeight
                        }))
                .Where(item => item.Config.IsEnabled && item.Config.Weight > 0)
                .ToList();

            if (candidates.Count == 0) {
                return new StcData { Content = "未启用可用的名句来源" };
            }

            for (int i = 0; i < 6; i++) {
                (IStcProvider provider,StcProviderConfig config) = SelectProvider(candidates);
                try {
                    StcData dataFetched = await provider.FetchAsync(
                        config,
                        lengthLimitation,
                        cancellationToken);
                    if (lengthLimitation == 0 || dataFetched.Content.Length <= lengthLimitation) {
                        return dataFetched;
                    }
                } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                    throw;
                } catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine($"Error fetching from {provider.Id}: {ex.Message}");
                }
            }

            // All providers exhausted — signal caller to skip this cycle
            return null;
        }

        static (IStcProvider Provider,StcProviderConfig Config) SelectProvider(
            IReadOnlyList<(IStcProvider Provider,StcProviderConfig Config)> candidates) {
            long totalWeight = candidates.Sum(item => (long)item.Config.Weight);
            long selectedWeight = Random.Shared.NextInt64(totalWeight);
            foreach ((IStcProvider provider,StcProviderConfig config) in candidates) {
                if (selectedWeight < config.Weight) {
                    return (provider,config);
                }
                selectedWeight -= config.Weight;
            }
            return candidates[^1];
        }
}

public class StcData {
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Catalog { get; set; } = string.Empty;
}