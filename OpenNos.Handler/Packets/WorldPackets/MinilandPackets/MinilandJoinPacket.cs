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

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.MinilandPackets
{
    [PacketHeader("mjoin")]
    public class MinilandJoinPacket
    {
        #region Properties

        public long CharacterId { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            MinilandJoinPacket packetDefinition = new MinilandJoinPacket();
            if (long.TryParse(packetSplit[3], out long characterId))
            {
                packetDefinition.CharacterId = characterId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MinilandJoinPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            ClientSession sess = ServerManager.Instance.GetSessionByCharacterId(CharacterId);
            if (sess?.Character != null)
            {
                if (sess.Character.MinilandState == MinilandState.Open)
                {
                    ServerManager.Instance.JoinMiniland(session, sess);
                }
                else
                {
                    session.SendPacket(UserInterfaceHelper.GenerateInfo(
                        Language.Instance.GetMessageFromKey("MINILAND_CLOSED_BY_FRIEND")));
                }
            }
        }

        #endregion
    }
}