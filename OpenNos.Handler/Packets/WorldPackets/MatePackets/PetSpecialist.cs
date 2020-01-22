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
using System.Diagnostics;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.MatePackets
{
    [PacketHeader("psl")]
    public class PetSpecialist
    {
        #region Properties

        public int Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            PetSpecialist packetDefinition = new PetSpecialist();
            if (int.TryParse(packetSplit[2], out int type))
            {
                packetDefinition.Type = type;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(PetSpecialist), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            Mate mate = session.Character.Mates.Find(x => x.IsTeamMember && x.MateType == MateType.Partner);
            if (mate == null)
            {
                return;
            }

            if (Type == 0)
            {
                if (mate.SpInstance == null)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NEED_WEAR_PARTNER_SP"), 0));
                    return;
                }

                var currentRunningSeconds = (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.AddHours(-2).AddSeconds(-50)).TotalSeconds;
                var timeSpanSinceLastUsage = currentRunningSeconds - mate.LastSp;

                if (timeSpanSinceLastUsage < mate.SpCooldown)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PARTNER_SP_IN_COOLDOWN"), mate.SpCooldown - (int)Math.Round(timeSpanSinceLastUsage, 0)), 0));
                    return;
                }

                if (mate.IsUsingSp)
                {
                    mate.RemoveSp();
                }
                else
                {
                    session.SendPacket("pdelay 5000 3 #psl^1 ");
                    session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(2, 2, mate.MateTransportId), mate.PositionX, mate.PositionY);
                }
            }
            else
            {
                if (mate.SpInstance == null)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NEED_WEAR_PARTNER_SP"), 0));
                    return;
                }

                mate.IsUsingSp = true;
                session.SendPacket(mate.GenerateCond());
                session.Character.MapInstance.Broadcast(mate.GenerateCMode(mate.SpInstance.Item.Morph));
                session.SendPacket(mate.GenerateScPacket());
                session.Character.MapInstance.Broadcast(mate.GenerateOut());
                session.Character.MapInstance.Broadcast(mate.GenerateIn());
                session.SendPacket(session.Character.GeneratePinit());
                session.Character.MapInstance.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, mate.MateTransportId, 196));
                session.SendPacket(mate.GeneratePski());
                MateHelper.Instance.AddPartnerBuffs(session, mate);
            }
        }

        #endregion
    }
}