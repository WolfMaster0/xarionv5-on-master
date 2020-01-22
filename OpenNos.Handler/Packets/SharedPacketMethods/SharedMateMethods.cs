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
using OpenNos.GameObject.Battle;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OpenNos.GameObject.Event.ACT4;

namespace OpenNos.Handler.Packets.SharedPacketMethods
{
    internal static class SharedMateMethods
    {
        #region Methods

        internal static void AttackCharacter(this ClientSession session, Mate attacker, Skill skil, Character target)
        {
            if (attacker == null || target == null)
            {
                return;
            }

            if (skil == null)
            {
                skil = ServerManager.GetSkill(200);
            }

            if (target.Hp > 0 && attacker.IsAlive)
            {
                if ((session.CurrentMapInstance.MapInstanceId == ServerManager.Instance.ArenaInstance.MapInstanceId
                     || session.CurrentMapInstance.MapInstanceId
                     == ServerManager.Instance.FamilyArenaInstance.MapInstanceId)
                    && (session.CurrentMapInstance.Map.JaggedGrid[session.Character.PositionX][
                            session.Character.PositionY]?.Value != 0
                        || target.Session.CurrentMapInstance.Map.JaggedGrid[target.PositionX][
                                target.PositionY]
                            ?.Value != 0))
                {
                    // User in SafeZone
                    session.SendPacket(StaticPacketHelper.Cancel(2, target.CharacterId));
                    return;
                }

                if (target.IsSitting)
                {
                    target.Rest();
                }

                int hitmode = 0;
                bool onyxWings = false;
                attacker.LastSkillUse = DateTime.UtcNow;
                BattleEntity battleEntity = new BattleEntity(attacker);
                BattleEntity battleEntityDefense = new BattleEntity(target, null);
                session.CurrentMapInstance?.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Npc,
                    attacker.MateTransportId, 1, target.CharacterId, skil.CastAnimation, skil.CastEffect, skil.SkillVNum));
                int damage = DamageHelper.Instance.CalculateDamage(battleEntity, battleEntityDefense, skil,
                    ref hitmode, ref onyxWings);
                if (target.HasGodMode)
                {
                    damage = 0;
                    hitmode = 1;
                }
                else if (target.LastPvpRevive > DateTime.UtcNow.AddSeconds(-10)
                         || session.Character.LastPvpRevive > DateTime.UtcNow.AddSeconds(-10))
                {
                    damage = 0;
                    hitmode = 1;
                }

                int[] manaShield = target.GetBuff(BCardType.CardType.LightAndShadow,
                    (byte)AdditionalTypes.LightAndShadow.InflictDamageToMP);
                if (manaShield[0] != 0 && hitmode != 1)
                {
                    int reduce = damage / 100 * manaShield[0];
                    if (target.Mp < reduce)
                    {
                        target.Mp = 0;
                    }
                    else
                    {
                        target.Mp -= reduce;
                    }
                }

