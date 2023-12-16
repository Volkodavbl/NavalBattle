using Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapHub<NavalBattleHub>("/NavalBattle");

app.Run();
