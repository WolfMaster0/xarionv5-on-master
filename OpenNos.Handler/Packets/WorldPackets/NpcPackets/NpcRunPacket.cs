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
using OpenNos.GameObject.Npc;

namespace OpenNos.Handler.Packets.WorldPackets.NpcPackets
{
    [PacketHeader("n_run")]
    public class NpcRunPacket
    {
        #region Properties

        public int NpcId { get; set; }

        public short Runner { get; set; }

        public short Type { get; set; }

        public short Value { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            NpcRunPacket packetDefinition = new NpcRunPacket();
            if (short.TryParse(packetSplit[2], out short runner))
            {
                packetDefinition.Runner = runner;

                if (packetSplit.Length > 3 && short.TryParse(packetSplit[3], out short type))
                {
                    packetDefinition.Type = type;
                }

                if (packetSplit.Length > 4 && short.TryParse(packetSplit[4], out short value))
                {
                    packetDefinition.Value = value;
                }

                if (packetSplit.Length > 5 && int.TryParse(packetSplit[5], out int npcId))
                {
                    packetDefinition.NpcId = npcId;
                }

                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(NpcRunPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            session.Character.LastNRunId = NpcId;
            session.Character.LastItemVNum = 0;
            if (session.Character.Hp > 0)
            {
                NRunHandler.NRun(session, NpcId, Runner, Type, Value);
            }
        }

        #endregion
    }
}