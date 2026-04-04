using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Soenneker.Quark;
using Soenneker.Quark.Gen.Lucide.Generated;
using Soenneker.Blazor.CallbackRegistry.Demo;
using Soenneker.Blazor.CallbackRegistry.Registrars;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazorCallbackRegistryAsScoped();
builder.Services.AddQuarkSuiteAsScoped();
builder.Services.AddLucideIconsAsScoped();

await builder.Build().RunAsync();


