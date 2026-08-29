[![](https://img.shields.io/nuget/v/soenneker.blazor.callbackregistry.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.callbackregistry/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.callbackregistry/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.callbackregistry/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.callbackregistry.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.callbackregistry/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.callbackregistry/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.callbackregistry/actions/workflows/codeql.yml)

# Soenneker.Blazor.CallbackRegistry

A scoped bridge for routing JavaScript events to typed, instance-specific .NET callbacks by ID.

## Installation and registration

```bash
dotnet add package Soenneker.Blazor.CallbackRegistry
```

```csharp
using Soenneker.Blazor.CallbackRegistry.Registrars;

builder.Services.AddBlazorCallbackRegistryAsScoped();
```

## Register a callback

Register after the component's first render so JavaScript interop is available. Unregister component-owned IDs when the component is disposed.

```razor
@implements IDisposable
@inject IBlazorCallbackRegistry CallbackRegistry

@code {
    private const string CallbackId = "orders:active";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await CallbackRegistry.Register<OrderUpdated>(CallbackId, OnOrderUpdated);
    }

    private Task OnOrderUpdated(OrderUpdated update)
    {
        // Apply the update and refresh component state as needed.
        return Task.CompletedTask;
    }

    public void Dispose() => CallbackRegistry.Unregister(CallbackId);
}
```

Registering the same ID again replaces its callback. IDs share the registry's DI scope, so use stable, collision-resistant names for independently rendered components. The stateful overload stores a state value with the callback:

```csharp
await CallbackRegistry.Register<MyComponent, OrderUpdated>(
    callbackId,
    this,
    static (component, update) => component.OnOrderUpdated(update));
```

## Send an event from JavaScript

Import the package module using the application's base URI, then await `sendToCallback` so .NET deserialization or callback failures are observable by JavaScript:

```javascript
const callbackRegistry = await import(new URL(
    "_content/Soenneker.Blazor.CallbackRegistry/js/callbackregistryinterop.js",
    document.baseURI));

await callbackRegistry.sendToCallback("orders:active", {
    orderId: "2d8f1d42",
    status: "shipped"
});
```

The payload is serialized in JavaScript and deserialized as the registered generic type in .NET. Keep payload types JSON-compatible. A missing ID is ignored; malformed JSON or a callback exception rejects the JavaScript promise.

`IBlazorCallbackRegistry` is scoped and owns the JavaScript bridge reference. Let DI dispose it. Do not manually dispose an injected registry while other components in the same scope may still use it.
