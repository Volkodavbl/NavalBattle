namespace Domain.Entities
{
    public class Room(int id, string name, int fieldSize, int shipCount)
    {
        public int Id { get; init; } = id;
        public string Name { get; init; } = name;
        public List<User> Users { get; init; } = new(2);
    }
}
