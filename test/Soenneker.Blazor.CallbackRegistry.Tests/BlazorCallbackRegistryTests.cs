using Soenneker.Blazor.CallbackRegistry.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Blazor.CallbackRegistry.Tests;

[Collection("Collection")]
public class BlazorCallbackRegistryTests : FixturedUnitTest
{
    private readonly IBlazorCallbackRegistry _util;

    public BlazorCallbackRegistryTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IBlazorCallbackRegistry>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