                target.GetDamage(damage / 2);
                target.LastDefence = DateTime.UtcNow;
                target.Session.SendPacket(target.GenerateStat());
                bool isAlive = target.Hp > 0;
                if (!isAlive && target.Session.HasCurrentMapInstance)
                {
                    if (target.Session.CurrentMapInstance.Map?.MapTypes.Any(
                            s => s.MapTypeId == (short)MapTypeEnum.Act4)
                        == true)
                    {
                        if (ServerManager.Instance.ChannelId == 51 && ServerManager.Instance.Act4DemonStat.Mode == 0
                                                                   && ServerManager.Instance.Act4AngelStat.Mode == 0)
                        {
                            switch (session.Character.Faction)
                            {
                                case FactionType.Angel:
                                    ServerManager.Instance.Act4AngelStat.Percentage += 500;
                                    break;

                                case FactionType.Demon:
                                    ServerManager.Instance.Act4DemonStat.Percentage += 500;
                                    break;
                            }
                        }

                        session.Character.Act4Kill++;
                        target.Act4Dead++;
                        target.GetAct4Points(-1);
                        if (target.Level + 20 >= session.Character.Level
                            && session.Character.Level <= target.Level - 20)
                        {
                            session.Character.GetAct4Points(2);
                        }

                        if (target.Reputation < 9999999999)
                        {
                            target.Session.SendPacket(session.Character.GenerateSay(
                                string.Format(Language.Instance.GetMessageFromKey("LOSE_REP"), 0), 11));
                        }
                        else
                        {
                            target.Reputation -= target.Level * 0;
                            session.Character.Reputation += target.Level * 150;
                            session.SendPacket(session.Character.GenerateLev());
                            target.Session.SendPacket(target.GenerateSay(
                                string.Format(Language.Instance.GetMessageFromKey("LOSE_REP"),
                                    (short)(target.Level * 0)), 11));
                        }

                        foreach (ClientSession sess in ServerManager.Instance.Sessions.Where(
                            s => s.HasSelectedCharacter))
                        {
                            if (sess.Character.Faction == session.Character.Faction)
                            {
                                sess.SendPacket(sess.Character.GenerateSay(
                                    string.Format(
                                        Language.Instance.GetMessageFromKey(
                                            $"ACT4_PVP_KILL{(int)target.Faction}"), session.Character.Name),
                                    12));
                            }
                            else if (sess.Character.Faction == target.Faction)
                            {
                                sess.SendPacket(sess.Character.GenerateSay(
                                    string.Format(
                                        Language.Instance.GetMessageFromKey(
                                            $"ACT4_PVP_DEATH{(int)target.Faction}"), target.Name),
                                    11));
                            }
                        }

                        target.Session.SendPacket(target.GenerateFd());
                        target.DisableBuffs(BuffType.All, force: true);
                        target.Session.CurrentMapInstance.Broadcast(target.Session, target.GenerateIn(),
                            ReceiverType.AllExceptMe);
                        target.Session.CurrentMapInstance.Broadcast(target.Session, target.GenerateGidx(),
                            ReceiverType.AllExceptMe);
                        target.Session.SendPacket(
                            target.GenerateSay(Language.Instance.GetMessageFromKey("ACT4_PVP_DIE"), 11));
                        target.Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ACT4_PVP_DIE"), 0));
                        if (target.MapInstanceId == CaligorRaid.CaligorMapInstance?.MapInstanceId)
                        {
                            target.Hp = (int)target.HPLoad();
                            target.Mp = (int)target.MPLoad();
                            if (target.Faction == FactionType.Angel)
                            {
                                ServerManager.Instance.ChangeMapInstance(target.CharacterId, CaligorRaid.CaligorMapInstance.MapInstanceId, 70, 159);
                            }
                            else if (target.Faction == FactionType.Demon)
                            {
                                ServerManager.Instance.ChangeMapInstance(target.CharacterId, CaligorRaid.CaligorMapInstance.MapInstanceId, 110, 159);
                            }

                            target.Session.CurrentMapInstance?.Broadcast(target.Session, target.GenerateTp());
                            target.Session.CurrentMapInstance?.Broadcast(target.GenerateRevive());
                            target.Session.SendPacket(target.GenerateStat());
                            return;
                        }
                        Observable.Timer(TimeSpan.FromMilliseconds(2000)).Subscribe(o =>
                        {
                            target.Session.CurrentMapInstance?.Broadcast(target.Session,
                                $"c_mode 1 {target.CharacterId} 1564 0 0 0");
                            target.Session.CurrentMapInstance?.Broadcast(target.GenerateRevive());
                        });
                        Observable.Timer(TimeSpan.FromMilliseconds(30000)).Subscribe(o =>
                        {
                            target.Hp = (int)target.HPLoad();
                            target.Mp = (int)target.MPLoad();
                            short x = (short)(39 + ServerManager.RandomNumber(-2, 3));
                            short y = (short)(42 + ServerManager.RandomNumber(-2, 3));
                            if (target.Faction == FactionType.Angel)
                            {
                                ServerManager.Instance.ChangeMap(target.CharacterId, 130, x, y);
                            }
                            else if (target.Faction == FactionType.Demon)
                            {
                                ServerManager.Instance.ChangeMap(target.CharacterId, 131, x, y);
                            }
                            else
                            {
                                target.MapId = 145;
                                target.MapX = 51;
                                target.MapY = 41;
                                string connection =
                                    CommunicationServiceClient.Instance.RetrieveOriginWorld(session.Account.AccountId);
                                if (string.IsNullOrWhiteSpace(connection))
                                {
                                    return;
                                }

                                int port = Convert.ToInt32(connection.Split(':')[1]);
                                session.Character.ChangeChannel(connection.Split(':')[0], port, 3);
                                return;
                            }

                            target.Session.CurrentMapInstance?.Broadcast(target.Session, target.GenerateTp());
                            target.Session.CurrentMapInstance?.Broadcast(target.GenerateRevive());
                            target.Session.SendPacket(target.GenerateStat());
                        });
                    }
                    else
                    {
                        session.Character.TalentWin++;
                        target.TalentLose++;
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateSay(
                            string.Format(Language.Instance.GetMessageFromKey("PVP_KILL"),
                                session.Character.Name, target.Name), 10));
                        Observable.Timer(TimeSpan.FromMilliseconds(1000)).Subscribe(o =>
                            ServerManager.Instance.AskPvpRevive(target.CharacterId));
                    }
                }

