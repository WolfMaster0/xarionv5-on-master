using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    [PacketHeader("u_ps")]
    public class UpsPacket
    {
        #region Properties

        public long MateTransportId { get; set; }

        public UserType TargetType { get; set; }

        public long TargetId { get; set; }

        public int SkillSlot { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 6)
            {
                return;
            }
            UpsPacket packetDefinition = new UpsPacket();
            if (long.TryParse(packetSplit[2], out var transportId) &&
                long.TryParse(packetSplit[3], out var targetType) &&
                long.TryParse(packetSplit[4], out var targetId) &&
                int.TryParse(packetSplit[5], out var skillSlot))
            {
                packetDefinition.MateTransportId = transportId;
                packetDefinition.TargetType = (UserType)targetType;
                packetDefinition.TargetId = targetId;
                packetDefinition.SkillSlot = skillSlot;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(UpsPacket), HandlePacket);

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
            if (attacker?.SpInstance == null || !attacker.IsUsingSp)
            {
                return;
            }

            short? skillVnum = null;
            byte value = 0;
            switch (SkillSlot)
            {
                case 0:
                    skillVnum = attacker.SpInstance.FirstPartnerSkill;
                    value = 0;
                    break;
                case 1:
                    skillVnum = attacker.SpInstance.SecondPartnerSkill;
                    value = 1;
                    break;
                case 2:
                    skillVnum = attacker.SpInstance.ThirdPartnerSkill;
                    value = 2;
                    break;
            }

            if (skillVnum == null)
            {
                return;
            }

            var mateSkill = ServerManager.GetSkill(skillVnum.Value);

            if (mateSkill == null)
            {
                return;
            }

            session.SendPacketAfter($"psr {value}", mateSkill.Cooldown * 100);

            if (attacker.IsSitting)
            {
                return;
            }

            attacker.Owner.MapInstance.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, attacker.MateTransportId, 5005), attacker.PositionX, attacker.PositionY);
            switch (TargetType)
            {
                case UserType.Monster:
                    if (attacker.Hp <= 0)
                    {
                        return;
                    }

                    var target = session.Character.MapInstance.GetMonster(TargetId);

                    if (target != null)
                    {
                        session.CurrentMapInstance.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Npc, attacker.MateTransportId, 3, target.MapMonsterId, mateSkill.CastAnimation, mateSkill.CastEffect, mateSkill.SkillVNum));
                        session.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Npc, attacker.MateTransportId, 3, target.MapMonsterId, mateSkill.SkillVNum, mateSkill.Cooldown, mateSkill.AttackAnimation, mateSkill.Effect, target.MapX, target.MapY, target.CurrentHp > 0, (int)(target.CurrentHp / (double)target.MaxHp * 100), 0, 0, 0));
                    }

                    session.AttackMonster(attacker, mateSkill, TargetId, target?.MapX ?? attacker.PositionX, target?.MapY ?? attacker.PositionY);

                    return;

                case UserType.Npc:
                    if (attacker.Hp <= 0)
                    {
                        return;
                    }

                    var npcTarget = session.Character.MapInstance.GetNpc(TargetId);

                    if (npcTarget != null)
                    {
                        session.CurrentMapInstance.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Npc, attacker.MateTransportId, 3, npcTarget.MapNpcId, mateSkill.CastAnimation, mateSkill.CastEffect, mateSkill.SkillVNum));
                        session.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Npc, attacker.MateTransportId, 3, npcTarget.MapNpcId, mateSkill.SkillVNum, mateSkill.Cooldown, mateSkill.AttackAnimation, mateSkill.Effect, npcTarget.MapX, npcTarget.MapY, true, 100, 0, 0, 0));
                    }

                    session.AttackMonster(attacker, mateSkill, TargetId, npcTarget?.MapX ?? attacker.PositionX, npcTarget?.MapY ?? attacker.PositionY);
                    return;

                case UserType.Player:
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
