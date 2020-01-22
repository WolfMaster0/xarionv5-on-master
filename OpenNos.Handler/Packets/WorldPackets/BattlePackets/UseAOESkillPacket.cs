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
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;

namespace OpenNos.Handler.Packets.WorldPackets.BattlePackets
{
    [PacketHeader("u_as")]
    public class UseAoeSkillPacket
    {
        #region Properties

        public int CastId { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 5)
            {
                return;
            }
            UseAoeSkillPacket packetDefinition = new UseAoeSkillPacket();
            if (int.TryParse(packetSplit[2], out int castId)
                && short.TryParse(packetSplit[3], out short mapX)
                && short.TryParse(packetSplit[4], out short mapY))
            {
                packetDefinition.CastId = castId;
                packetDefinition.MapX = mapX;
                packetDefinition.MapY = mapY;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(UseAoeSkillPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            bool isMuted = session.Character.MuteMessage();
            if (isMuted || session.Character.IsVehicled)
            {
                session.SendPacket(StaticPacketHelper.Cancel());
            }
            else
            {
                if (session.Character.LastTransform.AddSeconds(3) > DateTime.UtcNow)
                {
                    session.SendPacket(StaticPacketHelper.Cancel());
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_ATTACK"), 0));
                    return;
                }

                if (session.Character.CanFight && session.Character.Hp > 0)
                {
                    session.ZoneHit(CastId, MapX, MapY);
                }
            }
        }

        #endregion
    }
}