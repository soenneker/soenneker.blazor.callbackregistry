using Microsoft.JSInterop;
using Soenneker.Asyncs.Initializers;
using Soenneker.Asyncs.Locks;
using Soenneker.Blazor.CallbackRegistry.Abstract;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Extensions.CancellationTokens;
using Soenneker.Utils.CancellationScopes;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.CallbackRegistry;

/// <inheritdoc cref="IBlazorCallbackRegistry"/>
public sealed class BlazorCallbackRegistry : IBlazorCallbackRegistry
{
    private readonly ConcurrentDictionary<string, IBlazorCallbackWrapper> _callbacks = new();

    private const string _module = "_content/Soenneker.Blazor.CallbackRegistry/js/callbackregistryinterop.js";

    private readonly IModuleImportUtil _moduleImportUtil;

    private DotNetObjectReference<BlazorCallbackRegistry>? _dotNetObjectReference;
    private readonly AsyncInitializer _initializer;
    private readonly AsyncLock _gate = new();
    private int _disposed;

    private readonly CancellationScope _cancellationScope = new();

    public BlazorCallbackRegistry(IModuleImportUtil moduleImportUtil)
    {
        _moduleImportUtil = moduleImportUtil;
        _initializer = new AsyncInitializer(InitializeJs);
    }

    private async ValueTask EnsureJsInitialized(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        await _initializer.Init(cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask InitializeJs(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        DotNetObjectReference<BlazorCallbackRegistry> reference = DotNetObjectReference.Create(this);

        try
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_module, cancellationToken).ConfigureAwait(false);
            await module.InvokeVoidAsync("initialize", cancellationToken, reference).ConfigureAwait(false);
            _dotNetObjectReference = reference;
        }
        catch
        {
            reference.Dispose();
            throw;
        }
    }

    public async ValueTask Register<T>(string id, Func<T, Task> callback, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(callback);

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureJsInitialized(linked);
            using (await _gate.Lock(linked).ConfigureAwait(false))
            {
                ThrowIfDisposed();
                _callbacks[id] = new BlazorCallbackWrapper<T>(callback);
            }
        }
    }

    public async ValueTask Register<TState, T>(string id, TState state, Func<TState, T, Task> callback, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(callback);

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureJsInitialized(linked);
            using (await _gate.Lock(linked).ConfigureAwait(false))
            {
                ThrowIfDisposed();
                _callbacks[id] = new BlazorCallbackWrapperStateful<TState, T>(state, callback);
            }
        }
    }

    public void Unregister(string id)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        _callbacks.TryRemove(id, out _);
    }

    /// <summary>
    /// Routes a JavaScript callback payload to the .NET callback registered under the supplied ID.
    /// </summary>
    /// <param name="id">Identifier of the blazor callback registry instance or registration to target.</param>
    /// <param name="jsonPayload">JSON payload supplied to the callback.</param>
    /// <returns>A task that completes when the registered callback has processed the payload.</returns>
    [JSInvokable]
    public async Task ReceiveJsCallback(string id, string jsonPayload)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(id))
            return;

        if (_callbacks.TryGetValue(id, out IBlazorCallbackWrapper? callbackWrapper))
        {
            await callbackWrapper.Invoke(jsonPayload);
        }
    }

    /// <summary>
    /// Asynchronously releases resources used by the current instance.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        _cancellationScope.Cancel();
        await _initializer.DisposeAsync().ConfigureAwait(false);

        using (await _gate.Lock().ConfigureAwait(false))
        {
            if (_dotNetObjectReference != null)
            {
                try
                {
                    IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_module, CancellationToken.None).ConfigureAwait(false);
                    await module.InvokeVoidAsync("dispose", CancellationToken.None).ConfigureAwait(false);
                }
                catch
                {
                    // JavaScript may already be unavailable during application teardown.
                }

                _dotNetObjectReference.Dispose();
                _dotNetObjectReference = null;
            }

            _callbacks.Clear();
        }

        await _moduleImportUtil.DisposeContentModule(_module);
        await _cancellationScope.DisposeAsync();
        await _gate.DisposeAsync();
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
    }
}
