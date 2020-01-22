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
using System.Linq;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;

namespace OpenNos.GameObject.Event.MINILANDREFRESH
{
    public static class MinilandRefresh
    {
        #region Methods

        public static void GenerateMinilandEvent()
        {
            ServerManager.Instance.SaveAll();
            GeneralLogDTO gen = DAOFactory.GeneralLogDAO.LoadByAccount(null).LastOrDefault(s => s.LogData == nameof(MinilandRefresh) && s.LogType == "World" && s.Timestamp.Date == DateTime.UtcNow.Date);
            DAOFactory.GeneralLogDAO.Insert(new GeneralLogDTO { LogData = nameof(MinilandRefresh), LogType = "World", Timestamp = DateTime.UtcNow });
            foreach (CharacterDTO chara in DAOFactory.CharacterDAO.LoadAll())
            {
                int count = DAOFactory.GeneralLogDAO.LoadByAccount(chara.AccountId).Count(s => s.LogData == "MINILAND" && s.Timestamp > DateTime.UtcNow.AddDays(-1) && s.CharacterId == chara.CharacterId);

                ClientSession session = ServerManager.Instance.GetSessionByCharacterId(chara.CharacterId);
                if (session != null)
                {
                    session.Character.SetReputation(2 * count);
                    session.Character.MinilandPoint = 2000;
                }
                else if (!CommunicationServiceClient.Instance.IsCharacterConnected(ServerManager.Instance.ServerGroup, chara.CharacterId))
                {
                    if (gen == null)
                    {
                        chara.Reputation += 2 * count;
                    }
                    chara.MinilandPoint = 2000;
                    CharacterDTO chara2 = chara;
                    DAOFactory.CharacterDAO.InsertOrUpdate(ref chara2);
                }
            }
            ServerManager.Instance.StartedEvents.Remove(EventType.MinilandRefresh);
        }

        #endregion
    }
}