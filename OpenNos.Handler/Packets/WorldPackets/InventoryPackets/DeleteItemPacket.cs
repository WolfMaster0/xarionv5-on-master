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
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("b_i")]
    public class DeleteItemPacket
    {
        #region Properties

        public InventoryType InventoryType { get; set; }

        public byte? Option { get; set; }

        public byte Slot { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            DeleteItemPacket packetDefinition = new DeleteItemPacket();
            if (Enum.TryParse(packetSplit[2], out InventoryType inventoryType)
                && byte.TryParse(packetSplit[3], out byte slot))
            {
                packetDefinition.InventoryType = inventoryType;
                packetDefinition.Slot = slot;
                packetDefinition.Option = packetSplit.Length >= 5 && byte.TryParse(packetSplit[4], out byte option) ? option : (byte?)null;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(DeleteItemPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            switch (Option)
            {
                case null:
                    session.SendPacket(UserInterfaceHelper.GenerateDialog(
                        $"#b_i^{(byte)InventoryType}^{Slot}^1 #b_i^0^0^5 {Language.Instance.GetMessageFromKey("ASK_TO_DELETE")}"));
                    break;

                case 1:
                    session.SendPacket(UserInterfaceHelper.GenerateDialog(
                        $"#b_i^{(byte)InventoryType}^{Slot}^2 #b_i^{(byte)InventoryType}^{Slot}^5 {Language.Instance.GetMessageFromKey("SURE_TO_DELETE")}"));
                    break;

                case 2:
                    if (session.Character.InExchangeOrTrade || InventoryType == InventoryType.Bazaar)
                    {
                        return;
                    }

                    ItemInstance delInstance =
                        session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
                    session.Character.DeleteItem(InventoryType, Slot);

                    if (delInstance != null)
                    {
                        GameLogger.Instance.LogItemDelete(ServerManager.Instance.ChannelId, session.Character.Name,
                            session.Character.CharacterId, delInstance, session.CurrentMapInstance?.Map.MapId ?? -1,
                            session.Character.PositionX, session.Character.PositionY);
                    }

                    break;
            }
        }

        #endregion
    }
}