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

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.MinilandPackets
{
    [PacketHeader("rmvobj")]
    public class MinilandRemoveObject
    {
        #region Properties

        public short Slot { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            MinilandRemoveObject packetDefinition = new MinilandRemoveObject();
            if (short.TryParse(packetSplit[2], out short slot))
            {
                packetDefinition.Slot = slot;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MinilandRemoveObject), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            ItemInstance minilandobject =
                session.Character.Inventory.LoadBySlotAndType<ItemInstance>(Slot, InventoryType.Miniland);
            if (minilandobject != null)
            {
                if (session.Character.MinilandState == MinilandState.Lock)
                {
                    MinilandObject minilandObject =
                        session.Character.MinilandObjects.Find(s => s.ItemInstanceId == minilandobject.Id);
                    if (minilandObject != null)
                    {
                        if (minilandobject.Item.IsMinilandObject)
                        {
                            session.Character.WareHouseSize = 0;
                        }

                        session.Character.MinilandObjects.Remove(minilandObject);
                        session.SendPacket(minilandObject.GenerateMinilandEffect(true));
                        session.SendPacket(session.Character.GenerateMinilandPoint());
                        session.SendPacket(minilandObject.GenerateMinilandObject(true));
                    }
                }
                else
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_NEED_LOCK"), 0));
                }
            }
        }

        #endregion
    }
}