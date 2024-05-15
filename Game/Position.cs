using Silvarea.Game.Entities;

namespace Silvarea.Game
{
	public class Position
	{

		public int X { get; set; } = 0;
		public int Z { get; set; } = 0;
		public int Level { get; set; } = 0;

		public int RegionX { get => (X >> 3); }
		public int RegionZ { get => (Z >> 3); }

		public int LocalX { get => X - 8 * (RegionX - 6); }
		public int LocalZ { get => Z - 8 * (RegionZ - 6); }

		public Position(int x, int z, int level) 
		{
			X = x;
			Z = z;
			Level = level;
		}

	}
}
