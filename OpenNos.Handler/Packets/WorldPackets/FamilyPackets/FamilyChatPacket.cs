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
using System.Threading.Tasks;
using OpenNos.ChatLog.Networking;
using OpenNos.ChatLog.Shared;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader(":")]
    public class FamilyChatPacket
    {
        #region Properties

        public string Message { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] {' '}, 3);
            if (packetSplit.Length < 2)
            {
                return;
            }
            FamilyChatPacket packetDefinition = new FamilyChatPacket();
            if (!string.IsNullOrWhiteSpace(packetSplit[2]))
            {
                packetDefinition.Message = packetSplit[2];
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(FamilyChatPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (string.IsNullOrEmpty(Message))
            {
                return;
            }

            if (session.Character.Family != null && session.Character.FamilyCharacter != null)
            {
                string msg = Message;
                string ccmsg = $"[{session.Character.Name}]:{msg}";
                if (session.Account.Authority == AuthorityType.GameMaster)
                {
                    ccmsg = $"[GM {session.Character.Name}]:{msg}";
                }

                if (session.Account.Authority == AuthorityType.Moderator)
                {
                    ccmsg = $"[XH {session.Character.Name}]:{msg}";
                }

                if (ServerManager.Instance.Configuration.UseChatLogService)
                {
                    ChatLogServiceClient.Instance.LogChatMessage(new ChatLogEntry()
                    {
                        Sender = session.Character.Name,
                        SenderId = session.Character.CharacterId,
                        Receiver = session.Character.Family.Name,
                        ReceiverId = session.Character.Family.FamilyId,
                        MessageType = ChatLogType.Family,
                        Message = Message
                    });
                }

                CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
                {
                    DestinationCharacterId = session.Character.Family.FamilyId,
                    SourceCharacterId = session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = ccmsg,
                    Type = MessageType.FamilyChat
                });
                Parallel.ForEach(ServerManager.Instance.Sessions.ToList(), sess =>
                {
                    if (sess.HasSelectedCharacter && sess.Character.Family != null
                        && session.Character.Family != null
                        && sess.Character.Family?.FamilyId == session.Character.Family?.FamilyId)
                    {
                        if (session.HasCurrentMapInstance && sess.HasCurrentMapInstance
                            && session.CurrentMapInstance == sess.CurrentMapInstance)
                        {
                            if (session.Account.Authority != AuthorityType.Moderator && !session.Character.InvisibleGm)
                            {
                                sess.SendPacket(session.Character.GenerateSay(msg, 6));
                            }
                            else
                            {
                                sess.SendPacket(session.Character.GenerateSay(ccmsg, 6, true));
                            }
                        }
                        else
                        {
                            sess.SendPacket(session.Character.GenerateSay(ccmsg, 6));
                        }

                        if (!session.Character.InvisibleGm)
                        {
                            sess.SendPacket(session.Character.GenerateSpk(msg, 1));
                        }
                    }
                });
            }        }

        #endregion
    }
}