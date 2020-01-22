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
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("fins")]
    public class FInsPacket
    {
        #region Properties

        public long CharacterId { get; set; }

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
            FInsPacket packetDefinition = new FInsPacket();
            if (byte.TryParse(packetSplit[2], out byte type) && long.TryParse(packetSplit[3], out long charId))
            {
                packetDefinition.Type = type;
                packetDefinition.CharacterId = charId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(FInsPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.CurrentMapInstance?.MapInstanceType == MapInstanceType.BaseMapInstance)
            {
                if (!session.Character.IsFriendlistFull())
                {
                    long characterId = CharacterId;
                    if (!session.Character.IsFriendOfCharacter(characterId))
                    {
                        if (!session.Character.IsBlockedByCharacter(characterId))
                        {
                            if (!session.Character.IsBlockingCharacter(characterId))
                            {
                                ClientSession otherSession =
                                    ServerManager.Instance.GetSessionByCharacterId(characterId);
                                if (otherSession != null)
                                {
                                    if (otherSession.Character.FriendRequestBlocked)
                                    {
                                        session.SendPacket(
                                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("FRIEND_REJECTED")));
                                        return;
                                    }

                                    if (otherSession.Character.FriendRequestCharacters.GetAllItems()
                                        .Contains(session.Character.CharacterId))
                                    {
                                        switch (Type)
                                        {
                                            case 1:
                                                session.Character.AddRelation(characterId,
                                                    CharacterRelationType.Friend);
                                                session.SendPacket(
                                                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("FRIEND_ADDED")));
                                                otherSession.SendPacket(
                                                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("FRIEND_ADDED")));
                                                break;

                                            case 2:
                                                otherSession.SendPacket(
                                                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("FRIEND_REJECTED")));
                                                break;

                                            default:
                                                if (session.Character.IsFriendlistFull())
                                                {
                                                    session.SendPacket(
                                                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("FRIEND_FULL")));
                                                    otherSession.SendPacket(
                                                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("FRIEND_FULL")));
                                                }

                                                break;
                                        }
                                    }
                                    else
                                    {
                                        otherSession.SendPacket(UserInterfaceHelper.GenerateDialog(
                                            $"#fins^1^{session.Character.CharacterId} #fins^2^{session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("FRIEND_ADD"), session.Character.Name)}"));
                                        session.Character.FriendRequestCharacters.Add(characterId);
                                        if (otherSession.Character.IsAfk)
                                        {
                                            session.SendPacket(UserInterfaceHelper.GenerateInfo(
                                                string.Format(Language.Instance.GetMessageFromKey("PLAYER_IS_AFK"),
                                                    otherSession.Character.Name)));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKING")));
                            }
                        }
                        else
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                        }
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("ALREADY_FRIEND")));
                    }
                }
                else
                {
                    session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("FRIEND_FULL")));
                }
            }
            else
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("IMPOSSIBLE_TO_USE")));
            }
        }

        #endregion
    }
}