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
using OpenNos.Domain;
using OpenNos.Master.Library.Data;
using OpenNos.Master.Library.Interface;
using OpenNos.SCS.Communication.ScsServices.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace OpenNos.Master.Server
{
    internal class MallService : ScsService, IMallService
    {
        public bool Authenticate(string authKey)
        {
            if (string.IsNullOrWhiteSpace(authKey))
            {
                return false;
            }

            if (authKey == ConfigurationManager.AppSettings["MallAuthKey"])
            {
                MsManager.Instance.AuthentificatedClients.Add(CurrentClient.ClientId);
                return true;
            }

            return false;
        }

        public bool IsAuthenticated() => MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId));

        public AccountDTO ReceiveAccountInfo(string userName)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)) || string.IsNullOrEmpty(userName))
            {
                return null;
            }

            return DAOFactory.AccountDAO.LoadByName(userName);
        }

        public AuthorityType GetAuthority(long accountId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return AuthorityType.Closed;
            }

            return DAOFactory.AccountDAO.LoadById(accountId)?.Authority ?? AuthorityType.Closed;
        }

        public IEnumerable<CharacterDTO> GetCharacters(long accountId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }

            return DAOFactory.CharacterDAO.LoadByAccount(accountId);
        }

        public void SendItem(long characterId, MallItem item)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            ItemDTO dto = DAOFactory.ItemDAO.LoadById(item.ItemVNum);
            if (dto != null)
            {
                int limit = 99;

                if (dto.Type == InventoryType.Equipment || dto.Type == InventoryType.Miniland)
                {
                    limit = 1;
                }

                do
                {
                    MailDTO mailDTO = new MailDTO
                    {
                        AttachmentAmount = (byte)(item.Amount > limit ? limit : item.Amount),
                        AttachmentRarity = item.Rare,
                        AttachmentUpgrade = item.Upgrade,
                        AttachmentVNum = item.ItemVNum,
                        Date = DateTime.UtcNow,
                        EqPacket = string.Empty,
                        IsOpened = false,
                        IsSenderCopy = false,
                        Message = string.Empty,
                        ReceiverId = characterId,
                        SenderId = characterId,
                        Title = "NOSMALL"
                    };

                    DAOFactory.MailDAO.InsertOrUpdate(ref mailDTO);

                    AccountConnection account = MsManager.Instance.ConnectedAccounts.Find(a => a.CharacterId.Equals(mailDTO.ReceiverId));
                    account?.ConnectedWorld?.MailServiceClient.GetClientProxy<IMailClient>().MailSent(mailDTO);

                    item.Amount -= limit;
                } while (item.Amount > 0);
            }
        }

        public void SendStaticBonus(long characterId, MallStaticBonus item)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            StaticBonusDTO dto = DAOFactory.StaticBonusDAO.LoadByCharacterId(characterId).FirstOrDefault(s => s.StaticBonusType == item.StaticBonus);

            if (dto != null)
            {
                dto.DateEnd = dto.DateEnd.AddSeconds(item.Seconds);
            }
            else
            {
                dto = new StaticBonusDTO()
                {
                    CharacterId = characterId,
                    DateEnd = DateTime.UtcNow.AddSeconds(item.Seconds),
                    StaticBonusType = item.StaticBonus
                };
            }

            DAOFactory.StaticBonusDAO.InsertOrUpdate(ref dto);
            AccountConnection account = MsManager.Instance.ConnectedAccounts.Find(a => a.CharacterId.Equals(characterId));
            account?.ConnectedWorld?.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().UpdateStaticBonus(characterId);
        }

        public AccountDTO ValidateAccount(string userName, string passHash)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passHash))
            {
                return null;
            }

            AccountDTO account = DAOFactory.AccountDAO.LoadByName(userName);

            if (account?.Password == passHash)
            {
                return account;
            }
            return null;
        }
    }
}
