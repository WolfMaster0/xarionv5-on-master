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
using OpenNos.Master.Library.Interface;
using OpenNos.SCS.Communication.ScsServices.Service;
using System.Configuration;
using System.Linq;

namespace OpenNos.Master.Server
{
    internal class AuthentificationService : ScsService, IAuthentificationService
    {
        public bool Authenticate(string authKey)
        {
            if (string.IsNullOrWhiteSpace(authKey))
            {
                return false;
            }

            if (authKey == ConfigurationManager.AppSettings["AuthentificationServiceAuthKey"])
            {
                MsManager.Instance.AuthentificatedClients.Add(CurrentClient.ClientId);
                return true;
            }

            return false;
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

        public CharacterDTO ValidateAccountAndCharacter(string userName, string characterName, string passHash)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(characterName) || string.IsNullOrEmpty(passHash))
            {
                return null;
            }

            AccountDTO account = DAOFactory.AccountDAO.LoadByName(userName);

            if (account?.Password == passHash)
            {
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(characterName);
                if (character?.AccountId == account.AccountId)
                {
                    return character;
                }
                return null;
            }
            return null;
        }
    }
}
