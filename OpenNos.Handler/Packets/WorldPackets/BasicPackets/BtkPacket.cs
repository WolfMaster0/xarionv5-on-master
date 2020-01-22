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
    [PacketHeader("btk")]
    public class BtkPacket
    {
        #region Properties

        public long CharacterId { get; set; }

        public string Message { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] { ' ' }, 4);
            if (packetSplit.Length < 4)
            {
                return;
            }
            BtkPacket packetDefinition = new BtkPacket();
            string msg = packetSplit[3].Trim();
            if (long.TryParse(packetSplit[2], out long charId) && !string.IsNullOrWhiteSpace(msg))
            {
                packetDefinition.CharacterId = charId;
                packetDefinition.Message = msg;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BtkPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            session.Character.IsAfk = false;
            string message = Message;
            if (message.Length > 60)
            {
                message = message.Substring(0, 60);
            }

            message = message.Trim();

            CharacterDTO character = DAOFactory.CharacterDAO.LoadById(CharacterId);
            if (character != null)
            {
                int? sentChannelId = CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
                {
                    DestinationCharacterId = character.CharacterId,
                    SourceCharacterId = session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = session.Character.GenerateTalk(message),
                    Type = MessageType.PrivateChat
                });

                if (ServerManager.Instance.Configuration.UseChatLogService)
                {
                    ChatLogServiceClient.Instance.LogChatMessage(new ChatLogEntry
                    {
                        Sender = session.Character.Name,
                        SenderId = session.Character.CharacterId,
                        Receiver = character.Name,
                        ReceiverId = character.CharacterId,
                        MessageType = ChatLogType.BuddyTalk,
                        Message = message
                    });
                }

                if (!sentChannelId.HasValue) //character is even offline on different world
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("FRIEND_OFFLINE")));
                }
            }
        }

        #endregion
    }
}