                if (hitmode != 1)
                {
                    skil.BCards.Where(s => s.Type.Equals((byte)BCardType.CardType.Buff)).ToList()
                        .ForEach(s => s.ApplyBCards(target, session.Character));
                }

                session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Npc, attacker.MateTransportId, 5005));
                Observable.Timer(TimeSpan.FromMilliseconds(skil.CastTime)).Subscribe(o =>
                {
                    session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Npc,
                            attacker.MateTransportId, 1, target.CharacterId, skil.SkillVNum, skil.Cooldown,
                            skil.AttackAnimation, skil.Effect, 0, 0, target.Hp > 0,
                            (int)(target.Hp / (float)target.HPLoad() * 100), damage, hitmode, 0));
                });
                // switch (hitRequest.TargetHitType) { case TargetHitType.SingleTargetHit:
                // hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                // hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                // hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown,
                // hitRequest.Skill.AttackAnimation, hitRequest.SkillEffect,
                // hitRequest.Session.Character.PositionX, hitRequest.Session.Character.PositionY,
                // isAlive, (int) (target.Character.Hp / (float) target.Character.HPLoad() * 100),
                // damage, hitmode, (byte) (hitRequest.Skill.SkillType - 1))); break;
                //
                // case TargetHitType.SingleTargetHitCombo:
                // hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                // hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                // hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown,
                // hitRequest.SkillCombo.Animation, hitRequest.SkillCombo.Effect,
                // hitRequest.Session.Character.PositionX, hitRequest.Session.Character.PositionY,
                // isAlive, (int) (target.Character.Hp / (float) target.Character.HPLoad() * 100),
                // damage, hitmode, (byte) (hitRequest.Skill.SkillType - 1))); break;
                //
                // case TargetHitType.SingleAOETargetHit: switch (hitmode) { case 1: hitmode = 4; break;
                //
                // case 3: hitmode = 6; break;
                //
                // default: hitmode = 5; break; }
                //
                // if (hitRequest.ShowTargetHitAnimation) {
                // hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(
                // UserType.Player, hitRequest.Session.Character.CharacterId, 1,
                // target.Character.CharacterId, hitRequest.Skill.SkillVNum,
                // hitRequest.Skill.Cooldown, hitRequest.Skill.AttackAnimation,
                // hitRequest.SkillEffect, 0, 0, isAlive, (int) (target.Character.Hp / (float)
                // target.Character.HPLoad() * 100), 0, 0, (byte) (hitRequest.Skill.SkillType - 1))); }
                //
                // hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                // hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                // hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown,
                // hitRequest.Skill.AttackAnimation, hitRequest.SkillEffect,
                // hitRequest.Session.Character.PositionX, hitRequest.Session.Character.PositionY,
                // isAlive, (int) (target.Character.Hp / (float) target.Character.HPLoad() * 100),
                // damage, hitmode, (byte) (hitRequest.Skill.SkillType - 1))); break;
                //
                // case TargetHitType.AOETargetHit: switch (hitmode) { case 1: hitmode = 4; break;
                //
                // case 3: hitmode = 6; break;
                //
                // default: hitmode = 5; break; }
                //
                // hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                // hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                // hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown,
                // hitRequest.Skill.AttackAnimation, hitRequest.SkillEffect,
                // hitRequest.Session.Character.PositionX, hitRequest.Session.Character.PositionY,
                // isAlive, (int) (target.Character.Hp / (float) target.Character.HPLoad() * 100),
                // damage, hitmode, (byte) (hitRequest.Skill.SkillType - 1))); break;
                //
                // case TargetHitType.ZoneHit:
                // hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                // hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                // hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown,
                // hitRequest.Skill.AttackAnimation, hitRequest.SkillEffect, hitRequest.MapX,
                // hitRequest.MapY, isAlive, (int) (target.Character.Hp / (float)
                // target.Character.HPLoad() * 100), damage, 5, (byte) (hitRequest.Skill.SkillType -
                // 1))); break;
                //
                // case TargetHitType.SpecialZoneHit:
                // hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                // hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                // hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown,
                // hitRequest.Skill.AttackAnimation, hitRequest.SkillEffect,
                // hitRequest.Session.Character.PositionX, hitRequest.Session.Character.PositionY,
                // isAlive, (int) (target.Character.Hp / target.Character.HPLoad() * 100), damage, 0,
                // (byte) (hitRequest.Skill.SkillType - 1))); break;
                //
                // default: Logger.Warn("Not Implemented TargetHitType Handling!"); break; }
            }
            else
            {
                // player already has been killed, send cancel
                session.SendPacket(StaticPacketHelper.Cancel(2, target.CharacterId));
            }
        }

        internal static void AttackCharacter(this ClientSession session, Mate attacker, NpcMonsterSkill skill, Character target)
        {
            if (attacker == null || target == null)
            {
                return;
            }

            if (target.Hp > 0 && attacker.Hp > 0)
            {
                if ((session.CurrentMapInstance.MapInstanceId == ServerManager.Instance.ArenaInstance.MapInstanceId
                     || session.CurrentMapInstance.MapInstanceId
                     == ServerManager.Instance.FamilyArenaInstance.MapInstanceId)
                    && (session.CurrentMapInstance.Map.JaggedGrid[session.Character.PositionX][
                            session.Character.PositionY]?.Value != 0
                        || target.Session.CurrentMapInstance.Map.JaggedGrid[target.PositionX][
                                target.PositionY]
                            ?.Value != 0))
                {
                    // User in SafeZone
                    session.SendPacket(StaticPacketHelper.Cancel(2, target.CharacterId));
                    return;
                }

                if (target.IsSitting)
                {
                    target.Rest();
                }

                short castAnimation = -1;
                short castEffect = -1;
                short skillVnum = 0;
                short cooldown = 0;
                byte type = 0;

                if (skill != null)
                {
                    castAnimation = skill.Skill.CastAnimation;
                    castEffect = skill.Skill.CastEffect;
                    skillVnum = skill.SkillVNum;
                    cooldown = skill.Skill.Cooldown;
                    type = skill.Skill.Type;
                }

                var hitmode = 0;
                var onyxWings = false;
                BattleEntity battleEntity = new BattleEntity(attacker);
                BattleEntity battleEntityDefense = new BattleEntity(target);
                session.CurrentMapInstance?.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Npc,
                    attacker.MateTransportId, 3, target.CharacterId, castAnimation, castEffect, skillVnum));
                session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Npc, attacker.MateTransportId, 3, target.CharacterId, skillVnum, cooldown, castAnimation, castEffect, target.MapX, target.MapY, target.Hp > 0, (int)(target.Hp / (double)target.HPMax * 100), 0, 0, type));
                var damage = DamageHelper.Instance.CalculateDamage(battleEntity, battleEntityDefense, skill?.Skill,
                    ref hitmode, ref onyxWings);
                if (target.HasGodMode)
                {
                    damage = 0;
                    hitmode = 1;
                }
                else if (target.LastPvpRevive > DateTime.Now.AddSeconds(-10)
                         || session.Character.LastPvpRevive > DateTime.Now.AddSeconds(-10))
                {
                    damage = 0;
                    hitmode = 1;
                }

                int[] manaShield = target.GetBuff(BCardType.CardType.LightAndShadow,
                    (byte)AdditionalTypes.LightAndShadow.InflictDamageToMP);
                if (manaShield[0] != 0 && hitmode != 1)
                {
                    var reduce = damage / 100 * manaShield[0];
                    if (target.Mp < reduce)
                    {
                        target.Mp = 0;
                    }
                    else
                    {
                        target.Mp -= reduce;
                    }
                }

                target.GetDamage(damage / 2);
                target.LastDefence = DateTime.Now;
                target.Session.SendPacket(target.GenerateStat());
                var isAlive = target.Hp > 0;
                if (!isAlive && target.Session.HasCurrentMapInstance)
                {
                    if (target.Session.CurrentMapInstance.Map?.MapTypes.Any(
                            s => s.MapTypeId == (short)MapTypeEnum.Act4)
                        == true)
                    {
                        if (ServerManager.Instance.ChannelId == 51 && ServerManager.Instance.Act4DemonStat.Mode == 0
                                                                   && ServerManager.Instance.Act4AngelStat.Mode == 0)
                        {
                            switch (session.Character.Faction)
                            {
                                case FactionType.Angel:
                                    ServerManager.Instance.Act4AngelStat.Percentage += 100;
                                    break;

                                case FactionType.Demon:
                                    ServerManager.Instance.Act4DemonStat.Percentage += 100;
                                    break;
                            }
                        }

                        session.Character.Act4Kill++;
                        target.Act4Dead++;
                        target.GetAct4Points(-1);
                        if (target.Level + 10 >= session.Character.Level
                            && session.Character.Level <= target.Level - 10)
                        {
                            session.Character.GetAct4Points(2);
                        }

                        if (target.Reputation < 50000)
                        {
                            target.Session.SendPacket(session.Character.GenerateSay(
                                string.Format(Language.Instance.GetMessageFromKey("LOSE_REP"), 0), 11));
                        }
                        else
                        {
                            target.Reputation -= target.Level * 50;
                            session.Character.Reputation += target.Level * 50;
                            session.SendPacket(session.Character.GenerateLev());
                            target.Session.SendPacket(target.GenerateSay(
                                string.Format(Language.Instance.GetMessageFromKey("LOSE_REP"),
                                    (short)(target.Level * 50)), 11));
                        }

                        foreach (ClientSession sess in ServerManager.Instance.Sessions.Where(
                            s => s.HasSelectedCharacter))
                        {
                            if (sess.Character.Faction == session.Character.Faction)
                            {
                                sess.SendPacket(sess.Character.GenerateSay(
                                    string.Format(
                                        Language.Instance.GetMessageFromKey(
                                            $"ACT4_PVP_KILL{(int)target.Faction}"), session.Character.Name),
                                    12));
                            }
                            else if (sess.Character.Faction == target.Faction)
                            {
                                sess.SendPacket(sess.Character.GenerateSay(
                                    string.Format(
                                        Language.Instance.GetMessageFromKey(
                                            $"ACT4_PVP_DEATH{(int)target.Faction}"), target.Name),
                                    11));
                            }
                        }

                        target.Session.SendPacket(target.GenerateFd());
                        target.DisableBuffs(BuffType.All, force: true);
                        target.Session.CurrentMapInstance.Broadcast(target.Session, target.GenerateIn(),
                            ReceiverType.AllExceptMe);
                        target.Session.CurrentMapInstance.Broadcast(target.Session, target.GenerateGidx(),
                            ReceiverType.AllExceptMe);
                        target.Session.SendPacket(
                            target.GenerateSay(Language.Instance.GetMessageFromKey("ACT4_PVP_DIE"), 11));
                        target.Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ACT4_PVP_DIE"), 0));
                        Observable.Timer(TimeSpan.FromMilliseconds(2000)).Subscribe(o =>
                        {
                            target.Session.CurrentMapInstance?.Broadcast(target.Session,
                                $"c_mode 1 {target.CharacterId} 1564 0 0 0");
                            target.Session.CurrentMapInstance?.Broadcast(target.GenerateRevive());
                        });
                        Observable.Timer(TimeSpan.FromMilliseconds(30000)).Subscribe(o =>
                        {
                            target.Hp = (int)target.HPLoad();
                            target.Mp = (int)target.MPLoad();
                            var x = (short)(39 + ServerManager.RandomNumber<short>(-2, 3));
                            var y = (short)(42 + ServerManager.RandomNumber<short>(-2, 3));
                            switch (target.Faction)
                            {
                                case FactionType.Angel:
                                    ServerManager.Instance.ChangeMap(target.CharacterId, 130, x, y);
                                    break;
                                case FactionType.Demon:
                                    ServerManager.Instance.ChangeMap(target.CharacterId, 131, x, y);
                                    break;
                                default:
                                    {
                                        target.MapId = 145;
                                        target.MapX = 51;
                                        target.MapY = 41;
                                        var connection =
                                            CommunicationServiceClient.Instance.RetrieveOriginWorld(session.Account.AccountId);
                                        if (string.IsNullOrWhiteSpace(connection))
                                        {
                                            return;
                                        }

                                        var port = Convert.ToInt32(connection.Split(':')[1]);
                                        session.Character.ChangeChannel(connection.Split(':')[0], port, 3);
                                        return;
                                    }
                            }

                            target.Session.CurrentMapInstance?.Broadcast(target.Session, target.GenerateTp());
                            target.Session.CurrentMapInstance?.Broadcast(target.GenerateRevive());
                            target.Session.SendPacket(target.GenerateStat());
                        });
                    }
                }

                if (hitmode != 1)
                {
                    skill?.Skill?.BCards.Where(s => s.Type.Equals((byte)BCardType.CardType.Buff)).ToList()
                        .ForEach(s => s.ApplyBCards(target, session.Character));
                }

                session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Npc,
                    attacker.MateTransportId, 1, target.CharacterId, 0, 12, 11, 200, 0, 0, isAlive,
                    (int)(target.Hp / target.HPLoad() * 100), damage, hitmode, 0));
            }
            else
            {
                // monster already has been killed, send cancel
                session.SendPacket(StaticPacketHelper.Cancel(2, target.CharacterId));
            }

        }

        internal static void AttackMonster(this ClientSession session, Mate attacker, Skill skil, MapMonster target)
        {
            if (target == null || attacker == null || !target.IsAlive)
            {
                return;
            }

            if (skil == null)
            {
                skil = ServerManager.GetSkill(200);
            }

            short castAnimation = -1;
            short castEffect = -1;
            short skillVnum = 0;
            short cooldown = 0;
            byte type = 0;

            if (skil != null)
            {
                castAnimation = skil.CastAnimation;
                castEffect = skil.CastEffect;
                skillVnum = skil.SkillVNum;
                cooldown = (short)(skil.SkillVNum == 200 ? 0 : skil.Cooldown);
                type = skil.Type;
            }

            if (target.CurrentHp <= 0)
            {
                return;
            }

            session.CurrentMapInstance?.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Npc,
                attacker.MateTransportId, 3, target.MapMonsterId, castAnimation, castEffect, skillVnum));
            target.HitQueue.Enqueue(new HitRequest(TargetHitType.SingleTargetHit, session, attacker, skil));
            attacker.Owner?.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, attacker.MateTransportId, 5005), attacker.PositionX, attacker.PositionY);
        }

        internal static void AttackMonster(this ClientSession session, Mate attacker, Skill skill, long targetId, short x, short y)
        {
            if (skill?.MpCost > attacker.Mp)
            {
                return;
            }
            attacker.LastSkillUse = DateTime.Now;
            attacker.Mp -= skill?.MpCost ?? 0;
            if (skill?.TargetType == 1 && skill.HitType == 1)
            {
                if (!session.HasCurrentMapInstance || skill.TargetRange == 0)
                {
                    return;
                }

                //Probably some pvp stuff in here
                foreach (MapMonster mon in attacker.Owner.MapInstance.GetListMonsterInRange(attacker.PositionX, attacker.PositionY, skill.TargetRange).Where(s => s.CurrentHp > 0))
                {
                    mon.HitQueue.Enqueue(new HitRequest(TargetHitType.AoeTargetHit, session, attacker, skill));
                    session.CurrentMapInstance.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Npc, attacker.MateTransportId, 2, mon.MapMonsterId, skill.CastAnimation, skill.CastEffect, skill.SkillVNum));
                    session.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Npc, attacker.MateTransportId, 3, mon.MapMonsterId, skill.SkillVNum, skill.Cooldown, skill.CastAnimation, skill.CastEffect, mon.MapX, mon.MapY, mon.CurrentHp > 0, (int)(mon.CurrentHp / (double)mon.MaxHp * 100), 0, 0, skill.SkillType));
                }
            }
            else if (skill?.TargetType == 2 && skill.HitType == 0)
            {
                ClientSession target = attacker.Owner.Session ?? session;
                session.CurrentMapInstance.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Npc, attacker.MateTransportId, 2, targetId, skill.CastAnimation, skill.CastEffect, skill.SkillVNum));
                session.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Npc, attacker.MateTransportId, 3, targetId, skill.SkillVNum, skill.Cooldown, skill.CastAnimation, skill.CastEffect, x, y, true, 100, 0, 0, skill.SkillType));
                skill.BCards.ToList().ForEach(s =>
                {
                    // Apply skill bcards to owner and pet 
                    s.ApplyBCards(target.Character);
                    s.ApplyBCards(attacker);
                });
            }
            else if (skill?.TargetType == 1 && skill.HitType != 1)
            {
                session.CurrentMapInstance.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Npc, attacker.MateTransportId, 2, targetId, skill.CastAnimation, skill.CastEffect, skill.SkillVNum));
                session.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Npc, attacker.MateTransportId, 3, targetId, skill.SkillVNum, skill.Cooldown, skill.CastAnimation, skill.CastEffect, x, y, true, 100, 0, 0, skill.SkillType));
                switch (skill.HitType)
                {
                    case 2:
                        IEnumerable<MapMonster> entityInRange = session.Character.MapInstance?.GetListMonsterInRange(attacker.PositionX, attacker.PositionY, skill.TargetRange);
                        foreach (BCard sb in skill.BCards)
                        {
                            if (sb.Type != (short)BCardType.CardType.Buff)
                            {
                                continue;
                            }

                            Buff bf = new Buff((short)sb.SecondData);
                            if (bf.Card.BuffType != BuffType.Good)
                            {
                                continue;
                            }

                            var bonusBuff = 0;

                            if (attacker.SpInstance != null && attacker.IsUsingSp && sb.BuffCard?.CardId >= 2000)
                            {
                                if (attacker.SpInstance.FirstPartnerSkill == skill.SkillVNum)
                                {
                                    bonusBuff = (int)(attacker.SpInstance.FirstPartnerSkillRank - 1);
                                }
                                else if (attacker.SpInstance.SecondPartnerSkill == skill.SkillVNum)
                                {
                                    bonusBuff = (int)(attacker.SpInstance.SecondPartnerSkillRank - 1);
                                }
                                else if (attacker.SpInstance.ThirdPartnerSkill == skill.SkillVNum)
                                {
                                    bonusBuff = (int)(attacker.SpInstance.ThirdPartnerSkillRank - 1);
                                }
                            }

                            sb.ApplyBCards(attacker, buffLevel: (short)bonusBuff);
                            sb.ApplyBCards(attacker.Owner, buffLevel: (short)bonusBuff);
                        }

                        if (entityInRange != null)
                        {
                            foreach (var target in entityInRange)
                            {
                                foreach (BCard s in skill.BCards)
                                {
                                    if (s.Type != (short)BCardType.CardType.Buff)
                                    {
                                        s.ApplyBCards(target, attacker);
                                        continue;
                                    }

                                    switch (attacker.Owner.MapInstance.MapInstanceType)
                                    {
                                        default:
                                            s.ApplyBCards(target);
                                            break;
                                    }
                                }
                            }
                        }
                        break;
                    case 4:
                    case 0:
                        foreach (BCard bc in skill.BCards)
                        {
                            var bonusBuff = 0;

                            if (attacker.SpInstance != null && attacker.IsUsingSp && bc.BuffCard?.CardId >= 2000)
                            {
                                if (attacker.SpInstance.FirstPartnerSkill == skill.SkillVNum)
                                {
                                    bonusBuff = (int)(attacker.SpInstance.FirstPartnerSkillRank - 1);
                                }
                                else if (attacker.SpInstance.SecondPartnerSkill == skill.SkillVNum)
                                {
                                    bonusBuff = (int)(attacker.SpInstance.SecondPartnerSkillRank - 1);
                                }
                                else if (attacker.SpInstance.ThirdPartnerSkill == skill.SkillVNum)
                                {
                                    bonusBuff = (int)(attacker.SpInstance.ThirdPartnerSkillRank - 1);
                                }
                            }

                            if (bc.Type == (short)BCardType.CardType.Buff && bc.BuffCard?.BuffType == BuffType.Good)
                            {
                                bc.ApplyBCards(attacker, buffLevel: (short)bonusBuff);
                                bc.ApplyBCards(attacker.Owner, buffLevel: (short)bonusBuff);
                            }
                            else
                            {
                                bc.ApplyBCards(attacker);
                            }
                        }
                        break;
                }
            }
            else if (skill != null && skill.TargetType == 0 && session.HasCurrentMapInstance)
            {
                MapMonster monsterToAttack = attacker.Owner.MapInstance.GetMonster(targetId);
                if (monsterToAttack == null || attacker.Mp <= skill.MpCost)
                {
                    return;
                }

                if (Map.GetDistance(new MapCell { X = monsterToAttack.MapX, Y = monsterToAttack.MapY }, new MapCell { X = monsterToAttack.MapX, Y = monsterToAttack.MapY }) >=
                    skill.Range + 1 + monsterToAttack.Monster.BasicArea)
                {
                    return;
                }

                foreach (BCard bc in skill.BCards)
                {
                    var bf = new Buff((short)bc.SecondData);
                    if (bf.Card?.BuffType == BuffType.Bad || bf.Card?.BuffType == BuffType.Neutral)
                    {
                        bc.ApplyBCards(monsterToAttack, attacker);
                    }
                }

                session.SendPacket(attacker.GenerateStatInfo());

                if (skill.HitType == 3)
                {
                    monsterToAttack.HitQueue.Enqueue(new HitRequest(TargetHitType.SingleAoeTargetHit, session, attacker, skill));
                }
                else
                {
                    if (skill.TargetRange != 0)
                    {
                        IEnumerable<MapMonster> monstersInAorRange = attacker.Owner.MapInstance?.GetListMonsterInRange(monsterToAttack.MapX, monsterToAttack.MapY, skill.TargetRange);

                        monsterToAttack.HitQueue.Enqueue(new HitRequest(TargetHitType.SingleAoeTargetHit, session, attacker, skill));

                        if (monstersInAorRange != null)
                        {
                            foreach (MapMonster mon in monstersInAorRange)
                            {
                                mon.HitQueue.Enqueue(new HitRequest(TargetHitType.SingleAoeTargetHit, session, attacker, skill));
                            }
                        }
                    }
                    else
                    {
                        if (!monsterToAttack.IsAlive)
                        {
                            session.SendPacket("cancel 2 0");
                            return;
                        }

                        monsterToAttack.HitQueue.Enqueue(new HitRequest(TargetHitType.SingleAoeTargetHit, session, attacker, skill));
                    }
                }
            }
        }
        #endregion
    }
}