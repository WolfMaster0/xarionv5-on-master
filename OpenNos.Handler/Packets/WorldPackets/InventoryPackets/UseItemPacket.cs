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

using System;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("u_i")]
    public class UseItemPacket
    {
        #region Properties

        public InventoryType InventoryType { get; set; }

        public byte Slot { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 6)
            {
                return;
            }
            UseItemPacket packetDefinition = new UseItemPacket();
            if (Enum.TryParse(packetSplit[4], out InventoryType inventoryType)
                && byte.TryParse(packetSplit[5], out byte slot))
            {
                packetDefinition.InventoryType = inventoryType;
                packetDefinition.Slot = slot;
                packetDefinition.ExecuteHandler(session as ClientSession, packet);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(UseItemPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session, string originalPacket)
        {
            if ((byte)InventoryType >= 9)
            {
                return;
            }

            ItemInstance inv = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
            string[] packetsplit = originalPacket.Split(' ', '^');
            inv?.Item.Use(session, ref inv, packetsplit[1][0] == '#' ? (byte)255 : (byte)0, packetsplit);
        }

        #endregion
    }
}