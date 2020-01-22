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
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject
{
    public class BCard : BCardDTO
    {
        public BCard()
        {
        }

        public BCard(BCardDTO input)
        {
            BCardId = input.BCardId;
            CardId = input.CardId;
            CastType = input.CastType;
            FirstData = input.FirstData;
            IsLevelDivided = input.IsLevelDivided;
            IsLevelScaled = input.IsLevelScaled;
            ItemVNum = input.ItemVNum;
            NpcMonsterVNum = input.NpcMonsterVNum;
            SecondData = input.SecondData;
            SkillVNum = input.SkillVNum;
            SubType = input.SubType;
            ThirdData = input.ThirdData;
            Type = input.Type;
        }

        public Card BuffCard => ServerManager.GetCard((short)SecondData);

        #region Methods

        public void ApplyBCards(object session, object sender = null, short buffLevel = 0)
        {
            Type type = session.GetType();

            // int counterBuff = 0;

            switch ((BCardType.CardType)Type)
            {
                case BCardType.CardType.Buff:
                    {
                        if (type == typeof(Character) && session is Character character)
                        {
                            Buff buff = null;
                            if (sender != null)
                            {
                                Type sType = sender.GetType();
                                if (sType == typeof(Character) && sender is Character sendingCharacter)
                                {
                                    buff = new Buff((short)((short)SecondData + buffLevel), sendingCharacter.Level);

                                    //Todo: Get anti stats from BCard
                                }
                            }
                            else
                            {
                                buff = new Buff((short)((short)SecondData + buffLevel), character.Level);
                            }

                            int anti = 0;
                            if (buff?.Card.BuffType == BuffType.Bad)
                            {
                                anti += character.ShellEffectArmor?.FirstOrDefault(s =>
                                            s.Effect == (byte)ShellArmorEffectType.ReducedAllNegativeEffect)?.Value ??
                                        0;
                                switch (SecondData)
                                {
                                    case 1:
                                        anti += character.ShellEffectArmor?.FirstOrDefault(s =>
                                                        s.Effect == (byte)ShellArmorEffectType
                                                            .ReducedBleedingAndMinorBleeding)
                                                    ?.Value ?? 0;
                                        anti += character.ShellEffectArmor?.FirstOrDefault(s =>
                                                        s.Effect == (byte)ShellArmorEffectType.ReducedAllBleedingType)
                                                    ?.Value ?? 0;
                                        break;
                                    case 7:
                                        anti += character.ShellEffectArmor
                                                    ?.FirstOrDefault(s =>
                                                        s.Effect == (byte)ShellArmorEffectType.ReducedStun)?.Value ?? 0;
                                        anti += character.ShellEffectArmor?.FirstOrDefault(s =>
                                                    s.Effect == (byte)ShellArmorEffectType.ReducedAllStun)?.Value ?? 0;
                                        break;
                                    case 21:
                                        anti += character.ShellEffectArmor?.FirstOrDefault(s =>
                                                    s.Effect == (byte)ShellArmorEffectType.ReducedMinorBleeding)?.Value ??
                                                0;
                                        anti += character.ShellEffectArmor?.FirstOrDefault(s =>
                                                        s.Effect == (byte)ShellArmorEffectType
                                                            .ReducedBleedingAndMinorBleeding)
                                                    ?.Value ?? 0;
                                        anti += character.ShellEffectArmor?.FirstOrDefault(s =>
                                                        s.Effect == (byte)ShellArmorEffectType.ReducedAllBleedingType)
                                                    ?.Value ?? 0;
                                        break;
                                    case 27:
                                        anti += character.ShellEffectArmor?.FirstOrDefault(s =>
                                                    s.Effect == (byte)ShellArmorEffectType.ReducedFreeze)?.Value ?? 0;
                                        break;
                                    case 42:
                                        anti += character.ShellEffectArmor?.FirstOrDefault(s =>
                                                        s.Effect == (byte)ShellArmorEffectType.ReducedAllBleedingType)
                                                    ?.Value ?? 0;
                                        break;
                                    case 66:
                                        anti += character.ShellEffectArmor?.FirstOrDefault(s =>
                                                    s.Effect == (byte)ShellArmorEffectType.ReducedAllStun)?.Value ?? 0;
                                        break;
                                }
                            }

                            if (ServerManager.RandomNumber() < FirstData / 100D * (100 - anti))
                            {
                                character.AddBuff(buff);
                            }
                        }
                        else if (type == typeof(MapMonster))
                        {
                            if (ServerManager.RandomNumber() < FirstData && session is MapMonster mapMonster)
                            {
                                mapMonster.AddBuff(new Buff((short)((short)SecondData + buffLevel), mapMonster.Monster.Level));
                            }
                        }
                        else if (type == typeof(MapNpc))
                        {
                        }
                        else if (type == typeof(Mate))
                        {
                        }
                    }
                    break;

                case BCardType.CardType.Move:
                    {
                        if (type == typeof(Character) && session is Character character)
                        {
                            character.LastSpeedChange = DateTime.UtcNow;
                            character.Session.SendPacket(character.GenerateCond());
                        }
                    }
                    break;

                case BCardType.CardType.Summons:
                    {
                        if (type == typeof(Character))
                        {
                            // jajamaru spawn
                            if (session is Character character && character.MapInstance != null)
                            {
                                List<MonsterToSummon> summonParameters = new List<MonsterToSummon>();
                                for (int i = 0; i < FirstData; i++)
                                {
                                    short x, y;
                                    byte t = 0;
                                    do
                                    {
                                        x = (short) (ServerManager.RandomNumber(-3, 3) + character.PositionX);
                                        y = (short) (ServerManager.RandomNumber(-3, 3) + character.PositionY);
                                        t++;
                                    } while (!character.MapInstance.Map.IsBlockedZone(x, y) && t < byte.MaxValue);
                                    summonParameters.Add(new MonsterToSummon((short)SecondData, new MapCell { X = x, Y = y },
                                        -1, true));
                                }

                                if (ServerManager.RandomNumber() <= Math.Abs(ThirdData) || ThirdData == 0)
                                {
                                    switch (SubType)
                                    {
                                        case 2:
                                            EventHelper.Instance.RunEvent(new EventContainer(
                                                character.Session.CurrentMapInstance, EventActionType.SpawnMonsters,
                                                summonParameters));
                                            break;
                                    }
                                }
                            }
                        }
                        else if (type == typeof(MapMonster))
                        {
                            if (session is MapMonster mapMonster && mapMonster.MapInstance != null)
                            {
                                List<MonsterToSummon> summonParameters = new List<MonsterToSummon>();
                                for (int i = 0; i < FirstData; i++)
                                {
                                    short x, y;
                                    byte t = 0;
                                    do
                                    {
                                        x = (short) (ServerManager.RandomNumber(-3, 3) + mapMonster.MapX);
                                        y = (short) (ServerManager.RandomNumber(-3, 3) + mapMonster.MapY);
                                        t++;
                                    } while (!mapMonster.MapInstance.Map.IsBlockedZone(x, y) && t < byte.MaxValue);
                                    summonParameters.Add(new MonsterToSummon((short)SecondData, new MapCell { X = x, Y = y },
                                        -1, true));
                                }

                                if (ServerManager.RandomNumber() <= Math.Abs(ThirdData) || ThirdData == 0)
                                {
                                    switch (SubType)
                                    {
                                        case 2:
                                            EventHelper.Instance.RunEvent(new EventContainer(mapMonster.MapInstance,
                                                EventActionType.SpawnMonsters, summonParameters));
                                            break;

                                        default:
                                            if (mapMonster.OnDeathEvents.All(s =>
                                                s.EventActionType != EventActionType.SpawnMonsters))
                                            {
                                                mapMonster.OnDeathEvents.Add(new EventContainer(mapMonster.MapInstance,
                                                    EventActionType.SpawnMonsters, summonParameters));
                                            }

                                            break;
                                    }
                                }
                            }
                        }
                        else if (type == typeof(MapNpc))
                        {
                        }
                        else if (type == typeof(Mate))
                        {
                        }
                    }
                    break;

                case BCardType.CardType.SpecialAttack:
                    break;

                case BCardType.CardType.SpecialDefence:
                    break;

                case BCardType.CardType.AttackPower:
                    break;

                case BCardType.CardType.Target:
                    break;

                case BCardType.CardType.Critical:
                    break;

                case BCardType.CardType.SpecialCritical:
                    break;

                case BCardType.CardType.Element:
                    break;

                case BCardType.CardType.IncreaseDamage:
                    break;

                case BCardType.CardType.Defence:
                    break;

                case BCardType.CardType.DodgeAndDefencePercent:
                    break;

                case BCardType.CardType.Block:
                    break;

                case BCardType.CardType.Absorption:
                    break;

                case BCardType.CardType.ElementResistance:
                    break;

                case BCardType.CardType.EnemyElementResistance:
                    break;

                case BCardType.CardType.Damage:
                    break;

                case BCardType.CardType.GuarantedDodgeRangedAttack:
                    break;

                case BCardType.CardType.Morale:
                    break;

                case BCardType.CardType.Casting:
                    break;

                case BCardType.CardType.Reflection:
                    break;

                case BCardType.CardType.DrainAndSteal:
                    break;

                case BCardType.CardType.HealingBurningAndCasting:
                    {
                        if (type == typeof(Character))
                        {
                            if (session is Character character && character.Hp > 0)
                            {
                                int bonus;
                                if (SubType == (byte)AdditionalTypes.HealingBurningAndCasting.RestoreHP / 10)
                                {
                                    if (IsLevelScaled)
                                    {
                                        bonus = character.Level * FirstData;
                                    }
                                    else
                                    {
                                        bonus = FirstData;
                                    }

                                    if (character.Hp + bonus <= character.HPLoad())
                                    {
                                        character.Hp += bonus;
                                    }
                                    else
                                    {
                                        bonus = (int)character.HPLoad() - character.Hp;
                                        character.Hp = (int)character.HPLoad();
                                    }

                                    character.Session.CurrentMapInstance?.Broadcast(character.Session,
                                        character.GenerateRc(bonus));
                                }

                                if (SubType == (byte)AdditionalTypes.HealingBurningAndCasting.RestoreMP / 10)
                                {
                                    if (IsLevelScaled)
                                    {
                                        bonus = character.Level * FirstData;
                                    }
                                    else
                                    {
                                        bonus = FirstData;
                                    }

                                    if (character.Mp + bonus <= character.MPLoad())
                                    {
                                        character.Mp += bonus;
                                    }
                                    else
                                    {
                                        character.Mp = (int)character.MPLoad();
                                    }
                                }

                                character.Session.SendPacket(character.GenerateStat());
                            }
                        }
                        else if (type == typeof(MapMonster))
                        {
                            if (ServerManager.RandomNumber() < FirstData && session is MapMonster mapMonster)
                            {
                                mapMonster.AddBuff(new Buff((short)SecondData, mapMonster.Monster.Level));
                            }
                        }
                        else if (type == typeof(MapNpc))
                        {
                        }
                        else if (type == typeof(Mate))
                        {
                        }
                    }
                    break;

                case BCardType.CardType.Hpmp:
                    if (type == typeof(Character))
                    {
                        if (session is Character c)
                        {
                            switch (SubType)
                            {
                                case (byte)AdditionalTypes.Hpmp.RestoreDecreasedMP / 10:
                                    {
                                        int bonus = (int)(FirstData * c.MPLoad() / 100);

                                        c.Mp = c.Mp + bonus > 1 ? c.Mp + bonus : c.Mp = 1;
                                    }
                                    break;
                            }
                        }
                    }

                    break;

                case BCardType.CardType.SpecialisationBuffResistance:
                    {
                        switch (SubType)
                        {
                            case (byte)AdditionalTypes.SpecialisationBuffResistance.RemoveBadEffects / 10:
                                {
                                    if (session is Character c /*&& sender is ClientSession senderSession*/
                                        && ServerManager.RandomNumber() < Math.Abs(FirstData))
                                    {
                                        c.DisableBuffs(FirstData > 0 ? BuffType.Good : BuffType.Bad, SecondData);
                                    }
                                }
                                break;
                        }
                    }
                    break;

                case BCardType.CardType.SpecialEffects:
                    break;

                case BCardType.CardType.Capture:
                    {
                        if (type == typeof(MapMonster) && session is MapMonster mapMonster
                            && sender is ClientSession senderSession)
                        {
                            NpcMonster mateNpc = ServerManager.GetNpcMonster(mapMonster.MonsterVNum);
                            if (mateNpc != null)
                            {
                                if (mapMonster.Monster.Catch)
                                {
                                    if (mapMonster.IsAlive
                                        && mapMonster.CurrentHp <= (int)((double)mapMonster.MaxHp / 2))
                                    {
                                        if (mapMonster.Monster.Level < senderSession.Character.Level)
                                        {
                                            // TODO: find a new algorithm
                                            int[] chance = { 100, 80, 60, 40, 20, 0 };
                                            if (ServerManager.RandomNumber() < chance[ServerManager.RandomNumber(0, 5)])
                                            {
                                                Mate mate = new Mate(senderSession.Character, mateNpc,
                                                    (byte)(mapMonster.Monster.Level - 15 > 0
                                                        ? mapMonster.Monster.Level - 15
                                                        : 1), MateType.Pet);
                                                if (senderSession.Character.CanAddMate(mate))
                                                {
                                                    senderSession.Character.AddPetWithSkill(mate);
                                                    senderSession.SendPacket(
                                                        UserInterfaceHelper.GenerateMsg(
                                                            Language.Instance.GetMessageFromKey("CATCH_SUCCESS"), 0));
                                                    senderSession.CurrentMapInstance?.Broadcast(
                                                        StaticPacketHelper.GenerateEff(UserType.Player,
                                                            senderSession.Character.CharacterId, 197));
                                                    senderSession.CurrentMapInstance?.Broadcast(
                                                        StaticPacketHelper.SkillUsed(UserType.Player,
                                                            senderSession.Character.CharacterId, 3,
                                                            mapMonster.MapMonsterId, -1, 0, 15, -1, -1, -1, true,
                                                            (int)(mapMonster.CurrentHp / (float)mapMonster.MaxHp * 100
                                                            ), 0, -1, 0));
                                                    mapMonster.SetDeathStatement();
                                                    senderSession.CurrentMapInstance?.Broadcast(
                                                        StaticPacketHelper.Out(UserType.Monster,
                                                            mapMonster.MapMonsterId));
                                                }
                                                else
                                                {
                                                    senderSession.SendPacket(
                                                        senderSession.Character.GenerateSay(
                                                            Language.Instance.GetMessageFromKey("PET_SLOT_FULL"), 10));
                                                    senderSession.SendPacket(
                                                        StaticPacketHelper.Cancel(2, mapMonster.MapMonsterId));
                                                }
                                            }
                                            else
                                            {
                                                senderSession.SendPacket(
                                                    UserInterfaceHelper.GenerateMsg(
                                                        Language.Instance.GetMessageFromKey("CATCH_FAIL"), 0));
                                                senderSession.CurrentMapInstance?.Broadcast(
                                                    StaticPacketHelper.SkillUsed(UserType.Player,
                                                        senderSession.Character.CharacterId, 3, mapMonster.MapMonsterId,
                                                        -1, 0, 15, -1, -1, -1, true,
                                                        (int)(mapMonster.CurrentHp / (float)mapMonster.MaxHp * 100),
                                                        0, -1, 0));
                                            }
                                        }
                                        else
                                        {
                                            senderSession.SendPacket(UserInterfaceHelper.GenerateMsg(
                                                Language.Instance.GetMessageFromKey("LEVEL_LOWER_THAN_MONSTER"), 0));
                                            senderSession.SendPacket(
                                                StaticPacketHelper.Cancel(2, mapMonster.MapMonsterId));
                                        }
                                    }
                                    else
                                    {
                                        senderSession.SendPacket(UserInterfaceHelper.GenerateMsg(
                                            Language.Instance.GetMessageFromKey("CURRENT_HP_TOO_HIGH"), 0));
                                        senderSession.SendPacket(StaticPacketHelper.Cancel(2, mapMonster.MapMonsterId));
                                    }
                                }
                                else
                                {
                                    senderSession.SendPacket(UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("MONSTER_CANT_BE_CAPTURED"), 0));
                                    senderSession.SendPacket(StaticPacketHelper.Cancel(2, mapMonster.MapMonsterId));
                                }
                            }
                        }
                    }
                    break;

                case BCardType.CardType.SpecialDamageAndExplosions:
                    break;

                case BCardType.CardType.SpecialEffects2:
                    if (type == typeof(Character))
                    {
                        if (session is Character c)
                        {
                            short destinationX = c.PositionX;
                            short destinationY = c.PositionY;
                            switch (SubType)
                            {
                                case (byte)AdditionalTypes.SpecialEffects2.TeleportInRadius / 10:
                                    {
                                        switch (c.Direction)
                                        {
                                            case 0:
                                                // -y
                                                destinationY -= (short)FirstData;
                                                break;
                                            case 1:
                                                // +x
                                                destinationX += (short)FirstData;
                                                break;
                                            case 2:
                                                // +y
                                                destinationY += (short)FirstData;
                                                break;
                                            case 3:
                                                // -x
                                                destinationX -= (short)FirstData;
                                                break;
                                            case 4:
                                                // -x -y
                                                destinationX -= (short)FirstData;
                                                destinationY -= (short)FirstData;
                                                break;
                                            case 5:
                                                // +x +y
                                                destinationX += (short)FirstData;
                                                destinationY += (short)FirstData;
                                                break;
                                            case 6:
                                                // +x -y
                                                destinationX += (short)FirstData;
                                                destinationY -= (short)FirstData;
                                                break;
                                            case 7:
                                                // -x +y
                                                destinationX -= (short)FirstData;
                                                destinationY += (short)FirstData;
                                                break;
                                        }

                                        ServerManager.Instance.TeleportForward(c.Session, c.MapInstanceId, destinationX,
                                            destinationY);
                                    }
                                    break;
                            }
                        }
                    }

                    break;

                case BCardType.CardType.CalculatingLevel:
                    break;

                case BCardType.CardType.Recovery:
                    break;

                case BCardType.CardType.MaxHpmp:
                    break;

                case BCardType.CardType.MultAttack:
                    break;

                case BCardType.CardType.MultDefence:
                    break;

                case BCardType.CardType.TimeCircleSkills:
                    break;

                case BCardType.CardType.RecoveryAndDamagePercent:
                    break;

                case BCardType.CardType.Count:
                    break;

                case BCardType.CardType.NoDefeatAndNoDamage:
                    break;

                case BCardType.CardType.SpecialActions:
                    {
                        switch (SubType)
                        {
                            case (byte)AdditionalTypes.SpecialActions.RunAway / 10:
                                if (session is MapMonster m)
                                {
                                    m.MapInstance.Broadcast(StaticPacketHelper.Say(UserType.Monster, m.MapMonsterId, 0, "!!!"));
                                    m.RemoveTarget();
                                    for (int i = 0; i < 10; i++)
                                    {
                                        Observable.Timer(TimeSpan.FromSeconds(i)).Subscribe(o =>
                                        {
                                            m.MapX++;
                                            m.MapY++;
                                            m.IgnoreTargetsUntil = DateTime.UtcNow.AddSeconds(10);
                                            m.RemoveTarget();
                                        });
                                    }
                                }
                                break;
                        }
                    }
                    break;

                case BCardType.CardType.Mode:
                    break;

                case BCardType.CardType.NoCharacteristicValue:
                    break;

                case BCardType.CardType.LightAndShadow:
                    {
                        switch (SubType)
                        {
                            case (byte)AdditionalTypes.LightAndShadow.RemoveBadEffects / 10:
                                {
                                    if (session is Character c /*&& sender is ClientSession senderSession*/)
                                    {
                                        c.DisableBuffs(BuffType.Bad, FirstData);
                                    }
                                }
                                break;
                        }
                    }
                    break;

                case BCardType.CardType.Item:
                    break;

                case BCardType.CardType.DebuffResistance:
                    break;

                case BCardType.CardType.SpecialBehaviour:
                    break;

                case BCardType.CardType.Quest:
                    break;

                case BCardType.CardType.SecondSpCard:
                    break;

                case BCardType.CardType.SpCardUpgrade:
                    break;

                case BCardType.CardType.HugeSnowman:
                    break;

                case BCardType.CardType.Drain:
                    break;

                case BCardType.CardType.BossMonstersSkill:
                    break;

                case BCardType.CardType.LordHatus:
                    break;

                case BCardType.CardType.LordCalvinas:
                    break;

                case BCardType.CardType.SeSpecialist:
                    break;

                case BCardType.CardType.FourthGlacernonFamilyRaid:
                    break;

                case BCardType.CardType.SummonedMonsterAttack:
                    break;

                case BCardType.CardType.BearSpirit:
                    break;

                case BCardType.CardType.SummonSkill:
                    break;

                case BCardType.CardType.InflictSkill:
                    break;

                case BCardType.CardType.HideBarrelSkill:
                    break;

                case BCardType.CardType.FocusEnemyAttentionSkill:
                    break;

                case BCardType.CardType.TauntSkill:
                    break;

                case BCardType.CardType.FireCannoneerRangeBuff:
                    break;

                case BCardType.CardType.VulcanoElementBuff:
                    break;

                case BCardType.CardType.DamageConvertingSkill:
                    break;

                case BCardType.CardType.MeditationSkill:
                    {
                        if (type == typeof(Character) && session is Character character)
                        {
                            if (SkillVNum.HasValue
                                && SubType.Equals((byte)AdditionalTypes.MeditationSkill.CausingChance / 10)
                                && ServerManager.RandomNumber() < FirstData)
                            {
                                Skill skill = ServerManager.GetSkill(SkillVNum.Value);
                                Skill newSkill = ServerManager.GetSkill((short)SecondData);
                                Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(observer =>
                                {
                                    foreach (QuicklistEntryDTO quicklistEntry in character.QuicklistEntries.Where(s =>
                                        s.Morph == character.Morph && s.Pos.Equals(skill.CastId)))
                                    {
                                        character.Session.SendPacket(
                                            $"qset {quicklistEntry.Q1} {quicklistEntry.Q2} {quicklistEntry.Type}.{quicklistEntry.Slot}.{newSkill.CastId}.0");
                                    }

                                    character.Session.SendPacket($"mslot {newSkill.CastId} -1");
                                });
                                character.SkillComboCount++;
                                character.LastSkillComboUse = DateTime.UtcNow;
                                if (skill.CastId > 10)
                                {
                                    // HACK this way
                                    Observable.Timer(TimeSpan.FromMilliseconds((skill.Cooldown * 100) + 500))
                                        .Subscribe(observer =>
                                            character.Session.SendPacket(StaticPacketHelper.SkillReset(skill.CastId)));
                                }
                            }

                            switch (SubType)
                            {
                                case 2:
                                    character.MeditationDictionary[(short)SecondData] = DateTime.UtcNow.AddSeconds(4);
                                    break;

                                case 3:
                                    character.MeditationDictionary[(short)SecondData] = DateTime.UtcNow.AddSeconds(8);
                                    break;

                                case 4:
                                    character.MeditationDictionary[(short)SecondData] = DateTime.UtcNow.AddSeconds(12);
                                    break;
                            }
                        }
                    }
                    break;

                case BCardType.CardType.FalconSkill:
                    {
                        if (session is Character c)
                        {
                            switch (SubType)
                            {
                                case (byte)AdditionalTypes.FalconSkill.Hide:
                                    c.Invisible = true;
                                    c.Mates.Where(s => s.IsTeamMember).ToList().ForEach(s =>
                                        c.Session.CurrentMapInstance?.Broadcast(s.GenerateOut()));
                                    c.Session.CurrentMapInstance?.Broadcast(c.GenerateInvisible());
                                    c.Session.SendPacket(c.GenerateEq());
                                    break;
                            }
                        }
                    }
                    break;

                case BCardType.CardType.AbsorptionAndPowerSkill:
                    break;

                case BCardType.CardType.LeonaPassiveSkill:
                    break;

                case BCardType.CardType.FearSkill:
                    break;

                case BCardType.CardType.SniperAttack:
                    break;

                case BCardType.CardType.FrozenDebuff:
                    break;

                case BCardType.CardType.JumpBackPush:
                    break;

                case BCardType.CardType.FairyXPIncrease:
                    break;

                case BCardType.CardType.SummonAndRecoverHP:
                    break;

                case BCardType.CardType.TeamArenaBuff:
                    break;

                case BCardType.CardType.ArenaCamera:
                    break;

                case BCardType.CardType.DarkCloneSummon:
                    break;

                case BCardType.CardType.AbsorbedSpirit:
                    break;

                case BCardType.CardType.AngerSkill:
                    break;

                case BCardType.CardType.MeteoriteTeleport:
                    if (type == typeof(Character))
                    {
                        if (session is Character character)
                        {
                            switch (SubType)
                            {
                                case ((byte)AdditionalTypes.MeteoriteTeleport.TeleportYouAndGroupToSavedLocation / 10):
                                    if (character.TeleportSet == false)
                                    {
                                        character.TeleportX = character.PositionX;
                                        character.TeleportY = character.PositionY;
                                        character.TeleportMap = character.MapId;
                                        character.MapInstance.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, character.CharacterId, 4497));
                                        character.TeleportSet = true;
                                        Observable.Timer(TimeSpan.FromSeconds(40)).Subscribe(o => character.TeleportSet = false);
                                    }
                                    else
                                    {
                                        if (character.MapInstance.IsPvp)
                                        {
                                            if (character.MapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
                                            {
                                                character.TeleportSet = false;
                                                return;
                                            }

                                            character.MapInstance.Broadcast($"tp 1 {character.CharacterId} {character.TeleportX} {character.TeleportY} 0");
                                            character.PositionX = character.TeleportX;
                                            character.PositionY = character.TeleportY;
                                            character.TeleportSet = false;
                                        }
                                        else
                                        {
                                            if (character.MapId == character.TeleportMap)
                                            {
                                                if (character.MapInstance.MapInstanceType == MapInstanceType.CaligorInstance)
                                                {
                                                    character.TeleportSet = false;
                                                    return;
                                                }
                                                character.MapInstance.Broadcast($"tp 1 {character.CharacterId} {character.TeleportX} {character.TeleportY} 0");
                                                character.PositionX = character.TeleportX;
                                                character.PositionY = character.TeleportY;
                                                character.TeleportSet = false;
                                            }
                                            else
                                            {
                                                character.TeleportSet = false;
                                            }
                                        }
                                    }
                                    break;
                                case (byte)AdditionalTypes.MeteoriteTeleport.CauseMeteoriteFall / 10:
                                    {
                                        int amount = 10 + (character.Level / FirstData);

                                        for (int i = 0; i < amount; i++)
                                        {
                                            Observable.Timer(TimeSpan.FromMilliseconds(i * 500)).Subscribe(o => character.SpawnMeteorite());
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    break;

                case BCardType.CardType.StealBuff:
                    break;

                case BCardType.CardType.Unknown:
                    break;

                case BCardType.CardType.EffectSummon:
                    break;

                default:
                    Logger.Warn($"Card Type {Type} not defined!");
                    break;
            }
        }

        #endregion
    }
}