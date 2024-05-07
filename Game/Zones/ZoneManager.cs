/**
 * Nothing original to see here. Largely a conversion of 2004scape's zones
 * https://github.com/boogie-nights/Server-1/tree/main/src/lostcity/engine/zone
 */
namespace Silvarea.Game.Zones
{
	public static class ZoneManager
	{

		public static Dictionary<int, Zone> Zones = new Dictionary<int, Zone>();

		private static int ZoneIndex(int x, int z, int level)
		{
			return ((x >> 3) & 0x7FF) | (((z >> 3) & 0x7FF) << 11) | ((level & 0x2) << 22);
		}

		public static Zone GetZone(int absoluteX, int absoluteZ, int level)
		{
			var zoneIndex = ZoneManager.ZoneIndex(absoluteX, absoluteZ, level);

			Zone zone;

			if (!Zones.TryGetValue(zoneIndex, out zone))
			{
				zone = new Zone(zoneIndex);
				Zones.Add(zoneIndex, zone);
			}
			return zone;
		}

	}
}
