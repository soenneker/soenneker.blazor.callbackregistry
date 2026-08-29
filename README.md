[![](https://img.shields.io/nuget/v/soenneker.blazor.callbackregistry.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.callbackregistry/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.callbackregistry/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.callbackregistry/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.callbackregistry.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.callbackregistry/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.callbackregistry/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.callbackregistry/actions/workflows/codeql.yml)

# Soenneker.Blazor.CallbackRegistry

A generic registry to register and invoke instance-specific Blazor JS callbacks.

## Install

```bash
dotnet add package Soenneker.Blazor.CallbackRegistry
```

## Quick start

```csharp
using Soenneker.Blazor.CallbackRegistry.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddBlazorCallbackRegistryAsScoped();
```

Adds `IBlazorCallbackRegistry` as a scoped service.

## What you get

- `IBlazorCallbackRegistry` — A generic registry to register and invoke instance-specific Blazor JS callbacks.
- `IBlazorCallbackWrapper` — Defines the blazor callback wrapper contract.
- `BlazorCallbackRegistryRegistrar` — A generic registry to register and invoke instance-specific Blazor JS callbacks.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IBlazorCallbackRegistry.Register(id, callback, cancellationToken)` | Registers a callback with the blazor callback registry. | A task that completes when callback registration is finished. |
| `IBlazorCallbackRegistry.Register(id, state, callback, cancellationToken)` | Registers a callback with the blazor callback registry. | A task that completes when callback registration is finished. |
| `IBlazorCallbackRegistry.Unregister(id)` | Removes the callback identified by the supplied ID from the blazor callback registry. | Returns no value; the requested change is complete when the method returns. |
| `IBlazorCallbackWrapper.Invoke(jsonPayload)` | Invokes the blazor callback wrapper with the supplied payload. | A task that completes when the callback has finished running. |
| `BlazorCallbackRegistryRegistrar.AddBlazorCallbackRegistryAsScoped(services)` | Adds `IBlazorCallbackRegistry` as a scoped service. | The same service collection, so additional registrations can be chained. |

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
- Dispose instances you own when their scope ends so held resources can be released.
