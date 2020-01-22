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
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("rd")]
    public class RaidManagePacket
    {
        #region Properties

        public long CharacterId { get; set; }

        public short? Parameter { get; set; }

        public short Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            RaidManagePacket packetDefinition = new RaidManagePacket();
            if (short.TryParse(packetSplit[2], out short type) && long.TryParse(packetSplit[3], out long characterId))
            {
                packetDefinition.Type = type;
                packetDefinition.CharacterId = characterId;
                packetDefinition.Parameter = packetSplit.Length >= 5
                    && short.TryParse(packetSplit[4], out short parameter) ? parameter : (short?)null;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(RaidManagePacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.HasCurrentMapInstance)
            {
                Group grp;
                switch (Type)
                {
                    // Join Raid
                    case 1:
                        if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                        {
                            return;
                        }

                        ClientSession target = ServerManager.Instance.GetSessionByCharacterId(CharacterId);
                        if (Parameter == null && target?.Character?.Group == null
                                                       && session.Character.Group.IsLeader(session))
                        {
                            GroupJoinPacket.HandlePacket(session, $"1 pjoin {GroupRequestType.Invited} {CharacterId}");
                        }
                        else if (session.Character.Group == null)
                        {
                            GroupJoinPacket.HandlePacket(session, $"1 pjoin {GroupRequestType.Accepted} {CharacterId}");
                        }

                        break;

                    // Leave Raid
                    case 2:
                        ClientSession sender = ServerManager.Instance.GetSessionByCharacterId(CharacterId);
                        if (sender?.Character?.Group == null)
                        {
                            return;
                        }

                        session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("LEFT_RAID")),
                                0));
                        if (session.CurrentMapInstance?.MapInstanceType == MapInstanceType.RaidInstance)
                        {
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, session.Character.MapId,
                                session.Character.MapX, session.Character.MapY);
                        }

                        grp = sender.Character?.Group;
                        session.SendPacket(session.Character.GenerateRaid(1, true));
                        session.SendPacket(session.Character.GenerateRaid(2, true));

                        grp.Characters.ForEach(s =>
                        {
                            s.SendPacket(grp.GenerateRdlst());
                            s.SendPacket(grp.GeneraterRaidmbf(s));
                            s.SendPacket(s.Character.GenerateRaid(0));
                        });
                        break;

                    // Kick from Raid
                    case 3:
                        if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                        {
                            return;
                        }

                        if (session.Character.Group?.IsLeader(session) == true)
                        {
                            ClientSession chartokick = ServerManager.Instance.GetSessionByCharacterId(CharacterId);
                            if (chartokick.Character?.Group == null)
                            {
                                return;
                            }

                            chartokick.SendPacket(
                                UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("KICK_RAID"), 0));
                            grp = chartokick.Character?.Group;
                            chartokick.SendPacket(chartokick.Character?.GenerateRaid(1, true));
                            chartokick.SendPacket(chartokick.Character?.GenerateRaid(2, true));
                            grp?.LeaveGroup(chartokick);
                            grp?.Characters.ForEach(s =>
                            {
                                s.SendPacket(grp.GenerateRdlst());
                                s.SendPacket(s.Character.GenerateRaid(0));
                            });
                        }

                        break;

                    // Disolve Raid
                    case 4:
                        if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                        {
                            return;
                        }

                        if (session.Character.Group?.IsLeader(session) == true)
                        {
                            grp = session.Character.Group;

                            ClientSession[] grpmembers = new ClientSession[40];
                            grp.Characters.CopyTo(grpmembers);
                            foreach (ClientSession targetSession in grpmembers)
                            {
                                if (targetSession != null)
                                {
                                    targetSession.SendPacket(targetSession.Character.GenerateRaid(1, true));
                                    targetSession.SendPacket(targetSession.Character.GenerateRaid(2, true));
                                    targetSession.SendPacket(
                                        UserInterfaceHelper.GenerateMsg(
                                            Language.Instance.GetMessageFromKey("RAID_DISOLVED"), 0));
                                    grp.LeaveGroup(targetSession);
                                }
                            }

                            ServerManager.Instance.GroupList.RemoveAll(s => s.GroupId == grp.GroupId);
                            ServerManager.Instance.GroupsThreadSafe.Remove(grp.GroupId);
                        }

                        break;
                }
            }
        }

        #endregion
    }
}