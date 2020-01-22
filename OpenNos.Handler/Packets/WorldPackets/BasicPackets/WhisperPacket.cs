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
using OpenNos.ChatLog.Networking;
using OpenNos.ChatLog.Shared;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("/")]
    public class WhisperPacket
    {
        #region Properties

        public string Message { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] { ' ' }, 3);
            if (packetSplit.Length < 3)
            {
                return;
            }
            WhisperPacket packetDefinition = new WhisperPacket();
            string msg = packetSplit[2].Trim();
            if (!string.IsNullOrWhiteSpace(msg))
            {
                packetDefinition.Message = msg;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(WhisperPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            session.Character.IsAfk = false;
            try
            {
                string characterName =
                    Message.Split(' ')[
                            Message.StartsWith("GM ", StringComparison.CurrentCulture) ? 1 : 0]
                        .Replace("[XH]", string.Empty).Replace("[BitchNiggerFaggot]", string.Empty);
                string message = string.Empty;
                string[] packetsplit = Message.Split(' ');
                for (int i = packetsplit[0] == "GM" ? 2 : 1; i < packetsplit.Length; i++)
                {
                    message += packetsplit[i] + " ";
                }

                if (message.Length > 60)
                {
                    message = message.Substring(0, 60);
                }

                message = message.Trim();
                session.SendPacket(session.Character.GenerateSpk(message, 5));
                CharacterDTO receiver = DAOFactory.CharacterDAO.LoadByName(characterName);
                int? sentChannelId = null;
                if (receiver != null)
                {
                    if (receiver.CharacterId == session.Character.CharacterId)
                    {
                        return;
                    }

                    if (session.Character.IsBlockedByCharacter(receiver.CharacterId))
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                        return;
                    }

                    ClientSession receiverSession =
                        ServerManager.Instance.GetSessionByCharacterId(receiver.CharacterId);
                    if (receiverSession?.CurrentMapInstance?.Map.MapId == session.CurrentMapInstance?.Map.MapId
                        && session.Account.Authority >= AuthorityType.Moderator)
                    {
                        receiverSession?.SendPacket(session.Character.GenerateSay(message, 2));
                    }

                    if (ServerManager.Instance.Configuration.UseChatLogService)
                    {
                        ChatLogServiceClient.Instance.LogChatMessage(new ChatLogEntry
                        {
                            Sender = session.Character.Name,
                            SenderId = session.Character.CharacterId,
                            Receiver = receiver.Name,
                            ReceiverId = receiver.CharacterId,
                            MessageType = ChatLogType.Whisper,
                            Message = message
                        });
                    }

                    sentChannelId = CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
                    {
                        DestinationCharacterId = receiver.CharacterId,
                        SourceCharacterId = session.Character.CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = session.Character.Authority == AuthorityType.Moderator
                            ? session.Character.GenerateSay(
                                $"[XH]({session.Character.Name}):{message}", 11)
                            : session.Character.GenerateSpk(message,
                                session.Account.Authority == AuthorityType.GameMaster ? 15 : 5),
                        Type = packetsplit[0] == "GM" ? MessageType.WhisperGm : MessageType.Whisper
                    });
                }

                if (sentChannelId == null)
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED")));
                }
            }
            catch (Exception e)
            {
                Logger.Error("Whisper failed.", e);
            }
        }

        #endregion
    }
}