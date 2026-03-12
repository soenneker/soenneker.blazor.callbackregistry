using Soenneker.Blazor.CallbackRegistry.Abstract;
using Soenneker.Utils.Json;
using System;
using System.Threading.Tasks;

namespace Soenneker.Blazor.CallbackRegistry;

internal sealed class BlazorCallbackWrapperStateful<TState, T> : IBlazorCallbackWrapper
{
    private readonly TState _state;
    private readonly Func<TState, T, Task> _callback;

    public BlazorCallbackWrapperStateful(TState state, Func<TState, T, Task> callback)
    {
        _state = state;
        _callback = callback;
    }

    public ValueTask Invoke(string jsonPayload)
    {
        var data = JsonUtil.Deserialize<T>(jsonPayload);

        if (data is null)
            return ValueTask.CompletedTask;

        Task task = _callback(_state, data);

        // Avoid allocating an async state machine here.
        return task.IsCompletedSuccessfully ? ValueTask.CompletedTask : new ValueTask(task);
    }
}