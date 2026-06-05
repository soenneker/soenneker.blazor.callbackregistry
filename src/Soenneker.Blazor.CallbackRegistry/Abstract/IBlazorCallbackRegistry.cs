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
    /// Executes the register operation.
    /// </summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="id">The identifier.</param>
    /// <param name="callback">The callback.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask Register<T>(string id, Func<T, Task> callback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the register operation.
    /// </summary>
    /// <typeparam name="TState">The TState type.</typeparam>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="id">The identifier.</param>
    /// <param name="state">The state.</param>
    /// <param name="callback">The callback.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask Register<TState, T>(string id, TState state, Func<TState, T, Task> callback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the unregister operation.
    /// </summary>
    /// <param name="id">The identifier.</param>
    void Unregister(string id);
}