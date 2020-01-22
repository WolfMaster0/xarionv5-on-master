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

using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Buff", Authority = AuthorityType.GameMaster)]
    public class BuffPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public short CardId { get; set; }

        public byte? Level { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (!(session is ClientSession sess))
            {
                return;
            }

            if (packetSplit.Length < 3)
            {
                sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                return;
            }

            BuffPacket packetDefinition = new BuffPacket();
            if (short.TryParse(packetSplit[2], out short cardId))
            {
                packetDefinition._isParsed = true;
                packetDefinition.CardId = cardId;
                packetDefinition.Level = packetSplit.Length >= 4
                                         && byte.TryParse(packetSplit[3], out byte level)
                    ? level
                    : (byte?) null;
            }

            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BuffPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Buff CARDID LEVEL(?)";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Buff buff = new Buff(CardId, Level ?? 1);
                session.Character.AddBuff(buff);
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}