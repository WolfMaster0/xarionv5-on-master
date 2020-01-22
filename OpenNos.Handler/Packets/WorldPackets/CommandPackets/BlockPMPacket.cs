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
    [PacketHeader("$BlockPM", Authority = AuthorityType.GameMaster)]
    public class BlockPmPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            BlockPmPacket packetDefinition = new BlockPmPacket();
            packetDefinition.ExecuteHandler(session as ClientSession);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BlockPmPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$BlockPM";

        private void ExecuteHandler(ClientSession session)
        {
            Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(), "[BlockPM]");

            if (!session.Character.GmPvtBlock)
            {
                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GM_BLOCK_ENABLE"),
                    10));
                session.Character.GmPvtBlock = true;
            }
            else
            {
                session.SendPacket(
                    session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GM_BLOCK_DISABLE"), 10));
                session.Character.GmPvtBlock = false;
            }
        }

        #endregion
    }
}