using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blazor.CallbackRegistry.Abstract;
using Soenneker.Blazor.Utils.ModuleImport.Registrars;

namespace Soenneker.Blazor.CallbackRegistry.Registrars;

/// <summary>
/// A generic registry to register and invoke instance-specific Blazor JS callbacks
/// </summary>
public static class BlazorCallbackRegistryRegistrar
{
    /// <summary>
    /// Adds <see cref="IBlazorCallbackRegistry"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddBlazorCallbackRegistryAsScoped(this IServiceCollection services)
    {
        services.AddModuleImportUtilAsScoped().TryAddScoped<IBlazorCallbackRegistry, BlazorCallbackRegistry>();

        return services;
    }
}
