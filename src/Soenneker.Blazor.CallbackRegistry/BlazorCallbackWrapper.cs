using System.Text.Json.Serialization.Metadata;
using System;
using System.Threading.Tasks;
using Soenneker.Blazor.CallbackRegistry.Abstract;
using Soenneker.Utils.Json;

namespace Soenneker.Blazor.CallbackRegistry;

public sealed class BlazorCallbackWrapper<T> : IBlazorCallbackWrapper
{
    private readonly JsonTypeInfo<T> _typeInfo;
    private readonly Func<T, Task> _callback;

    public BlazorCallbackWrapper(Func<T, Task> callback, JsonTypeInfo<T> typeInfo)
    {
        _callback = callback;
        _typeInfo = typeInfo ?? throw new ArgumentNullException(nameof(typeInfo));
    }

    public ValueTask Invoke(string jsonPayload)
    {
        var data = JsonUtil.Deserialize<T>(jsonPayload, _typeInfo);

        if (data is null)
            return ValueTask.CompletedTask;

        Task task = _callback(data);
        return task.IsCompletedSuccessfully ? ValueTask.CompletedTask : new ValueTask(task);
    }
}