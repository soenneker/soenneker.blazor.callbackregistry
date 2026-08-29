using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.CallbackRegistry.Abstract;

/// <summary>
/// A generic registry to register and invoke instance-specific Blazor JS callbacks
/// </summary>
public interface IBlazorCallbackRegistry : IAsyncDisposable
{
    /// <summary>
    /// Registers or replaces a callback under the supplied ID and initializes the JavaScript bridge if necessary.
    /// </summary>
    /// <typeparam name="T">Type of value handled by the blazor callback registry.</typeparam>
    /// <param name="id">Identifier of the blazor callback registry instance or registration to target.</param>
    /// <param name="callback">Callback to invoke when a matching payload is received.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when callback registration is finished.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is null.</exception>
    ValueTask Register<T>(string id, Func<T, Task> callback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers or replaces a stateful callback under the supplied ID and initializes the JavaScript bridge if necessary.
    /// </summary>
    /// <typeparam name="TState">Type of state passed to the callback.</typeparam>
    /// <typeparam name="T">Type of value handled by the blazor callback registry.</typeparam>
    /// <param name="id">Identifier of the blazor callback registry instance or registration to target.</param>
    /// <param name="state">State value passed to the callback when it is invoked.</param>
    /// <param name="callback">Callback to invoke when a matching payload is received.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when callback registration is finished.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is null.</exception>
    ValueTask Register<TState, T>(string id, TState state, Func<TState, T, Task> callback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the callback identified by the supplied ID from the blazor callback registry.
    /// </summary>
    /// <param name="id">Identifier of the blazor callback registry instance or registration to target.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is empty or whitespace.</exception>
    void Unregister(string id);
}
