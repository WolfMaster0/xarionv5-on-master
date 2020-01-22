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
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;

namespace OpenNos.Handler.Packets.WorldPackets.NpcPackets
{
    [PacketHeader("buy")]
    public class BuyPacket
    {
        #region Properties

        public BuyShopType Type { get; set; }

        public long OwnerId { get; set; }

        public short Slot { get; set; }

        public byte Amount { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 5)
            {
                return;
            }
            BuyPacket packetDefinition = new BuyPacket();
            if (Enum.TryParse(packetSplit[2], out BuyShopType type)
                && long.TryParse(packetSplit[3], out long ownerId)
                && short.TryParse(packetSplit[4], out short slot))
            {
                packetDefinition.Type = type;
                packetDefinition.OwnerId = ownerId;
                packetDefinition.Slot = slot;
                packetDefinition.Amount = packetSplit.Length > 5
                    && byte.TryParse(packetSplit[5], out byte amount) ? amount : (byte)1;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BuyPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.InExchangeOrTrade)
            {
                return;
            }

            switch (Type)
            {
                case BuyShopType.CharacterShop:
                    if (!session.HasCurrentMapInstance)
                    {
                        return;
                    }

                    KeyValuePair<long, MapShop> shop =
                        session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop =>
                            mapshop.Value.OwnerId.Equals(OwnerId));
                    PersonalShopItem item = shop.Value?.Items.Find(i => i.ShopSlot.Equals(Slot));
                    ClientSession sess = ServerManager.Instance.GetSessionByCharacterId(shop.Value?.OwnerId ?? 0);
                    if (sess == null || item == null || Amount <= 0 || Amount > 999)
                    {
                        return;
                    }

                    GameLogger.Instance.LogItemBuyPlayerShop(ServerManager.Instance.ChannelId, session.Character.Name,
                        session.Character.CharacterId, sess.Character.Name, OwnerId, item.ItemInstance, Amount,
                        item.Price, session.CurrentMapInstance.Map.MapId, session.Character.PositionX,
                        session.Character.PositionY);

                    if (Amount > item.SellAmount)
                    {
                        Amount = item.SellAmount;
                    }

