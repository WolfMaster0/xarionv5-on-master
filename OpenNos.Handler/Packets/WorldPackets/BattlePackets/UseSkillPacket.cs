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
using System.Reactive.Linq;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;

namespace OpenNos.Handler.Packets.WorldPackets.BattlePackets
{
    [PacketHeader("u_s")]
    public class UseSkillPacket
    {
        #region Properties

        public int CastId { get; set; }

        public int MapMonsterId { get; set; }

        public short? MapX { get; set; }

        public short? MapY { get; set; }

        public UserType UserType { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 5)
            {
                return;
            }
            UseSkillPacket packetDefinition = new UseSkillPacket();
            if (int.TryParse(packetSplit[2], out int castId)
                && Enum.TryParse(packetSplit[3], out UserType userType)
                && int.TryParse(packetSplit[4], out int mapMonsterId))
            {
                packetDefinition.CastId = castId;
                packetDefinition.UserType = userType;
                packetDefinition.MapMonsterId = mapMonsterId;
                packetDefinition.MapX = packetSplit.Length >= 6 && short.TryParse(packetSplit[5], out short mapX) ? mapX : (short?)null;
                packetDefinition.MapY = packetSplit.Length >= 7 && short.TryParse(packetSplit[6], out short mapY) ? mapY : (short?)null;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(UseSkillPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            CharacterSkill ski =(session.Character.UseSp
                    ? session.Character.SkillsSp?.GetAllItems()
                    : session.Character.Skills?.GetAllItems())?.Find(s =>
                    s.Skill?.CastId == CastId && (s.Skill?.UpgradeSkill == 0 || s.Skill?.UpgradeSkill == 3));
            if (session.Character.NoAttack > 0 || ski?.CanBeUsed() != true)
            {
                session.SendPacket(StaticPacketHelper.Cancel(2));
                return;
            }

            if (session.Character.CanFight)
            {
                bool isMuted = session.Character.MuteMessage();
                if (isMuted || session.Character.IsVehicled || session.Character.InvisibleGm)
                {
                    session.SendPacket(StaticPacketHelper.Cancel());
                    return;
                }

                bool sendCoordinates = false;
                if (MapX.HasValue && MapY.HasValue)
                {
                    session.Character.PositionX = MapX.Value;
                    session.Character.PositionY = MapY.Value;
                    sendCoordinates = true;
                }

                if (session.Character.IsSitting)
                {
                    session.Character.Rest();
                }

                switch (UserType)
                {
                    case UserType.Monster:
                        if (session.Character.Hp > 0)
                        {
                            session.TargetHit(CastId, MapMonsterId, sendCoordinates: sendCoordinates);
                            int[] fairyWings = session.Character.GetBuff(BCardType.CardType.EffectSummon, 11);
                            int random = ServerManager.RandomNumber();
                            if (fairyWings[0] > random)
                            {
                                Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(o =>
                                {
                                    ski.LastUse = DateTime.UtcNow.AddMilliseconds(ski.Skill.Cooldown * 100 * -1);
                                    session.SendPacket(StaticPacketHelper.SkillReset(CastId));
                                });
                            }
                        }
                        break;

                    case UserType.Player:
                        if (session.Character.Hp > 0)
                        {
                            if (MapMonsterId != session.Character.CharacterId)
                            {
                                session.TargetHit(CastId, MapMonsterId, true, sendCoordinates);
                            }
                            else
                            {
                                session.TargetHit(CastId, MapMonsterId, sendCoordinates: sendCoordinates);
                            }

                            int[] fairyWings = session.Character.GetBuff(BCardType.CardType.EffectSummon, 11);
                            int random = ServerManager.RandomNumber();
                            if (fairyWings[0] > random)
                            {
                                Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(o =>
                                {
                                    ski.LastUse = DateTime.UtcNow.AddMilliseconds(ski.Skill.Cooldown * 100 * -1);
                                    session.SendPacket(StaticPacketHelper.SkillReset(CastId));
                                });
                            }
                        }
                        else
                        {
                            session.SendPacket(StaticPacketHelper.Cancel(2));
                        }
                        break;

                    default:
                        session.SendPacket(StaticPacketHelper.Cancel(2));
                        return;
                }

                Observable.Timer(TimeSpan.FromMilliseconds(200)).Subscribe(observer =>
                {
                    session.Character.RemoveBuff(614);
                    session.Character.RemoveBuff(615);
                    session.Character.RemoveBuff(616);
                });
            }
            else
            {
                session.SendPacket(StaticPacketHelper.Cancel(2));
            }
        }

        #endregion
    }
}