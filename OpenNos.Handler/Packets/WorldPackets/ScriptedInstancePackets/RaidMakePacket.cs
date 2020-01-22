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

using System.Linq;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.ScriptedInstancePackets
{
    [PacketHeader("mkraid")]
    public class RaidMakePacket
    {
        #region Properties

        public short Parameter { get; set; }

        public byte Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            RaidMakePacket packetDefinition = new RaidMakePacket();
            if (byte.TryParse(packetSplit[2], out byte type)
                && short.TryParse(packetSplit[2], out short parameter))
            {
                packetDefinition.Type = type;
                packetDefinition.Parameter = parameter;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(RaidMakePacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Group?.Raid != null && session.Character.Group.IsLeader(session))
            {
                if (session.Character.Group.CharacterCount > 4 && session.Character.Group.Characters.All(s =>
                        s.CurrentMapInstance.Portals.Any(p => p.Type == (short)PortalType.Raid)))
                {
                    if (session.Character.Group.Raid.FirstMap == null)
                    {
                        session.Character.Group.Raid.LoadScript(MapInstanceType.RaidInstance);
                    }

                    if (session.Character.Group.Raid.FirstMap == null)
                    {
                        return;
                    }

                    session.Character.Group.Raid.InstanceBag.Lock = true;

                    //Session.Character.Group.Characters.Where(s => s.CurrentMapInstance != Session.CurrentMapInstance).ToList().ForEach(
                    //session =>
                    //{
                    //    Session.Character.Group.LeaveGroup(session);
                    //    session.SendPacket(session.Character.GenerateRaid(1, true));
                    //    session.SendPacket(session.Character.GenerateRaid(2, true));
                    //});

                    session.Character.Group.Raid.InstanceBag.Lives = (short)session.Character.Group.CharacterCount;

                    foreach (ClientSession sess in session.Character.Group.Characters.GetAllItems())
                    {
                        if (sess != null)
                        {
                            ServerManager.Instance.ChangeMapInstance(sess.Character.CharacterId,
                                sess.Character.Group.Raid.FirstMap.MapInstanceId,
                                sess.Character.Group.Raid.StartX, sess.Character.Group.Raid.StartY);
                            sess.SendPacket("raidbf 0 0 25");
                            sess.SendPacket(sess.Character.Group.GeneraterRaidmbf(sess));
                            sess.SendPacket(sess.Character.GenerateRaid(5));
                            sess.SendPacket(sess.Character.GenerateRaid(4));
                            sess.SendPacket(sess.Character.GenerateRaid(3));
                            if (sess.Character.Group.Raid.Id == 16)
                            {
                                sess.Character.RaidDracoRuns++;
                            }
                            if (sess.Character.Group.Raid.Id == 17)
                            {
                                sess.Character.RaidGlacerusRuns++;
                            }
                        }
                    }

                    ServerManager.Instance.GroupList.Remove(session.Character.Group);

                    GameLogger.Instance.LogRaidStart(ServerManager.Instance.ChannelId, session.Character.Name,
                        session.Character.CharacterId, session.Character.Group.GroupId,
                        session.Character.Group.Characters.GetAllItems().Select(s => s.Character).Cast<CharacterDTO>()
                            .ToList());
                }
                else
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg("RAID_TEAM_NOT_READY", 0));
                }
            }
        }

        #endregion
    }
}