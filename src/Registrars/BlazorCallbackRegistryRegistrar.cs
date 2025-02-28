using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blazor.CallbackRegistry.Abstract;
using Soenneker.Blazor.Utils.ModuleImport.Registrars;
using Soenneker.Blazor.Utils.ResourceLoader.Registrars;

namespace Soenneker.Blazor.CallbackRegistry.Registrars;

/// <summary>
/// A generic registry to register and invoke instance-specific Blazor JS callbacks
/// </summary>
public static class BlazorCallbackRegistryRegistrar
{
    /// <summary>
    /// Adds <see cref="IBlazorCallbackRegistry"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddBlazorCallbackRegistryAsSingleton(this IServiceCollection services)
    {
        services.AddResourceLoaderAsScoped().TryAddSingleton<IBlazorCallbackRegistry, BlazorCallbackRegistry>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IBlazorCallbackRegistry"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddBlazorCallbackRegistryAsScoped(this IServiceCollection services)
    {
        services.AddResourceLoaderAsScoped().TryAddScoped<IBlazorCallbackRegistry, BlazorCallbackRegistry>();

        return services;
    }
}