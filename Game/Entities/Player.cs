using Silvarea.Network;
using Silvarea.Network.Codec;
using Silvarea.Utility;

namespace Silvarea.Game.Entities
{
	public class Player
    {

        public string Username {  get; set; }
        public string Password { get; set; }

        public long Username37 { get; set; }

        public int Rights {  get; set; }
		public long Uid { get; set; }
        public int Pid { get; set; }

		public bool IsMembers {  get; set; }
        public bool IsLowMemory {  get; set; }

        public Session Session { get; set; }

		public Position Position { get; set; } = new Position(3370, 3485, 0);

        public PlayerUpdateMasks UpdateMasks { get; set; }

        public Player(string username, string password, Session session, int rights, bool isLowMemory, bool members)
        {
            Username = username;
            Password = password;
            Session = session;
            Rights = rights;
            IsLowMemory = isLowMemory;
            IsMembers = members;
			Uid = GenerateUid();
            Username37 = TextUtils.playerNameToLong(username);
        }

        public void Send(Packet packet)
        {
            GameCodec.Encode(Session, packet);
        }

        private long GenerateUid()
        {
            return ((TextUtils.playerNameToLong(Username) & 0x1fffff) << 11 | Pid) >>> 0;
		}
    }
}
