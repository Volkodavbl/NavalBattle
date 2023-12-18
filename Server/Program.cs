using Newtonsoft.Json;
using Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR().AddNewtonsoftJsonProtocol(opts =>
        opts.PayloadSerializerSettings.TypeNameHandling = TypeNameHandling.Auto);

var app = builder.Build();

app.UseHttpsRedirection();
app.MapHub<NavalBattleHub>("/NavalBattle");

app.Run();
