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
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("dir")]
    public class DirectionPacket
    {
        #region Properties

        public long CharacterId { get; set; }

        public byte Direction { get; set; }

        public int Option { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 5)
            {
                return;
            }
            DirectionPacket packetDefinition = new DirectionPacket();
            if (byte.TryParse(packetSplit[2], out byte dir)
                && int.TryParse(packetSplit[3], out int option)
                && long.TryParse(packetSplit[4], out long charId))
            {
                packetDefinition.Direction = dir;
                packetDefinition.Option = option;
                packetDefinition.CharacterId = charId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(DirectionPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (CharacterId == session.Character.CharacterId)
            {
                session.Character.Direction = Direction;
                session.CurrentMapInstance?.Broadcast(session.Character.GenerateDir());
            }
        }

        #endregion
    }
}