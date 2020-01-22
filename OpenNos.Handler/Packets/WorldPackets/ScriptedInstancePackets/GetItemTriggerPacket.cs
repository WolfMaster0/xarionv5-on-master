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

namespace OpenNos.Handler.Packets.WorldPackets.ScriptedInstancePackets
{
    [PacketHeader("git")]
    public class GetItemTriggerPacket
    {
        #region Properties

        public int ButtonId { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            GetItemTriggerPacket packetDefinition = new GetItemTriggerPacket();
            if (int.TryParse(packetSplit[2], out int buttonId))
            {
                packetDefinition.ButtonId = buttonId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(GetItemTriggerPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            MapButton button = session.CurrentMapInstance.Buttons.Find(s => s.MapButtonId == ButtonId);
            if (button != null)
            {
                session.CurrentMapInstance.Broadcast(StaticPacketHelper.Out(UserType.Object, button.MapButtonId));
                button.RunAction();
                session.CurrentMapInstance.Broadcast(button.GenerateIn());
            }
        }

        #endregion
    }
}