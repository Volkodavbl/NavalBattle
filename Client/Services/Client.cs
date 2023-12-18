using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.SignalR.Client;
using System.Runtime.InteropServices;

namespace Client.Services
{
    public class ClientService : IClientContract
    {

        public event Action<List<Room>> RoomListChanged;
        public event Action<Room> RoomChanged;
        public event Action<string> ErrorOcured;
        public void Error(string message)
        {
            ErrorOcured.Invoke(message);
        }

        public void ShowRoom(Room room)
        {
            RoomChanged.Invoke(room);
        }

        public void ShowRoomList(List<Room> rooms)
        {
            RoomListChanged.Invoke(rooms);
        }
    }
}
