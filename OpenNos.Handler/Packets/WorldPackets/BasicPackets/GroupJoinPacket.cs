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

using System;
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("pjoin")]
    public class GroupJoinPacket
    {
        #region Properties

        public long CharacterId { get; set; }

        public GroupRequestType RequestType { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            GroupJoinPacket packetDefinition = new GroupJoinPacket();
            if (Enum.TryParse(packetSplit[2], out GroupRequestType requestType) && long.TryParse(packetSplit[3], out long characterId))
            {
                packetDefinition.RequestType = requestType;
                packetDefinition.CharacterId = characterId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(GroupJoinPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            bool createNewGroup = true;
            ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterId(CharacterId);

            if (targetSession == null && !RequestType.Equals(GroupRequestType.Sharing))
            {
                return;
            }
            // HUANTERS FIX
            if (session.Character.IsBlockedByCharacter(CharacterId))
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(
                        Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                return;
            }
            if (RequestType.Equals(GroupRequestType.Requested)
                || RequestType.Equals(GroupRequestType.Invited))
            {
                if (CharacterId == 0)
                {
                    return;
                }

                if (ServerManager.Instance.IsCharactersGroupFull(CharacterId))
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                    return;
                }

                if (ServerManager.Instance.IsCharacterMemberOfGroup(CharacterId)
                    && ServerManager.Instance.IsCharacterMemberOfGroup(session.Character.CharacterId))
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("ALREADY_IN_GROUP")));
                    return;
                }

                if (session.Character.CharacterId != CharacterId && targetSession != null)
                {
                    if (session.Character.IsBlockedByCharacter(CharacterId))
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                        return;
                    }

                    if (targetSession.Character.GroupRequestBlocked)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("GROUP_BLOCKED"),
                                0));
                    }
                    else
                    {
                        // save sent group request to current character
                        session.Character.GroupSentRequestCharacterIds.Add(targetSession.Character.CharacterId);
                        if (session.Character.Group == null || session.Character.Group.GroupType == GroupType.Group)
                        {
                            if (targetSession.Character?.Group == null
                                || targetSession.Character?.Group.GroupType == GroupType.Group)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateInfo(
                                    string.Format(Language.Instance.GetMessageFromKey("GROUP_REQUEST"),
                                        targetSession.Character.Name)));
                                targetSession.SendPacket(UserInterfaceHelper.GenerateDialog(
                                    $"#pjoin^3^{session.Character.CharacterId} #pjoin^4^{session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("INVITED_YOU"), session.Character.Name)}"));
                                if (targetSession.Character.IsAfk)
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateInfo(
                                        string.Format(Language.Instance.GetMessageFromKey("PLAYER_IS_AFK"),
                                            targetSession.Character.Name)));
                                }
                            }
                        }
                        else
                        {
                            targetSession.SendPacket(
                                $"qna #rd^1^{session.Character.CharacterId}^1 {string.Format(Language.Instance.GetMessageFromKey("INVITE_RAID"), session.Character.Name)}");
                        }
                    }
                }
            }
            else if (RequestType.Equals(GroupRequestType.Sharing))
            {
                if (session.Character.Group != null)
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_SHARE_INFO")));
                    session.Character.Group.Characters
                        .Where(s => s.Character.CharacterId != session.Character.CharacterId).ToList().ForEach(s =>
                        {
                            s.SendPacket(UserInterfaceHelper.GenerateDialog(
                                $"#pjoin^6^{session.Character.CharacterId} #pjoin^7^{session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("INVITED_YOU_SHARE"), session.Character.Name)}"));
                            session.Character.GroupSentRequestCharacterIds.Add(s.Character.CharacterId);
                        });
                }
            }
            else if (RequestType.Equals(GroupRequestType.Accepted))
            {
                if (targetSession?.Character.GroupSentRequestCharacterIds.GetAllItems()
                        .Contains(session.Character.CharacterId) == false)
                {
                    return;
                }

                try
                {
                    targetSession?.Character.GroupSentRequestCharacterIds.Remove(session.Character.CharacterId);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                if (ServerManager.Instance.IsCharacterMemberOfGroup(session.Character.CharacterId)
                    && ServerManager.Instance.IsCharacterMemberOfGroup(CharacterId))
                {
                    // everyone is in group, return
                    return;
                }

                if (ServerManager.Instance.IsCharactersGroupFull(CharacterId)
                    || ServerManager.Instance.IsCharactersGroupFull(session.Character.CharacterId))
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                    targetSession?.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                    return;
                }

                // get group and add to group
                if (ServerManager.Instance.IsCharacterMemberOfGroup(session.Character.CharacterId))
                {
                    // target joins source
                    Group currentGroup =
                        ServerManager.Instance.GetGroupByCharacterId(session.Character.CharacterId);

                    if (currentGroup != null)
                    {
                        currentGroup.JoinGroup(targetSession);
                        targetSession?.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("JOINED_GROUP"),
                                10));
                        createNewGroup = false;
                    }
                }
                else if (ServerManager.Instance.IsCharacterMemberOfGroup(CharacterId))
                {
                    // source joins target
                    Group currentGroup = ServerManager.Instance.GetGroupByCharacterId(CharacterId);

                    if (currentGroup != null)
                    {
                        createNewGroup = false;
                        if (currentGroup.GroupType == GroupType.Group)
                        {
                            currentGroup.JoinGroup(session);
                        }
                        else
                        {
                            if (currentGroup.Raid != null)
                            {
                                if ((currentGroup.Raid.Id == 16 && session.Character.RaidDracoRuns > 255)
                                    || (currentGroup.Raid.Id == 17 && session.Character.RaidGlacerusRuns > 255))
                                {
                                    session.SendPacket(session.Character.GenerateSay(
                                        Language.Instance.GetMessageFromKey("RAID_LIMIT_EXCEEDED"), 10));
                                    return;
                                }

                                if ((currentGroup.Raid.Id == 16 && session.Character.Inventory
                                        .LoadBySlotAndType((short) EquipmentType.Amulet, InventoryType.Wear) ?.ItemVNum != 4503)
                                        || (currentGroup.Raid.Id == 17 && session.Character.Inventory
                                        .LoadBySlotAndType((short)EquipmentType.Amulet, InventoryType.Wear) ?.ItemVNum != 4504))
                                {
                                    session.SendPacket(session.Character.GenerateSay(
                                        Language.Instance.GetMessageFromKey("RAID_MISSING_ITEM"), 10));
                                    return;
                                }

                                session.SendPacket(
                                    session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RAID_JOIN"),
                                        10));
                                if (session.Character.Level > currentGroup.Raid.LevelMaximum
                                    || session.Character.Level < currentGroup.Raid.LevelMinimum)
                                {
                                    session.SendPacket(session.Character.GenerateSay(
                                        Language.Instance.GetMessageFromKey("RAID_LEVEL_INCORRECT"), 10));
                                    if (session.Character.Level
                                        >= currentGroup.Raid.LevelMaximum + 10 /* && AlreadySuccededToday*/)
                                    {
                                        //modal 1 ALREADY_SUCCEDED_AS_ASSISTANT
                                    }
                                }

                                currentGroup.JoinGroup(session);
                                session.SendPacket(session.Character.GenerateRaid(1));
                                currentGroup.Characters.ForEach(s =>
                                {
                                    s.SendPacket(currentGroup.GenerateRdlst());
                                    s.SendPacket(s.Character.GenerateSay(
                                        string.Format(Language.Instance.GetMessageFromKey("JOIN_TEAM"),
                                            session.Character.Name), 10));
                                    // TEMPORARY: This fix is Temporary, confirm it works or fix.
                                    if (currentGroup.IsLeader(s))
                                    {
                                        s.SendPacket(s.Character.GenerateRaid(0));
                                    }
                                    else
                                    {
                                        s.SendPacket(s.Character.GenerateRaid(2));
                                    }
                                });
                            }
                        }
                    }
                }

                if (createNewGroup)
                {
                    Group group = new Group
                    {
                        GroupType = GroupType.Group
                    };
                    group.JoinGroup(CharacterId);
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("GROUP_JOIN"),
                            targetSession?.Character.Name), 10));
                    group.JoinGroup(session.Character.CharacterId);
                    ServerManager.Instance.AddGroup(group);
                    targetSession?.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_ADMIN")));

                    // set back reference to group
                    session.Character.Group = group;
                    if (targetSession != null)
                    {
                        targetSession.Character.Group = group;
                    }
                }

                if (session.Character.Group.GroupType == GroupType.Group)
                {
                    // player join group
                    ServerManager.Instance.UpdateGroup(CharacterId);
                    session.CurrentMapInstance?.Broadcast(session.Character.GeneratePidx());
                }
            }
            else if (RequestType == GroupRequestType.Declined)
            {
                if (targetSession?.Character.GroupSentRequestCharacterIds.GetAllItems()
                        .Contains(session.Character.CharacterId) == false)
                {
                    return;
                }

                targetSession?.Character.GroupSentRequestCharacterIds.Remove(session.Character.CharacterId);

                targetSession?.SendPacket(session.Character.GenerateSay(
                    string.Format(Language.Instance.GetMessageFromKey("REFUSED_GROUP_REQUEST"),
                        session.Character.Name), 10));
            }
            else if (RequestType == GroupRequestType.AcceptedShare)
            {
                if (targetSession?.Character.GroupSentRequestCharacterIds.GetAllItems()
                        .Contains(session.Character.CharacterId) == false)
                {
                    return;
                }

                targetSession?.Character.GroupSentRequestCharacterIds.Remove(session.Character.CharacterId);

                session.SendPacket(UserInterfaceHelper.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("ACCEPTED_SHARE"),
                        targetSession?.Character.Name), 0));
                if (session.Character?.Group?.IsMemberOfGroup(CharacterId) == true
                    && targetSession != null)
                {
                    session.Character.SetReturnPoint(targetSession.Character.Return.DefaultMapId,
                        targetSession.Character.Return.DefaultX, targetSession.Character.Return.DefaultY);
                    targetSession.SendPacket(UserInterfaceHelper.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("CHANGED_SHARE"),
                            targetSession.Character.Name), 0));
                }
            }
            else if (RequestType == GroupRequestType.DeclinedShare)
            {
                if (targetSession?.Character.GroupSentRequestCharacterIds.GetAllItems()
                        .Contains(session.Character.CharacterId) == false)
                {
                    return;
                }

                targetSession?.Character.GroupSentRequestCharacterIds.Remove(session.Character.CharacterId);

                session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("REFUSED_SHARE"), 0));
            }
        }

        #endregion
    }
}