                    if ((item.Price * Amount)
                        + sess.Character.Gold
                        > ServerManager.Instance.Configuration.MaxGold)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateShopMemo(3,
                            Language.Instance.GetMessageFromKey("MAX_GOLD")));
                        return;
                    }

                    if (item.Price * Amount >= session.Character.Gold)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateShopMemo(3,
                            Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                        return;
                    }

                    // check if the item has been removed successfully from previous owner and remove it
                    if (session.BuyValidate(shop, Slot, Amount))
                    {
                        session.Character.Gold -= item.Price * Amount;
                        session.SendPacket(session.Character.GenerateGold());

                        KeyValuePair<long, MapShop> shop2 =
                            session.CurrentMapInstance.UserShops.FirstOrDefault(s =>
                                s.Value.OwnerId.Equals(OwnerId));
                        session.LoadShopItem(OwnerId, shop2);
                    }
                    else
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"),
                                0));
                    }

                    break;

                case BuyShopType.ItemShop:
                    if (!session.HasCurrentMapInstance)
                    {
                        return;
                    }

                    MapNpc npc =
                        session.CurrentMapInstance.Npcs.Find(n => n.MapNpcId.Equals((short)OwnerId));
                    if (npc != null)
                    {
                        int dist = Map.GetDistance(
                            new MapCell { X = session.Character.PositionX, Y = session.Character.PositionY },
                            new MapCell { X = npc.MapX, Y = npc.MapY });
                        if (npc.Shop == null || dist > 5)
                        {
                            return;
                        }

                        if (npc.Shop.ShopSkills.Count > 0)
                        {
                            if (!npc.Shop.ShopSkills.Exists(s => s.SkillVNum == Slot))
                            {
                                return;
                            }

                            // skill shop
                            if (session.Character.UseSp)
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("REMOVE_SP"),
                                        0));
                                return;
                            }

                            if (session.Character.Skills.Any(s =>
                                s.LastUse.AddMilliseconds(s.Skill.Cooldown * 100) > DateTime.UtcNow))
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("SKILL_NEED_COOLDOWN"), 0));
                                return;
                            }

                            Skill skillinfo = ServerManager.GetSkill(Slot);
                            if (session.Character.Skills.Any(s => s.SkillVNum == Slot) || skillinfo == null)
                            {
                                return;
                            }

                            GameLogger.Instance.LogSkillBuy(ServerManager.Instance.ChannelId, session.Character.Name,
                                session.Character.CharacterId, npc.MapNpcId, skillinfo.SkillVNum, skillinfo.Price,
                                session.CurrentMapInstance.Map.MapId, session.Character.PositionX,
                                session.Character.PositionY);

                            if (session.Character.Gold < skillinfo.Price)
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 0));
                            }
                            else if (session.Character.GetCP() < skillinfo.CPCost)
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_CP"), 0));
                            }
                            else
                            {
                                if (skillinfo.SkillVNum < 200)
                                {
                                    int skillMiniumLevel = 0;
                                    if (skillinfo.MinimumSwordmanLevel == 0 && skillinfo.MinimumArcherLevel == 0
                                                                            && skillinfo.MinimumMagicianLevel == 0)
                                    {
                                        skillMiniumLevel = skillinfo.MinimumAdventurerLevel;
                                    }
                                    else
                                    {
                                        switch (session.Character.Class)
                                        {
                                            case ClassType.Adventurer:
                                                skillMiniumLevel = skillinfo.MinimumAdventurerLevel;
                                                break;

                                            case ClassType.Swordman:
                                                skillMiniumLevel = skillinfo.MinimumSwordmanLevel;
                                                break;

                                            case ClassType.Archer:
                                                skillMiniumLevel = skillinfo.MinimumArcherLevel;
                                                break;

                                            case ClassType.Magician:
                                                skillMiniumLevel = skillinfo.MinimumMagicianLevel;
                                                break;
                                        }
                                    }

                                    if (skillMiniumLevel == 0)
                                    {
                                        session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                            Language.Instance.GetMessageFromKey("SKILL_CANT_LEARN"), 0));
                                        return;
                                    }

                                    if (session.Character.Level < skillMiniumLevel)
                                    {
                                        session.SendPacket(
                                            UserInterfaceHelper.GenerateMsg(
                                                Language.Instance.GetMessageFromKey("LOW_LVL"), 0));
                                        return;
                                    }

                                    foreach (CharacterSkill skill in session.Character.Skills.GetAllItems())
                                    {
                                        if (skillinfo.CastId == skill.Skill.CastId && skill.Skill.SkillVNum < 200)
                                        {
                                            session.Character.Skills.Remove(skill.SkillVNum);
                                        }
                                    }
                                }
                                else
                                {
                                    if ((byte)session.Character.Class != skillinfo.Class)
                                    {
                                        session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                            Language.Instance.GetMessageFromKey("SKILL_CANT_LEARN"), 0));
                                        return;
                                    }

                                    if (session.Character.JobLevel < skillinfo.LevelMinimum)
                                    {
                                        session.SendPacket(
                                            UserInterfaceHelper.GenerateMsg(
                                                Language.Instance.GetMessageFromKey("LOW_JOB_LVL"), 0));
                                        return;
                                    }

                                    if (skillinfo.UpgradeSkill != 0)
                                    {
                                        CharacterSkill oldupgrade = session.Character.Skills.FirstOrDefault(s =>
                                            s.Skill.UpgradeSkill == skillinfo.UpgradeSkill
                                            && s.Skill.UpgradeType == skillinfo.UpgradeType
                                            && s.Skill.UpgradeSkill != 0);
                                        if (oldupgrade != null)
                                        {
                                            session.Character.Skills.Remove(oldupgrade.SkillVNum);
                                        }
                                    }
                                }

                                session.Character.Skills[Slot] = new CharacterSkill
                                {
                                    SkillVNum = Slot,
                                    CharacterId = session.Character.CharacterId
                                };

                                session.Character.Gold -= skillinfo.Price;
                                session.SendPacket(session.Character.GenerateGold());
                                session.SendPacket(session.Character.GenerateSki());
                                session.SendPackets(session.Character.GenerateQuicklist());
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
                                session.Character.LoadPassiveSkills();
                                session.SendPacket(session.Character.GenerateLev());
                            }
                        }
                        else if (npc.Shop.ShopItems.Count > 0)
                        {
                            // npc shop
                            ShopItemDTO shopItem = npc.Shop.ShopItems.Find(it => it.Slot == Slot);
                            if (shopItem == null || Amount <= 0 || Amount > 999)
                            {
                                return;
                            }

                            Item iteminfo = ServerManager.GetItem(shopItem.ItemVNum);
                            long price = iteminfo.Price * Amount;
                            long reputprice = iteminfo.ReputPrice * Amount;
                            double percent;
                            switch (session.Character.GetDignityIco())
                            {
                                case 3:
                                    percent = 1.10;
                                    break;

                                case 4:
                                    percent = 1.20;
                                    break;

                                case 5:
                                case 6:
                                    percent = 1.5;
                                    break;

                                default:
                                    percent = 1;
                                    break;
                            }

                            GameLogger.Instance.LogItemBuyNpcShop(ServerManager.Instance.ChannelId,
                                session.Character.Name, session.Character.CharacterId, npc.MapNpcId, iteminfo.VNum,
                                Amount, (long)(price * percent), session.CurrentMapInstance.Map.MapId,
                                session.Character.PositionX, session.Character.PositionY);

                            sbyte rare = shopItem.Rare;
                            if (iteminfo.Type == 0)
                            {
                                Amount = 1;
                            }

                            if (iteminfo.ReputPrice == 0)
                            {
                                if (price < 0 || price * percent > session.Character.Gold)
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateShopMemo(3,
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                                    return;
                                }
                            }
                            else
                            {
                                if (reputprice <= 0 || reputprice > session.Character.Reputation)
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateShopMemo(3,
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_REPUT")));
                                    return;
                                }

                                byte ra = (byte)ServerManager.RandomNumber();

                                int[] rareprob = { 100, 100, 70, 50, 30, 15, 5, 1 };
                                if (iteminfo.ReputPrice != 0)
                                {
                                    for (int i = 0; i < rareprob.Length; i++)
                                    {
                                        if (ra <= rareprob[i])
                                        {
                                            rare = (sbyte)i;
                                        }
                                    }
                                }
                            }

                            List<ItemInstance> newItems = session.Character.Inventory.AddNewToInventory(
                                shopItem.ItemVNum, Amount, rare: rare, upgrade: shopItem.Upgrade,
                                design: shopItem.Color);
                            if (newItems.Count == 0)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateShopMemo(3,
                                    Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE")));
                                return;
                            }

                            if (newItems.Count > 0)
                            {
                                foreach (ItemInstance itemInst in newItems)
                                {
                                    switch (itemInst.Item.EquipmentSlot)
                                    {
                                        case EquipmentType.Armor:
                                        case EquipmentType.MainWeapon:
                                        case EquipmentType.SecondaryWeapon:
                                            bool isPartner = (itemInst.ItemVNum >= 990 && itemInst.ItemVNum <= 992) || (itemInst.ItemVNum >= 995 && itemInst.ItemVNum <= 997);
                                            itemInst.SetRarityPoint(isPartner);
                                            break;

                                        case EquipmentType.Boots:
                                        case EquipmentType.Gloves:
                                            itemInst.FireResistance =
                                                (short)(itemInst.Item.FireResistance * shopItem.Upgrade);
                                            itemInst.DarkResistance =
                                                (short)(itemInst.Item.DarkResistance * shopItem.Upgrade);
                                            itemInst.LightResistance =
                                                (short)(itemInst.Item.LightResistance * shopItem.Upgrade);
                                            itemInst.WaterResistance =
                                                (short)(itemInst.Item.WaterResistance * shopItem.Upgrade);
                                            break;
                                    }
                                }

                                if (iteminfo.ReputPrice == 0)
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateShopMemo(1,
                                        string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM_VALID"),
                                            iteminfo.Name, Amount)));
                                    session.Character.Gold -= (long)(price * percent);
                                    session.SendPacket(session.Character.GenerateGold());
                                }
                                else
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateShopMemo(1,
                                        string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM_VALID"),
                                            iteminfo.Name, Amount)));
                                    session.Character.Reputation -= reputprice;
                                    session.SendPacket(session.Character.GenerateFd());
                                    session.SendPacket(
                                        session.Character.GenerateSay(
                                            Language.Instance.GetMessageFromKey("REPUT_DECREASED"), 11));
                                }
                            }
                            else
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                            }
                        }
                    }

                    break;
            }
        }

        #endregion
    }
}