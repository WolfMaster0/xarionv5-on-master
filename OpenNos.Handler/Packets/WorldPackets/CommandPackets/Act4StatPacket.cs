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
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Act4Stat", Authority = AuthorityType.GameMaster)]
    public class Act4StatPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public byte Faction { get; set; }

        public int Value { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (!(session is ClientSession sess))
            {
                return;
            }
            if (packetSplit.Length < 4)
            {
                sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                return;
            }
            Act4StatPacket packetDefinition = new Act4StatPacket();
            if (byte.TryParse(packetSplit[2], out byte faction) && int.TryParse(packetSplit[3], out int value))
            {
                packetDefinition._isParsed = true;
                packetDefinition.Faction = faction;
                packetDefinition.Value = value;
            }
            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(Act4StatPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Act4Stat FACTION VALUE";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed && ServerManager.Instance.ChannelId == 51)
            {
                switch (Faction)
                {
                    case 1:
                        ServerManager.Instance.Act4AngelStat.Percentage = Value;
                        break;

                    case 2:
                        ServerManager.Instance.Act4DemonStat.Percentage = Value;
                        break;
                }

                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}