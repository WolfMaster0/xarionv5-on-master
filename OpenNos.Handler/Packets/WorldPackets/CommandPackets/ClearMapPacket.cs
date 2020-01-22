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

using System.Linq;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$ClearMap", Authority = AuthorityType.GameMaster)]
    public class ClearMapPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(' ');
                if (packetSplit.Length < 2)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }

                ClearMapPacket packetDefinition = new ClearMapPacket();
                if (true /*parsing here*/)
                {
                    packetDefinition._isParsed = true;

                    // Set Packet Properties after parsing
                }

                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ClearMapPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$ClearMap";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[ClearMap]MapId: {session.CurrentMapInstance.MapInstanceId}");

                Parallel.ForEach(session.CurrentMapInstance.Monsters.Where(s => s.ShouldRespawn != true), monster =>
                {
                    session.CurrentMapInstance.Broadcast(StaticPacketHelper.Out(UserType.Monster,
                        monster.MapMonsterId));
                    session.CurrentMapInstance.RemoveMonster(monster);
                });
                Parallel.ForEach(session.CurrentMapInstance.DroppedList.GetAllItems(), drop =>
                {
                    session.CurrentMapInstance.Broadcast(StaticPacketHelper.Out(UserType.Object, drop.TransportId));
                    session.CurrentMapInstance.DroppedList.Remove(drop.TransportId);
                });
                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}