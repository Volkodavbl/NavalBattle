namespace Domain.Entities
{
    public class Point(int x, int y)
    {
        public int X { get; init; } = x;
        public int Y { get; init; } = y;
    }

    public enum ShipState
    {
        Ok,
        Damaged,
        Destroyed
    }

    public class Ship(Point start, Point end)
    {
        public Point Start { get; init; } = start;
        public Point End { get; init; } = end;
        public ShipState State { get; init; } = ShipState.Ok;
    }
}
