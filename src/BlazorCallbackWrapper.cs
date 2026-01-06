using System;
using System.Threading.Tasks;
using Soenneker.Blazor.CallbackRegistry.Abstract;
using Soenneker.Utils.Json;

namespace Soenneker.Blazor.CallbackRegistry;

///<inheritdoc cref="IBlazorCallbackWrapper"/>
public sealed class BlazorCallbackWrapper<T> : IBlazorCallbackWrapper
{
    private readonly Func<T, Task> _callback;

    public BlazorCallbackWrapper(Func<T, Task> callback)
    {
        _callback = callback;
    }

    public async ValueTask Invoke(string jsonPayload)
    {
        var data = JsonUtil.Deserialize<T>(jsonPayload);

        if (data != null)
        {
            await _callback(data);
        }
    }
}
