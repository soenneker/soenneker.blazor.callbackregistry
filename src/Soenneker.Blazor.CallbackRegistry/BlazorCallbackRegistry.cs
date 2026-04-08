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

    private readonly CancellationScope _cancellationScope = new();

    public BlazorCallbackRegistry(IModuleImportUtil moduleImportUtil)
    {
        _moduleImportUtil = moduleImportUtil;
    }

    private async ValueTask EnsureJsInitialized(CancellationToken cancellationToken)
    {
        if (_dotNetObjectReference != null)
            return;

        _dotNetObjectReference = DotNetObjectReference.Create(this);
        IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_module, cancellationToken);
        await module.InvokeVoidAsync("initialize", cancellationToken, _dotNetObjectReference);
    }

    public async ValueTask Register<T>(string id, Func<T, Task> callback, CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureJsInitialized(linked);
            _callbacks[id] = new BlazorCallbackWrapper<T>(callback);
        }
    }

    public async ValueTask Register<TState, T>(string id, TState state, Func<TState, T, Task> callback, CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureJsInitialized(linked);
            _callbacks[id] = new BlazorCallbackWrapperStateful<TState, T>(state, callback);
        }
    }

    public void Unregister(string id)
    {
        _callbacks.TryRemove(id, out _);
    }

    [JSInvokable]
    public async Task ReceiveJsCallback(string id, string jsonPayload)
    {
        if (_callbacks.TryGetValue(id, out IBlazorCallbackWrapper? callbackWrapper))
        {
            await callbackWrapper.Invoke(jsonPayload);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_dotNetObjectReference != null)
        {
            _dotNetObjectReference.Dispose();
            _dotNetObjectReference = null;
        }

        await _moduleImportUtil.DisposeContentModule(_module);

        await _cancellationScope.DisposeAsync();
    }
}