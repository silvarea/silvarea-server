using Silvarea.Network;
using Silvarea.Network.Codec;
using Silvarea.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silvarea.Game.Entities
{
    public class Player
    {
        string _username {  get; set; }
        string _password { get; set; }
        public int _rights {  get; set; }
        public bool _members {  get; set; }
        bool _isLowMemory {  get; set; }
        Session _session { get; set; }

        public Player(string username, string password, Session session, int rights, bool isLowMemory, bool members)
        {
            _username = username;
            _password = password;
            _session = session;
            _rights = rights;
            _isLowMemory = isLowMemory;
            _members = members;
        }

        public void Send(Packet packet)
        {
            GameCodec.Encode(_session, packet);
        }

    }
}
