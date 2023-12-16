using Domain.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs
{
    public class NavalBattleHub : Hub<IClientContract>, IServerContract
    {
        public string TestServerMethod()
        {
            return "Test";
        }
    }
}
