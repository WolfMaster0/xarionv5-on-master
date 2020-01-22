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
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.ScriptedInstancePackets
{
    [PacketHeader("escape")]
    public class EscapePacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            EscapePacket packetDefinition = new EscapePacket();
            packetDefinition.ExecuteHandler(session as ClientSession);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(EscapePacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance)
            {
                ServerManager.Instance.ChangeMap(session.Character.CharacterId, session.Character.MapId,
                    session.Character.MapX, session.Character.MapY);
                session.Character.Timespace = null;
            }
            else if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
            {
                ServerManager.Instance.ChangeMap(session.Character.CharacterId, session.Character.MapId,
                    session.Character.MapX, session.Character.MapY);
                session.Character.Group?.Characters.ForEach(
                    sess => sess.SendPacket(sess.Character.Group.GenerateRdlst()));
                session.SendPacket(session.Character.GenerateRaid(1, true));
                session.SendPacket(session.Character.GenerateRaid(2, true));
                session.Character.Group?.LeaveGroup(session);
            }
        }

        #endregion
    }
}