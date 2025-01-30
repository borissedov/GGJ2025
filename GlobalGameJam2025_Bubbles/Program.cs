using BlazorApplicationInsights;
using GlobalGameJam2025_Bubbles.Components;
using GlobalGameJam2025_Bubbles.Infrastructure;
using GlobalGameJam2025_Bubbles.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry(config =>
    config.ConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING"));
builder.Services.AddBlazorApplicationInsights(config => config.ConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING"));

builder.Services
    .AddAuthentication("BasicAuth")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuth", null);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicAuthPolicy", policy =>
    {
        policy.AddAuthenticationSchemes("BasicAuth");
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<NewsService>();
builder.Services.AddSingleton<OpenAiClient>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();