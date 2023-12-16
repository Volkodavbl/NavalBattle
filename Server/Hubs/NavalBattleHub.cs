using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using System.Collections;

namespace Server.Hubs
{
	public class NavalBattleHub : Hub<IClientContract>, IServerContract
	{
		private static List<Room> rooms = new List<Room>();
		private static List<User> users = new List<User>();
		private static Hashtable connectionId_user = new Hashtable();

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

			user.UserType = UserType.Player;

			await Clients.Group(room.Id.ToString()).SendAsync("PlayerReady", user);
		}

		public async Task CreateRoom(string roomName, int fieldSize, int shipCount)
		{
			var user = GetUserByConnectionId(Context.ConnectionId);
			if (user == null)
			{
				await Clients.Caller.SendAsync("Error", "You must log in first");
				return;
			}

			var room = new Room(rooms.Select(x => x.Id).Max() + 1,roomName, fieldSize, shipCount);
			room.Users.Add(user);
			rooms.Add(room);

			await Clients.Caller.SendAsync("RoomCreated", room);
		}

		//TODO//
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
				user.UserType = UserType.Observer;
				await Clients.Caller.SendAsync("Error", "Room is already full");
			}
			else
			{
				user.UserType = UserType.Player;
				user.Field = new();
				await Clients.Caller.SendAsync("Good", "You are a player");
			}
			
			room.Users.Add(user);

			await Clients.Caller.SendAsync("JoinedRoom", room);
			await Clients.Group(roomId.ToString()).SendAsync("UserJoined", user);
		}

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
		//TODO
		public async Task ShipPlacement()
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

			var field = user.Field;
			if (field == null)
			{
				await Clients.Caller.SendAsync("Error", "You don't have a field in this room");
				return;
			}

			if (field.Ships.Count > 9)
			{
				await Clients.Caller.SendAsync("Error", "You already have ships on your field");
				return;
			}

			var random = new Random();
			for (int i = 0; i < field.Ships.Count; i++)
			{
				int startX = random.Next(0, field.Size);
				int startY = random.Next(0, field.Size);
				int endX = startX + random.Next(1, 4); 
				int endY = startY;

				if (endX >= field.Size)
				{
					endX = field.Size - 1;
				}

				var start = new Point(startX, startY);
				var end = new Point(endX, endY);
				var ship = new Ship(start, end);

				field.Ships.Add(ship);
			}

			await Clients.Caller.SendAsync("ShipsAutoPlaced", field.Ships);
		}
		private Room GetRoomByUser(User user)
		{
			return rooms.FirstOrDefault(room => room.Users.Contains(user));
		}

		private User GetUserByConnectionId(string connectionId)
		{
			return users.FirstOrDefault(user => user.Login == connectionId_user[connectionId].ToString());
		}
	}
}
