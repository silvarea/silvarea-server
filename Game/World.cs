using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silvarea.Game.Entities;

namespace Silvarea.Game
{
    public class World
    {
        public static List<Player> Players {  get; set; } = new List<Player>(2000);
        
        public World()
        {
            //initialize 600ms game tick
        }
    }
}
