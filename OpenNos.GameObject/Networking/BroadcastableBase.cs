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
using OpenNos.Core.Threading;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Networking
{
    public abstract class BroadcastableBase : IDisposable
    {
        #region Members

        /// <summary>
        /// List of all connected clients.
        /// </summary>
        private readonly ThreadSafeSortedList<long, ClientSession> _sessions;

        private bool _disposed;

        #endregion

        #region Instantiation

        protected BroadcastableBase()
        {
            LastUnregister = DateTime.UtcNow.AddMinutes(-1);
            _sessions = new ThreadSafeSortedList<long, ClientSession>();
        }

        #endregion

        #region Properties

        public IEnumerable<ClientSession> Sessions => _sessions.Where(s => s.HasSelectedCharacter && !s.IsDisposing && s.IsConnected);

        protected DateTime LastUnregister { get; private set; }

        #endregion

        #region Methods

        public void Broadcast(string packet, ReceiverType receiver = ReceiverType.All) => Broadcast(null, packet, receiver);

        public void Broadcast(string packet, int xRangeCoordinate, int yRangeCoordinate) => Broadcast(new BroadcastPacket(null, packet, ReceiverType.AllInRange, xCoordinate: xRangeCoordinate, yCoordinate: yRangeCoordinate));

        public void Broadcast(BroadcastPacket packet)
        {
            try
            {
                SpreadBroadcastpacket(packet);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void Broadcast(ClientSession client, string content, ReceiverType receiver = ReceiverType.All, string characterName = "", long characterId = -1)
        {
            try
            {
                SpreadBroadcastpacket(new BroadcastPacket(client, content, receiver, characterName, characterId));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ClientSession GetSessionByCharacterId(long characterId) => _sessions.ContainsKey(characterId) ? _sessions[characterId] : null;

        public void RegisterSession(ClientSession session)
        {
            if (!session.HasSelectedCharacter)
            {
                return;
            }
            session.RegisterTime = DateTime.UtcNow;

            // Create a ChatClient and store it in a collection
            _sessions[session.Character.CharacterId] = session;
            if (session.HasCurrentMapInstance)
            {
                session.CurrentMapInstance.IsSleeping = false;
            }
        }

        public void UnregisterSession(long characterId)
        {
            // Get client from client list, if not in list do not continue
            ClientSession session = _sessions[characterId];
            if (session == null)
            {
                return;
            }

            // Remove client from online clients list
            _sessions.Remove(characterId);
            if (session.HasCurrentMapInstance && _sessions.Count == 0)
            {
                session.CurrentMapInstance.IsSleeping = true;
            }
            LastUnregister = DateTime.UtcNow;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _sessions.Dispose();
                }
                _disposed = true;
            }
        }

        private void SpreadBroadcastpacket(BroadcastPacket sentPacket)
        {
            if (Sessions != null && !string.IsNullOrEmpty(sentPacket?.Packet))
            {
                switch (sentPacket.Receiver)
                {
                    case ReceiverType.All: // send packet to everyone
                        if (sentPacket.Packet.StartsWith("out", StringComparison.CurrentCulture))
                        {
                            foreach (ClientSession session in Sessions)
                            {
                                if (session.HasSelectedCharacter)
                                {
                                    if (sentPacket.Sender != null)
                                    {
                                        if (!sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId))
                                        {
                                            session.SendPacket(sentPacket.Packet);
                                        }
                                    }
                                    else
                                    {
                                        session.SendPacket(sentPacket.Packet);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Parallel.ForEach(Sessions, session =>
                            {
                                if (session?.HasSelectedCharacter == true)
                                {
                                    if (sentPacket.Sender != null)
                                    {
                                        if (!sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId))
                                        {
                                            session.SendPacket(sentPacket.Packet);
                                        }
                                    }
                                    else
                                    {
                                        session.SendPacket(sentPacket.Packet);
                                    }
                                }
                            });
                        }
                        break;

                    case ReceiverType.AllExceptMe: // send to everyone except the sender
                        if (sentPacket.Packet.StartsWith("out", StringComparison.CurrentCulture))
                        {
                            foreach (ClientSession session in Sessions.Where(s => s?.SessionId != sentPacket.Sender?.SessionId))
                            {
                                if (session.HasSelectedCharacter)
                                {
                                    if (sentPacket.Sender != null)
                                    {
                                        if (!sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId))
                                        {
                                            session.SendPacket(sentPacket.Packet);
                                        }
                                    }
                                    else
                                    {
                                        session.SendPacket(sentPacket.Packet);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Parallel.ForEach(Sessions.Where(s => s?.SessionId != sentPacket.Sender?.SessionId), session =>
                            {
                                if (session?.HasSelectedCharacter == true)
                                {
                                    if (sentPacket.Sender != null)
                                    {
                                        if (!sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId))
                                        {
                                            session.SendPacket(sentPacket.Packet);
                                        }
                                    }
                                    else
                                    {
                                        session.SendPacket(sentPacket.Packet);
                                    }
                                }
                            });
                        }
                        break;

                    case ReceiverType.AllExceptGroup:
                        if (sentPacket.Packet.StartsWith("out", StringComparison.CurrentCulture))
                        {
                            foreach (ClientSession session in Sessions.Where(s => s.SessionId != sentPacket.Sender.SessionId))
                            {
                                if (session.HasSelectedCharacter)
                                {
                                    if (sentPacket.Sender != null)
                                    {
                                        if (!sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId))
                                        {
                                            session.SendPacket(sentPacket.Packet);
                                        }
                                    }
                                    else
                                    {
                                        session.SendPacket(sentPacket.Packet);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (ClientSession session in Sessions.Where(s => s.SessionId != sentPacket.Sender.SessionId && (s.Character?.Group == null || (s.Character?.Group?.GroupId != sentPacket.Sender?.Character?.Group?.GroupId))))
                            {
                                if (session.HasSelectedCharacter && !sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId))
                                {
                                    session.SendPacket(sentPacket.Packet);
                                }
                            }
                        }
                        break;

                    case ReceiverType.AllExceptMeAct4: // send to everyone except the sender(Act4)
                        Parallel.ForEach(Sessions.Where(s => s.SessionId != sentPacket.Sender.SessionId), session =>
                        {
                            if (session?.HasSelectedCharacter == true)
                            {
                                if (sentPacket.Sender != null)
                                {
                                    if (!sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId))
                                    {
                                        if (session.Character.Faction == sentPacket.Sender.Character.Faction)
                                        {
                                            session.SendPacket(sentPacket.Packet);
                                        }
                                        // ReSharper disable once RedundantIfElseBlock
                                        else
                                        {
                                            // TODO: Scrambled Packet for Act4
                                        }
                                    }
                                }
                                else
                                {
                                    session.SendPacket(sentPacket.Packet);
                                }
                            }
                        });
                        break;

                    case ReceiverType.AllInRange: // send to everyone which is in a range of 50x50
                        if (sentPacket.XCoordinate != 0 && sentPacket.YCoordinate != 0)
                        {
                            Parallel.ForEach(Sessions.Where(s => s?.Character.IsInRange(sentPacket.XCoordinate, sentPacket.YCoordinate) == true), session =>
                            {
                                if (session?.HasSelectedCharacter == true)
                                {
                                    if (sentPacket.Sender != null)
                                    {
                                        if (!sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId))
                                        {
                                            session.SendPacket(sentPacket.Packet);
                                        }
                                    }
                                    else
                                    {
                                        session.SendPacket(sentPacket.Packet);
                                    }
                                }
                            });
                        }
                        break;

                    case ReceiverType.OnlySomeone:
                        if (sentPacket.SomeonesCharacterId > 0 || !string.IsNullOrEmpty(sentPacket.SomeonesCharacterName))
                        {
                            ClientSession targetSession = Sessions.SingleOrDefault(s => s.Character.CharacterId == sentPacket.SomeonesCharacterId || s.Character.Name == sentPacket.SomeonesCharacterName);
                            if (targetSession?.HasSelectedCharacter == true)
                            {
                                if (sentPacket.Sender != null)
                                {
                                    if (!sentPacket.Sender.Character.IsBlockedByCharacter(targetSession.Character.CharacterId))
                                    {
                                        targetSession.SendPacket(sentPacket.Packet);
                                    }
                                    else
                                    {
                                        sentPacket.Sender.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                                    }
                                }
                                else
                                {
                                    targetSession.SendPacket(sentPacket.Packet);
                                }
                            }
                        }
                        break;

                    case ReceiverType.AllNoEmoBlocked:
                        Parallel.ForEach(Sessions.Where(s => s?.Character.EmoticonsBlocked == false), session =>
                        {
                            if (session?.HasSelectedCharacter == true && sentPacket.Sender?.Character.IsBlockedByCharacter(session.Character.CharacterId) == false)
                            {
                                session.SendPacket(sentPacket.Packet);
                            }
                        });
                        break;

                    case ReceiverType.AllNoHeroBlocked:
                        Parallel.ForEach(Sessions.Where(s => s?.Character.HeroChatBlocked == false), session =>
                        {
                            if (session?.HasSelectedCharacter == true && sentPacket.Sender?.Character.IsBlockedByCharacter(session.Character.CharacterId) == false)
                            {
                                session.SendPacket(sentPacket.Packet);
                            }
                        });
                        break;

                    case ReceiverType.Group:
                        foreach (ClientSession session in Sessions.Where(s => s.Character?.Group != null && sentPacket.Sender?.Character?.Group != null && s.Character.Group.GroupId == sentPacket.Sender.Character.Group.GroupId))
                        {
                            session.SendPacket(sentPacket.Packet);
                        }
                        break;

                    case ReceiverType.Unknown:
                        break;
                }
            }
        }

        #endregion
    }
}