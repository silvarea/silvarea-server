using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silvarea.Game.Entities;
using Silvarea.Utility;

namespace Silvarea.Game
{
    public class World
    {
        public static List<Player> Players {  get; set; } = new List<Player>(2000);
        
        public World()
        {
            //initialize 600ms game tick
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

    }
}
