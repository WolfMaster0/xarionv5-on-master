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
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Help", Authority = AuthorityType.User)]
    public class HelpPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string Contents { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] { ' ' }, 3);
            if (!(session is ClientSession sess))
            {
                return;
            }
            HelpPacket packetDefinition = new HelpPacket();
            if (packetSplit.Length >= 3 && !string.IsNullOrEmpty(packetSplit[2]))
            {
                packetDefinition._isParsed = true;
                packetDefinition.Contents = packetSplit[2];
            }
            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(HelpPacket), HandlePacket);

        // we dont want help to be displayed in Help lol
        //public static string ReturnHelp() => "$Help";

        private void ExecuteHandler(ClientSession session)
        {
            Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(), "[Help]");

            List<string> messages = PacketFacility.GetHelpMessages(session.Account.Authority).ToList();

            // send messages
            messages.Sort();
            if (Contents == "*" || !_isParsed)
            {
                session.SendPacket(session.Character.GenerateSay("-------------Commands Info-------------", 11));
                foreach (string message in messages)
                {
                    session.SendPacket(session.Character.GenerateSay(message, 12));
                }
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay("-------------Command Info-------------", 11));
                foreach (string message in messages.Where(s =>
                    s.IndexOf(Contents, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    session.SendPacket(session.Character.GenerateSay(message, 12));
                }
            }

            session.SendPacket(session.Character.GenerateSay("-----------------------------------------------", 11));
        }

        #endregion
    }
}