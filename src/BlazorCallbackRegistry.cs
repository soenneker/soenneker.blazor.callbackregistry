using Microsoft.JSInterop;
using Soenneker.Blazor.CallbackRegistry.Abstract;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using Soenneker.Asyncs.Initializers;
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

    private readonly AsyncInitializer _moduleInitializer;

    private const string _module = "Soenneker.Blazor.CallbackRegistry/js/callbackregistryinterop.js";
    private const string _moduleNamespace = "CallbackRegistryInterop";

    private readonly IResourceLoader _resourceLoader;
    private readonly IJSRuntime _jSRuntime;

    private DotNetObjectReference<BlazorCallbackRegistry>? _dotNetObjectReference;

    private readonly CancellationScope _cancellationScope = new();

    public BlazorCallbackRegistry(IResourceLoader resourceLoader, IJSRuntime jSRuntime)
    {
        _resourceLoader = resourceLoader;
        _jSRuntime = jSRuntime;
        _moduleInitializer = new AsyncInitializer(Initialize);
    }

    private async ValueTask Initialize(CancellationToken token)
    {
        await _resourceLoader.ImportModuleAndWaitUntilAvailable(_module, _moduleNamespace, 100, token);

        _dotNetObjectReference = DotNetObjectReference.Create(this);

        await _jSRuntime.InvokeVoidAsync("CallbackRegistryInterop.initialize", token, _dotNetObjectReference);
    }

    public async ValueTask Register<T>(string id, Func<T, Task> callback, CancellationToken cancellationToken = default)
    {
        var linked = _cancellationScope.CancellationToken.Link(cancellationToken, out var source);

        using (source)
        {
            await _moduleInitializer.Init(linked);
            _callbacks[id] = new BlazorCallbackWrapper<T>(callback);
        }
    }

    public async ValueTask Register<TState, T>(string id, TState state, Func<TState, T, Task> callback, CancellationToken cancellationToken = default)
    {
        var linked = _cancellationScope.CancellationToken.Link(cancellationToken, out var source);

        using (source)
        {
            await _moduleInitializer.Init(linked);
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

        await _resourceLoader.DisposeModule(_module);

        await _moduleInitializer.DisposeAsync();
        await _cancellationScope.DisposeAsync();
    }

    public void Dispose()
    {
        if (_dotNetObjectReference != null)
        {
            _dotNetObjectReference.Dispose();
            _dotNetObjectReference = null;
        }

        _resourceLoader.DisposeModule(_module);

        _moduleInitializer.Dispose();
        _cancellationScope.DisposeAsync().GetAwaiter().GetResult();
    }
}