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
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;

namespace OpenNos.Handler.Packets.WorldPackets.ScriptedInstancePackets
{
    [PacketHeader("treq")]
    public class TreqPacket
    {
        #region Properties

        public byte? RecordPress { get; set; }

        public byte? StartPress { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            TreqPacket packetDefinition = new TreqPacket();
            if (int.TryParse(packetSplit[2], out int x)
                && int.TryParse(packetSplit[3], out int y))
            {
                packetDefinition.X = x;
                packetDefinition.Y = y;
                packetDefinition.StartPress = packetSplit.Length >= 5
                    && byte.TryParse(packetSplit[4], out byte startPress) ? startPress : (byte?)null;
                packetDefinition.StartPress = packetSplit.Length >= 5
                    && byte.TryParse(packetSplit[4], out byte recordPress) ? recordPress : (byte?)null;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(TreqPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            ScriptedInstance orgTimespace = session.CurrentMapInstance.ScriptedInstances
                .Find(s => X == s.PositionX && Y == s.PositionY);

            if (orgTimespace != null)
            {
                ScriptedInstance timespace = new ScriptedInstance(orgTimespace);
                timespace.LoadGlobals();
                if (StartPress == 1 || RecordPress == 1)
                {
                    session.EnterInstance(timespace);
                }
                else
                {
                    session.SendPacket(timespace.GenerateRbr());
                }
            }
        }

        #endregion
    }
}