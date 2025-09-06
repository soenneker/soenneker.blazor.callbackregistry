using System.Threading.Tasks;

namespace Soenneker.Blazor.CallbackRegistry.Abstract;

public interface IBlazorCallbackWrapper
{
    ValueTask Invoke(string jsonPayload);
}
