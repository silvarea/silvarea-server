using Silvarea.Game.Entities;

/**
 * Nothing original to see here. Largely a conversion of 2004scape's zones
 * https://github.com/boogie-nights/Server-1/tree/main/src/lostcity/engine/zone
 */
namespace Silvarea.Game.Zones
{
	public class Zone
	{
		public int ZoneIndex { get; set; } = -1;

		public HashSet<long> Players = new HashSet<long>();
		public HashSet<int> Npcs = new HashSet<int>();
		
		// We'll have to handle locs, objs, and static locs and objs here as well

		// Also Zone Events which would be things like anims, loc changes, etc.


		public Zone(int index) 
		{ 
			ZoneIndex = index;
		}

		public void AddPlayer(Player player)
		{
			Players.Add(player.Uid);
		}

		public void AddNpc(Npc npc) 
		{
			Npcs.Add(npc.Nid);
		}

		public void RemovePlayer(Player player)
		{
			Players.Remove(player.Uid);
		}

		public void RemoveNpc(Npc npc)
		{
			Npcs.Remove(npc.Nid);
		}
	}
}
