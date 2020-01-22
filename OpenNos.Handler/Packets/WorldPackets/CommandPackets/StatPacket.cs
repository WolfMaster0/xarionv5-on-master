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
using System.Diagnostics;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Stat", Authority = AuthorityType.Moderator)]
    public class StatPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                StatPacket packetDefinition = new StatPacket();
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(StatPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Stat ";

        private void ExecuteHandler(ClientSession session)
        {
            Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(), "[Stat]");

            session.SendPacket(session.Character.GenerateSay(
                $"{Language.Instance.GetMessageFromKey("XP_RATE_NOW")}: {ServerManager.Instance.Configuration.RateXP} ",
                13));
            session.SendPacket(session.Character.GenerateSay(
                $"{Language.Instance.GetMessageFromKey("DROP_RATE_NOW")}: {ServerManager.Instance.Configuration.RateDrop} ",
                13));
            session.SendPacket(session.Character.GenerateSay(
                $"{Language.Instance.GetMessageFromKey("GOLD_RATE_NOW")}: {ServerManager.Instance.Configuration.RateGold} ",
                13));
            session.SendPacket(session.Character.GenerateSay(
                $"{Language.Instance.GetMessageFromKey("GOLD_DROPRATE_NOW")}: {ServerManager.Instance.Configuration.RateGoldDrop} ",
                13));
            session.SendPacket(session.Character.GenerateSay(
                $"{Language.Instance.GetMessageFromKey("HERO_XPRATE_NOW")}: {ServerManager.Instance.Configuration.RateHeroicXP} ",
                13));
            session.SendPacket(session.Character.GenerateSay(
                $"{Language.Instance.GetMessageFromKey("FAIRYXP_RATE_NOW")}: {ServerManager.Instance.Configuration.RateFairyXP} ",
                13));
            session.SendPacket(session.Character.GenerateSay(
                $"{Language.Instance.GetMessageFromKey("SERVER_WORKING_TIME")}: {(Process.GetCurrentProcess().StartTime.AddHours(-2) - DateTime.UtcNow).ToString(@"d\ hh\:mm\:ss")} ",
                13));

            foreach (string message in CommunicationServiceClient.Instance.RetrieveServerStatistics())
            {
                session.SendPacket(session.Character.GenerateSay(message, 13));
            }
        }

        #endregion
    }
}