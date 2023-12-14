namespace Domain.Entities
{
    public class Room(int id, string name, int fieldSize, int shipCount)
    {
        public int Id { get; init; } = id;
        public string Name { get; init; } = name;
        public Field FieldA { get; init; } = new(fieldSize, shipCount);
        public Field FieldB { get; init; } = new(fieldSize, shipCount);
        public List<User> Users { get; init; } = new(2);
    }
}
