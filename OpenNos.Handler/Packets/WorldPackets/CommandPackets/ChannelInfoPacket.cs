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
    [PacketHeader("$ChannelInfo", Authority = AuthorityType.GameMaster)]
    public class ChannelInfoPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            ChannelInfoPacket packetDefinition = new ChannelInfoPacket();
            packetDefinition.ExecuteHandler(session as ClientSession);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ChannelInfoPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$ChannelInfo";

        private void ExecuteHandler(ClientSession session)
        {
            Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(), "[ChannelInfo]");

            session.SendPacket(session.Character.GenerateSay(
                $"-----------Channel Info-----------\n-------------Channel:{ServerManager.Instance.ChannelId}-------------",
                11));
            foreach (ClientSession targetSession in ServerManager.Instance.Sessions)
            {
                session.SendPacket(
                    session.Character.GenerateSay(
                        $"CharacterName: {targetSession.Character.Name} SessionId: {targetSession.SessionId}", 12));
            }

            session.SendPacket(session.Character.GenerateSay("----------------------------------------", 11));
        }

        #endregion
    }
}