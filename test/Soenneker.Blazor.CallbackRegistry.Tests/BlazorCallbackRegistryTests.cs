using Soenneker.Blazor.CallbackRegistry.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Blazor.CallbackRegistry.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class BlazorCallbackRegistryTests : HostedUnitTest
{
    private readonly IBlazorCallbackRegistry _util;

    public BlazorCallbackRegistryTests(Host host) : base(host)
    {
        _util = Resolve<IBlazorCallbackRegistry>(true);
    }

    [Test]
    public void Default()
    {

    }
}
