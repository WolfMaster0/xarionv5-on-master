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
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("rl")]
    public class RaidListRegisterPacket
    {
        #region Properties

        public string CharacterName { get; set; }

        public short Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 2)
            {
                return;
            }
            RaidListRegisterPacket packetDefinition = new RaidListRegisterPacket();
            if (true)
            {
                if (packetSplit.Length > 2 && short.TryParse(packetSplit[2], out short type))
                {
                    packetDefinition.Type = type;
                }

                if (packetSplit.Length > 3 && !string.IsNullOrEmpty(packetSplit[3]))
                {
                    packetDefinition.CharacterName = packetSplit[3];
                }

                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(RaidListRegisterPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            switch (Type)
            {
                case 0:
                    if (session.Character.Group?.IsLeader(session) == true
                        && session.Character.Group.GroupType != GroupType.Group
                        && ServerManager.Instance.GroupList.Any(s => s.GroupId == session.Character.Group.GroupId))
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateRl(1));
                    }
                    else if (session.Character.Group != null && session.Character.Group.GroupType != GroupType.Group
                                                             && session.Character.Group.IsLeader(session))
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateRl(2));
                    }
                    else if (session.Character.Group != null)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateRl(3));
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateRl(0));
                    }
                    break;

                case 1:
                    if (session.Character.Group != null && session.Character.Group.GroupType != GroupType.Group
                        && ServerManager.Instance.GroupList.All(s => s.GroupId != session.Character.Group.GroupId)
                        && session.Character.Group.Raid?.InstanceBag?.Lock == false)
                    {
                        ServerManager.Instance.GroupList.Add(session.Character.Group);
                        session.SendPacket(UserInterfaceHelper.GenerateRl(1));
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("RAID_REGISTERED")));
                        ServerManager.Instance.Broadcast(session,
                            $"qnaml 100 #rl {string.Format(Language.Instance.GetMessageFromKey("SEARCH_TEAM_MEMBERS"), session.Character.Name, session.Character.Group.Raid?.Label)}",
                            ReceiverType.AllExceptGroup);
                    }

                    break;

                case 2:
                    if (session.Character.Group != null && session.Character.Group.GroupType != GroupType.Group
                                                        && ServerManager.Instance.GroupList.Any(s =>
                                                            s.GroupId == session.Character.Group.GroupId))
                    {
                        ServerManager.Instance.GroupList.Remove(session.Character.Group);
                        session.SendPacket(UserInterfaceHelper.GenerateRl(2));
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("RAID_UNREGISTERED")));
                    }
                    break;

                case 3:
                    ClientSession cl = ServerManager.Instance.GetSessionByCharacterName(CharacterName);
                    if (cl != null)
                    {
                        cl.Character.GroupSentRequestCharacterIds.Add(session.Character.CharacterId);
                        GroupJoinPacket.HandlePacket(session, $"1 pjoin {GroupRequestType.Accepted} {cl.Character.CharacterId}");
                    }
                    break;
            }
        }

        #endregion
    }
}