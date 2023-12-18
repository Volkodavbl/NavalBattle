using Domain.Entities;

namespace Domain.Contracts
{
    public interface IClientContract
    {
        public void Error(string message);
        public void ShowRoomList(List<Room> rooms);
        public void ShowRoom(Room room);
    }
}
