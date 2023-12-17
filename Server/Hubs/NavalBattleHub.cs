using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using System.Collections;

namespace Server.Hubs
{
	public class NavalBattleHub : Hub<IClientContract>, IServerContract
	{
		private static List<Room> rooms = [];
		private static List<User> users = [];
		private static Hashtable connectionId_user = [];

		//Смена типа пользователя на новый
		public async Task ChangeUserType(UserType newUserType)
		{
			var user = GetUserByConnectionId(Context.ConnectionId);
			if (user == null)
			{
				await Clients.Caller.SendAsync("Error", "You must log in first");
				return;
			}

			var room = GetRoomByUser(user);
			if (room == null)
			{
				await Clients.Caller.SendAsync("Error", "You are not in a room");
				return;
			}

			user.UserType = newUserType;

			await Clients.Group(room.Id.ToString()).SendAsync("UserTypeChanged", user);
		}

		//Добавление нового корабля на игровую площадку до начала игры
		public async Task AddShip(Point start, Point end)
		{
			var user = GetUserByConnectionId(Context.ConnectionId);
			if (user == null)
			{
				await Clients.Caller.SendAsync("Error", "You must log in first");
				return;
			}

			var room = GetRoomByUser(user);
			if (room == null)
			{
				await Clients.Caller.SendAsync("Error", "You are not in a room");
				return;
			}

			if (room.Users.Select(x => (x.UserType == UserType.Player)).Count() < 2)
			{
				await Clients.Caller.SendAsync("Error", "Wait for another player to join the room");
				return;
			}

			var field = user.Field;
			if (field == null)
			{
				await Clients.Caller.SendAsync("Error", "You don't have a field in this room");
				return;
			}
			var ship = new Ship(start, end);
			field.Ships.Add(ship);
			
			await Clients.Group(room.Id.ToString()).SendAsync("ShipAdded", ship);
		}

		//Проверка на попадание ко кораблю хода игрока
		public async Task CheckHit(Point point)
		{
			var user = GetUserByConnectionId(Context.ConnectionId);
			if (user == null)
			{
				await Clients.Caller.SendAsync("Error", "You must log in first");
				return;
			}

			var room = GetRoomByUser(user);
			if (room == null)
			{
				await Clients.Caller.SendAsync("Error", "You are not in a room");
				return;
			}

			if (room.Users.Count < 2)
			{
				await Clients.Caller.SendAsync("Error", "Wait for another player to join the room");
				return;
			}

			var opponent = room.Users.Where(x => x.UserType == UserType.Player).FirstOrDefault(u => u != user);
			if (opponent == null)
			{
				await Clients.Caller.SendAsync("Error", "Opponent not found");
				return;
			}

			var opponentField = opponent.Field;
			if (opponentField == null)
			{
				await Clients.Caller.SendAsync("Error", "Opponent's field not found");
				return;
			}

			// узнаем, попали ли мы в кораблик или нет
			var hit = opponentField.Ships.Any(x => x.CheckHit(point));
			if (hit)
			{
				await Clients.Caller.SendAsync("Hit", hit);
				return;
			}
			else
			{
				await Clients.Caller.SendAsync("Miss", hit);
				return;
			}

		}

		//Игрок подтверждает готовность для старта игры
		public async Task ClientReady()
		{
			var user = GetUserByConnectionId(Context.ConnectionId);
			if (user == null)
			{
				await Clients.Caller.SendAsync("Error", "You must log in first");
				return;
			}

			var room = GetRoomByUser(user);
			if (room == null)
			{
				await Clients.Caller.SendAsync("Error", "You are not in a room");
				return;
			}

			await ChangeUserType(UserType.Player);

			await Clients.Group(room.Id.ToString()).SendAsync("PlayerReady", user);
		}

		//Создание новой комнаты, пользователь автоматически ---ПРИСОЕДИНЯЕТСЯ--- к созданной комнате
		public async Task CreateRoom(string roomName, int fieldSize, int shipCount)
		{
			var user = GetUserByConnectionId(Context.ConnectionId);
			if (user == null)
			{
				await Clients.Caller.SendAsync("Error", "You must log in first");
				return;
			}

			var room = new Room(rooms.Select(x => x.Id).Max() + 1,roomName, fieldSize, shipCount);
			rooms.Add(room);

			await JoinRoom(room.Id);

			await Clients.Caller.SendAsync("RoomCreated", room);
		}

		//TODO
		//Получение состояния игры на данный момент
		public async Task GetGameState()
		{
			var user = GetUserByConnectionId(Context.ConnectionId);
			if (user == null)
			{
				await Clients.Caller.SendAsync("Error", "You must log in first");
				return;
			}

			var room = GetRoomByUser(user);
			if (room == null)
			{
				await Clients.Caller.SendAsync("Error", "You are not in a room");
				return;
			}

			var filteredRoom = user.UserType == UserType.Player ? room.FilterByUser(user) : room;

			await Clients.Caller.SendAsync("GameState", filteredRoom);
		}

