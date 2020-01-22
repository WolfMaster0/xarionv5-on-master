using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;

namespace OpenNos.Handler.Packets.WorldPackets.MatePackets
{
    [PacketHeader("u_pet")]
    public class UpetPacket
    {
        #region Properties

        public long MateTransportId { get; set; }

        public UserType TargetType { get; set; }

        public long TargetId { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 5)
            {
                return;
            }
            UpetPacket packetDefinition = new UpetPacket();
            if (long.TryParse(packetSplit[2], out var transportId) &&
                long.TryParse(packetSplit[3], out var targetType) &&
                long.TryParse(packetSplit[4], out var targetId))
            {
                packetDefinition.MateTransportId = transportId;
                packetDefinition.TargetType = (UserType)targetType;
                packetDefinition.TargetId = targetId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(UpetPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            PenaltyLogDTO penalty = session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (session.Character.IsMuted() && penalty != null)
            {
                if (session.Character.Gender == GenderType.Female)
                {
                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }
                else
                {
                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }

                return;
            }

            Mate attacker = session.Character.Mates.FirstOrDefault(x => x.MateTransportId == MateTransportId);
            if (attacker == null)
            {
                return;
            }

            NpcMonsterSkill mateSkill = null;
            if (attacker.Monster.Skills.Any())
            {
                mateSkill = attacker.Monster.Skills.FirstOrDefault(sk => MateHelper.Instance.PetSkills.Contains(sk.SkillVNum));
            }

            if (mateSkill == null)
            {
                mateSkill = new NpcMonsterSkill
                {
                    SkillVNum = 200
                };
            }

            if (attacker.IsSitting)
            {
                return;
            }

            MapMonster target = session.CurrentMapInstance?.GetMonster(TargetId);
            switch (TargetType)
            {
                case UserType.Monster:
                case UserType.Npc:
                    if (attacker.Hp <= 0)
                    {
                        return;
                    }

                    if (target != null)
                    {
                        session.CurrentMapInstance.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Npc, attacker.MateTransportId, 3, target.MapMonsterId, mateSkill.Skill.CastAnimation, mateSkill.Skill.CastEffect, mateSkill.SkillVNum));
                        session.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Npc, attacker.MateTransportId, 3, target.MapMonsterId, mateSkill.SkillVNum, mateSkill.Skill.Cooldown, mateSkill.Skill.AttackAnimation, mateSkill.Skill.Effect, target.MapX, target.MapY, target.CurrentHp > 0, (int)(target.CurrentHp / (double)target.MaxHp * 100), 0, 0, 0));
                    }

                    session.AttackMonster(attacker, mateSkill.Skill, TargetId, target?.MapX ?? attacker.PositionX, target?.MapY ?? attacker.PositionY);
                    session.SendPacketAfter("petsr 0", mateSkill.Skill.Cooldown * 100);

                    return;

                case UserType.Player:
                    if (attacker.Hp <= 0)
                    {
                        return;
                    }

                    Character targetChar = session.CurrentMapInstance?.GetSessionByCharacterId(TargetId)
                        ?.Character;

                    if (targetChar != null && session.CurrentMapInstance != null &&
                        (targetChar.Session.CurrentMapInstance == session.CurrentMapInstance
                            && targetChar.CharacterId != session.Character.CharacterId &&
                            session.CurrentMapInstance.Map.MapTypes.Any(s =>
                                s.MapTypeId == (short)MapTypeEnum.Act4) && session.Character.Faction !=
                            targetChar.Faction && session.CurrentMapInstance.Map
                                .MapId != 130 && session.CurrentMapInstance.Map
                                .MapId != 131 ||
                            session.CurrentMapInstance.Map.MapTypes.Any(m =>
                                m.MapTypeId == (short)MapTypeEnum.PvpMap) &&
                            (session.Character.Group == null
                                || !session.Character.Group
                                    .IsMemberOfGroup(
                                        targetChar.CharacterId)) ||
                            session.CurrentMapInstance.IsPvp && (session.Character.Group == null
                                || !session.Character.Group.IsMemberOfGroup(
                                    targetChar.CharacterId))))
                    {
                        session.AttackCharacter(attacker, mateSkill, targetChar);
                    }
                    return;

                case UserType.Object:
                    return;

                default:
                    return;
            }
        }

        #endregion
    }
}
