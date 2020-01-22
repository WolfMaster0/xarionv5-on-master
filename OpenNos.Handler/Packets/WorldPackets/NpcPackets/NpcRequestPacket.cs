// This file is part of the OpenNos NosTale Emulator Project.
// 
// This program is licensed under a deviated version of the Fair Source License,
// granting you a non-exclusive, non-transferable, royalty-free and fully-paid-up
// license, under all of the Licensor's copyright and patent rights, to use, copy, prepare
// derivative works of, publicly perform and display the Software, subject to the
// conditions found in the LICENSE file.
// 
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR
// CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN
// AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE.

using System.Collections.Generic;
using System.Linq;
using OpenNos.Core.Serializing;
using OpenNos.GameObject;
using OpenNos.GameObject.EventArguments;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;

namespace OpenNos.Handler.Packets.WorldPackets.NpcPackets
{
    [PacketHeader("npc_req")]
    public class NpcRequestPacket
    {
        #region Properties

        public long OwnerId { get; set; }

        public int Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            NpcRequestPacket packetDefinition = new NpcRequestPacket();
            if (int.TryParse(packetSplit[2], out int type)
                && long.TryParse(packetSplit[3], out long ownerId))
            {
                packetDefinition.Type = type;
                packetDefinition.OwnerId = ownerId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(NpcRequestPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (!session.HasCurrentMapInstance)
            {
                return;
            }

            if (Type == 1)
            {
                // User Shop
                KeyValuePair<long, MapShop> shopList = session.CurrentMapInstance.UserShops.FirstOrDefault(s => s.Value.OwnerId.Equals(OwnerId));
                session.LoadShopItem(OwnerId, shopList);
            }
            else
            {
                // Npc Shop , ignore if has drop
                MapNpc npc = session.CurrentMapInstance.Npcs.Find(n => n.MapNpcId.Equals((int)OwnerId));
                if (npc == null)
                {
                    return;
                }

                session.Character.OnTalk(new TalkEventArgs(npc));

                if (npc.Npc.Drops.Any(s => s.MonsterVNum != null) && npc.Npc.Race == 8
                                                                  && (npc.Npc.RaceType == 7 || npc.Npc.RaceType == 5))
                {
                    session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 4, $"#guri^400^{npc.MapNpcId}"));
                }
                else if (npc.Npc.VNumRequired > 0 && npc.Npc.Race == 8
                                                  && (npc.Npc.RaceType == 7 || npc.Npc.RaceType == 5))
                {
                    session.SendPacket(UserInterfaceHelper.GenerateDelay(6000, 4, $"#guri^400^{npc.MapNpcId}"));
                }
                else if (npc.Npc.MaxHP == 0 && npc.Npc.Drops.All(s => s.MonsterVNum == null) && npc.Npc.Race == 8
                         && (npc.Npc.RaceType == 7 || npc.Npc.RaceType == 5))
                {
                    session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 1,
                        $"#guri^710^{npc.MapX}^{npc.MapY}^{npc.MapNpcId}")); // #guri^710^DestinationX^DestinationY^MapNpcId
                }
                else if (!string.IsNullOrEmpty(npc.GetNpcDialog()))
                {
                    session.SendPacket(npc.GetNpcDialog());
                }
            }
        }

        #endregion
    }
}