		//Получение списка всех комнат
		public async Task GetRoomList()
		{
			var user = GetUserByConnectionId(Context.ConnectionId);
			if (user == null)
			{
				await Clients.Caller.SendAsync("Error", "You must log in first");
				return;
			}

			var filteredRooms = rooms.ToList();

			await Clients.Caller.SendAsync("RoomList", filteredRooms);
		}

		//присоединение пользователя к комнате, если уже имеется два игрока со статусом Игрок, то пользователь становится наблюдателем
		public async Task JoinRoom(int roomId)
		{
			var user = GetUserByConnectionId(Context.ConnectionId);
			if (user == null)
			{
				await Clients.Caller.SendAsync("Error", "You must log in first");
				return;
			}

			var room = rooms.FirstOrDefault(r => r.Id == roomId);
			if (room == null)
			{
				await Clients.Caller.SendAsync("Error", "Room not found");
				return;
			}

			if (room.Users.Select(x => (x.UserType == UserType.Player)).Count() == 2)
			{
				await ChangeUserType(UserType.Observer);
				await Clients.Caller.SendAsync("Error", "Room is already full");
			}
			else
			{
				await ChangeUserType(UserType.Player);
				user.Field = new();
				await Clients.Caller.SendAsync("Good", "You are a player");
			}
			
			room.Users.Add(user);

			await Clients.Caller.SendAsync("JoinedRoom", room);
			await Clients.Group(roomId.ToString()).SendAsync("UserJoined", user);
		}

		//Логин нового юзера
		public async Task LoginUser(string login)
		{
			var notUnique = users.Any(x => x.Login == login);
			if (notUnique)
			{
				await Clients.Caller.SendAsync("notUniqueLogin", notUnique);
				return;
			}
			var user = new User(login);
			users.Add(user);
			connectionId_user.Add(Context.ConnectionId, user);

			await Clients.Caller.SendAsync("ShowRoomList", notUnique);
		}

		//Случайная расстановка всех кораблей. Учитывает, что игрок поместил уже какие-то корабли на поле, стирает их, ставит все корабли заново
		public async Task RandomShipPlacement()
		{
			var field = GetUserByConnectionId(Context.ConnectionId).Field;

			if(field == null)
			{
				await Clients.Caller.SendAsync("Error", field);
				return;
			}
			else
			{
				field.Ships.Clear();
				field.Events.Clear();
			}

			Random random = new Random();

			// Определение размеров поля
			int fieldSize = field.Size;
			int maxCoordinate = fieldSize - 1;

			// Создание списка доступных точек на поле
			List<Point> availablePoints = new List<Point>();
			for (int x = 0; x < fieldSize; x++)
			{
				for (int y = 0; y < fieldSize; y++)
				{
					availablePoints.Add(new Point(x, y));
				}
			}

			// Метод для получения случайной точки из списка доступных точек
			Point GetRandomPoint()
			{
				int index = random.Next(availablePoints.Count);
				Point point = availablePoints[index];
				availablePoints.RemoveAt(index);
				return point;
			}

			// Метод для проверки, является ли точка безопасной для размещения корабля
			bool IsSafePoint(Point point)
			{
				// Проверка наличия других кораблей в соседних точках
				int[] dx = { 0, 1, 0, -1 };
				int[] dy = { -1, 0, 1, 0 };
				foreach (int i in Enumerable.Range(0, 4))
				{
					int nx = point.X + dx[i];
					int ny = point.Y + dy[i];
					if (nx >= 0 && nx < fieldSize && ny >= 0 && ny < fieldSize)
					{
						Point neighborPoint = new Point(nx, ny);
						if (field.Ships.Any(ship => ship.CheckHit(neighborPoint)))
						{
							return false;
						}
					}
				}

				return true;
			}

			// Расстановка кораблей
			foreach (int shipSize in new[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 })
			{
				bool placed = false;
				while (!placed)
				{
					Point startPoint = GetRandomPoint();
					bool isVertical = random.Next(2) == 0;
					Point? endPoint = null;

					if (isVertical)
					{
						int maxY = startPoint.Y + shipSize - 1;
						if (maxY <= maxCoordinate && IsSafePoint(new Point(startPoint.X, maxY)))
						{
							endPoint = new Point(startPoint.X, maxY);
							placed = true;
						}
					}
					else
					{
						int maxX = startPoint.X + shipSize - 1;
						if (maxX <= maxCoordinate && IsSafePoint(new Point(maxX, startPoint.Y)))
						{
							endPoint = new Point(maxX, startPoint.Y);
							placed = true;
						}
					}

					if (placed)
					{
						Ship ship = new Ship(startPoint, endPoint);
						field.Ships.Add(ship);
					}
				}
			}

			await Clients.Caller.SendAsync("RandomShipPlacementApplied", field);
			return;
		}

		//Получение объекта команты по юзеру
		private Room GetRoomByUser(User user)
		{
			return rooms.FirstOrDefault(room => room.Users.Contains(user));
		}

		//Получение юзера по его строке подключения
		private User GetUserByConnectionId(string connectionId)
		{
			return users.FirstOrDefault(user => user.Login == connectionId_user[connectionId].ToString());
		}
	}
}
