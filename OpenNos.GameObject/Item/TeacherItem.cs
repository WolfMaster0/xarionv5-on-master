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
using System;
using System.Linq;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class TeacherItem : Item
    {
        #region Instantiation

        public TeacherItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        private static void RemovePetLevel(ref Mate mate, Guid idToRemove)
        {
            if (mate == null || mate.Level <= 1)
            {
                return;
            }

            mate.Level -= 1;
            mate.Experience = 0;
            mate.Hp = mate.HpLoad();
            mate.Mp = mate.MpLoad();
            mate.Owner?.Session.SendPacket(mate.GenerateCond());
            mate.Owner?.Session.SendPacket(mate.GenerateScPacket());
            mate.Owner?.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, mate.MateTransportId, 8), mate.PositionX, mate.PositionY);
            mate.Owner?.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, mate.MateTransportId, 198), mate.PositionX, mate.PositionY);
            mate.Owner?.Session.Character.Inventory.RemoveItemFromInventory(idToRemove);
        }

        private static void AddPetLevel(ref Mate mate, Guid idToRemove)
        {
            if (mate == null || mate.Level >= mate.Owner?.Session.Character.Level - 5)
            {
                return;
            }
            mate.Level++;
            mate.Experience = 0;
            mate.Loyalty = 1000;
            mate.Hp = mate.HpLoad();
            mate.Mp = mate.MpLoad();
            mate.Owner?.Session.SendPacket(mate.GenerateScPacket());
            mate.Owner?.Session.SendPacket(mate.GenerateCond());
            mate.Owner?.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, mate.MateTransportId, 8), mate.PositionX, mate.PositionY);
            mate.Owner?.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, mate.MateTransportId, 198), mate.PositionX, mate.PositionY);
            mate.Owner?.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, mate.MateTransportId, 5002), mate.PositionX, mate.PositionY);
            mate.Owner?.Session.Character.Inventory.RemoveItemFromInventory(idToRemove);

        }

        public override void Use(ClientSession session, ref ItemInstance inv, byte option = 0, string[] packetsplit = null)
        {
            if (packetsplit == null)
            {
                return;
            }

            void ReleasePet(MateType mateType, Guid itemToRemoveId)
            {
                if (!int.TryParse(packetsplit[3], out int mateTransportId))
                {
                    return;
                }

                Mate mate = session.Character.Mates.Find(s => s.MateTransportId == mateTransportId && s.MateType == mateType);
                if (mate == null)
                {
                    return;
                }

                if (mate.MateType == MateType.Partner)
                {
                    if (mate.SpInstance != null || mate.GlovesInstance != null || mate.BootsInstance != null || mate.WeaponInstance != null || mate.ArmorInstance != null)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("MUST_REMOVE_PARTNER_EQUIPMENT")));
                        return;
                    }
                }

                if (!mate.IsTeamMember)
                {
                    session.Character.Mates.Remove(mate);
                    session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("PET_RELEASED")));
                    session.SendPacket(UserInterfaceHelper.GeneratePClear());
                    session.SendPackets(session.Character.GenerateScP());
                    session.SendPackets(session.Character.GenerateScN());
                    session.CurrentMapInstance?.Broadcast(mate.GenerateOut());
                    session.Character.Inventory.RemoveItemFromInventory(itemToRemoveId);
                }
                else
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("PET_IN_TEAM_UNRELEASABLE"), 0));
                }
            }

            switch (Effect)
            {
                // loyalty & xp petfood
                case 10:
                    if (int.TryParse(packetsplit[3], out var mateTransportId))
                    {
                        Mate mate = session.Character.Mates.Find(s => s.MateTransportId == mateTransportId && s.MateType == MateType.Pet);

                        if (mate == null || mate.Loyalty >= 1000 || mate.MateType == MateType.Partner)
                        {
                            return;
                        }

                        mate.Loyalty = (short)(mate.Loyalty + 100 > 1000 ? 1000 : mate.Loyalty + 100);
                        mate.GenerateXp(EffectValue);
                        session.SendPacket(mate.GenerateScPacket());
                        mate.Owner?.Session.SendPacket(mate.GenerateCond());
                        mate.Owner?.Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Npc, mate.MateTransportId, 5));
                        mate.Owner?.Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Npc, mate.MateTransportId, 5002));
                        mate.Owner?.Session.SendPacket(mate.Owner.GenerateSay(Language.Instance.GetMessageFromKey("MATE_EATS_FOOD"), 10));
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                case 11:
                case 12:
                    if (int.TryParse(packetsplit[3], out mateTransportId))
                    {
                        Mate mate = session.Character.Mates.Find(s => s.MateTransportId == mateTransportId && s.MateType == (Effect == 11 ? MateType.Pet : MateType.Partner));
                        AddPetLevel(ref mate, inv.Id);
                    }
                    break;

                case 13:
                    if (int.TryParse(packetsplit[3], out mateTransportId) && session.Character.Mates.Any(s => s.MateTransportId == mateTransportId))
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateGuri(10, 1, mateTransportId, 2));
                    }
                    break;

                case 14:
                    if (int.TryParse(packetsplit[3], out mateTransportId))
                    {
                        Mate mate = session.Character.Mates.Find(s => s.MateTransportId == mateTransportId && s.MateType == MateType.Pet);
                        if (mate?.CanPickUp == false)
                        {
                            session.CurrentMapInstance.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, mate.MateTransportId, 5));
                            session.CurrentMapInstance.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, mate.MateTransportId, 5002));
                            mate.CanPickUp = true;
                            session.SendPackets(session.Character.GenerateScP());
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PET_CAN_PICK_UP"), 10));
                            session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                        }
                    }
                    break;

                case 16:
                    if (int.TryParse(packetsplit[3], out mateTransportId))
                    {
                        Mate mate = session.Character.Mates.Find(s => s.MateTransportId == mateTransportId && s.MateType == MateType.Pet);
                        RemovePetLevel(ref mate, inv.Id);
                    }
                    break;

                case 17:
                    if (int.TryParse(packetsplit[3], out mateTransportId))
                    {
                        Mate mate = session.Character.Mates.Find(s => s.MateTransportId == mateTransportId);
                        if (mate?.IsSummonable == false)
                        {
                            mate.IsSummonable = true;
                            session.SendPackets(session.Character.GenerateScP());
                            session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PET_SUMMONABLE"), mate.Name), 10));
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PET_SUMMONABLE"), mate.Name), 0));
                            session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                        }
                    }
                    break;

                case 18:
                    if (int.TryParse(packetsplit[3], out mateTransportId))
                    {
                        Mate mate = session.Character.Mates.Find(s => s.MateTransportId == mateTransportId && s.MateType == MateType.Partner);
                        RemovePetLevel(ref mate, inv.Id);
                    }
                    break;

                case 1000:
                    ReleasePet(MateType.Pet, inv.Id);
                    break;

                case 1001:
                    ReleasePet(MateType.Partner, inv.Id);
                    break;

                // Pet trainer
                case 10000:
                    if (session.Character.MapInstance != session.Character.Miniland)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("NOT_IN_MINILAND"), 1));
                        return;
                    }

                    var monster = new MapMonster
                    {
                        MonsterVNum = (short)EffectValue,
                        MapY = session.Character.PositionY,
                        MapX = session.Character.PositionX,
                        MapId = session.Character.MapInstance.Map.MapId,
                        Position = session.Character.Direction,
                        IsMoving = true,
                        IsHostile = true,
                        MapMonsterId = session.CurrentMapInstance.GetNextMonsterId(),
                        IsMateTrainer = true,
                        ShouldRespawn = false
                    };

                    monster.Initialize(session.CurrentMapInstance);
                    session.CurrentMapInstance.AddMonster(monster);
                    session.CurrentMapInstance.Broadcast(monster.GenerateIn());
                    session.Character.Inventory.RemoveItemAmount(inv.ItemVNum);
                    monster.GetNearestOponent();
                    break;

                default:
                    Logger.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType(), VNum, Effect, EffectValue));
                    break;
            }
        }

        #endregion
    }
}