using Client.Components;
using Client.Services;
using Microsoft.AspNetCore.SignalR.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

HubConnection connection = new HubConnectionBuilder()
    .WithUrl(new Uri("https://127.0.0.1:7155/NavalBattle"), (opts) =>
    {
        opts.HttpMessageHandlerFactory = (message) =>
        {
            if (message is HttpClientHandler clientHandler)
                // always verify the SSL certificate
                clientHandler.ServerCertificateCustomValidationCallback +=
                    (sender, certificate, chain, sslPolicyErrors) => { return true; };
            return message;
        };
    })
    .WithAutomaticReconnect([TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromSeconds(10)])
    .Build();

await connection.StartAsync();

var client = new ClientService();

connection.On("TestClientMethod", client.TestClientMethod);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
