using Domain.Entities;

namespace Domain.Contracts
{
    public interface IServerContract
    {
		public Task LoginUser(string login);

		public Task CreateRoom(string roomName, int fieldSize, int shipCount);

		public Task GetRoomList();

		public Task JoinRoom(int roomId);

		public Task AddShip(Point start, Point end);

		public Task CheckHit(Point point);

		public Task ClientReady();

		public Task GetGameState();

		public Task RandomShipPlacement();

		public Task ChangeUserType(UserType newUserType);
	}
}
