using BlazorApp.Components;
using BlazorApp.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Root components mounted into index.html.
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Per-user store that persists the application model to the browser's localStorage.
// In standalone WASM, Scoped behaves as a singleton (one user per app instance) — exactly what we want.
builder.Services.AddScoped<DraftStore>();

await builder.Build().RunAsync();
