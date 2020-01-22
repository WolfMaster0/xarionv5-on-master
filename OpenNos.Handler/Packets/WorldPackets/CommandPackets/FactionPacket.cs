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
    [PacketHeader("$Faction", Authority = AuthorityType.GameMaster)]
    public class FactionPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                FactionPacket packetDefinition = new FactionPacket();
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(FactionPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Faction";

        private void ExecuteHandler(ClientSession session)
        {
            session.SendPacket("scr 0 0 0 0 0 0 0");
            session.SendPacket(session.Character.GenerateFaction());
            if (session.Character.Faction == FactionType.Angel)
            {
                session.Character.Faction = FactionType.Demon;
                session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId,
                    4801));
                session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("GET_PROTECTION_POWER_2"),
                        0));
            }
            else
            {
                session.Character.Faction = FactionType.Angel;
                session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId,
                    4800));
                session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("GET_PROTECTION_POWER_1"),
                        0));
            }
        }

        #endregion
    }
}