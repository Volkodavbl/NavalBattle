using Domain.Entities;

namespace Client.Extensions;

public static class PointExtension
{
    public static bool IsShip(this Point point, List<Ship> ships)
    {
        return ships.Any((s) => point.IsBetween(s.Start, s.End));
    }

    public static bool IsBetween(this Point point, Point start, Point end)
    {
        return (
            (start.X <= point.X && end.X >= point.X) && (start.Y <= point.Y && end.Y >= point.Y) ||
            (start.X >= point.X && end.X <= point.X) && (start.Y <= point.Y && end.Y >= point.Y));
    }
}
