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
using System.Linq;
using System.Collections.Generic;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;

namespace OpenNos.Handler.Packets.WorldPackets.MatePackets
{
    [PacketHeader("suctl")]
    public class MateControlPacket
    {
        #region Properties

        public int CastId { get; set; }

        public int MateTransportId { get; set; }

        public int TargetId { get; set; }

        public UserType TargetType { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 7)
            {
                return;
            }
            MateControlPacket packetDefinition = new MateControlPacket();
            if (int.TryParse(packetSplit[2], out int castId)
                && int.TryParse(packetSplit[4], out int mateTransportId)
                && Enum.TryParse(packetSplit[5], out UserType targetType)
                && int.TryParse(packetSplit[6], out int targetId))
            {
                packetDefinition.CastId = castId;
                packetDefinition.MateTransportId = mateTransportId;
                packetDefinition.TargetType = targetType;
                packetDefinition.TargetId = targetId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MateControlPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            PenaltyLogDTO penalty = session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (session.Character.IsMuted() && penalty != null)
            {
                if (session.Character.Gender == GenderType.Female)
                {
                    session.CurrentMapInstance?.Broadcast(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    session.SendPacket(session.Character.GenerateSay(
                        string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"),
                            (penalty.DateEnd - DateTime.UtcNow).ToString("hh\\:mm\\:ss")), 11));
                }
                else
                {
                    session.CurrentMapInstance?.Broadcast(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    session.SendPacket(session.Character.GenerateSay(
                        string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"),
                            (penalty.DateEnd - DateTime.UtcNow).ToString("hh\\:mm\\:ss")), 11));
                }

                return;
            }

            Mate attacker = session.Character.Mates.First(x => x.MateTransportId == MateTransportId);
            if (attacker != null)
            {
                switch (TargetType)
                {
                    case UserType.Monster:
                        if (attacker.IsAlive)
                        {
                             MapMonster target = session.CurrentMapInstance?.GetMonster(TargetId);
                            NpcMonsterSkill skill = attacker.Monster.Skills.Find(x => x.NpcMonsterSkillId == CastId);
                            session.AttackMonster(attacker, skill?.Skill, target);
                        }
                        return;

                    case UserType.Player:
                        if (attacker.IsAlive)
                        {
                            Character target = session.CurrentMapInstance?.GetSessionByCharacterId(TargetId)
                                ?.Character;
                            if (target != null && session.CurrentMapInstance != null
                                && ((target.Session.CurrentMapInstance == session.CurrentMapInstance
                                 && target.CharacterId != session.Character.CharacterId
                                 && session.CurrentMapInstance.Map.MapTypes.Any(s =>
                                     s.MapTypeId == (short)MapTypeEnum.Act4) && session.Character.Faction
                                 != target.Faction && session.CurrentMapInstance.Map
                                     .MapId != 130 && session.CurrentMapInstance.Map
                                     .MapId != 131)
                                 || (session.CurrentMapInstance.Map.MapTypes.Any(m =>
                                     m.MapTypeId == (short)MapTypeEnum.PvpMap)
                                 && (session.Character.Group?
                                      .IsMemberOfGroup(
                                          target.CharacterId) != true))
                                 || (session.CurrentMapInstance.IsPvp && (session.Character.Group?.IsMemberOfGroup(
                                                                          target.CharacterId) != true))))
                            {
                                NpcMonsterSkill skill = attacker.Monster.Skills.Find(x => x.NpcMonsterSkillId == CastId);
                                session.AttackCharacter(attacker, skill?.Skill, target);
                            }
                        }
                        return;

                    case UserType.Npc:
                    case UserType.Object:
                        return;
                }
            }
        }
        #endregion
    }
}