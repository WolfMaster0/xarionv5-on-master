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
using OpenNos.GameLog.Shared;
using Hik.Communication.ScsServices.Client;
using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core;
using System.Collections.Generic;

namespace OpenNos.GameLog.Networking
{
    public class GameLogServiceClient : IGameLogService
    {
        #region Members

        private static GameLogServiceClient _instance;

        private readonly IScsServiceClient<IGameLogService> _client;

        #endregion

        #region Instantiation

        public GameLogServiceClient()
        {
            string ip = ConfigurationManager.AppSettings["GameLogIP"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["GameLogPort"]);
            _client = ScsServiceClientBuilder.CreateClient<IGameLogService>(new ScsTcpEndPoint(ip, port));
            System.Threading.Thread.Sleep(1000);
            while (_client.CommunicationState != CommunicationStates.Connected)
            {
                try
                {
                    _client.Connect();
                }
                catch (Exception)
                {
                    Logger.Error(Language.Instance.GetMessageFromKey("RETRY_CONNECTION"),
                        memberName: nameof(GameLogServiceClient));
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        #endregion

        #region Properties

        public static GameLogServiceClient Instance => _instance ?? (_instance = new GameLogServiceClient());

        public CommunicationStates CommunicationState => _client.CommunicationState;

        #endregion

        #region Methods

        public void LogEntry(GameLogEntry logEntry) => _client.ServiceProxy.LogEntry(logEntry);

        public bool AuthenticateAdmin(string user, string passHash) =>
            _client.ServiceProxy.AuthenticateAdmin(user, passHash);

        public List<GameLogEntry> GetLogEntries(int? channelId, string sender, long? senderid,
            Dictionary<string, string> content, DateTime? start, DateTime? end, GameLogType? logType) =>
            _client.ServiceProxy.GetLogEntries(channelId, sender, senderid, content, start, end, logType);

        public bool Authenticate(string authKey) => _client.ServiceProxy.Authenticate(authKey);

        #endregion
    }
}