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
        public List<bool> Sections = new(Math.Max(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y)));
        public ShipState State { get; init; } = ShipState.Ok;

		public bool CheckHit(Point target)
		{
			bool isHit = (Start.X <= target.X && End.X >= target.X) && (Start.Y <= target.Y && End.Y >= target.Y) ||
						 (Start.X >= target.X && End.X <= target.X) && (Start.Y <= target.Y && End.Y >= target.Y);

			if (isHit)
			{
				int sectionIndex = GetSectionIndex(target);
				if (sectionIndex >= 0 && sectionIndex < Sections.Count)
				{
					Sections[sectionIndex] = true;
				}
			}

			return isHit;
		}

		private int GetSectionIndex(Point target)
		{
			int sectionIndex = -1;
			if (Start.X == End.X)
			{
				int minY = Math.Min(Start.Y, End.Y);
				int maxY = Math.Max(Start.Y, End.Y);
				if (target.Y >= minY && target.Y <= maxY)
				{
					sectionIndex = target.Y - minY;
				}
			}
			else if (Start.Y == End.Y)
			{
				int minX = Math.Min(Start.X, End.X);
				int maxX = Math.Max(Start.X, End.X);
				if (target.X >= minX && target.X <= maxX)
				{
					sectionIndex = target.X - minX;
				}
			}

			return sectionIndex;
		}
	}
}
