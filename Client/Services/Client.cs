﻿using Domain.Contracts;
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
        public Task Error(string message)
        {
            ErrorOcured.Invoke(message);
            return Task.CompletedTask;
        }

        public Task ShowRoom(Room room)
        {
            RoomChanged.Invoke(room);
            return Task.CompletedTask;

        }

        public Task ShowRoomList(List<Room> rooms)
        {
            RoomListChanged.Invoke(rooms);
            return Task.CompletedTask;

        }
    }
}
