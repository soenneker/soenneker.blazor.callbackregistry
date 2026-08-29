using System.Threading.Tasks;

namespace Soenneker.Blazor.CallbackRegistry.Abstract;

/// <summary>
/// Defines the blazor callback wrapper contract.
/// </summary>
public interface IBlazorCallbackWrapper
{
    /// <summary>
    /// Invokes the blazor callback wrapper with the supplied payload.
    /// </summary>
    /// <param name="jsonPayload">JSON payload supplied to the callback.</param>
    /// <returns>A task that completes when the callback has finished running.</returns>
    ValueTask Invoke(string jsonPayload);
}
