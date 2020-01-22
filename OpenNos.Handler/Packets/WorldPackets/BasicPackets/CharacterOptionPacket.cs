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
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("gop")]
    public class CharacterOptionPacket
    {
        #region Properties

        public bool IsActive { get; set; }

        public CharacterOption Option { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            CharacterOptionPacket packetDefinition = new CharacterOptionPacket();
            if (Enum.TryParse(packetSplit[2], out CharacterOption option))
            {
                packetDefinition.Option = option;
                packetDefinition.IsActive = packetSplit[3] == "1";
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(CharacterOptionPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            switch (Option)
            {
                case CharacterOption.BuffBlocked:
                    session.Character.BuffBlocked = IsActive;
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(session.Character.BuffBlocked
                            ? "BUFF_BLOCKED"
                            : "BUFF_UNLOCKED"), 0));
                    break;

                case CharacterOption.EmoticonsBlocked:
                    session.Character.EmoticonsBlocked = IsActive;
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(session.Character.EmoticonsBlocked
                            ? "EMO_BLOCKED"
                            : "EMO_UNLOCKED"), 0));
                    break;

                case CharacterOption.ExchangeBlocked:
                    session.Character.ExchangeBlocked = !IsActive;
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(session.Character.ExchangeBlocked
                            ? "EXCHANGE_BLOCKED"
                            : "EXCHANGE_UNLOCKED"), 0));
                    break;

                case CharacterOption.FriendRequestBlocked:
                    session.Character.FriendRequestBlocked = !IsActive;
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(session.Character.FriendRequestBlocked
                            ? "FRIEND_REQ_BLOCKED"
                            : "FRIEND_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.GroupRequestBlocked:
                    session.Character.GroupRequestBlocked = !IsActive;
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(session.Character.GroupRequestBlocked
                            ? "GROUP_REQ_BLOCKED"
                            : "GROUP_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.HeroChatBlocked:
                    session.Character.HeroChatBlocked = IsActive;
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(session.Character.HeroChatBlocked
                            ? "HERO_CHAT_BLOCKED"
                            : "HERO_CHAT_UNLOCKED"), 0));
                    break;

                case CharacterOption.HpBlocked:
                    session.Character.HpBlocked = IsActive;
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(session.Character.HpBlocked ? "HP_BLOCKED" : "HP_UNLOCKED"),
                        0));
                    break;

                case CharacterOption.MinilandInviteBlocked:
                    session.Character.MinilandInviteBlocked = IsActive;
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(session.Character.MinilandInviteBlocked
                            ? "MINI_INV_BLOCKED"
                            : "MINI_INV_UNLOCKED"), 0));
                    break;

                case CharacterOption.MouseAimLock:
                    session.Character.MouseAimLock = IsActive;
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(session.Character.MouseAimLock
                            ? "MOUSE_LOCKED"
                            : "MOUSE_UNLOCKED"), 0));
                    break;

                case CharacterOption.QuickGetUp:
                    session.Character.QuickGetUp = IsActive;
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(session.Character.QuickGetUp
                            ? "QUICK_GET_UP_ENABLED"
                            : "QUICK_GET_UP_DISABLED"), 0));
                    break;

                case CharacterOption.WhisperBlocked:
                    session.Character.WhisperBlocked = !IsActive;
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(session.Character.WhisperBlocked
                            ? "WHISPER_BLOCKED"
                            : "WHISPER_UNLOCKED"), 0));
                    break;

                case CharacterOption.FamilyRequestBlocked:
                    session.Character.FamilyRequestBlocked = !IsActive;
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(session.Character.FamilyRequestBlocked
                            ? "FAMILY_REQ_LOCKED"
                            : "FAMILY_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.GroupSharing:
                    Group grp = ServerManager.Instance.Groups.Find(
                        g => g.IsMemberOfGroup(session.Character.CharacterId));
                    if (grp == null)
                    {
                        return;
                    }

                    if (!grp.IsLeader(session))
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_MASTER"), 0));
                        return;
                    }

                    if (!IsActive)
                    {
                        Group group =
                            ServerManager.Instance.Groups.Find(s => s.IsMemberOfGroup(session.Character.CharacterId));
                        if (group != null)
                        {
                            group.SharingMode = 1;
                        }

                        session.CurrentMapInstance?.Broadcast(session,
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHARING"), 0),
                            ReceiverType.Group);
                    }
                    else
                    {
                        Group group =
                            ServerManager.Instance.Groups.Find(s => s.IsMemberOfGroup(session.Character.CharacterId));
                        if (group != null)
                        {
                            group.SharingMode = 0;
                        }

                        session.CurrentMapInstance?.Broadcast(session,
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHARING_BY_ORDER"), 0),
                            ReceiverType.Group);
                    }

                    break;

                case CharacterOption.AllowRevivalPet:
                    session.Character.IsPetAutoRelive = IsActive;
                        session.SendPacket(UserInterfaceHelper.GenerateMsg(
                            Language.Instance.GetMessageFromKey(session.Character.IsPetAutoRelive
                                ? "REVIVAL_THIS_PET"
                                : "NOT_REVIVAL_THIS_PET"), 0));
                    break;

                case CharacterOption.AllowRevivalPartner:
                    session.Character.IsPartnerAutoRelive = IsActive;
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(session.Character.IsPartnerAutoRelive
                            ? "REVIVAL_THIS_PARTNER"
                            : "NOT_REVIVAL_THIS_PARTNER"), 0));
                    break;
            }

            session.SendPacket(session.Character.GenerateStat());
        }

        #endregion
    }
}