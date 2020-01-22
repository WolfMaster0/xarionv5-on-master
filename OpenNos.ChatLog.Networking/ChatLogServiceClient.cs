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
using System.Configuration;
using OpenNos.ChatLog.Shared;
using Hik.Communication.ScsServices.Client;
using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core;
using System.Collections.Generic;

namespace OpenNos.ChatLog.Networking
{
    public class ChatLogServiceClient : IChatLogService
    {
        #region Members

        private static ChatLogServiceClient _instance;

        private readonly IScsServiceClient<IChatLogService> _client;

        #endregion

        #region Instantiation

        public ChatLogServiceClient()
        {
            string ip = ConfigurationManager.AppSettings["ChatLogIP"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["ChatLogPort"]);
            _client = ScsServiceClientBuilder.CreateClient<IChatLogService>(new ScsTcpEndPoint(ip, port));
            System.Threading.Thread.Sleep(1000);
            while (_client.CommunicationState != CommunicationStates.Connected)
            {
                try
                {
                    _client.Connect();
                }
                catch (Exception)
                {
                    Logger.Error(Language.Instance.GetMessageFromKey("RETRY_CONNECTION"), memberName: nameof(ChatLogServiceClient));
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        #endregion

        #region Properties

        public static ChatLogServiceClient Instance => _instance ?? (_instance = new ChatLogServiceClient());

        public CommunicationStates CommunicationState => _client.CommunicationState;

        #endregion

        #region Methods

        public void LogChatMessage(ChatLogEntry logEntry) => _client.ServiceProxy.LogChatMessage(logEntry);

        public bool AuthenticateAdmin(string user, string passHash) => _client.ServiceProxy.AuthenticateAdmin(user, passHash);

        public List<ChatLogEntry> GetChatLogEntries(string sender, long? senderid, string receiver, long? receiverid, string message, DateTime? start, DateTime? end, ChatLogType? logType) => _client.ServiceProxy.GetChatLogEntries(sender, senderid, receiver, receiverid, message, start, end, logType);

        public bool Authenticate(string authKey) => _client.ServiceProxy.Authenticate(authKey);

        #endregion
    }
}