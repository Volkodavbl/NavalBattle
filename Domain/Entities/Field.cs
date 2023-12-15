namespace Domain.Entities
{
    public class Field(int size, int shipCount)
    {
        public int Size { get; init; } = size;
        public List<Ship> Ships { get; init; } = new(shipCount);
        public List<Point> Events { get; init; } = new(0);
    }
}
