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
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.Master.Library.Data;
using OpenNos.Master.Library.Interface;
using OpenNos.SCS.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.SCS.Communication.ScsServices.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.Master.Server
{
    internal class CommunicationService : ScsService, ICommunicationService
    {
        #region Methods

        public bool Authenticate(string authKey)
        {
            if (string.IsNullOrWhiteSpace(authKey))
            {
                return false;
            }

            if (authKey == ConfigurationManager.AppSettings["MasterAuthKey"])
            {
                MsManager.Instance.AuthentificatedClients.Add(CurrentClient.ClientId);
                return true;
            }

            return false;
        }

        public void Cleanup()
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            MsManager.Instance.ConnectedAccounts.Clear();
            MsManager.Instance.WorldServers.Clear();
        }

        public void CleanupOutdatedSession()
        {
            AccountConnection[] tmp = new AccountConnection[MsManager.Instance.ConnectedAccounts.Count + 20];
            lock (MsManager.Instance.ConnectedAccounts)
            {
                MsManager.Instance.ConnectedAccounts.CopyTo(tmp);
            }
            foreach (AccountConnection account in tmp.Where(a => a?.LastPulse.AddMinutes(5) <= DateTime.UtcNow))
            {
                KickSession(account.AccountId, null);
            }
        }

        public bool ConnectAccount(Guid worldId, long accountId, int sessionId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            AccountConnection account = MsManager.Instance.ConnectedAccounts.Find(a => a.AccountId.Equals(accountId) && a.SessionId.Equals(sessionId));
            if (account != null)
            {
                account.ConnectedWorld = MsManager.Instance.WorldServers.Find(w => w.Id.Equals(worldId));
            }
            return account?.ConnectedWorld != null;
        }

        public bool ConnectAccountCrossServer(Guid worldId, long accountId, int sessionId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }
            AccountConnection account = MsManager.Instance.ConnectedAccounts.Where(a => a.AccountId.Equals(accountId) && a.SessionId.Equals(sessionId)).FirstOrDefault();
            if (account != null)
            {
                Observable.Timer(TimeSpan.FromMinutes(1)).Subscribe(observer => account.CanLoginCrossServer = false);
                account.OriginWorld = account.ConnectedWorld;
                account.ConnectedWorld = MsManager.Instance.WorldServers.Find(s => s.Id.Equals(worldId));
                if (account.ConnectedWorld != null)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ConnectCharacter(Guid worldId, long characterId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            //Multiple WorldGroups not yet supported by DAOFactory
            long accountId = DAOFactory.CharacterDAO.LoadById(characterId)?.AccountId ?? 0;

            AccountConnection account = MsManager.Instance.ConnectedAccounts.Find(a => a.AccountId.Equals(accountId) && a.ConnectedWorld.Id.Equals(worldId));
            if (account != null)
            {
                account.CharacterId = characterId;
                foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(account.ConnectedWorld.WorldGroup)))
                {
                    world.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().CharacterConnected(characterId);
                }
                return true;
            }
            return false;
        }

        public void DisconnectAccount(long accountId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            if (!MsManager.Instance.ConnectedAccounts.Any(s => s.AccountId.Equals(accountId) && s.CanLoginCrossServer))
            {
                MsManager.Instance.ConnectedAccounts.RemoveAll(c => c.AccountId.Equals(accountId));
            }
        }

        public void DisconnectCharacter(Guid worldId, long characterId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (AccountConnection account in MsManager.Instance.ConnectedAccounts.Where(c => c.CharacterId.Equals(characterId) && c.ConnectedWorld.Id.Equals(worldId)))
            {
                foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(account.ConnectedWorld.WorldGroup)))
                {
                    world.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().CharacterDisconnected(characterId);
                }
                if (!account.CanLoginCrossServer)
                {
                    account.CharacterId = 0;
                    account.ConnectedWorld = null;
                }
            }
        }

        public int? GetChannelIdByWorldId(Guid worldId) => MsManager.Instance.WorldServers.Find(w => w.Id == worldId)?.ChannelId;

        public bool IsAccountConnected(long accountId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            return MsManager.Instance.ConnectedAccounts.Any(c => c.AccountId == accountId && c.ConnectedWorld != null);
        }

        public bool IsCharacterConnected(string worldGroup, long characterId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            return MsManager.Instance.ConnectedAccounts.Any(c => c.ConnectedWorld != null && c.ConnectedWorld.WorldGroup == worldGroup && c.CharacterId == characterId);
        }

        public bool IsCrossServerLoginPermitted(long accountId, int sessionId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            return MsManager.Instance.ConnectedAccounts.Any(s => s.AccountId.Equals(accountId) && s.SessionId.Equals(sessionId) && s.CanLoginCrossServer);
        }

        public bool IsLoginPermitted(long accountId, int sessionId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            return MsManager.Instance.ConnectedAccounts.Any(s => s.AccountId.Equals(accountId) && s.SessionId.Equals(sessionId) && s.ConnectedWorld == null);
        }

        public void KickSession(long? accountId, int? sessionId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MsManager.Instance.WorldServers)
            {
                world.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().KickSession(accountId, sessionId);
            }
            if (accountId.HasValue)
            {
                MsManager.Instance.ConnectedAccounts.RemoveAll(s => s.AccountId.Equals(accountId.Value));
            }
            else if (sessionId.HasValue)
            {
                MsManager.Instance.ConnectedAccounts.RemoveAll(s => s.SessionId.Equals(sessionId.Value));
            }
        }

        public void PulseAccount(long accountId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            AccountConnection account = MsManager.Instance.ConnectedAccounts.Find(a => a.AccountId.Equals(accountId));
            if (account != null)
            {
                account.LastPulse = DateTime.UtcNow;
            }
        }

        public void RefreshPenalty(int penaltyId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MsManager.Instance.WorldServers)
            {
                world.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().UpdatePenaltyLog(penaltyId);
            }
            foreach (IScsServiceClient login in MsManager.Instance.LoginServers)
            {
                login.GetClientProxy<ICommunicationClient>().UpdatePenaltyLog(penaltyId);
            }
        }

        public void RegisterAccountLogin(long accountId, int sessionId, string ipAddress)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            MsManager.Instance.ConnectedAccounts.RemoveAll(a => a.AccountId.Equals(accountId));
            MsManager.Instance.ConnectedAccounts.Add(new AccountConnection(accountId, sessionId, ipAddress));
        }

        public void RegisterCrossServerAccountLogin(long accountId, int sessionId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            AccountConnection account = MsManager.Instance.ConnectedAccounts.Where(a => a.AccountId.Equals(accountId) && a.SessionId.Equals(sessionId)).FirstOrDefault();

            if (account != null)
            {
                account.CanLoginCrossServer = true;
            }
        }

        public int? RegisterWorldServer(SerializableWorldServer worldServer)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }
            WorldServer ws = new WorldServer(worldServer.Id, new ScsTcpEndPoint(worldServer.EndPointIP, worldServer.EndPointPort), worldServer.AccountLimit, worldServer.WorldGroup)
            {
                CommunicationServiceClient = CurrentClient,
                ChannelId = Enumerable.Range(1, 30).Except(MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldServer.WorldGroup)).OrderBy(w => w.ChannelId).Select(w => w.ChannelId)).First()
            };
            if (worldServer.EndPointPort == MsManager.Instance.ConfigurationObject.Act4Port)
            {
                ws.ChannelId = 51;
            }
            MsManager.Instance.WorldServers.Add(ws);
            return ws.ChannelId;
        }

        public void Restart(string worldGroup)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            if (worldGroup == "*")
            {
                foreach (WorldServer world in MsManager.Instance.WorldServers)
                {
                    world.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().Restart();
                }
            }
            else
            {
                foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
                {
                    world.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().Restart();
                }
            }
        }

        public long[][] RetrieveOnlineCharacters(long characterId)
        {
            List<AccountConnection> connections = MsManager.Instance.ConnectedAccounts.Where(s => s.IpAddress == MsManager.Instance.ConnectedAccounts.Find(f => f.CharacterId == characterId)?.IpAddress && s.CharacterId != 0);

            long[][] result = new long[connections.Count][];

            int i = 0;
            foreach (AccountConnection acc in connections)
            {
                result[i] = new long[2];
                result[i][0] = acc.CharacterId;
                result[i][1] = acc.ConnectedWorld?.ChannelId ?? 0;
                i++;
            }
            return result;
        }

        public string RetrieveOriginWorld(long accountId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }

            AccountConnection account = MsManager.Instance.ConnectedAccounts.Find(s => s.AccountId.Equals(accountId));
            if (account?.OriginWorld != null)
            {
                return $"{account.OriginWorld.Endpoint.IpAddress}:{account.OriginWorld.Endpoint.TcpPort}";
            }
            return null;
        }

        public string RetrieveRegisteredWorldServers(string username, int sessionId, bool ignoreUserName)
        {
            string lastGroup = string.Empty;
            byte worldCount = 0;
            string channelPacket = "NsTeST" + (ignoreUserName ? string.Empty : " " + username) + $" {sessionId} ";

            foreach (WorldServer world in MsManager.Instance.WorldServers.OrderBy(w => w.WorldGroup))
            {
                if (lastGroup != world.WorldGroup)
                {
                    worldCount++;
                }
                lastGroup = world.WorldGroup;

                int currentlyConnectedAccounts = MsManager.Instance.ConnectedAccounts.CountLinq(a => a.ConnectedWorld?.ChannelId == world.ChannelId);
                int channelcolor = (int)Math.Round(((double)currentlyConnectedAccounts / world.AccountLimit) * 20) + 1;

                if (world.ChannelId == 51)
                {
                    continue;
                }

                channelPacket += $"{world.Endpoint.IpAddress}:{world.Endpoint.TcpPort}:{channelcolor}:{worldCount}.{world.ChannelId}.{world.WorldGroup} ";
            }
            return channelPacket;
        }

        public IEnumerable<string> RetrieveServerStatistics()
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }

            List<string> result = new List<string>();

            try
            {
                List<string> groups = new List<string>();
                foreach (string s in MsManager.Instance.WorldServers.Select(s => s.WorldGroup))
                {
                    if (!groups.Contains(s))
                    {
                        groups.Add(s);
                    }
                }
                int totalsessions = 0;
                foreach (string message in groups)
                {
                    result.Add($"==={message}===");
                    int groupsessions = 0;
                    foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(message)))
                    {
                        int sessions = MsManager.Instance.ConnectedAccounts.CountLinq(a => a.ConnectedWorld?.Id.Equals(world.Id) == true);
                        result.Add($"Channel {world.ChannelId}: {sessions} Sessions");
                        groupsessions += sessions;
                    }
                    result.Add($"Group Total: {groupsessions} Sessions");
                    totalsessions += groupsessions;
                }
                result.Add($"Environment Total: {totalsessions} Sessions");
            }
            catch (Exception ex)
            {
                Logger.LogEventError("RETRIEVE_EXCEPTION", "Error while retreiving server Statistics:", ex);
            }

            return result;
        }

        public void RunGlobalEvent(EventType eventType)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MsManager.Instance.WorldServers)
            {
                world.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().RunGlobalEvent(eventType);
            }
        }

        public int? SendMessageToCharacter(ScsCharacterMessage message)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }

            WorldServer sourceWorld = MsManager.Instance.WorldServers.Find(s => s.Id.Equals(message.SourceWorldId));
            if (message?.Message == null || sourceWorld == null)
            {
                return null;
            }
            switch (message.Type)
            {
                case MessageType.Family:
                case MessageType.FamilyChat:
                    foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(sourceWorld.WorldGroup)))
                    {
                        world.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().SendMessageToCharacter(message);
                    }
                    return -1;

                case MessageType.PrivateChat:
                case MessageType.Whisper:
                case MessageType.WhisperSupport:
                case MessageType.WhisperGm:
                    if (message.DestinationCharacterId.HasValue)
                    {
                        AccountConnection account = MsManager.Instance.ConnectedAccounts.Find(a => a.CharacterId.Equals(message.DestinationCharacterId.Value));
                        if (account?.ConnectedWorld != null)
                        {
                            account.ConnectedWorld.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().SendMessageToCharacter(message);
                            return account.ConnectedWorld.ChannelId;
                        }
                    }
                    break;

                case MessageType.Shout:
                    foreach (WorldServer world in MsManager.Instance.WorldServers)
                    {
                        world.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().SendMessageToCharacter(message);
                    }
                    return -1;
            }
            return null;
        }

        public void Shutdown(string worldGroup)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            if (worldGroup == "*")
            {
                foreach (WorldServer world in MsManager.Instance.WorldServers)
                {
                    world.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().Shutdown();
                }
            }
            else
            {
                foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
                {
                    world.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().Shutdown();
                }
            }
        }

        public void UnregisterWorldServer(Guid worldId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            MsManager.Instance.ConnectedAccounts.RemoveAll(a => a?.ConnectedWorld?.Id.Equals(worldId) == true);
            MsManager.Instance.WorldServers.RemoveAll(w => w.Id.Equals(worldId));
        }

        public void UpdateBazaar(string worldGroup, long bazaarItemId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().UpdateBazaar(bazaarItemId);
            }
        }

        public void UpdateFamily(string worldGroup, long familyId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().UpdateFamily(familyId);
            }
        }

        public void UpdateRelation(string worldGroup, long relationId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.CommunicationServiceClient.GetClientProxy<ICommunicationClient>().UpdateRelation(relationId);
            }
        }

        #endregion
    }
}