using Domain.Entities;

namespace Domain.Contracts
{
    public interface IClientContract
    {
        public void ShowRoom(Room room);
        public void ShowRoomList(List<Room> rooms);
    }
}
