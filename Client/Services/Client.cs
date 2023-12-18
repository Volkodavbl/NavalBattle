using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.SignalR.Client;
using System.Runtime.InteropServices;

namespace Client.Services
{
    public class ClientService : IClientContract
    {
        public void Error(string message)
        {
            throw new NotImplementedException();
        }

        public void ShowRoom(Room room)
        {
            throw new NotImplementedException();
        }

        public void ShowRoomList(List<Room> rooms)
        {
            throw new NotImplementedException();
        }
    }
}
