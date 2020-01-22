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
using OpenNos.GameObject;
using OpenNos.GameObject.Battle;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OpenNos.GameObject.EventArguments;
using static OpenNos.Domain.BCardType;

namespace OpenNos.Handler.Packets.SharedPacketMethods
{
    internal static class SharedBattleMethods
    {
        #region Methods

        internal static void TargetHit(this ClientSession session, int castingId, int targetId, bool isPvp = false, bool sendCoordinates = false)
        {
            bool shouldCancel = true;
            if ((DateTime.UtcNow - session.Character.LastTransform).TotalSeconds < 3)
            {
                session.SendPacket(StaticPacketHelper.Cancel());
                session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_ATTACK"),
                    0));
                return;
            }

            List<CharacterSkill> skills = session.Character.UseSp
                ? session.Character.SkillsSp?.GetAllItems()
                : session.Character.Skills?.GetAllItems();
            if (skills != null)
            {
                CharacterSkill
                    ski = skills.Find(s =>
                        s.Skill?.CastId
                        == castingId); // && (s.Skill?.UpgradeSkill == 0 || s.Skill?.UpgradeSkill == 3));
                if (castingId != 0)
                {
                    session.SendPacket("ms_c 0");
                }

                if (ski != null)
                {
                    if (!session.Character.WeaponLoaded(ski) || !ski.CanBeUsed())
                    {
                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        return;
                    }

                    foreach (BCard bc in ski.Skill.BCards.Where(s => s.Type.Equals((byte)CardType.MeditationSkill)))
                    {
                        shouldCancel = false;
                        bc.ApplyBCards(session.Character);
                    }

                    if (session.Character.Buff.FirstOrDefault(s => s.Card.BCards.Any(x =>
                        x.Type == (byte)CardType.SpecialActions
                        && x.SubType == (byte)AdditionalTypes.SpecialActions.Hide / 10)) is Buff buff)
                    {
                        // Don't disable invisibility on certain skills
                        if (ski.SkillVNum != 848 && ski.SkillVNum != 847 && ski.SkillVNum != 849 && ski.SkillVNum != 845)
                        {
                            session.Character.RemoveBuff(buff.Card.CardId);
                        }
                    }
                    if (session.Character.Buff.FirstOrDefault(s => s.Card.BCards.Any(x =>
                        x.Type == (byte)CardType.FalconSkill
                        && x.SubType == (byte)AdditionalTypes.FalconSkill.Hide / 10)) is Buff hide)
                    {
                        session.Character.AddBuff(new Buff(560, session.Character.Level));
                        session.Character.RemoveBuff(hide.Card.CardId);
                        session.Character.Invisible = true;
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateInvisible());
                    }

                    if (session.Character.Buff.FirstOrDefault(s => s.Card.BCards.Any(x =>
                    x.Type == (byte)CardType.HealingBurningAndCasting
                    && x.SubType == (byte)AdditionalTypes.HealingBurningAndCasting.HPDecreasedByConsumingMP / 10)) is Buff mpremove)
                    {
                        if (session.Character.Hp > 0)
                        {
                            session.Character.Hp = session.Character.Hp - ski.Skill.MpCost > 1 ? session.Character.Hp - ski.Skill.MpCost : session.Character.Hp = 1;
                            session.SendPacket(session.Character.GenerateStat());
                        }
                    }

                    if (session.Character.Mp >= ski.Skill.MpCost && session.HasCurrentMapInstance)
                    {
                        // AOE Target hit
                        if (ski.Skill.TargetType == 1 && ski.Skill.HitType == 1)
                        {
                            if (!session.Character.HasGodMode)
                            {
                                session.Character.Mp -= ski.Skill.MpCost;
                            }

                            if (session.Character.UseSp && ski.Skill.CastEffect != -1)
                            {
                                session.SendPackets(session.Character.GenerateQuicklist());
                            }

                            session.SendPacket(session.Character.GenerateStat());
                            CharacterSkill skillinfo = session.Character.Skills.FirstOrDefault(s =>
                                s.Skill.UpgradeSkill == ski.Skill.SkillVNum && s.Skill.Effect > 0
                                                                            && s.Skill.SkillType == 2);
                            session.CurrentMapInstance.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Player,
                                session.Character.CharacterId, 1, session.Character.CharacterId,
                                ski.Skill.CastAnimation, skillinfo?.Skill.CastEffect ?? ski.Skill.CastEffect,
                                ski.Skill.SkillVNum));

                            if (session.Character.MapInstance.IsPvp)
                            {
                                foreach (BCard bCard in ski.Skill.BCards)
                                {
                                    if (bCard.Type == 25 && bCard.SubType == 1)
                                    {
                                        Buff b = new Buff((short)bCard.SecondData, session.Character.Level);

                                        switch (b.Card.BuffType)
                                        {
                                            case BuffType.Good:
                                                ski.Skill.BCards.Where(s => !s.Type.Equals((byte)CardType.MeditationSkill)).ToList()
                                                         .ForEach(s => s.ApplyBCards(session.Character));
                                                break;
                                        }
                                    }
                                }
                            }

                            // Generate scp
                            ski.LastUse = DateTime.UtcNow;

                            Observable.Timer(ski.Skill.CastEffect != 0
                                    ? TimeSpan.FromMilliseconds(ski.Skill.CastTime * 100)
                                    : TimeSpan.Zero).Subscribe(observer =>
                                    TargetHitRunAoeTargetHit(session, ski, skillinfo, sendCoordinates, targetId));
                        }
                        else if (ski.Skill.TargetType == 2 && ski.Skill.HitType == 0)
                        {
                            if (!session.Character.HasGodMode)
                            {
                                session.Character.Mp -= ski.Skill.MpCost;
                            }

                            session.CurrentMapInstance?.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Player,
                                session.Character.CharacterId, 1, session.Character.CharacterId,
                                ski.Skill.CastAnimation, ski.Skill.CastEffect, ski.Skill.SkillVNum));
                            session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                                session.Character.CharacterId, 1, targetId, ski.Skill.SkillVNum, ski.Skill.Cooldown,
                                ski.Skill.AttackAnimation, ski.Skill.Effect, session.Character.PositionX,
                                session.Character.PositionY, true,
                                (int)(session.Character.Hp / (float)session.Character.HPLoad() * 100), 0, -1,
                                (byte)(ski.Skill.SkillType - 1)));
                            ClientSession target = ServerManager.Instance.GetSessionByCharacterId(targetId) ?? session;
                            if (session.Character.MapInstance.IsPvp)
                            {
                                if (session.Character.Group != null && target.Character.Group?.IsMemberOfGroup(session.Character.CharacterId) == true)
                                {
                                    foreach (BCard bCard in ski.Skill.BCards)
                                    {
                                        Buff b = new Buff((short)bCard.SecondData, session.Character.Level);

                                        switch (b.Card.BuffType)
                                        {
                                            case BuffType.Good:
                                                ski.Skill.BCards.Where(s => !s.Type.Equals((byte)CardType.MeditationSkill)).ToList()
                                                    .ForEach(s => s.ApplyBCards(target.Character, session.Character));
                                                ski.LastUse = DateTime.UtcNow;
                                                break;

                                            case BuffType.Bad:
                                                return;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (BCard bCard in ski.Skill.BCards)
                                    {
                                        Buff b = new Buff((short)bCard.SecondData, session.Character.Level);

                                        switch (b.Card.BuffType)
                                        {
                                            case BuffType.Good:
                                                ski.Skill.BCards.Where(s => !s.Type.Equals((byte)CardType.MeditationSkill)).ToList()
                                                    .ForEach(s => s.ApplyBCards(session.Character));
                                                ski.LastUse = DateTime.UtcNow;
                                                break;

                                            case BuffType.Bad:
                                                ski.Skill.BCards.Where(s => !s.Type.Equals((byte)CardType.MeditationSkill)).ToList()
                                                    .ForEach(s => s.ApplyBCards(target.Character, session.Character));
                                                ski.LastUse = DateTime.UtcNow;
                                                return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ski.Skill.BCards.Where(s => !s.Type.Equals((byte)CardType.MeditationSkill)).ToList()
                                .ForEach(s => s.ApplyBCards(target.Character, session.Character));
                                ski.LastUse = DateTime.UtcNow;
                            }
                        }
                        else if (ski.Skill.TargetType == 1 && ski.Skill.HitType != 1)
                        {
                            if (!session.Character.HasGodMode)
                            {
                                session.Character.Mp -= ski.Skill.MpCost;
                            }
                            session.CurrentMapInstance?.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Player,
                                session.Character.CharacterId, 1, session.Character.CharacterId,
                                ski.Skill.CastAnimation, ski.Skill.CastEffect, ski.Skill.SkillVNum));
                            session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                                session.Character.CharacterId, 1, session.Character.CharacterId, ski.Skill.SkillVNum,
                                ski.Skill.Cooldown, ski.Skill.AttackAnimation, ski.Skill.Effect,
                                sendCoordinates ? session.Character.PositionX : (short)0,
                                sendCoordinates ? session.Character.PositionY : (short)0, true,
                                (int)(session.Character.Hp / (float)session.Character.HPLoad() * 100), 0, -1,
                                (byte)(ski.Skill.SkillType - 1)));
                            switch (ski.Skill.HitType)
                            {
                                case 2:
                                    IEnumerable<ClientSession> clientSessions =
                                        session.CurrentMapInstance?.Sessions?.Where(s =>
                                            s.Character.IsInRange(session.Character.PositionX,
                                                session.Character.PositionY, ski.Skill.TargetRange));
                                    if (clientSessions != null)
                                    {
                                        if (session.Character.MapInstance.IsPvp)
                                        {
                                            foreach (ClientSession target in clientSessions)
                                            {
                                                if (session.Character.Group != null && target.Character.Group?.IsMemberOfGroup(session.Character.CharacterId) == true)
                                                {
                                                    foreach (BCard bCard in ski.Skill.BCards)
                                                    {
                                                        Buff b = new Buff((short)bCard.SecondData, session.Character.Level);

                                                        switch (b.Card.BuffType)
                                                        {
                                                            case BuffType.Good:
                                                                ski.Skill.BCards.Where(s => !s.Type.Equals((byte)CardType.MeditationSkill))
                                                                    .ToList().ForEach(s =>
                                                                        s.ApplyBCards(target.Character, session.Character));
                                                                break;

                                                            case BuffType.Bad:
                                                                return;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (BCard bCard in ski.Skill.BCards)
                                                    {
                                                        Buff b = new Buff((short)bCard.SecondData, session.Character.Level);

                                                        switch (b.Card.BuffType)
                                                        {
                                                            case BuffType.Good:
                                                                ski.Skill.BCards.Where(s => !s.Type.Equals((byte)CardType.MeditationSkill))
                                                                    .ToList().ForEach(s =>
                                                                        s.ApplyBCards(session.Character));
                                                                break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (ClientSession target in clientSessions)
                                            {
                                                ski.Skill.BCards.Where(s => !s.Type.Equals((byte)CardType.MeditationSkill))
                                                    .ToList().ForEach(s =>
                                                        s.ApplyBCards(target.Character, session.Character));
                                            }
                                        }
                                    }
                                    break;

                                case 4:
                                case 0:
                                    ski.Skill.BCards.Where(s => !s.Type.Equals((byte)CardType.MeditationSkill))
                                        .ToList().ForEach(s => s.ApplyBCards(session.Character));
                                    break;
                            }
                            ski.LastUse = DateTime.UtcNow;
                        }
                        else if (ski.Skill.TargetType == 0) // monster target
                        {
                            if (isPvp)
                            {
                                ClientSession playerToAttack = ServerManager.Instance.GetSessionByCharacterId(targetId);
                                if (playerToAttack != null && session.Character.Mp >= ski.Skill.MpCost)
                                {
                                    if (Map.GetDistance(
                                            new MapCell
                                            {
                                                X = session.Character.PositionX,
                                                Y = session.Character.PositionY
                                            },
                                            new MapCell
                                            {
                                                X = playerToAttack.Character.PositionX,
                                                Y = playerToAttack.Character.PositionY
                                            }) <= ski.Skill.Range + 5)
                                    {
                                        if (!session.Character.HasGodMode)
                                        {
                                            session.Character.Mp -= ski.Skill.MpCost;
                                        }

                                        if (session.Character.UseSp && ski.Skill.CastEffect != -1)
                                        {
                                            session.SendPackets(session.Character.GenerateQuicklist());
                                        }

                                        session.SendPacket(session.Character.GenerateStat());
                                        CharacterSkill characterSkillInfo = session.Character.Skills.FirstOrDefault(s =>
                                            s.Skill.UpgradeSkill == ski.Skill.SkillVNum && s.Skill.Effect > 0
                                                                                        && s.Skill.SkillType == 2);
                                        session.CurrentMapInstance?.Broadcast(
                                            StaticPacketHelper.CastOnTarget(UserType.Player,
                                                session.Character.CharacterId, 1, targetId, ski.Skill.CastAnimation,
                                                characterSkillInfo?.Skill.CastEffect ?? ski.Skill.CastEffect,
                                                ski.Skill.SkillVNum));
                                        session.Character.Skills.Where(s => s.Id != ski.Id).ForEach(i => i.Hit = 0);

                                        // Generate scp
                                        if ((DateTime.UtcNow - ski.LastUse).TotalSeconds > 3)
                                        {
                                            ski.Hit = 0;
                                        }
                                        else
                                        {
                                            ski.Hit++;
                                        }

                                        ski.LastUse = DateTime.UtcNow;

                                        Observable
                                            .Timer(ski.Skill.CastEffect != 0
                                                ? TimeSpan.FromMilliseconds(ski.Skill.CastTime * 100)
                                                : TimeSpan.Zero).Subscribe(observer => TargetHitRunRegularPvpHit(session, playerToAttack, ski, targetId));
                                    }
                                    else
                                    {
                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                    }
                                }
                                else
                                {
                                    session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                }
                            }
                            else
                            {
                                MapMonster monsterToAttack = session.CurrentMapInstance?.GetMonster(targetId);
                                if (monsterToAttack != null && session.Character.Mp >= ski.Skill.MpCost)
                                {
                                    if (Map.GetDistance(
                                            new MapCell
                                            {
                                                X = session.Character.PositionX,
                                                Y = session.Character.PositionY
                                            },
                                            new MapCell { X = monsterToAttack.MapX, Y = monsterToAttack.MapY })
                                        <= ski.Skill.Range + 5 + monsterToAttack.Monster.BasicArea)
                                    {
                                        if (!session.Character.HasGodMode)
                                        {
                                            session.Character.Mp -= ski.Skill.MpCost;
                                        }

                                        if (session.Character.UseSp && ski.Skill.CastEffect != -1)
                                        {
                                            session.SendPackets(session.Character.GenerateQuicklist());
                                        }
                                        // TODO: check this
                                        monsterToAttack.Monster.BCards.Where(s => s.CastType == 1).ToList()
                                            .ForEach(s => s.ApplyBCards(session));
                                        session.SendPacket(session.Character.GenerateStat());
                                        CharacterSkill characterSkillInfo = session.Character.Skills.FirstOrDefault(s =>
                                            s.Skill.UpgradeSkill == ski.Skill.SkillVNum && s.Skill.Effect > 0
                                                                                        && s.Skill.SkillType == 2);

                                        session.CurrentMapInstance.Broadcast(StaticPacketHelper.CastOnTarget(
                                            UserType.Player, session.Character.CharacterId, 3,
                                            monsterToAttack.MapMonsterId, ski.Skill.CastAnimation,
                                            characterSkillInfo?.Skill.CastEffect ?? ski.Skill.CastEffect,
                                            ski.Skill.SkillVNum));
                                        session.Character.Skills.Where(s => s.Id != ski.Id).ForEach(i => i.Hit = 0);

                                        // Generate scp
                                        if ((DateTime.UtcNow - ski.LastUse).TotalSeconds > 3)
                                        {
                                            ski.Hit = 0;
                                        }
                                        else
                                        {
                                            ski.Hit++;
                                        }

                                        ski.LastUse = DateTime.UtcNow;
                                        Observable
                                            .Timer(ski.Skill.CastEffect != 0
                                                ? TimeSpan.FromMilliseconds(ski.Skill.CastTime * 100)
                                                : TimeSpan.Zero).Subscribe(observer =>
                                                {
                                                    TargetHitRunRegularPveHit(session, monsterToAttack, ski,
                                                        characterSkillInfo, targetId);
                                                });
                                    }
                                    else
                                    {
                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                    }
                                }
                                else
                                {
                                    session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                }
                            }

                            if (ski.Skill.HitType == 3)
                            {
                                session.Character.MtListTargetQueue.Clear();
                            }
                        }
                        else
                        {
                            session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        }

                        if (ski.Skill.UpgradeSkill == 3 && ski.Skill.SkillType == 1)
                        {
                            session.SendPacket(
                                StaticPacketHelper.SkillResetWithCoolDown(castingId, ski.Skill.Cooldown));
                        }

                        session.SendPacketAfter(StaticPacketHelper.SkillReset(castingId), ski.Skill.Cooldown * 100);
                    }
                    else
                    {
                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        session.SendPacket(
                            session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MP"), 10));
                    }
                }
            }
            else
            {
                session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
            }

            if ((castingId != 0 && castingId < 11 && shouldCancel) || session.Character.SkillComboCount > 7)
            {
                session.SendPackets(session.Character.GenerateQuicklist());
                session.SendPacket("mslot 0 -1");
            }

            session.Character.LastSkillUse = DateTime.UtcNow;
        }

        internal static void ZoneHit(this ClientSession session, int castingId, short x, short y)
        {
            List<CharacterSkill> skills = session.Character.UseSp
                ? session.Character.SkillsSp.GetAllItems()
                : session.Character.Skills.GetAllItems();
            CharacterSkill characterSkill = skills?.Find(s => s.Skill?.CastId == castingId);
            if (characterSkill == null || !session.Character.WeaponLoaded(characterSkill)
                                       || !session.HasCurrentMapInstance)
            {
                session.SendPacket(StaticPacketHelper.Cancel(2));
                return;
            }

            if (characterSkill.CanBeUsed())
            {
                if (session.Character.Mp >= characterSkill.Skill.MpCost && session.HasCurrentMapInstance)
                {
                    session.CurrentMapInstance.Broadcast(
                        $"ct_n 1 {session.Character.CharacterId} 3 -1 {characterSkill.Skill.CastAnimation}" +
                        $" {characterSkill.Skill.CastEffect} {characterSkill.Skill.SkillVNum}");
                    characterSkill.LastUse = DateTime.UtcNow;
                    if (!session.Character.HasGodMode)
                    {
                        session.Character.Mp -= characterSkill.Skill.MpCost;
                    }

                    session.SendPacket(session.Character.GenerateStat());
                    characterSkill.LastUse = DateTime.UtcNow;
                    Observable.Timer(TimeSpan.FromMilliseconds(characterSkill.Skill.CastTime * 100)).Subscribe(o =>
                    {
                        session.Character.LastSkillUse = DateTime.UtcNow;

                        session.CurrentMapInstance.Broadcast(
                            $"bs 1 {session.Character.CharacterId} {x} {y} {characterSkill.Skill.SkillVNum}" +
                            $" {characterSkill.Skill.Cooldown} {characterSkill.Skill.AttackAnimation}" +
                            $" {characterSkill.Skill.Effect} 0 0 1 1 0 0 0");

                        foreach (long id in session.Character.MtListTargetQueue
                            .Where(s => s.EntityType == UserType.Monster).Select(s => s.TargetId).Concat(session
                                .CurrentMapInstance.GetListMonsterInRange(x, y, characterSkill.Skill.TargetRange)
                                .Where(s => s.CurrentHp > 0).Select(s => (long) s.MapMonsterId)))
                        {
                            MapMonster mon = session.CurrentMapInstance.GetMonster(id);
                            if (mon?.CurrentHp > 0)
                            {
                                mon.HitQueue.Enqueue(new HitRequest(TargetHitType.ZoneHit, session,
                                    characterSkill.Skill, characterSkill.Skill.Effect, x, y));
                            }
                        }

                        if (characterSkill.Skill.BCards.ToList().Any(s =>
                            s.Type == (byte) CardType.FairyXPIncrease && s.SubType
                            == ((byte)AdditionalTypes.FairyXPIncrease.TeleportToLocation / 10)))
                        {
                            characterSkill.Skill.BCards.ToList().ForEach(s => s.ApplyBCards(session.Character));
                            session.Character.MapInstance.Broadcast($"tp 1 {session.Character.CharacterId} {x} {y} 0");
                            session.Character.PositionX = x;
                            session.Character.PositionY = y;
                        }

                        foreach (long id in session.Character.MtListTargetQueue
                            .Where(s => s.EntityType == UserType.Player).Select(s => s.TargetId).Concat(ServerManager
                                .Instance.Sessions.Where(s =>
                                    s.CurrentMapInstance == session.CurrentMapInstance
                                    && s.Character.CharacterId != session.Character.CharacterId
                                    && s.Character.IsInRange(x, y, characterSkill.Skill.TargetRange))
                                .Select(s => s.Character.CharacterId)))
                        {
                            ClientSession character = ServerManager.Instance.GetSessionByCharacterId(id);
                            if (character?.HasCurrentMapInstance == true
                                && character.CurrentMapInstance == session.CurrentMapInstance
                                && character.Character.CharacterId != session.Character.CharacterId)
                            {
                                if (session.CurrentMapInstance.Map.MapTypes.Any(s =>
                                    s.MapTypeId == (short) MapTypeEnum.Act4))
                                {
                                    if (session.Character.Faction != character.Character.Faction
                                        && session.CurrentMapInstance.Map.MapId != 130
                                        && session.CurrentMapInstance.Map.MapId != 131)
                                    {
                                        PvpHit(session,
                                            new HitRequest(TargetHitType.ZoneHit, session, characterSkill.Skill, characterSkill.Skill.Effect, x, y),
                                            character);
                                    }
                                }
                                else if (session.CurrentMapInstance.Map.MapTypes.Any(m =>
                                    m.MapTypeId == (short) MapTypeEnum.PvpMap))
                                {
                                    if (session.Character.Group?.IsMemberOfGroup(character.Character.CharacterId) != true)
                                    {
                                        PvpHit(session,
                                            new HitRequest(TargetHitType.ZoneHit, session, characterSkill.Skill, characterSkill.Skill.Effect, x, y),
                                            character);
                                    }
                                }
                                else if (session.CurrentMapInstance.IsPvp)
                                {
                                    if (session.Character.Group?.IsMemberOfGroup(character.Character.CharacterId) != true)
                                    {
                                        PvpHit(session,
                                            new HitRequest(TargetHitType.ZoneHit, session, characterSkill.Skill, characterSkill.Skill.Effect, x, y),
                                            character);
                                    }
                                }
                            }
                        }

                        session.Character.MtListTargetQueue.Clear();
                    });
                    Observable.Timer(TimeSpan.FromMilliseconds(characterSkill.Skill.Cooldown * 100))
                        .Subscribe(o => session.SendPacket(StaticPacketHelper.SkillReset(castingId)));
                }
                else
                {
                    session.SendPacket(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MP"), 10));
                    session.SendPacket(StaticPacketHelper.Cancel(2));
                }
            }
            else
            {
                session.SendPacket(StaticPacketHelper.Cancel(2));
            }
        }

        private static void PvpHit(ClientSession session, HitRequest hitRequest, ClientSession target)
        {
            if (target?.Character.Hp > 0 && hitRequest?.Session.Character.Hp > 0 && session.HasCurrentMapInstance)
            {
                if ((session.CurrentMapInstance.MapInstanceId == ServerManager.Instance.ArenaInstance.MapInstanceId
                     || session.CurrentMapInstance.MapInstanceId
                     == ServerManager.Instance.FamilyArenaInstance.MapInstanceId)
                    && (session.CurrentMapInstance.Map.JaggedGrid[session.Character.PositionX][
                                session.Character.PositionY]
                            ?.Value != 0
                        || target.CurrentMapInstance.Map.JaggedGrid[target.Character.PositionX][
                                target.Character.PositionY]
                            ?.Value != 0))
                {
                    // User in SafeZone
                    hitRequest.Session.SendPacket(StaticPacketHelper.Cancel(2, target.Character.CharacterId));
                    return;
                }

                if (target.Character.IsSitting)
                {
                    target.Character.Rest();
                }

                int hitmode = 0;
                bool onyxWings = false;
                BattleEntity battleEntity = new BattleEntity(hitRequest.Session.Character, hitRequest.Skill);
                BattleEntity battleEntityDefense = new BattleEntity(target.Character, null);
                int damage = DamageHelper.Instance.CalculateDamage(battleEntity, battleEntityDefense, hitRequest.Skill,
                    ref hitmode, ref onyxWings);

                if (hitRequest.Mate == null
                    && hitRequest.Session.Character.Morph != 0 && hitRequest.Session.Character.Morph != 1
                    && hitRequest.Session.Character.Morph != 8 && hitRequest.Session.Character.Morph != 16
                    && hitRequest.Skill.BCards.All(s =>
                        s.Type != (byte)CardType.AttackPower && s.Type != (byte)CardType.Element))
                {
                    damage = 0;
                    hitmode = 0;
                }

                /* Disable for now as this definitely and sure as hell needs to be done differently.
                ItemInstance weapon = session.Character.Inventory.LoadBySlotAndType<ItemInstance>((short)EquipmentType.MainWeapon, InventoryType.Wear);
                if (weapon != null)
                {
                    foreach (BCard bcard in weapon.Item.BCards)
                    {
                        Buff b = new Buff((short)bcard.SecondData, target.Character.Level);
                        switch (b.Card?.BuffType)
                        {
                            case BuffType.Good:
                                bcard.ApplyBCards(session.Character);
                                break;
                            case BuffType.Bad:
                                bcard.ApplyBCards(target.Character, session.Character);
                                break;
                        }
                    }
                }
                ItemInstance armor = target.Character.Inventory.LoadBySlotAndType<ItemInstance>((short)EquipmentType.Armor, InventoryType.Wear);
                if (armor != null)
                {
                    foreach (BCard bcard in armor.Item.BCards)
                    {
                        Buff b = new Buff((short)bcard.SecondData, target.Character.Level);
                        switch (b.Card?.BuffType)
                        {
                            case BuffType.Good:
                                bcard.ApplyBCards(target.Character);
                                break;
                            case BuffType.Bad:
                                bcard.ApplyBCards(session.Character, target.Character);
                                break;
                        }
                    }
                }
                */
                if (hitRequest.Skill.SkillVNum == 1085)
                {
                    hitRequest.Session.Character.MapInstance.Broadcast(
                        $"tp 1 {hitRequest.Session.Character.CharacterId} {target.Character.PositionX} {target.Character.PositionY} 0");
                    hitRequest.Session.Character.PositionX = target.Character.PositionX;
                    hitRequest.Session.Character.PositionY = target.Character.PositionY;
                }

                if (target.Character.HasGodMode)
                {
                    damage = 0;
                    hitmode = 1;
                }
                else if (target.Character.LastPvpRevive > DateTime.UtcNow.AddSeconds(-10)
                         || hitRequest.Session.Character.LastPvpRevive > DateTime.UtcNow.AddSeconds(-10))
                {
                    damage = 0;
                    hitmode = 1;
                }

                target.Character.OnReceiveHit(new HitEventArgs(UserType.Player, session, hitRequest.Skill, damage));
                session.Character.OnLandHit(new HitEventArgs(UserType.Player, target, hitRequest.Skill, damage));

                int mindSink = target.Character.GetBuff(CardType.DarkCloneSummon,
                    (byte)AdditionalTypes.DarkCloneSummon.ConvertDamageToHPChance)[0];
                if (mindSink != 0 && hitmode != 1)
                {
                    target.Character.Hp = target.Character.Hp + damage / 2 < target.Character.HPMax ? target.Character.Hp + damage / 2 : target.Character.Hp = target.Character.HPMax;
                    target.Character.MapInstance.Broadcast(target.Character.GenerateRc(damage / 2));
                    target.Character.Collect += damage / 2;
                    damage = 0;
                }

                int[] manaShield = target.Character.GetBuff(CardType.LightAndShadow,
                    (byte)AdditionalTypes.LightAndShadow.InflictDamageToMP);
                if (manaShield[0] != 0 && hitmode != 1)
                {
                    int reduce = damage / 100 * manaShield[0];
                    if (target.Character.Mp < reduce)
                    {
                        target.Character.Mp = 0;
                    }
                    else
                    {
                        target.Character.Mp -= reduce;
                    }
                }

                int[] reflect = target.Character.GetBuff(CardType.TauntSkill,
                    (byte)AdditionalTypes.TauntSkill.ReflectsMaximumDamageFrom);

                if (reflect[0] != 0 && hitmode != 1)
                {
                    int reflected = Math.Abs(reflect[0]);
                    if (damage > reflect[0])
                    {
                        target.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player, target.Character.CharacterId, 1,
                            session.Character.CharacterId, -1, 0, -1, 4500, -1, -1, true, 100, reflected, 0, 1));
                    }
                    else
                    {
                        target.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player, target.Character.CharacterId, 1,
                            session.Character.CharacterId, -1, 0, -1, 4500, -1, -1, true, 100, reflected = damage, 0, 1));
                    }

                    session.Character.Hp = session.Character.Hp - reflected > 1 ? session.Character.Hp - reflected : session.Character.Hp = 1;

                    damage = 0;
                }

                if (target.Character.Buff.ContainsKey(144))
                {
                    if (ServerManager.RandomNumber() < 50)
                    {
                        session.Character.AddBuff(new Buff(372, 1));
                    }
                    damage = 1;
                }

                int[] criticalDefence = target.Character.GetBuff(CardType.VulcanoElementBuff, (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefence);

                if (criticalDefence[0] != 0 && hitmode == 3)
                {
                    if (damage > criticalDefence[0])
                    {
                        damage = criticalDefence[0];
                    }
                }

                foreach (BCard bcard in hitRequest.Skill.BCards)
                {
                    switch (bcard.Type)
                    {
                        case (byte)CardType.Reflection:
                            switch(bcard.SubType)
                            {
                                case (byte)AdditionalTypes.Reflection.DecreaseMP / 10:
                                    if(ServerManager.RandomNumber() < bcard.SecondData)
                                    {
                                        int bonus = (int)(bcard.FirstData * target.Character.MPLoad() / 100);
                                        target.Character.Mp = target.Character.Mp + bonus > 1 ? target.Character.Mp + bonus : target.Character.Mp = 1;
                                    }
                                    break;
                            }
                            break;
                        case (byte)CardType.SpecialActions:
                            switch (bcard.SubType)
                            {
                                case ((byte)AdditionalTypes.SpecialActions.PushBack / 10):
                                    {
                                        short destinationX = target.Character.PositionX;
                                        short destinationY = target.Character.PositionY;
                                        if (target.Character.PositionX < hitRequest.Session.Character.PositionX)
                                        {
                                            destinationX--;
                                        }
                                        else if (target.Character.PositionX > hitRequest.Session.Character.PositionX)
                                        {
                                            destinationX++;
                                        }

                                        if (target.Character.PositionY < hitRequest.Session.Character.PositionY)
                                        {
                                            destinationY--;
                                        }
                                        else if (target.Character.PositionY > hitRequest.Session.Character.PositionY)
                                        {
                                            destinationY++;
                                        }
                                        hitRequest.Session.Character.MapInstance.Broadcast($"guri 3 1 {target.Character.CharacterId} {destinationX} {destinationY} 3 {bcard.SecondData} 2 -1");
                                        target.Character.PositionX = destinationX;
                                        target.Character.PositionY = destinationY;
                                    }
                                    break;
                                case ((byte)AdditionalTypes.SpecialActions.FocusEnemies / 10):
                                    {
                                        short destinationX = session.Character.PositionX;
                                        short destinationY = session.Character.PositionY;
                                        if (target.Character.PositionX < session.Character.PositionX)
                                        {
                                            destinationX--;
                                        }
                                        else if (target.Character.PositionX > session.Character.PositionX)
                                        {
                                            destinationX++;
                                        }

                                        if (target.Character.PositionY < session.Character.PositionY)
                                        {
                                            destinationY--;
                                        }
                                        else if (target.Character.PositionY > session.Character.PositionY)
                                        {
                                            destinationY++;
                                        }
                                        session.Character.MapInstance.Broadcast($"guri 3 1 {target.Character.CharacterId} {destinationX} {destinationY} 3 {bcard.SecondData} 2 -1");
                                        target.Character.PositionX = destinationX;
                                        target.Character.PositionY = destinationY;
                                    }
                                    break;
                            }
                            break;
                    }
                }

                if (hitRequest.Skill.SkillVNum == 1138)
                {
                    int rdn = ServerManager.RandomNumber(0, 100);
                    int srdn = ServerManager.RandomNumber(0, 2);
                    short[] effct1 = { 4005, 4017 };
                    short[] effect2 = { 3807, 3819 };
                    short[] effect3 = { 4405, 4421 };
                    short[] effect4 = { 3908, 3916 };
                    short[] clone12 = { 42, 13 };
                    short[] clone34 = { 42, 40 };
                    int dam = session.Character.Level * 17;

                    if (rdn < 35)
                    {
                        session.Character.SpawnDarkClone(2, 2, 2112, 1, 1, target.Character.CharacterId, (short)(1141 + srdn), clone12[srdn], effct1[srdn], dam);
                        session.Character.SpawnDarkClone(-2, 2, 2113, 2, 1, target.Character.CharacterId, (short)(1143 + srdn), clone12[srdn], effect2[srdn], dam);

                        target.Character.Hp = target.Character.Hp - dam * 2 > 0 ? target.Character.Hp - dam * 2 : target.Character.Hp = 1;
                    }
                    else if (rdn < 70)
                    {
                        session.Character.SpawnDarkClone(2, 2, 2112, 1, 1, target.Character.CharacterId, (short)(1141 + srdn), clone12[srdn], effct1[srdn], dam);
                        session.Character.SpawnDarkClone(-2, 2, 2113, 2, 1, target.Character.CharacterId, (short)(1143 + srdn), clone12[srdn], effect2[srdn], dam);
                        session.Character.SpawnDarkClone(-2, -2, 2114, 3, 1, target.Character.CharacterId, (short)(1145 + srdn), clone34[srdn], effect3[srdn], dam);

                        target.Character.Hp = target.Character.Hp - dam * 3 > 0 ? target.Character.Hp - dam * 3 : target.Character.Hp = 1;
                    }
                    else
                    {

                        session.Character.SpawnDarkClone(2, 2, 2112, 1, 1, target.Character.CharacterId, (short)(1141 + srdn), clone12[srdn], effct1[srdn], dam);
                        session.Character.SpawnDarkClone(-2, 2, 2113, 2, 1, target.Character.CharacterId, (short)(1143 + srdn), clone12[srdn], effect2[srdn], dam);
                        session.Character.SpawnDarkClone(-2, -2, 2114, 3, 1, target.Character.CharacterId, (short)(1145 + srdn), clone34[srdn], effect3[srdn], dam);
                        session.Character.SpawnDarkClone(2, -2, 2115, 4, 1, target.Character.CharacterId, (short)(1147 + srdn), clone34[srdn], effect4[srdn], dam);

                        target.Character.Hp = target.Character.Hp - dam * 4 > 0 ? target.Character.Hp - dam * 4 : target.Character.Hp = 1;
                    }
                }

                if (onyxWings && hitmode != 1)
                {
                    short onyxX = (short)(hitRequest.Session.Character.PositionX + 2);
                    short onyxY = (short)(hitRequest.Session.Character.PositionY + 2);
                    int onyxId = session.CurrentMapInstance.GetNextMonsterId();
                    MapMonster onyx = new MapMonster
                    {
                        MonsterVNum = 2371,
                        MapX = onyxX,
                        MapY = onyxY,
                        MapMonsterId = onyxId,
                        IsHostile = false,
                        IsMoving = false,
                        ShouldRespawn = false
                    };
                    target.CurrentMapInstance.Broadcast(UserInterfaceHelper.GenerateGuri(31, 1,
                        hitRequest.Session.Character.CharacterId, onyxX, onyxY));
                    onyx.Initialize(target.CurrentMapInstance);
                    target.CurrentMapInstance.AddMonster(onyx);
                    target.CurrentMapInstance.Broadcast(onyx.GenerateIn());
                    target.Character.Hp -= damage / 2;
                    Observable.Timer(TimeSpan.FromMilliseconds(350)).Subscribe(o =>
                    {
                        target.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Monster, onyxId, 1,
                            target.Character.CharacterId, -1, 0, -1, hitRequest.Skill.Effect, -1, -1, true, 92,
                            damage / 2, 0, 0));
                        target.CurrentMapInstance.RemoveMonster(onyx);
                        target.CurrentMapInstance.Broadcast(StaticPacketHelper.Out(UserType.Monster,
                            onyx.MapMonsterId));
                    });
                }

                target.Character.GetDamage(damage / 2);
                target.Character.LastDefence = DateTime.UtcNow;
                target.SendPacket(target.Character.GenerateStat());
                bool isAlive = target.Character.Hp > 0;
                if (!isAlive && target.HasCurrentMapInstance)
                {
                    if (target.CurrentMapInstance.Map?.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4)
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

                        hitRequest.Session.Character.Act4Kill++;
                        target.Character.Act4Dead++;
                        target.Character.GetAct4Points(-1);
                        if (target.Character.Level + 10 >= hitRequest.Session.Character.Level
                            && hitRequest.Session.Character.Level <= target.Character.Level - 10)
                        {
                            hitRequest.Session.Character.GetAct4Points(2);
                        }

                        if (target.Character.Reputation < 50000)
                        {
                            target.SendPacket(session.Character.GenerateSay(
                                string.Format(Language.Instance.GetMessageFromKey("LOSE_REP"), 0), 11));
                        }
                        else
                        {
                            target.Character.SetReputation(target.Character.Level * -50);
                            hitRequest.Session.Character.SetReputation(target.Character.Level * 50);
                        }

                        foreach (ClientSession sess in ServerManager.Instance.Sessions.Where(
                            s => s.HasSelectedCharacter))
                        {
                            if (sess.Character.Faction == session.Character.Faction)
                            {
                                sess.SendPacket(sess.Character.GenerateSay(
                                    string.Format(
                                        Language.Instance.GetMessageFromKey(
                                            $"ACT4_PVP_KILL{(int)target.Character.Faction}"), session.Character.Name),
                                    12));
                            }
                            else if (sess.Character.Faction == target.Character.Faction)
                            {
                                sess.SendPacket(sess.Character.GenerateSay(
                                    string.Format(
                                        Language.Instance.GetMessageFromKey(
                                            $"ACT4_PVP_DEATH{(int)target.Character.Faction}"), target.Character.Name),
                                    11));
                            }
                        }

                        target.SendPacket(target.Character.GenerateFd());
                        target.Character.DisableBuffs(BuffType.All, force: true);
                        target.CurrentMapInstance.Broadcast(target, target.Character.GenerateIn(),
                            ReceiverType.AllExceptMe);
                        target.CurrentMapInstance.Broadcast(target, target.Character.GenerateGidx(),
                            ReceiverType.AllExceptMe);
                        target.SendPacket(
                            target.Character.GenerateSay(Language.Instance.GetMessageFromKey("ACT4_PVP_DIE"), 11));
                        target.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ACT4_PVP_DIE"), 0));
                        Observable.Timer(TimeSpan.FromMilliseconds(2000)).Subscribe(o =>
                        {
                            target.CurrentMapInstance?.Broadcast(target,
                                $"c_mode 1 {target.Character.CharacterId} 1564 0 0 0");
                            target.CurrentMapInstance?.Broadcast(target.Character.GenerateRevive());
                        });
                        Observable.Timer(TimeSpan.FromMilliseconds(30000)).Subscribe(o =>
                        {
                            target.Character.Hp = (int)target.Character.HPLoad();
                            target.Character.Mp = (int)target.Character.MPLoad();
                            short x = (short)(39 + ServerManager.RandomNumber(-2, 3));
                            short y = (short)(42 + ServerManager.RandomNumber(-2, 3));
                            if (target.Character.Faction == FactionType.Angel)
                            {
                                ServerManager.Instance.ChangeMap(target.Character.CharacterId, 130, x, y);
                            }
                            else if (target.Character.Faction == FactionType.Demon)
                            {
                                ServerManager.Instance.ChangeMap(target.Character.CharacterId, 131, x, y);
                            }
                            else
                            {
                                target.Character.MapId = 145;
                                target.Character.MapX = 51;
                                target.Character.MapY = 41;
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

                            target.CurrentMapInstance?.Broadcast(target, target.Character.GenerateTp());
                            target.CurrentMapInstance?.Broadcast(target.Character.GenerateRevive());
                            target.SendPacket(target.Character.GenerateStat());
                        });
                    }
                    else
                    {
                        hitRequest.Session.Character.TalentWin++;
                        target.Character.TalentLose++;
                        hitRequest.Session.CurrentMapInstance?.Broadcast(session.Character.GenerateSay(
                            string.Format(Language.Instance.GetMessageFromKey("PVP_KILL"),
                                hitRequest.Session.Character.Name, target.Character.Name), 10));
                        Observable.Timer(TimeSpan.FromMilliseconds(1000)).Subscribe(o =>
                            ServerManager.Instance.AskPvpRevive(target.Character.CharacterId));
                    }
                }

                if (hitmode != 1)
                {
                    hitRequest.Skill.BCards.Where(s => s.Type.Equals((byte)CardType.Buff)).ToList()
                        .ForEach(s => s.ApplyBCards(target.Character, session.Character));

                    if (battleEntity?.ShellWeaponEffects != null)
                    {
                        foreach (ShellEffectDTO shell in battleEntity.ShellWeaponEffects)
                        {
                            switch (shell.Effect)
                            {
                                case (byte) ShellWeaponEffectType.Blackout:
                                {
                                    Buff buff = new Buff(7, battleEntity.Level);
                                    if (ServerManager.RandomNumber() < shell.Value / 100D
                                        * (100
                                         - (battleEntityDefense.ShellArmorEffects?.Find(s =>
                                              s.Effect == (byte)ShellArmorEffectType.ReducedStun)?.Value
                                          + battleEntityDefense.ShellArmorEffects?.Find(s =>
                                              s.Effect == (byte)ShellArmorEffectType.ReducedAllStun)?.Value
                                          + battleEntityDefense.ShellArmorEffects?.Find(s =>
                                                  s.Effect == (byte)ShellArmorEffectType.ReducedAllNegativeEffect)
                                              ?.Value)))
                                    {
                                        target.Character.AddBuff(buff);
                                    }

                                    break;
                                }
                                case (byte) ShellWeaponEffectType.DeadlyBlackout:
                                {
                                    Buff buff = new Buff(66, battleEntity.Level);
                                    if (ServerManager.RandomNumber() < shell.Value / 100D
                                        * (100 - (battleEntityDefense.ShellArmorEffects?.Find(s =>
                                                    s.Effect == (byte)ShellArmorEffectType.ReducedAllStun)?.Value
                                                + battleEntityDefense.ShellArmorEffects?.Find(s =>
                                                        s.Effect == (byte)ShellArmorEffectType
                                                            .ReducedAllNegativeEffect)
                                                    ?.Value)))
                                    {
                                        target.Character.AddBuff(buff);
                                    }

                                    break;
                                }
                                case (byte) ShellWeaponEffectType.MinorBleeding:
                                {
                                    Buff buff = new Buff(1, battleEntity.Level);
                                    if (ServerManager.RandomNumber() < shell.Value / 100D
                                        * (100
                                         - (battleEntityDefense.ShellArmorEffects?.Find(s =>
                                              s.Effect == (byte)ShellArmorEffectType.ReducedMinorBleeding)?.Value
                                          + battleEntityDefense.ShellArmorEffects?.Find(s =>
                                                  s.Effect == (byte)ShellArmorEffectType
                                                      .ReducedBleedingAndMinorBleeding)
                                              ?.Value + battleEntityDefense.ShellArmorEffects?.Find(s =>
                                              s.Effect == (byte)ShellArmorEffectType.ReducedAllBleedingType)?.Value
                                          + battleEntityDefense.ShellArmorEffects?.Find(s =>
                                                  s.Effect == (byte)ShellArmorEffectType.ReducedAllNegativeEffect)
                                              ?.Value)))
                                    {
                                        target.Character.AddBuff(buff);
                                    }

                                    break;
                                }
                                case (byte) ShellWeaponEffectType.Bleeding:
                                {
                                    Buff buff = new Buff(21, battleEntity.Level);
                                    if (ServerManager.RandomNumber() < shell.Value / 100D
                                        * (100 - (battleEntityDefense.ShellArmorEffects?.Find(s =>
                                                    s.Effect == (byte)ShellArmorEffectType
                                                        .ReducedBleedingAndMinorBleeding)?.Value
                                                + battleEntityDefense.ShellArmorEffects?.Find(s =>
                                                        s.Effect == (byte)ShellArmorEffectType.ReducedAllBleedingType)
                                                    ?.Value + battleEntityDefense.ShellArmorEffects?.Find(s =>
                                                        s.Effect == (byte)ShellArmorEffectType
                                                            .ReducedAllNegativeEffect)
                                                    ?.Value)))
                                    {
                                        target.Character.AddBuff(buff);
                                    }

                                    break;
                                }
                                case (byte) ShellWeaponEffectType.HeavyBleeding:
                                {
                                    Buff buff = new Buff(42, battleEntity.Level);
                                    if (ServerManager.RandomNumber() < shell.Value / 100D
                                        * (100 - (battleEntityDefense.ShellArmorEffects?.Find(s =>
                                                        s.Effect == (byte)ShellArmorEffectType.ReducedAllBleedingType)
                                                    ?.Value + battleEntityDefense.ShellArmorEffects?.Find(s =>
                                                        s.Effect == (byte)ShellArmorEffectType
                                                            .ReducedAllNegativeEffect)
                                                    ?.Value)))
                                    {
                                        target.Character.AddBuff(buff);
                                    }

                                    break;
                                }
                                case (byte) ShellWeaponEffectType.Freeze:
                                {
                                    Buff buff = new Buff(27, battleEntity.Level);
                                    if (ServerManager.RandomNumber() < shell.Value / 100D
                                        * (100 - (battleEntityDefense.ShellArmorEffects?.Find(s =>
                                                    s.Effect == (byte)ShellArmorEffectType.ReducedFreeze)?.Value
                                                + battleEntityDefense.ShellArmorEffects?.Find(s =>
                                                        s.Effect == (byte)ShellArmorEffectType
                                                            .ReducedAllNegativeEffect)
                                                    ?.Value)))
                                    {
                                        target.Character.AddBuff(buff);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }

                switch (hitRequest.TargetHitType)
                {
                    case TargetHitType.SingleTargetHit:
                        hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                            hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                            hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown, hitRequest.Skill.AttackAnimation,
                            hitRequest.SkillEffect, hitRequest.Session.Character.PositionX,
                            hitRequest.Session.Character.PositionY, isAlive,
                            (int)(target.Character.Hp / (float)target.Character.HPLoad() * 100), damage, hitmode,
                            (byte)(hitRequest.Skill.SkillType - 1)));
                        break;

                    case TargetHitType.SingleTargetHitCombo:
                        hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                            hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                            hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown, hitRequest.SkillCombo.Animation,
                            hitRequest.SkillCombo.Effect, hitRequest.Session.Character.PositionX,
                            hitRequest.Session.Character.PositionY, isAlive,
                            (int)(target.Character.Hp / (float)target.Character.HPLoad() * 100), damage, hitmode,
                            (byte)(hitRequest.Skill.SkillType - 1)));
                        break;

                    case TargetHitType.SingleAoeTargetHit:
                        switch (hitmode)
                        {
                            case 1:
                                hitmode = 4;
                                break;

                            case 3:
                                hitmode = 6;
                                break;

                            default:
                                hitmode = 5;
                                break;
                        }

                        hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                            hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                            hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown, hitRequest.Skill.AttackAnimation,
                            hitRequest.SkillEffect, hitRequest.Session.Character.PositionX,
                            hitRequest.Session.Character.PositionY, isAlive,
                            (int)(target.Character.Hp / (float)target.Character.HPLoad() * 100), damage, hitmode,
                            (byte)(hitRequest.Skill.SkillType - 1)));
                        break;

                    case TargetHitType.AoeTargetHit:
                        switch (hitmode)
                        {
                            case 1:
                                hitmode = 4;
                                break;

                            case 3:
                                hitmode = 6;
                                break;

                            default:
                                hitmode = 5;
                                break;
                        }

                        hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                            hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                            hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown, hitRequest.Skill.AttackAnimation,
                            hitRequest.SkillEffect, /*hitRequest.Session.Character.PositionX,
                            hitRequest.Session.Character.PositionY,*/ 0, 0, isAlive,
                            (int)(target.Character.Hp / (float)target.Character.HPLoad() * 100), damage, hitmode,
                            (byte)(hitRequest.Skill.SkillType - 1)));
                        break;

                    case TargetHitType.ZoneHit:
                        hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                            hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                            hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown, hitRequest.Skill.AttackAnimation,
                            hitRequest.SkillEffect, /*hitRequest.MapX, hitRequest.MapY,*/ 0, 0, isAlive,
                            (int)(target.Character.Hp / (float)target.Character.HPLoad() * 100), damage, 5,
                            (byte)(hitRequest.Skill.SkillType - 1)));
                        break;

                    case TargetHitType.SpecialZoneHit:
                        hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                            hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                            hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown, hitRequest.Skill.AttackAnimation,
                            hitRequest.SkillEffect, hitRequest.Session.Character.PositionX,
                            hitRequest.Session.Character.PositionY, isAlive,
                            (int)(target.Character.Hp / (float)target.Character.HPLoad() * 100), damage, 0,
                            (byte)(hitRequest.Skill.SkillType - 1)));
                        break;

                    default:
                        Logger.Warn("Not Implemented TargetHitType Handling!");
                        break;
                }
            }
            else
            {
                // monster already has been killed, send cancel
                if (target != null)
                {
                    hitRequest?.Session.SendPacket(StaticPacketHelper.Cancel(2, target.Character.CharacterId));
                }
            }
        }

        private static void TargetHitRunAoeTargetHit(ClientSession session, CharacterSkill ski,
            CharacterSkill skillinfo, bool sendCoordinates, long targetId)
        {
            session.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                session.Character.CharacterId, 1, session.Character.CharacterId, ski.Skill.SkillVNum,
                ski.Skill.Cooldown, ski.Skill.AttackAnimation,
                skillinfo?.Skill.Effect ?? ski.Skill.Effect,
                sendCoordinates ? session.Character.PositionX : (short) 0,
                sendCoordinates ? session.Character.PositionY : (short) 0, true,
                (int) (session.Character.Hp / (float) session.Character.HPLoad() * 100), 0, -2,
                (byte) (ski.Skill.SkillType - 1)));
            if (ski.Skill.TargetRange != 0)
            {
                foreach (ClientSession character in ServerManager.Instance.Sessions.Where(s =>
                    s.CurrentMapInstance == session.CurrentMapInstance
                    && s.Character.CharacterId != session.Character.CharacterId
                    && s.Character.IsInRange(session.Character.PositionX, session.Character.PositionY,
                        ski.Skill.TargetRange)))
                {
                    if (session.CurrentMapInstance.Map.MapTypes.Any(s =>
                        s.MapTypeId == (short) MapTypeEnum.Act4))
                    {
                        if (session.Character.Faction != character.Character.Faction
                            && session.CurrentMapInstance.Map.MapId != 130
                            && session.CurrentMapInstance.Map.MapId != 131)
                        {
                            PvpHit(session, new HitRequest(TargetHitType.AoeTargetHit, session, ski.Skill),
                                character);
                        }
                    }
                    else if (session.CurrentMapInstance.Map.MapTypes.Any(m =>
                        m.MapTypeId == (short) MapTypeEnum.PvpMap))
                    {
                        if (session.Character.Group?.IsMemberOfGroup(character.Character.CharacterId) != true)
                        {
                            PvpHit(session, new HitRequest(TargetHitType.AoeTargetHit, session, ski.Skill),
                                character);
                        }
                    }
                    else if (session.CurrentMapInstance.IsPvp)
                    {
                        if (session.Character.Group?.IsMemberOfGroup(character.Character.CharacterId) != true)
                        {
                            PvpHit(session, new HitRequest(TargetHitType.AoeTargetHit, session, ski.Skill),
                                character);
                        }
                    }
                    else
                    {
                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                    }
                }

                foreach (MapMonster mon in session.CurrentMapInstance
                    .GetListMonsterInRange(session.Character.PositionX, session.Character.PositionY,
                        ski.Skill.TargetRange).Where(s => s.CurrentHp > 0))
                {
                    mon.HitQueue.Enqueue(new HitRequest(TargetHitType.AoeTargetHit, session, ski.Skill,
                        skillinfo?.Skill.Effect ?? ski.Skill.Effect));
                }
            }
        }

        private static void TargetHitRunRegularPvpHit(ClientSession session, ClientSession playerToAttack,
            CharacterSkill ski, long targetId)
        {
            if (ski.Skill.HitType == 3)
            {
                int count = 0;
                if (playerToAttack.CurrentMapInstance == session.CurrentMapInstance
                    && playerToAttack.Character.CharacterId
                    != session.Character.CharacterId)
                {
                    if (session.CurrentMapInstance.Map.MapTypes.Any(s =>
                        s.MapTypeId == (short) MapTypeEnum.Act4))
                    {
                        if (session.Character.Faction != playerToAttack.Character.Faction
                            && session.CurrentMapInstance.Map.MapId != 130
                            && session.CurrentMapInstance.Map.MapId != 131)
                        {
                            count++;
                            PvpHit(session,
                                new HitRequest(TargetHitType.SingleTargetHit, session,
                                    ski.Skill), playerToAttack);
                        }
                    }
                    else if (session.CurrentMapInstance.Map.MapTypes.Any(m =>
                        m.MapTypeId == (short) MapTypeEnum.PvpMap))
                    {
                        if (session.Character.Group?.IsMemberOfGroup(playerToAttack
                                .Character.CharacterId) != true)
                        {
                            count++;
                            PvpHit(session,
                                new HitRequest(TargetHitType.SingleTargetHit, session,
                                    ski.Skill), playerToAttack);
                        }
                    }
                    else if (session.CurrentMapInstance.IsPvp)
                    {
                        if (session.Character.Group?.IsMemberOfGroup(playerToAttack
                                .Character.CharacterId) != true)
                        {
                            count++;
                            PvpHit(session,
                                new HitRequest(TargetHitType.SingleTargetHit, session,
                                    ski.Skill), playerToAttack);
                        }
                    }
                }

                foreach (long id in session.Character.MtListTargetQueue
                    .Where(s => s.EntityType == UserType.Player).Select(s => s.TargetId))
                {
                    ClientSession character =
                        ServerManager.Instance.GetSessionByCharacterId(id);
                    if (character != null
                        && character.CurrentMapInstance == session.CurrentMapInstance
                        && character.Character.CharacterId != session.Character.CharacterId)
                    {
                        if (session.CurrentMapInstance.Map.MapTypes.Any(s =>
                            s.MapTypeId == (short) MapTypeEnum.Act4))
                        {
                            if (session.Character.Faction != character.Character.Faction
                                && session.CurrentMapInstance.Map.MapId != 130
                                && session.CurrentMapInstance.Map.MapId != 131)
                            {
                                count++;
                                PvpHit(session,
                                    new HitRequest(TargetHitType.SingleAoeTargetHit,
                                        session, ski.Skill), character);
                            }
                        }
                        else if (session.CurrentMapInstance.Map.MapTypes.Any(m =>
                            m.MapTypeId == (short) MapTypeEnum.PvpMap))
                        {
                            if (session.Character.Group?.IsMemberOfGroup(character
                                    .Character
                                    .CharacterId) != true)
                            {
                                count++;
                                PvpHit(session,
                                    new HitRequest(TargetHitType.SingleAoeTargetHit,
                                        session, ski.Skill), character);
                            }
                        }
                        else if (session.CurrentMapInstance.IsPvp)
                        {
                            if (session.Character.Group?.IsMemberOfGroup(character
                                    .Character
                                    .CharacterId) != true)
                            {
                                count++;
                                PvpHit(session,
                                    new HitRequest(TargetHitType.SingleAoeTargetHit,
                                        session, ski.Skill), character);
                            }
                        }
                    }
                }

                if (count == 0)
                {
                    session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                }
            }
            else
            {
                // check if we will hit mutltiple targets
                if (ski.Skill.TargetRange != 0)
                {
                    ComboDTO skillCombo = ski.Skill.Combos.Find(s => ski.Hit == s.Hit);
                    if (skillCombo != null)
                    {
                        if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit
                            == ski.Hit)
                        {
                            ski.Hit = 0;
                        }

                        IEnumerable<ClientSession> playersInAoeRange =
                            ServerManager.Instance.Sessions.Where(s =>
                                s.CurrentMapInstance == session.CurrentMapInstance
                                && s.Character.CharacterId != session.Character.CharacterId
                                && s.Character.CharacterId != playerToAttack.Character.CharacterId
                                && s.Character.IsInRange(session.Character.PositionX,
                                    session.Character.PositionY, ski.Skill.TargetRange));
                        int count = 0;
                        if (session.CurrentMapInstance.Map.MapTypes.Any(s =>
                            s.MapTypeId == (short) MapTypeEnum.Act4))
                        {
                            if (session.Character.Faction
                                != playerToAttack.Character.Faction
                                && session.CurrentMapInstance.Map.MapId != 130
                                && session.CurrentMapInstance.Map.MapId != 131)
                            {
                                count++;
                                PvpHit(session,
                                    new HitRequest(TargetHitType.SingleTargetHitCombo,
                                        session, ski.Skill, skillCombo: skillCombo),
                                    playerToAttack);
                            }
                        }
                        else if (session.CurrentMapInstance.Map.MapTypes.Any(m =>
                            m.MapTypeId == (short) MapTypeEnum.PvpMap))
                        {
                            if (session.Character.Group?.IsMemberOfGroup(playerToAttack
                                    .Character.CharacterId) != true)
                            {
                                count++;
                                PvpHit(session,
                                    new HitRequest(TargetHitType.SingleTargetHitCombo,
                                        session, ski.Skill, skillCombo: skillCombo),
                                    playerToAttack);
                            }
                        }
                        else if (session.CurrentMapInstance.IsPvp)
                        {
                            if (session.Character.Group?.IsMemberOfGroup(playerToAttack
                                    .Character.CharacterId) != true)
                            {
                                count++;
                                PvpHit(session,
                                    new HitRequest(TargetHitType.SingleTargetHitCombo,
                                        session, ski.Skill, skillCombo: skillCombo),
                                    playerToAttack);
                            }
                        }

                        foreach (ClientSession character in playersInAoeRange)
                        {
                            if (session.CurrentMapInstance.Map.MapTypes.Any(s =>
                                s.MapTypeId == (short) MapTypeEnum.Act4))
                            {
                                if (session.Character.Faction
                                    != character.Character.Faction
                                    && session.CurrentMapInstance.Map.MapId != 130
                                    && session.CurrentMapInstance.Map.MapId != 131)
                                {
                                    count++;
                                    PvpHit(session,
                                        new HitRequest(TargetHitType.SingleTargetHitCombo,
                                            session, ski.Skill, skillCombo: skillCombo),
                                        character);
                                }
                            }
                            else if (session.CurrentMapInstance.Map.MapTypes.Any(m =>
                                m.MapTypeId == (short) MapTypeEnum.PvpMap))
                            {
                                if (session.Character.Group?.IsMemberOfGroup(
                                        character.Character.CharacterId) != true)
                                {
                                    count++;
                                    PvpHit(session,
                                        new HitRequest(TargetHitType.SingleTargetHitCombo,
                                            session, ski.Skill, skillCombo: skillCombo),
                                        character);
                                }
                            }
                            else if (session.CurrentMapInstance.IsPvp)
                            {
                                if (session.Character.Group?.IsMemberOfGroup(
                                        character.Character.CharacterId) != true)
                                {
                                    count++;
                                    PvpHit(session,
                                        new HitRequest(TargetHitType.SingleTargetHitCombo,
                                            session, ski.Skill, skillCombo: skillCombo),
                                        character);
                                }
                            }
                        }

                        if (playerToAttack.Character.Hp <= 0 || count == 0)
                        {
                            session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        }
                    }
                    else
                    {
                        IEnumerable<ClientSession> playersInAoeRange =
                            ServerManager.Instance.Sessions.Where(s =>
                                s.CurrentMapInstance == session.CurrentMapInstance
                                && s.Character.CharacterId != session.Character.CharacterId
                                && s.Character.CharacterId != playerToAttack.Character.CharacterId
                                && s.Character.IsInRange(session.Character.PositionX,
                                    session.Character.PositionY, ski.Skill.TargetRange));

                        // hit the targetted monster
                        if (session.CurrentMapInstance.Map.MapTypes.Any(s =>
                            s.MapTypeId == (short) MapTypeEnum.Act4))
                        {
                            if (session.Character.Faction
                                != playerToAttack.Character.Faction)
                            {
                                if (session.CurrentMapInstance.Map.MapId != 130
                                    && session.CurrentMapInstance.Map.MapId != 131)
                                {
                                    PvpHit(session,
                                        new HitRequest(TargetHitType.SingleAoeTargetHit,
                                            session, ski.Skill), playerToAttack);
                                }
                                else
                                {
                                    session.SendPacket(
                                        StaticPacketHelper.Cancel(2, targetId));
                                }
                            }
                            else
                            {
                                session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                            }
                        }
                        else if (session.CurrentMapInstance.Map.MapTypes.Any(m =>
                            m.MapTypeId == (short) MapTypeEnum.PvpMap))
                        {
                            if (session.Character.Group?.IsMemberOfGroup(playerToAttack
                                    .Character.CharacterId) != true)
                            {
                                PvpHit(session,
                                    new HitRequest(TargetHitType.SingleAoeTargetHit,
                                        session, ski.Skill), playerToAttack);
                            }
                            else
                            {
                                session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                            }
                        }
                        else if (session.CurrentMapInstance.IsPvp)
                        {
                            if (session.Character.Group?.IsMemberOfGroup(playerToAttack
                                    .Character.CharacterId) != true)
                            {
                                PvpHit(session,
                                    new HitRequest(TargetHitType.SingleAoeTargetHit,
                                        session, ski.Skill), playerToAttack);
                            }
                            else
                            {
                                session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                            }
                        }
                        else
                        {
                            session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        }

                        //hit all other monsters
                        foreach (ClientSession character in playersInAoeRange)
                        {
                            if (session.CurrentMapInstance.Map.MapTypes.Any(s =>
                                s.MapTypeId == (short) MapTypeEnum.Act4))
                            {
                                if (session.Character.Faction
                                    != character.Character.Faction
                                    && session.CurrentMapInstance.Map.MapId != 130
                                    && session.CurrentMapInstance.Map.MapId != 131)
                                {
                                    PvpHit(session,
                                        new HitRequest(TargetHitType.SingleAoeTargetHit,
                                            session, ski.Skill), character);
                                }
                            }
                            else if (session.CurrentMapInstance.Map.MapTypes.Any(m =>
                                m.MapTypeId == (short) MapTypeEnum.PvpMap))
                            {
                                if (session.Character.Group?.IsMemberOfGroup(
                                        character.Character.CharacterId) != true)
                                {
                                    PvpHit(session,
                                        new HitRequest(TargetHitType.SingleAoeTargetHit,
                                            session, ski.Skill), character);
                                }
                            }
                            else if (session.CurrentMapInstance.IsPvp)
                            {
                                if (session.Character.Group?.IsMemberOfGroup(
                                        character.Character.CharacterId) != true)
                                {
                                    PvpHit(session,
                                        new HitRequest(TargetHitType.SingleAoeTargetHit,
                                            session, ski.Skill), character);
                                }
                            }
                        }

                        if (playerToAttack.Character.Hp <= 0)
                        {
                            session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        }
                    }
                }
                else
                {
                    ComboDTO skillCombo = ski.Skill.Combos.Find(s => ski.Hit == s.Hit);
                    if (skillCombo != null)
                    {
                        if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit
                            == ski.Hit)
                        {
                            ski.Hit = 0;
                        }

                        if (session.CurrentMapInstance.Map.MapTypes.Any(s =>
                            s.MapTypeId == (short) MapTypeEnum.Act4))
                        {
                            if (session.Character.Faction
                                != playerToAttack.Character.Faction)
                            {
                                if (session.CurrentMapInstance.Map.MapId != 130
                                    && session.CurrentMapInstance.Map.MapId != 131)
                                {
                                    PvpHit(session,
                                        new HitRequest(TargetHitType.SingleTargetHitCombo,
                                            session, ski.Skill, skillCombo: skillCombo),
                                        playerToAttack);
                                }
                                else
                                {
                                    session.SendPacket(
                                        StaticPacketHelper.Cancel(2, targetId));
                                }
                            }
                            else
                            {
                                session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                            }
                        }
                        else if (session.CurrentMapInstance.Map.MapTypes.Any(m =>
                            m.MapTypeId == (short) MapTypeEnum.PvpMap))
                        {
                            if (session.Character.Group?.IsMemberOfGroup(playerToAttack
                                    .Character.CharacterId) != true)
                            {
                                PvpHit(session,
                                    new HitRequest(TargetHitType.SingleTargetHitCombo,
                                        session, ski.Skill, skillCombo: skillCombo),
                                    playerToAttack);
                            }
                            else
                            {
                                session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                            }
                        }
                        else if (session.CurrentMapInstance.IsPvp)
                        {
                            if (session.CurrentMapInstance.MapInstanceId
                                != ServerManager.Instance.FamilyArenaInstance.MapInstanceId)
                            {
                                if (session.Character.Group?.IsMemberOfGroup(
                                        playerToAttack
                                            .Character.CharacterId) != true)
                                {
                                    PvpHit(session, new HitRequest(TargetHitType.SingleTargetHit,
                                        session,
                                        ski.Skill), playerToAttack);
                                }
                                else
                                {
                                    session.SendPacket(
                                        StaticPacketHelper.Cancel(2, targetId));
                                }
                            }
                            else
                            {
                                if (session.Character.Faction
                                    != playerToAttack.Character.Faction)
                                {
                                    PvpHit(session,
                                        new HitRequest(TargetHitType.SingleTargetHit,
                                            session, ski.Skill), playerToAttack);
                                }
                                else
                                {
                                    session.SendPacket(
                                        StaticPacketHelper.Cancel(2, targetId));
                                }
                            }
                        }
                        else
                        {
                            session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        }
                    }
                    else
                    {
                        if (session.CurrentMapInstance.Map.MapTypes.Any(s =>
                            s.MapTypeId == (short) MapTypeEnum.Act4))
                        {
                            if (session.Character.Faction
                                != playerToAttack.Character.Faction)
                            {
                                if (session.CurrentMapInstance.Map.MapId != 130
                                    && session.CurrentMapInstance.Map.MapId != 131)
                                {
                                    PvpHit(session,
                                        new HitRequest(TargetHitType.SingleTargetHit,
                                            session, ski.Skill), playerToAttack);
                                }
                                else
                                {
                                    session.SendPacket(
                                        StaticPacketHelper.Cancel(2, targetId));
                                }
                            }
                            else
                            {
                                session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                            }
                        }
                        else if (session.CurrentMapInstance.Map.MapTypes.Any(m =>
                            m.MapTypeId == (short) MapTypeEnum.PvpMap))
                        {
                            if (session.Character.Group?.IsMemberOfGroup(playerToAttack
                                    .Character.CharacterId) != true)
                            {
                                PvpHit(session,
                                    new HitRequest(TargetHitType.SingleTargetHit, session,
                                        ski.Skill), playerToAttack);
                            }
                            else
                            {
                                session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                            }
                        }
                        else if (session.CurrentMapInstance.IsPvp)
                        {
                            if (session.Character.Group?.IsMemberOfGroup(playerToAttack
                                    .Character.CharacterId) != true)
                            {
                                PvpHit(session,
                                    new HitRequest(TargetHitType.SingleTargetHit, session,
                                        ski.Skill), playerToAttack);
                            }
                            else
                            {
                                session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                            }
                        }
                        else
                        {
                            session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        }
                    }
                }
            }
        }

        private static void TargetHitRunRegularPveHit(ClientSession session, MapMonster monsterToAttack,
            CharacterSkill ski, CharacterSkill characterSkillInfo, long targetId)
        {
            if (ski.Skill.HitType == 3)
            {
                monsterToAttack.HitQueue.Enqueue(new HitRequest(
                    TargetHitType.SingleTargetHit, session, ski.Skill,
                    characterSkillInfo?.Skill.Effect ?? ski.Skill.Effect,
                    showTargetAnimation: true));

                foreach (long id in session.Character.MtListTargetQueue
                    .Where(s => s.EntityType == UserType.Monster).Select(s => s.TargetId))
                {
                    MapMonster mon = session.CurrentMapInstance.GetMonster(id);
                    if (mon?.CurrentHp > 0)
                    {
                        mon.HitQueue.Enqueue(new HitRequest(
                            TargetHitType.SingleAoeTargetHit, session, ski.Skill,
                            characterSkillInfo?.Skill.Effect ?? ski.Skill.Effect));
                    }
                }
            }
            else
            {
                if (ski.Skill.TargetRange != 0) // check if we will hit mutltiple targets
                {
                    ComboDTO skillCombo = ski.Skill.Combos.Find(s => ski.Hit == s.Hit);
                    if (skillCombo != null)
                    {
                        if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit
                            == ski.Hit)
                        {
                            ski.Hit = 0;
                        }

                        List<MapMonster> monstersInAoeRange = session.CurrentMapInstance?
                            .GetListMonsterInRange(monsterToAttack.MapX,
                                monsterToAttack.MapY, ski.Skill.TargetRange).ToList();
                        if (monstersInAoeRange != null && monstersInAoeRange.Count != 0)
                        {
                            foreach (MapMonster mon in monstersInAoeRange)
                            {
                                mon.HitQueue.Enqueue(
                                    new HitRequest(TargetHitType.SingleTargetHitCombo,
                                        session, ski.Skill, skillCombo: skillCombo));
                            }
                        }
                        else
                        {
                            session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        }

                        if (!monsterToAttack.IsAlive)
                        {
                            session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        }
                    }
                    else
                    {
                        List<MapMonster> monstersInAoeRange = session.CurrentMapInstance?
                            .GetListMonsterInRange(
                                monsterToAttack.MapX,
                                monsterToAttack.MapY,
                                ski.Skill.TargetRange)
                            ?.ToList();

                        //hit the targetted monster
                        monsterToAttack.HitQueue.Enqueue(
                            new HitRequest(TargetHitType.SingleTargetHit, session,
                                ski.Skill,
                                characterSkillInfo?.Skill.Effect ?? ski.Skill.Effect,
                                showTargetAnimation: true));

                        //hit all other monsters
                        if (monstersInAoeRange != null && monstersInAoeRange.Count != 0)
                        {
                            foreach (MapMonster mon in monstersInAoeRange.Where(m =>
                                m.MapMonsterId != monsterToAttack.MapMonsterId)) //exclude targetted monster
                            {
                                mon.HitQueue.Enqueue(
                                    new HitRequest(TargetHitType.SingleAoeTargetHit,
                                        session, ski.Skill,
                                        characterSkillInfo?.Skill.Effect ??
                                        ski.Skill.Effect));
                            }
                        }
                        else
                        {
                            session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        }

                        if (!monsterToAttack.IsAlive)
                        {
                            session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        }
                    }
                }
                else
                {
                    ComboDTO skillCombo = ski.Skill.Combos.Find(s => ski.Hit == s.Hit);
                    if (skillCombo != null)
                    {
                        if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit
                            == ski.Hit)
                        {
                            ski.Hit = 0;
                        }

                        monsterToAttack.HitQueue.Enqueue(
                            new HitRequest(TargetHitType.SingleTargetHitCombo, session,
                                ski.Skill, skillCombo: skillCombo));
                    }
                    else
                    {
                        monsterToAttack.HitQueue.Enqueue(
                            new HitRequest(TargetHitType.SingleTargetHit, session,
                                ski.Skill));
                    }
                }
            }
        }

        #endregion
    }
}