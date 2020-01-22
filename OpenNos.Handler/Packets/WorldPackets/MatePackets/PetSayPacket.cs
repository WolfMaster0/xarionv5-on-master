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
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.MatePackets
{
    [PacketHeader("say_p")]
    public class PetSayPacket
    {
        #region Properties

        public string Message { get; set; }

        public int PetId { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] { ' ' }, 4);
            if (packetSplit.Length < 4)
            {
                return;
            }
            PetSayPacket packetDefinition = new PetSayPacket();
            string msg = packetSplit[3].Trim();
            if (int.TryParse(packetSplit[2], out int petId) && !string.IsNullOrEmpty(msg))
            {
                packetDefinition.PetId = petId;
                packetDefinition.Message = msg;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(PetSayPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            Mate mate = session.Character.Mates.Find(s => s.MateTransportId == PetId);
            if (mate != null)
            {
                session.CurrentMapInstance.Broadcast(StaticPacketHelper.Say(UserType.Npc, mate.MateTransportId, 2, Message));
            }
        }

        #endregion
    }
}