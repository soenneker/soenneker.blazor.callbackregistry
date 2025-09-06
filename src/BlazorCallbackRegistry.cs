using Microsoft.JSInterop;
using Soenneker.Blazor.CallbackRegistry.Abstract;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.AsyncSingleton;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.CallbackRegistry;

/// <inheritdoc cref="IBlazorCallbackRegistry"/>
public sealed class BlazorCallbackRegistry : IBlazorCallbackRegistry
{
    private readonly ConcurrentDictionary<string, IBlazorCallbackWrapper> _callbacks = new();

    private readonly AsyncSingleton _moduleInitializer;

    private const string _module = "Soenneker.Blazor.CallbackRegistry/js/callbackregistryinterop.js";
    private const string _moduleNamespace = "CallbackRegistryInterop";

    private readonly IResourceLoader _resourceLoader;

    private DotNetObjectReference<BlazorCallbackRegistry>? _dotNetObjectReference;

    public BlazorCallbackRegistry(IResourceLoader resourceLoader, IJSRuntime jSRuntime)
    {
        _resourceLoader = resourceLoader;

        _moduleInitializer = new AsyncSingleton(async (token, _) =>
        {
            await _resourceLoader.ImportModuleAndWaitUntilAvailable(_module, _moduleNamespace, 100, token);

            _dotNetObjectReference = DotNetObjectReference.Create(this);

            await jSRuntime.InvokeVoidAsync($"{_moduleNamespace}.initialize", _dotNetObjectReference);

            return new object();
        });
    }

    public async ValueTask Register<T>(string id, Func<T, Task> callback, CancellationToken cancellationToken = default)
    {
        await _moduleInitializer.Init(cancellationToken);

        _callbacks[id] = new BlazorCallbackWrapper<T>(callback);
    }

    public void Unregister(string id)
    {
        if (_callbacks.ContainsKey(id))
        {
            _callbacks.Remove(id, out _);
        }
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
    }
}
