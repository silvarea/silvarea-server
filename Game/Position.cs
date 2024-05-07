namespace Silvarea.Game
{
	public class Position
	{

		public int X { get; set; } = 0;
		public int Z { get; set; } = 0;
		public int Level { get; set; } = 0;

		public Position(int x, int z, int level) 
		{
			X = x;
			Z = z;
			Level = level;
		}
	}
}
