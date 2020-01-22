// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.

using System;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Ban", Authority = AuthorityType.GameMaster)]
    public class BanPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string CharacterName { get; set; }

        public int Duration { get; set; }

        public string Reason { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] { ' ' }, 5);
            if (!(session is ClientSession sess))
            {
                return;
            }
            if (packetSplit.Length < 4)
            {
                sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                return;
            }
            BanPacket packetDefinition = new BanPacket();
            if (!string.IsNullOrEmpty(packetSplit[2]) && int.TryParse(packetSplit[3], out int duration))
            {
                packetDefinition._isParsed = true;
                packetDefinition.CharacterName = packetSplit[2];
                packetDefinition.Duration = duration;
                packetDefinition.Reason = string.IsNullOrEmpty(packetSplit[4]) ? string.Empty : packetSplit[4].Trim();
            }
            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BanPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Ban CHARACTERNAME DURATION(DAYS) REASON";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[Ban]CharacterName: {CharacterName} Reason: {Reason} Until: {(Duration == 0 ? DateTime.UtcNow.AddYears(15) : DateTime.UtcNow.AddDays(Duration))}");
                session.BanMethod(CharacterName, Duration, Reason);
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}