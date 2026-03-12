using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.CallbackRegistry.Abstract;

/// <summary>
/// A generic registry to register and invoke instance-specific Blazor JS callbacks
/// </summary>
public interface IBlazorCallbackRegistry : IAsyncDisposable, IDisposable
{
    ValueTask Register<T>(string id, Func<T, Task> callback, CancellationToken cancellationToken = default);

    ValueTask Register<TState, T>(string id, TState state, Func<TState, T, Task> callback, CancellationToken cancellationToken = default);

    void Unregister(string id);
}