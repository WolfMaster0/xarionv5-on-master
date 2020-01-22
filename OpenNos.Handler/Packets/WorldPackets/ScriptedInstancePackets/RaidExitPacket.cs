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
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.ScriptedInstancePackets
{
    [PacketHeader("rxit")]
    public class RaidExitPacket
    {
        #region Properties

        public byte State { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            RaidExitPacket packetDefinition = new RaidExitPacket();
            if (byte.TryParse(packetSplit[2], out byte state))
            {
                packetDefinition.State = state;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(RaidExitPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (State == 1
                && (session.CurrentMapInstance?.MapInstanceType == MapInstanceType.TimeSpaceInstance
                 || session.CurrentMapInstance?.MapInstanceType == MapInstanceType.RaidInstance))
            {
                if (session.CurrentMapInstance.InstanceBag.Lock)
                {
                    //5seed
                    session.CurrentMapInstance.InstanceBag.DeadList.Add(session.Character.CharacterId);
                    session.SendPacket(
                        session.Character.GenerateSay(
                            string.Format(Language.Instance.GetMessageFromKey("DIGNITY_LOST"), 20), 11));
                    session.Character.Dignity =
                        session.Character.Dignity < -980 ? -1000 : session.Character.Dignity - 20;
                }
                else
                {
                    //1seed
                }

                ServerManager.Instance.GroupLeave(session);
                ServerManager.Instance.ChangeMap(session.Character.CharacterId, session.Character.MapId,
                    session.Character.MapX, session.Character.MapY);
            }
        }

        #endregion
    }
}