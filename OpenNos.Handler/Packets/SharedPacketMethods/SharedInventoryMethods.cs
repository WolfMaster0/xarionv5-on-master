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
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OpenNos.Core.Threading;
using OpenNos.Data;
using OpenNos.GameLog.LogHelper;

namespace OpenNos.Handler.Packets.SharedPacketMethods
{
    internal static class SharedInventoryMethods
    {
        #region Methods

        internal static void ChangeSp(this ClientSession session)
        {
            ItemInstance sp =
                session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
            ItemInstance fairy =
                session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Fairy, InventoryType.Wear);
            if (sp != null)
            {
                if (session.Character.GetReputationIco() < sp.Item.ReputationMinimum)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_REP"),
                        0));
                    return;
                }

                if (fairy != null && sp.Item.Element != 0 && fairy.Item.Element != sp.Item.Element
                    && fairy.Item.Element != sp.Item.SecondaryElement)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_FAIRY"),
                        0));
                    return;
                }

                session.Character.DisableBuffs(BuffType.All, force: true);
                session.Character.EquipmentBCards.AddRange(sp.Item.BCards);
                session.Character.LastTransform = DateTime.UtcNow;
                session.Character.UseSp = true;
                session.Character.Morph = sp.Item.Morph;
                session.Character.MorphUpgrade = sp.Upgrade;
                session.Character.MorphUpgrade2 = sp.Design;
                session.CurrentMapInstance?.Broadcast(session.Character.GenerateCMode());
                session.SendPacket(session.Character.GenerateLev());
                session.CurrentMapInstance?.Broadcast(
                    StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 196),
                    session.Character.PositionX, session.Character.PositionY);
                session.CurrentMapInstance?.Broadcast(
                    UserInterfaceHelper.GenerateGuri(6, 1, session.Character.CharacterId), session.Character.PositionX,
                    session.Character.PositionY);
                session.SendPacket(session.Character.GenerateSpPoint());
                session.Character.LoadSpeed();
                session.SendPacket(session.Character.GenerateCond());
                session.SendPacket(session.Character.GenerateStat());
                session.SendPacket(session.Character.GenerateStatChar());
                session.Character.SkillsSp = new ThreadSafeSortedList<int, CharacterSkill>();
                Parallel.ForEach(ServerManager.GetAllSkill(), skill =>
                {
                    if (skill.Class == session.Character.Morph + 31 && sp.SpLevel >= skill.LevelMinimum)
                    {
                        session.Character.SkillsSp[skill.SkillVNum] = new CharacterSkill
                        {
                            SkillVNum = skill.SkillVNum,
                            CharacterId = session.Character.CharacterId
                        };
                    }
                });
                session.SendPacket(session.Character.GenerateSki());
                session.SendPackets(session.Character.GenerateQuicklist());
                GameLogger.Instance.LogSpecialistWear(ServerManager.Instance.ChannelId, session.Character.Name,
                    session.Character.CharacterId, sp.Item.Morph);
            }
        }

        internal static void CloseExchange(this ClientSession session, ClientSession targetSession)
        {
            if (targetSession?.Character.ExchangeInfo != null)
            {
                targetSession.SendPacket("exc_close 0");
                targetSession.Character.ExchangeInfo = null;
            }

            if (session?.Character.ExchangeInfo != null)
            {
                session.SendPacket("exc_close 0");
                session.Character.ExchangeInfo = null;
            }
        }

        internal static void RemoveSp(this ClientSession session, short vnum)
        {
            if (session?.HasSession == true && !session.Character.IsVehicled)
            {
                session.Character.DisableBuffs(BuffType.All, force: true);
                session.Character.EquipmentBCards.RemoveAll(s => s.ItemVNum.Equals(vnum));
                session.Character.UseSp = false;
                session.Character.LoadSpeed();
                session.SendPacket(session.Character.GenerateCond());
                session.SendPacket(session.Character.GenerateLev());
                session.Character.SpCooldown = 30;
                if (session.Character.SkillsSp != null)
                {
                    foreach (CharacterSkill ski in session.Character.SkillsSp.Where(s => !s.CanBeUsed()))
                    {
                        short time = ski.Skill.Cooldown;
                        double temp = (ski.LastUse - DateTime.UtcNow).TotalMilliseconds + (time * 100);
                        temp /= 1000;
                        session.Character.SpCooldown = temp > session.Character.SpCooldown
                            ? (int)temp
                            : session.Character.SpCooldown;
                    }
                }

                session.SendPacket(session.Character.GenerateSay(
                    string.Format(Language.Instance.GetMessageFromKey("STAY_TIME"), session.Character.SpCooldown), 11));
                session.SendPacket($"sd {session.Character.SpCooldown}");
                session.CurrentMapInstance?.Broadcast(session.Character.GenerateCMode());
                session.CurrentMapInstance?.Broadcast(
                    UserInterfaceHelper.GenerateGuri(6, 1, session.Character.CharacterId), session.Character.PositionX,
                    session.Character.PositionY);

                // ms_c
                session.SendPacket(session.Character.GenerateSki());
                session.SendPackets(session.Character.GenerateQuicklist());
                session.SendPacket(session.Character.GenerateStat());
                session.SendPacket(session.Character.GenerateStatChar());

                GameLogger.Instance.LogSpecialistUnwear(ServerManager.Instance.ChannelId, session.Character.Name,
                    session.Character.CharacterId, session.Character.SpCooldown);

                Observable.Timer(TimeSpan.FromMilliseconds(session.Character.SpCooldown * 1000)).Subscribe(o =>
                {
                    session.SendPacket(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TRANSFORM_DISAPPEAR"), 11));
                    session.SendPacket("sd 0");
                });
                session.Character.AbsorbedDamage = 0;
                session.SendPacket($"bf 1 {session.Character.CharacterId} 0.0.0 {session.Character.Level}");
            }
        }

        internal static void Exchange(this ClientSession session, ClientSession targetSession)
        {
            if (session?.Character.ExchangeInfo == null
                || session.Character.Gold < session.Character.ExchangeInfo.Gold
                || session.Character.GoldBank < session.Character.ExchangeInfo.GoldBank * 1000)
            {
                return;
            }

            // remove all items from source session
            foreach (ItemInstance item in session.Character.ExchangeInfo.ExchangeList)
            {
                ItemInstance invtemp = session.Character.Inventory.GetItemInstanceById(item.Id);
                if (invtemp?.Amount >= item.Amount)
                {
                    session.Character.Inventory.RemoveItemFromInventory(invtemp.Id, item.Amount);
                }
                else
                {
                    return;
                }
            }

            // add all items to target session
            foreach (ItemInstance item in session.Character.ExchangeInfo.ExchangeList)
            {
                ItemInstance item2 = item.DeepCopy();
                item2.Id = Guid.NewGuid();
                List<ItemInstance> inv = targetSession.Character.Inventory.AddToInventory(item2);
                if (inv.Count == 0)
                {
                    // do what?
                }
            }

            // handle gold
            session.Character.Gold -= session.Character.ExchangeInfo.Gold;
            session.SendPacket(session.Character.GenerateGold());
            targetSession.Character.Gold += session.Character.ExchangeInfo.Gold;
            targetSession.SendPacket(targetSession.Character.GenerateGold());

            // handle goldbank
            session.Character.GoldBank -= session.Character.ExchangeInfo.GoldBank * 1000;
            targetSession.Character.GoldBank += session.Character.ExchangeInfo.GoldBank * 1000;

            // log this trade
            GameLogger.Instance.LogTrade(ServerManager.Instance.ChannelId, session.Character.Name,
                session.Character.CharacterId, targetSession.Character.Name, targetSession.Character.CharacterId,
                session.Character.ExchangeInfo.Gold, session.Character.ExchangeInfo.GoldBank * 1000,
                session.Character.ExchangeInfo.ExchangeList.Cast<ItemInstanceDTO>().ToList());

            // all items and gold from sourceSession have been transferred, clean exchange info
            session.Character.ExchangeInfo = null;
        }

        #endregion
    }
}