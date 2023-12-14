namespace Domain.Entities
{
    public class Field(int size, int shipCount)
    {
        public int Size { get; init; } = size;
        public List<Ship> Ships { get; init; } = new List<Ship>(shipCount);
    }
}
