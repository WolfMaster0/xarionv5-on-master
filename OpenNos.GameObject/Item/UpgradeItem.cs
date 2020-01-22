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
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class UpgradeItem : Item
    {
        #region Instantiation

        public UpgradeItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte option = 0, string[] packetsplit = null)
        {
            if (Effect == 0)
            {
                if (EffectValue != 0)
                {
                    if (session.Character.IsSitting)
                    {
                        session.Character.IsSitting = false;
                        session.SendPacket(session.Character.GenerateRest());
                    }
                    session.SendPacket(UserInterfaceHelper.GenerateGuri(12, 1, session.Character.CharacterId, EffectValue));
                }
                else if (EffectValue == 0)
                {
                    if (packetsplit?.Length > 9 && byte.TryParse(packetsplit[8], out byte typeEquip) && short.TryParse(packetsplit[9], out short slotEquip))
                    {
                        if (session.Character.IsSitting)
                        {
                            session.Character.IsSitting = false;
                            session.SendPacket(session.Character.GenerateRest());
                        }
                        if (option != 0)
                        {
                            bool isUsed = false;
                            switch (inv.ItemVNum)
                            {
                                case 1219:
                                    ItemInstance equip = session.Character.Inventory.LoadBySlotAndType(slotEquip, (InventoryType)typeEquip);
                                    if (equip?.IsFixed == true)
                                    {
                                        equip.IsFixed = false;
                                        session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 3003));
                                        session.SendPacket(UserInterfaceHelper.GenerateGuri(17, 1, session.Character.CharacterId, slotEquip));
                                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_UNFIXED"), 12));
                                        isUsed = true;
                                    }
                                    break;

                                case 1365:
                                case 9039:
                                    ItemInstance specialist = session.Character.Inventory.LoadBySlotAndType(slotEquip, (InventoryType)typeEquip);
                                    if (specialist?.Rare == -2)
                                    {
                                        specialist.Rare = 0;
                                        session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SP_RESURRECTED"), 0));
                                        session.SendPacket(UserInterfaceHelper.GenerateGuri(13, 1, session.Character.CharacterId, 1));
                                        session.Character.SpPoint = 10000;
                                        if (session.Character.SpPoint > 10000)
                                        {
                                            session.Character.SpPoint = 10000;
                                        }
                                        session.SendPacket(session.Character.GenerateSpPoint());
                                        session.SendPacket(specialist.GenerateInventoryAdd());
                                        isUsed = true;
                                    }
                                    break;
                            }
                            if (!isUsed)
                            {
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_IS_NOT_FIXED"), 11));
                            }
                            else
                            {
                                session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                            }
                        }
                        else
                        {
                            session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(byte)inv.Type}^{inv.Slot}^0^1^{typeEquip}^{slotEquip} {Language.Instance.GetMessageFromKey("QNA_ITEM")}");
                        }
                    }
                }
            }
            else
            {
                if (!OpenBoxItem(session, inv))
                {
                    Logger.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType(),
                        VNum, Effect, EffectValue));
                }
            }
        }

        #endregion
    }
}