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
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("say")]
    public class SayPacket
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
            SayPacket packetDefinition = new SayPacket();
            string msg = packetSplit[2].Trim();
            if (!string.IsNullOrWhiteSpace(msg))
            {
                packetDefinition.Message = msg;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(SayPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            session.Character.IsAfk = false;
            session.Character.MessageCounter += 2;
            if (session.Character.MessageCounter > 11)
            {
                return;
            }

            bool isMuted = session.Character.MuteMessage();
            if (ServerManager.Instance.Configuration.UseChatLogService)
            {
                ChatLogServiceClient.Instance.LogChatMessage(new ChatLogEntry
                {
                    Sender = session.Character.Name,
                    SenderId = session.Character.CharacterId,
                    Receiver = session.CurrentMapInstance?.Map.Name,
                    ReceiverId = session.CurrentMapInstance?.Map.MapId,
                    MessageType = ChatLogType.Map,
                    Message = Message
                });
            }

            if (!isMuted)
            {
                byte type = 0;
                if (session.Character.Authority == AuthorityType.Moderator)
                {
                    type = 12;
                    session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateSay(Message, 1),
                        ReceiverType.AllExceptMe);
                    Message = $"[XH {session.Character.Name}]: {Message}";
                }

                session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateSay(Message, type),
                    ReceiverType.AllExceptMe);
            }
        }

        #endregion
    }
}