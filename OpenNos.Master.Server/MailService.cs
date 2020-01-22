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
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Master.Library.Data;
using OpenNos.Master.Library.Interface;
using OpenNos.SCS.Communication.ScsServices.Service;
using System;
using System.Configuration;
using System.Linq;

namespace OpenNos.Master.Server
{
    internal class MailService : ScsService, IMailService
    {
        public bool Authenticate(string authKey, Guid serverId)
        {
            if (string.IsNullOrWhiteSpace(authKey))
            {
                return false;
            }

            if (authKey == ConfigurationManager.AppSettings["MasterAuthKey"])
            {
                MsManager.Instance.AuthentificatedClients.Add(CurrentClient.ClientId);

                WorldServer ws = MsManager.Instance.WorldServers.Find(s => s.Id == serverId);
                if (ws != null)
                {
                    ws.MailServiceClient = CurrentClient;
                }
                return true;
            }

            return false;
        }

        public void SendMail(MailDTO mail)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            DAOFactory.MailDAO.InsertOrUpdate(ref mail);

            if (mail.IsSenderCopy)
            {
                AccountConnection account = MsManager.Instance.ConnectedAccounts.Find(a => a.CharacterId.Equals(mail.SenderId));
                if (account?.ConnectedWorld != null)
                {
                    account.ConnectedWorld.MailServiceClient.GetClientProxy<IMailClient>().MailSent(mail);
                }
            }
            else
            {
                AccountConnection account = MsManager.Instance.ConnectedAccounts.Find(a => a.CharacterId.Equals(mail.ReceiverId));
                if (account?.ConnectedWorld != null)
                {
                    account.ConnectedWorld.MailServiceClient.GetClientProxy<IMailClient>().MailSent(mail);
                }
            }
        }
    }
}
