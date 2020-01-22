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

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Guri", Authority = AuthorityType.GameMaster)]
    public class GuriPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public byte Argument { get; set; }

        public byte Type { get; set; }

        public int Value { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(' ');
                if (packetSplit.Length < 5)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                GuriPacket packetDefinition = new GuriPacket();
                if (byte.TryParse(packetSplit[2], out byte type) && byte.TryParse(packetSplit[3], out byte arg) && int.TryParse(packetSplit[4], out int value))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.Type = type;
                    packetDefinition.Argument = arg;
                    packetDefinition.Value = value;
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(GuriPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Guri TYPE ARGUMENT VALUE";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[Guri]Type: {Type} Value: {Value} Arguments: {Argument}");

                session.SendPacket(UserInterfaceHelper.GenerateGuri(Type, Argument,
                    session.Character.CharacterId, Value));
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}