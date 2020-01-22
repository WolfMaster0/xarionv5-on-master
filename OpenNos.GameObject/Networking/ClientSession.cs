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
using OpenNos.Core.ConcurrencyExtensions;
using OpenNos.Core.Cryptography;
using OpenNos.Core.Handling;
using OpenNos.Core.Networking;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.Master.Library.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject.Networking
{
    public class ClientSession
    {
        #region Members

        public bool HealthStop;

        private static CryptographyBase _encryptor;

        private readonly INetworkClient _client;

        private readonly ConcurrentQueue<byte[]> _receiveQueue;

        private readonly IList<string> _waitForPacketList = new List<string>();

        private Character _character;

        private int _lastPacketId;

        // private byte countPacketReceived;

        private int? _waitForPacketsAmount;

        private readonly IDisposable _packetHandler;

        #endregion

        #region Instantiation

        public ClientSession(INetworkClient client)
        {
            // initialize lagging mode
            bool isLagMode = string.Equals(ConfigurationManager.AppSettings["LagMode"], "true", StringComparison.CurrentCultureIgnoreCase);

            // initialize network client
            _client = client;

            // absolutely new instantiated Client has no SessionId
            SessionId = 0;

            // register for NetworkClient events
            _client.MessageReceived += OnNetworkClientMessageReceived;

            // start observer for receiving packets
            _receiveQueue = new ConcurrentQueue<byte[]>();
            _packetHandler = Observable.Interval(new TimeSpan(0, 0, 0, 0, isLagMode ? 1000 : 10)).Subscribe(x => HandlePackets());
        }

        #endregion

        #region Properties

        public Account Account { get; private set; }

        public Character Character
        {
            get
            {
                if (_character == null || !HasSelectedCharacter)
                {
                    // cant access an
                    Logger.Warn("Uninitialized Character cannot be accessed.");
                }

                return _character;
            }
            private set => _character = value;
        }

        public long ClientId => _client.ClientId;

        public MapInstance CurrentMapInstance { get; set; }

        public bool HasCurrentMapInstance => CurrentMapInstance != null;

        public bool HasSelectedCharacter { get; private set; }

        public bool HasSession => _client != null;

        public string IpAddress => _client.IpAddress;

        public bool IsAuthenticated { get; private set; }

        public bool IsConnected => _client.IsConnected;

        public bool IsDisposing
        {
            get => _client.IsDisposing;
            internal set => _client.IsDisposing = value;
        }

        public bool IsLocalhost => IpAddress.Contains("127.0.0.1");

        public bool IsOnMap => CurrentMapInstance != null;

        public DateTime RegisterTime { get; internal set; }

        public int SessionId { get; private set; }

        #endregion

        #region Methods

        public void ClearLowPriorityQueue() => _client.ClearLowPriorityQueueAsync();

        public void Destroy()
        {
            // unregister from WCF events
            CommunicationServiceClient.Instance.CharacterConnectedEvent -= OnOtherCharacterConnected;
            CommunicationServiceClient.Instance.CharacterDisconnectedEvent -= OnOtherCharacterDisconnected;

            // do everything necessary before removing client, DB save, Whatever
            if (HasSelectedCharacter)
            {
                GameLogger.Instance.LogCharacterLogout(ServerManager.Instance.ChannelId, Character.Name, Character.CharacterId);
                Character.Dispose();
                if (Character.MapInstance?.MapInstanceType == MapInstanceType.TimeSpaceInstance || Character.MapInstance?.MapInstanceType == MapInstanceType.RaidInstance)
                {
                    Character.MapInstance.InstanceBag.DeadList.Add(Character.CharacterId);
                    if (Character.MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        Character?.Group?.Characters?.ForEach(s =>
                        {
                            if (s?.Character != null)
                            {
                                s.SendPacket(s.Character.Group.GeneraterRaidmbf(s));
                                s.SendPacket(s.Character.Group.GenerateRdlst());
                            }
                        });
                    }
                }
                if (Character?.Miniland != null)
                {
                    ServerManager.RemoveMapInstance(Character.Miniland.MapInstanceId);
                }

                Character.CloseExchangeOrTrade();

                // disconnect client
                CommunicationServiceClient.Instance.DisconnectCharacter(ServerManager.Instance.WorldId, Character.CharacterId);

                // unregister from map if registered
                if (CurrentMapInstance != null)
                {
                    CurrentMapInstance.UnregisterSession(Character.CharacterId);
                    CurrentMapInstance = null;
                    ServerManager.Instance.UnregisterSession(Character.CharacterId);
                }
            }

            if (Account != null)
            {
                CommunicationServiceClient.Instance.DisconnectAccount(Account.AccountId);
            }

            _receiveQueue.Clear();
        }

        public void Disconnect() => _client.Disconnect();

        public string GenerateIdentity()
        {
            if (Character != null)
            {
                return $"Character: {Character.Name}";
            }
            return $"Account: {Account.Name}";
        }

        public void Initialize(CryptographyBase encryptor, bool isWorldServer)
        {
            _encryptor = encryptor;
            _client.Initialize(encryptor);
        }

        public void InitializeAccount(Account account, bool crossServer = false)
        {
            Account = account;
            if (crossServer)
            {
                CommunicationServiceClient.Instance.ConnectAccountCrossServer(ServerManager.Instance.WorldId, account.AccountId, SessionId);
            }
            else
            {
                CommunicationServiceClient.Instance.ConnectAccount(ServerManager.Instance.WorldId, account.AccountId, SessionId);
            }
            IsAuthenticated = true;
        }

        public void ReceivePacket(string packet, bool ignoreAuthority = false)
        {
            string header = packet.Split(' ')[0];
            TriggerHandler(header, $"{_lastPacketId} {packet}", false, ignoreAuthority);
            _lastPacketId++;
        }

        public void SendPacket(string packet, byte priority = 10)
        {
            if (!IsDisposing)
            {
                _client.SendPacket(packet, priority);
            }
        }

        public void SendPacketAfter(string packet, int milliseconds)
        {
            if (!IsDisposing)
            {
                Observable.Timer(TimeSpan.FromMilliseconds(milliseconds)).Subscribe(o => SendPacket(packet));
            }
        }

        public void SendPacketFormat(string packet, params object[] param)
        {
            if (!IsDisposing)
            {
                _client.SendPacketFormat(packet, param);
            }
        }

        public void SendPackets(IEnumerable<string> packets, byte priority = 10)
        {
            if (!IsDisposing)
            {
                _client.SendPackets(packets, priority);
            }
        }

        public void SetCharacter(Character character)
        {
            Character = character;
            HasSelectedCharacter = true;

            GameLogger.Instance.LogCharacterLogin(ServerManager.Instance.ChannelId, Character.Name, Character.CharacterId);

            // register CSC events
            CommunicationServiceClient.Instance.CharacterConnectedEvent += OnOtherCharacterConnected;
            CommunicationServiceClient.Instance.CharacterDisconnectedEvent += OnOtherCharacterDisconnected;

            // register for servermanager
            ServerManager.Instance.RegisterSession(this);
            ServerManager.Instance.CharacterScreenSessions.Remove(character.AccountId);
            Character.SetSession(this);
        }

        /// <summary>
        /// Handle the packet received by the Client.
        /// </summary>
        private void HandlePackets()
        {
            while (_receiveQueue.TryDequeue(out byte[] packetData))
            {
                // determine first packet
                if (_encryptor.HasCustomParameter && SessionId == 0)
                {
                    string sessionPacket = _encryptor.DecryptCustomParameter(packetData);
                    string[] sessionParts = sessionPacket.Split(' ');
                    if (sessionParts.Length == 0)
                    {
                        return;
                    }
                    if (!int.TryParse(sessionParts[0], out int packetId))
                    {
                        Disconnect();
                    }
                    _lastPacketId = packetId;

                    // set the SessionId if Session Packet arrives
                    if (sessionParts.Length < 2)
                    {
                        return;
                    }
                    if (int.TryParse(sessionParts[1].Split('\\').FirstOrDefault(), out int sessid))
                    {
                        SessionId = sessid;
                        Logger.Debug(string.Format(Language.Instance.GetMessageFromKey("CLIENT_ARRIVED"), SessionId));
                        if (!_waitForPacketsAmount.HasValue)
                        {
                            TriggerHandler("OpenNos.EntryPoint", string.Empty, false);
                        }
                    }
                    return;
                }

                string packetConcatenated = _encryptor.Decrypt(packetData, SessionId);
                foreach (string packet in packetConcatenated.Split(new[] { (char)0xFF }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string packetstring = packet.Replace('^', ' ');
                    string[] packetsplit = packetstring.Split(' ');

                    if (_encryptor.HasCustomParameter)
                    {
                        string nextRawPacketId = packetsplit[0];
                        if (!int.TryParse(nextRawPacketId, out int nextPacketId) && nextPacketId != _lastPacketId + 1)
                        {
                            Logger.Error(string.Format(Language.Instance.GetMessageFromKey("CORRUPTED_KEEPALIVE"), _client.ClientId));
                            _client.Disconnect();
                            return;
                        }
                        if (nextPacketId == 0)
                        {
                            if (_lastPacketId == ushort.MaxValue)
                            {
                                _lastPacketId = nextPacketId;
                            }
                        }
                        else
                        {
                            _lastPacketId = nextPacketId;
                        }

                        if (_waitForPacketsAmount.HasValue)
                        {
                            _waitForPacketList.Add(packetstring);
                            string[] packetssplit = packetstring.Split(' ');
                            if (packetssplit.Length > 3 && packetsplit[1] == "DAC")
                            {
                                _waitForPacketList.Add("0 CrossServerAuthenticate");
                            }
                            if (_waitForPacketList.Count == _waitForPacketsAmount)
                            {
                                _waitForPacketsAmount = null;
                                string queuedPackets = string.Join(" ", _waitForPacketList.ToArray());
                                string header = queuedPackets.Split(' ', '^')[1];
                                TriggerHandler(header, queuedPackets, true);
                                _waitForPacketList.Clear();
                                return;
                            }
                        }
                        else if (packetsplit.Length > 1)
                        {
                            if (packetsplit[1].Length >= 1 && (packetsplit[1][0] == '/' || packetsplit[1][0] == ':' || packetsplit[1][0] == ';'))
                            {
                                packetsplit[1] = packetsplit[1][0].ToString();
                                packetstring = packet.Insert(packet.IndexOf(' ') + 2, " ");
                            }
                            if (packetsplit[1] != "0")
                            {
                                TriggerHandler(packetsplit[1].Replace("#", string.Empty), packetstring, false);
                            }
                        }
                    }
                    else
                    {
                        string packetHeader = packetstring.Split(' ')[0];

                        // simple messaging
                        if (packetHeader[0] == '/' || packetHeader[0] == ':' || packetHeader[0] == ';')
                        {
                            packetHeader = packetHeader[0].ToString();
                            packetstring = packet.Insert(packet.IndexOf(' ') + 2, " ");
                        }

                        TriggerHandler(packetHeader.Replace("#", string.Empty), packetstring, false);
                    }
                }
            }

            if (!IsConnected)
            {
                _packetHandler.Dispose();
            }
        }

        /// <summary>
        /// This will be triggered when the underlying NetworkClient receives a packet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNetworkClientMessageReceived(object sender, MessageEventArgs e)
        {
            if (!(e.Message is ScsRawDataMessage message))
            {
                return;
            }
            if (message.MessageData.Length > 2)
            {
                _receiveQueue.Enqueue(message.MessageData);
            }
        }

        private void OnOtherCharacterConnected(object sender, EventArgs e)
        {
            Tuple<long, string> loggedInCharacter = (Tuple<long, string>)sender;

            if (Character.IsFriendOfCharacter(loggedInCharacter.Item1) && Character != null && Character.CharacterId != loggedInCharacter.Item1)
            {
                _client.SendPacket(Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("CHARACTER_LOGGED_IN"), loggedInCharacter.Item2), 10));
                _client.SendPacket(Character.GenerateFinfo(loggedInCharacter.Item1, true));
            }
            FamilyCharacter chara = Character.Family?.FamilyCharacters.Find(s => s.CharacterId == loggedInCharacter.Item1);
            if (chara != null && loggedInCharacter.Item1 != Character?.CharacterId)
            {
                _client.SendPacket(Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("CHARACTER_FAMILY_LOGGED_IN"), loggedInCharacter.Item2, Language.Instance.GetMessageFromKey(chara.Authority.ToString().ToUpper())), 10));
            }
        }

        private void OnOtherCharacterDisconnected(object sender, EventArgs e)
        {
            Tuple<long, string> loggedOutCharacter = (Tuple<long, string>)sender;
            if (Character.IsFriendOfCharacter(loggedOutCharacter.Item1) && Character != null && Character.CharacterId != loggedOutCharacter.Item1)
            {
                _client.SendPacket(Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("CHARACTER_LOGGED_OUT"), loggedOutCharacter.Item2), 10));
                _client.SendPacket(Character.GenerateFinfo(loggedOutCharacter.Item1, false));
            }
        }

        private void TriggerHandler(string packetHeader, string packet, bool force, bool ignoreAuthority = false)
        {
            if (ServerManager.Instance.InShutdown || string.IsNullOrWhiteSpace(packetHeader))
            {
                return;
            }
            if (!IsDisposing)
            {
                HandlerMethodReference methodReference = PacketFacility.GetHandlerMethodReference(packetHeader);
                if (methodReference != null)
                {
                    if (!force && methodReference.Amount > 1 && !_waitForPacketsAmount.HasValue)
                    {
                        // we need to wait for more
                        _waitForPacketsAmount = methodReference.Amount;
                        _waitForPacketList.Add(packet != string.Empty ? packet : $"1 {packetHeader} ");
                        return;
                    }
                    try
                    {
                        if ((HasSelectedCharacter || !methodReference.CharacterRequired)
                            && methodReference.PacketDefinitionParameterType != null)
                        {
                            //check for the correct authority
                            if (!IsAuthenticated || Account.Authority >= methodReference.Authority
                                || (Account.Authority == AuthorityType.BitchNiggerFaggot && methodReference.Authority == AuthorityType.User)
                                || ignoreAuthority)
                            {
                                packetHeader = packetHeader.ToLower();
                                string[] allowedPackets = {"nos0575", "char_new", "char_del", "opennos.entrypoint", "game_start", "guri", "pulse", "$totprequest", "$totpreset", "select"};
                                if (Account?.IsVerified == true || allowedPackets.Any(s => s == packetHeader))
                                    PacketFacility.HandlePacket(this, packetHeader.ToLower(), packet);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // disconnect if something unexpected happens
                        Logger.Error("Handler Error SessionId: " + SessionId, ex);
                        Disconnect();
                    }
                }
                else
                {
                    Logger.Warn(string.Format(Language.Instance.GetMessageFromKey("HANDLER_NOT_FOUND"), packetHeader));
                }
            }
            else
            {
                Logger.Warn(string.Format(Language.Instance.GetMessageFromKey("CLIENTSESSION_DISPOSING"), packetHeader));
            }
        }

        #endregion
    }
}