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
using Hik.Communication.ScsServices.Service;
using OpenNos.GameLog.Networking;
using OpenNos.GameLog.Shared;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Master.Library.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using OpenNos.Domain;

namespace OpenNos.GameLog.Server
{
    internal class GameLogService : ScsService, IGameLogService
    {
        public bool AuthenticateAdmin(string user, string passHash)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(passHash))
            {
                return false;
            }

            if (AuthentificationServiceClient.Instance.ValidateAccount(user, passHash) is AccountDTO account
                && account.Authority > AuthorityType.User)
            {
                GameLogManager.Instance.AuthentificatedClients.Add(CurrentClient.ClientId);
                return true;
            }

            return false;
        }

        public bool Authenticate(string authKey)
        {
            if (string.IsNullOrWhiteSpace(authKey))
            {
                return false;
            }

            if (authKey == ConfigurationManager.AppSettings["GameLogKey"])
            {
                GameLogManager.Instance.AuthentificatedClients.Add(CurrentClient.ClientId);
                return true;
            }

            return false;
        }

        public List<GameLogEntry> GetLogEntries(int? channelId, string sender, long? senderid,
            Dictionary<string, string> content, DateTime? start, DateTime? end, GameLogType? logType)
        {
            if (!GameLogManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }

            string contentString = string.Empty;
            if (content != null)
            {
                foreach (KeyValuePair<string, string> entry in content)
                {
                    contentString += $"{{{entry.Key}:{entry.Value}}} ";
                }
            }

            Logger.Info(
                $"Received Log Request - Sender: {sender} SenderId: {senderid} Content: {contentString} DateStart: {start} DateEnd: {end} LogType: {logType}");
            List<GameLogEntry> tmp = GameLogManager.Instance.AllGameLogs.GetAllItems();
            if (channelId.HasValue)
            {
                tmp = tmp.Where(s => s.ChannelId == channelId).ToList();
            }

            if (!string.IsNullOrWhiteSpace(sender))
            {
                tmp = tmp.Where(s => s.CharacterName.IndexOf(sender, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    .ToList();
            }

            if (senderid.HasValue)
            {
                tmp = tmp.Where(s => s.CharacterId == senderid).ToList();
            }

            if (content != null)
            {
                foreach (KeyValuePair<string, string> entry in content)
                {
                    tmp = tmp.Where(s => s.Content.Any(x =>
                        x.Key.Equals(entry.Key, StringComparison.CurrentCultureIgnoreCase)
                        && x.Value.IndexOf(entry.Value, StringComparison.CurrentCultureIgnoreCase) >= 0)).ToList();
                }
            }

            if (start.HasValue)
            {
                tmp = tmp.Where(s => s.Timestamp >= start).ToList();
            }

            if (end.HasValue)
            {
                tmp = tmp.Where(s => s.Timestamp <= end).ToList();
            }

            if (logType.HasValue)
            {
                tmp = tmp.Where(s => s.GameLogType == logType).ToList();
            }

            return tmp;
        }

        public void LogEntry(GameLogEntry logEntry)
        {
            if (!GameLogManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId))
                || logEntry == null)
            {
                return;
            }

            logEntry.Timestamp = DateTime.UtcNow;
            GameLogManager.Instance.GameLogs.Add(logEntry);
            GameLogManager.Instance.AllGameLogs.Add(logEntry);
        }
    }
}