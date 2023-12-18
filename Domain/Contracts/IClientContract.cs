using Domain.Entities;

namespace Domain.Contracts
{
    public interface IClientContract
    {
        public Task Error(string message);
        public Task ShowRoomList(List<Room> rooms);
        public Task ShowRoom(Room room, User user);
    }
}
