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
using OpenNos.Core.Serializing;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("pulse")]
    public class PulsePacket
    {
        #region Properties

        public int Tick { get; set; }

        public bool IsAfk { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            PulsePacket packetDefinition = new PulsePacket();
            if (int.TryParse(packetSplit[2], out int tick))
            {
                packetDefinition.Tick = tick;
                packetDefinition.IsAfk = packetSplit[3] == "1";
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(PulsePacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.LastPulse.AddMilliseconds(80000) >= DateTime.UtcNow
                && DateTime.UtcNow >= session.Character.LastPulse.AddMilliseconds(40000))
            {
                session.Character.LastPulse = DateTime.UtcNow;
                session.Character.IsAfk = IsAfk;
                session.Character.MuteMessage();
                session.Character.DeleteTimeout();
                CommunicationServiceClient.Instance.PulseAccount(session.Account.AccountId);
            }
            else
            {
                session.Disconnect();
            }
        }

        #endregion
    }
}