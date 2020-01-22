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

using System.Collections.Concurrent;
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Networking;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Networking
{
    public class SessionManager
    {
        #region Members

        protected ConcurrentDictionary<long, ClientSession> Sessions = new ConcurrentDictionary<long, ClientSession>();

        #endregion

        #region Instantiation

        public SessionManager(bool isWorldServer) => IsWorldServer = isWorldServer;

        #endregion

        #region Properties

        public bool IsWorldServer { get; set; }

        #endregion

        #region Methods

        public void AddSession(INetworkClient customClient)
        {
            Logger.Info(Language.Instance.GetMessageFromKey("NEW_CONNECT") + customClient.ClientId);

            ClientSession session = IntializeNewSession(customClient);
            customClient.SetClientSession(session);

            if (session != null && IsWorldServer && !Sessions.TryAdd(customClient.ClientId, session))
            {
                Logger.Warn(string.Format(Language.Instance.GetMessageFromKey("FORCED_DISCONNECT"), customClient.ClientId));
                customClient.Disconnect();
                Sessions.TryRemove(customClient.ClientId, out session);
            }
        }

        public virtual void StopServer()
        {
            Sessions.Clear();
            ServerManager.StopServer();
        }

        protected virtual ClientSession IntializeNewSession(INetworkClient client)
        {
            ClientSession session = new ClientSession(client);
            client.SetClientSession(session);
            return session;
        }

        protected void RemoveSession(INetworkClient client)
        {
            Sessions.TryRemove(client.ClientId, out ClientSession session);

            // check if session hasnt been already removed
            if (session != null)
            {
                session.IsDisposing = true;

                if (IsWorldServer && session.HasSelectedCharacter)
                {
                    if (session.Character.Hp < 1)
                    {
                        session.Character.Hp = 1;
                    }

                    session.Character.Save();

                    session.Character.Mates.Where(s => s.IsTeamMember && !s.Owner.IsVehicled).ToList().ForEach(s => session.CurrentMapInstance?.Broadcast(session, s.GenerateOut(), ReceiverType.AllExceptMe));
                    session.CurrentMapInstance?.Broadcast(session, StaticPacketHelper.Out(UserType.Player, session.Character.CharacterId), ReceiverType.AllExceptMe);

                    if (ServerManager.Instance.Groups.Any(s => s.IsMemberOfGroup(session.Character.CharacterId)))
                    {
                        ServerManager.Instance.GroupLeave(session);
                    }
                }

                session.Destroy();

                client.Disconnect();
                Logger.Info(Language.Instance.GetMessageFromKey("DISCONNECT") + client.ClientId);

                // session = null;
            }
        }

        #endregion
    }
}