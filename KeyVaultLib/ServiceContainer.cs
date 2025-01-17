using Microsoft.Extensions.DependencyInjection;
using System;

namespace KeyVaultLib;

public static class ServiceContainer
{
    private static readonly Lazy<ServiceProvider> _provider = new(() =>
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ICacheAsideHelper, CacheAsideHelper>();
        // Add other services here
        return serviceCollection.BuildServiceProvider();
    });

    public static ServiceProvider Provider => _provider.Value;
}
