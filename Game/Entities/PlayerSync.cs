using Silvarea.Game.Zones;
using Silvarea.Network;
using Silvarea.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silvarea.Game.Entities
{
    public class PlayerSync
    {
        public static void Update(Player player) {

            player.UpdateMasks |= PlayerUpdateMasks.APPEARANCE;

			//check if player's location changes constitute a region change, if so sendMapRegion();

			Packet byteBlock = new Packet();
            Packet bitBlock = new Packet(170);

            bitBlock.openBitBuffer();
            bitBlock.pBits(1, 1);//are we movin? 0 = no

            if (true)
            {
				bitBlock.pBits(2, 3); // Update Type

				bitBlock.pBits(7, player.Position.X); // Not sure

				bitBlock.pBits(2, player.Position.Level); // This one is correct 5 sure

				bitBlock.pBits(1, 1);

				bitBlock.pBits(1, 1);

				bitBlock.pBits(7, player.Position.Z); // Not Sure
            }


			//UpdatePlayerMovement(player, bitBlock);

            // TODO: Have an observed players and NPCs Sets on the player.
            //var playersInZone = ZoneManager.GetZone(player.Position.X, player.Position.Z, player.Position.Level).Players;

            //bitBlock.pBits(8, playersInZone.Count);//player count

            //foreach (var p in playersInZone)
            //{
            //    var curPlayer = World.getPlayerByUid(p);

            //    bitBlock.pBits(1, 1);// player needs updating
            //    UpdatePlayerMovement(curPlayer, bitBlock);
            //    //if otherPlayer requires an update
            //    UpdatePlayer(curPlayer, bitBlock);
            //}
            //foreach (var p in playersInZone)
            //{
            //    var curPlayer = World.getPlayerByUid(p);
            //    bitBlock.p1((byte)curPlayer.UpdateMasks);
            //    AppearanceUpdate(curPlayer, byteBlock);
            //}
            //bitBlock.pBits(11, 2047);
            bitBlock.closeBitBuffer();
            bitBlock.pdata(byteBlock.toByteArray(), (int) byteBlock.Length);
            player.Send(bitBlock);

            //AppearanceUpdate(player, updatePacket);

        }

        public static void TestRender(Player player)
        {
            //APPEARANCE = 0x40
            Packet updatePacket = new Packet(170);
            updatePacket.openBitBuffer();

            updatePacket.pBits(1, 0); //no movement

            updatePacket.pBits(8, 1); //player count

            //would start to loop through players

            updatePacket.pBits(1, 1); //this player has changed, need to update

            updatePacket.pBits(2, 0); //player needs to update

            updatePacket.pBits(11, 0); //player index



            player.Send(updatePacket);
        }

        private static void AppearanceUpdate(Player player, Packet packet)
        {
            Packet subPacket = new Packet();
            subPacket.p1(0);//Gender
            subPacket.p1(1);//Head Icons
            subPacket.p1(0);//Skull Icon

            //TODO Check if player isNpc

            byte[] defaultAppearance = [0, 0, 0, 0, 18, 0, 26, 36, 0, 33, 42, 0];

            for (int i = 0; i < 12; i++) //TODO Equipment Slots
            {
                if (defaultAppearance[i] < 1)
                    subPacket.p1(defaultAppearance[i]);
                else
                    subPacket.p2(defaultAppearance[i]);
            }

            subPacket.p1(0);//hair color
            subPacket.p1(0);//torso color
            subPacket.p1(0);//leg color
            subPacket.p1(0);//feet color
            subPacket.p1(0);//skin color

            subPacket.p2(0x328);
            subPacket.p2(0x337);
            subPacket.p2(0x333);
            subPacket.p2(0x334);
            subPacket.p2(0x335);
            subPacket.p2(0x336);
            subPacket.p2(0x338);

            subPacket.p8(player.Username37);

            subPacket.p1(3); // combat level
            subPacket.p2(1); // skill level

            packet.p1_alt3((byte) subPacket.Length);
            packet.pdata(subPacket.toByteArray(), (int)subPacket.Length);
        }

        private static void UpdatePlayerMovement(Player player, Packet packet)
        {
            packet.pBits(2, 0);// player has not moved. 1 for normal, 2 for running??

        }

        private static void UpdatePlayer(Player player, Packet packet)
        {
            packet.pBits(11, player.Pid);//player index
            //AppearanceUpdate(player, packet);
            packet.pBits(5, 0);//x pos?
            packet.pBits(5, 0);//z pos?
            packet.pBits(1, 0);//is teleing
            packet.pBits(1, 1);//is initial update, use this to optimize
        }
    }
}
