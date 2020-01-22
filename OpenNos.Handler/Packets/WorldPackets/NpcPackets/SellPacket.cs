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
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.NpcPackets
{
    [PacketHeader("sell")]
    public class SellPacket
    {
        #region Properties

        public short Data { get; set; }

        public byte? Slot { get; set; }

        public byte? Amount { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            SellPacket packetDefinition = new SellPacket();
            if (short.TryParse(packetSplit[4], out short data))
            {
                packetDefinition.Data = data;
                packetDefinition.Slot = packetSplit.Length >= 6 && byte.TryParse(packetSplit[5], out byte slot) ? slot : (byte?)null;
                packetDefinition.Amount = packetSplit.Length >= 7 && byte.TryParse(packetSplit[6], out byte amount) ? amount : (byte?)null;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(SellPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.ExchangeInfo?.ExchangeList.Count > 0 || session.Character.IsShopping)
            {
                return;
            }

            if (Amount.HasValue && Slot.HasValue)
            {
                InventoryType inventoryType = (InventoryType)Data;
                byte amount = Amount.Value, slot = Slot.Value;

                if (inventoryType == InventoryType.Bazaar)
                {
                    return;
                }

                ItemInstance inv = session.Character.Inventory.LoadBySlotAndType(slot, inventoryType);
                if (inv == null || amount > inv.Amount)
                {
                    return;
                }

                if (session.Character.MinilandObjects.Any(s => s.ItemInstanceId == inv.Id))
                {
                    return;
                }

                if (!inv.Item.IsSoldable)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateShopMemo(2,
                        string.Format(Language.Instance.GetMessageFromKey("ITEM_NOT_SOLDABLE"))));
                    return;
                }

                long price = inv.Item.ItemType == ItemType.Sell ? inv.Item.Price : inv.Item.Price / 20;

                if (session.Character.Gold + (price * amount) > ServerManager.Instance.Configuration.MaxGold)
                {
                    string message =
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0);
                    session.SendPacket(message);
                    return;
                }

                session.Character.Gold += price * amount;
                session.SendPacket(UserInterfaceHelper.GenerateShopMemo(1,
                    string.Format(Language.Instance.GetMessageFromKey("SELL_ITEM_VALID"), inv.Item.Name, amount)));

                session.Character.Inventory.RemoveItemFromInventory(inv.Id, amount);
                session.SendPacket(session.Character.GenerateGold());
            }
            else
            {
                short vNum = Data;
                CharacterSkill skill = session.Character.Skills[vNum];
                if (skill == null || vNum == 200 + (20 * (byte)session.Character.Class)
                                  || vNum == 201 + (20 * (byte)session.Character.Class))
                {
                    return;
                }

                session.Character.Gold -= skill.Skill.Price;
                session.SendPacket(session.Character.GenerateGold());

                foreach (CharacterSkill loadedSkill in session.Character.Skills.GetAllItems())
                {
                    if (skill.Skill.SkillVNum == loadedSkill.Skill.UpgradeSkill)
                    {
                        session.Character.Skills.Remove(loadedSkill.SkillVNum);
                    }
                }

                session.Character.Skills.Remove(skill.SkillVNum);
                session.SendPacket(session.Character.GenerateSki());
                session.SendPackets(session.Character.GenerateQuicklist());
                session.SendPacket(session.Character.GenerateLev());
            }
        }

        #endregion
    }
}