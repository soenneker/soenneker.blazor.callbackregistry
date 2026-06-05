using System.Threading.Tasks;

namespace Soenneker.Blazor.CallbackRegistry.Abstract;

/// <summary>
/// Defines the blazor callback wrapper contract.
/// </summary>
public interface IBlazorCallbackWrapper
{
    /// <summary>
    /// Executes the invoke operation.
    /// </summary>
    /// <param name="jsonPayload">The json payload.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask Invoke(string jsonPayload);
}