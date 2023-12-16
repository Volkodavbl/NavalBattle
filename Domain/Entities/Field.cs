namespace Domain.Entities
{
    public class Field(int size = 10, int shipCount = 10)
    {
        public int Size { get; init; } = size;
        public List<Ship> Ships { get; init; } = new(shipCount);
        public List<Point> Events { get; init; } = new(0);
    }
}
