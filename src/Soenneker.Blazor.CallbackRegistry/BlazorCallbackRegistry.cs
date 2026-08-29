using Microsoft.JSInterop;
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
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private int _disposed;

    private readonly CancellationScope _cancellationScope = new();

    public BlazorCallbackRegistry(IModuleImportUtil moduleImportUtil)
    {
        _moduleImportUtil = moduleImportUtil;
    }

    private async ValueTask EnsureJsInitialized(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        if (_dotNetObjectReference != null)
            return;

        await _initializationLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            ThrowIfDisposed();

            if (_dotNetObjectReference != null)
                return;

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
        finally
        {
            _initializationLock.Release();
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
            await _initializationLock.WaitAsync(linked).ConfigureAwait(false);

            try
            {
                ThrowIfDisposed();
                _callbacks[id] = new BlazorCallbackWrapper<T>(callback);
            }
            finally
            {
                _initializationLock.Release();
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
            await _initializationLock.WaitAsync(linked).ConfigureAwait(false);

            try
            {
                ThrowIfDisposed();
                _callbacks[id] = new BlazorCallbackWrapperStateful<TState, T>(state, callback);
            }
            finally
            {
                _initializationLock.Release();
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
        await _initializationLock.WaitAsync().ConfigureAwait(false);

        try
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
        finally
        {
            _initializationLock.Release();
        }

        await _moduleImportUtil.DisposeContentModule(_module);
        await _cancellationScope.DisposeAsync();
        _initializationLock.Dispose();
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
    }
}
