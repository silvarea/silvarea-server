using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Silvarea.Game.Entities;
using Silvarea.Utility;

namespace Silvarea.Game
{
    public class World
    {
        public static List<Player> Players { get; set; } = new List<Player>(2000);

        long _tickLength = 600;
        long currentTick = 0;
        long lastTick = 0;
        public Timer WorldTimer {get;}
        
        public World()
        {
			WorldTimer = new Timer(Tick, null, 0, 600);
        }

        public static Player getPlayerByUid(long uid)
        {
            var pid = (int) (uid & 0x7ff);
            var name37 = (uid >> 11) & 0x1fffff;

            var player = Players[pid];
            
            if(player == null)
            {
                return null;
            }


            Console.WriteLine($"Player Username37 & 0x1fffff {player.Username37 & 0x1fffff} Name37 {name37}");

			if ((player.Username37 & 0x1fffff) != name37)
            {
                return null;
            }

            return player;
        }

        public static void Unregister(Player player)
        {
            Players.Remove(player);
            //TODO Some saving stuff
        }

        public static void Tick(Object stateInfo)
        {
            foreach (Player p in Players)
            {
                PlayerSync.Update(p);
            }
        }
    }
}
