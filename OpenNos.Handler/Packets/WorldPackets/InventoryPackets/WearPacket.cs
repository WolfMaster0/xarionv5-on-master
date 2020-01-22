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

using System.Linq;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("wear")]
    public class WearPacket
    {
        #region Properties

        public byte Slot { get; set; }

        public byte Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            WearPacket packetDefinition = new WearPacket();
            if (byte.TryParse(packetSplit[2], out byte slot)
                && byte.TryParse(packetSplit[3], out byte type))
            {
                packetDefinition.Slot = slot;
                packetDefinition.Type = type;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(WearPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.ExchangeInfo?.ExchangeList.Count > 0
                || session.Character.Speed == 0)
            {
                return;
            }

            if (session.HasCurrentMapInstance && session.CurrentMapInstance.UserShops
                    .FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(session.Character.CharacterId)).Value
                == null)
            {
                ItemInstance inv =
                    session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType.Equipment);
                if (inv?.Item != null)
                {
                    inv.Item.Use(session, ref inv, Type); //inv.Item.Use(session, ref inv, Type);
                    session.Character.LoadSpeed();
                    session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId,
                        123));
                }
            }
        }

        #endregion
    }
}