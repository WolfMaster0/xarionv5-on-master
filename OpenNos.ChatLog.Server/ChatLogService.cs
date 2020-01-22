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
using OpenNos.ChatLog.Networking;
using OpenNos.ChatLog.Shared;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Master.Library.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace OpenNos.ChatLog.Server
{
    internal class ChatLogService : ScsService, IChatLogService
    {
        public bool AuthenticateAdmin(string user, string passHash)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(passHash))
            {
                return false;
            }

            if (AuthentificationServiceClient.Instance.ValidateAccount(user, passHash) is AccountDTO account && account.Authority > Domain.AuthorityType.User)
            {
                ChatLogManager.Instance.AuthentificatedClients.Add(CurrentClient.ClientId);
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

            if (authKey == ConfigurationManager.AppSettings["ChatLogKey"])
            {
                ChatLogManager.Instance.AuthentificatedClients.Add(CurrentClient.ClientId);
                return true;
            }

            return false;
        }

        public List<ChatLogEntry> GetChatLogEntries(string sender, long? senderid, string receiver, long? receiverid, string message, DateTime? start, DateTime? end, ChatLogType? logType)
        {
            if (!ChatLogManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }

            Logger.Info($"Received Log Request - Sender: {sender} SenderId: {senderid} Receiver: {receiver} ReceiverId: {receiverid} Message: {message} DateStart: {start} DateEnd: {end} ChatLogType: {logType}");
            List<ChatLogEntry> tmp = ChatLogManager.Instance.AllChatLogs.GetAllItems();
            if (!string.IsNullOrWhiteSpace(sender))
            {
                tmp = tmp.Where(s => s.Sender.IndexOf(sender, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();
            }
            if (senderid.HasValue)
            {
                tmp = tmp.Where(s => s.SenderId == senderid).ToList();
            }
            if (!string.IsNullOrWhiteSpace(receiver))
            {
                tmp = tmp.Where(s => s.Receiver?.IndexOf(receiver, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();
            }
            if (receiverid.HasValue)
            {
                tmp = tmp.Where(s => s.ReceiverId == receiverid).ToList();
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                tmp = tmp.Where(s => s.Message.IndexOf(message, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();
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
                tmp = tmp.Where(s => s.MessageType == logType).ToList();
            }
            return tmp;
        }

        public void LogChatMessage(ChatLogEntry logEntry)
        {
            if (!ChatLogManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)) || logEntry == null)
            {
                return;
            }

            logEntry.Timestamp = DateTime.UtcNow;
            ChatLogManager.Instance.ChatLogs.Add(logEntry);
            ChatLogManager.Instance.AllChatLogs.Add(logEntry);
        }
    }
}