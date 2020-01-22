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

using OpenNos.Core.Serializing;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("npinfo")]
    public class NpInfoPacket
    {
        #region Properties

        public byte Page { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            NpInfoPacket packetDefinition = new NpInfoPacket();
            if (byte.TryParse(packetSplit[2], out byte page))
            {
                packetDefinition.Page = page;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(NpInfoPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            session.SendPacket(session.Character.GenerateStatChar());
            session.Character.ScPage = Page;
            session.SendPacket(UserInterfaceHelper.GeneratePClear());
            session.SendPackets(session.Character.GenerateScP(Page));
            session.SendPackets(session.Character.GenerateScN());
        }

        #endregion
    }
}