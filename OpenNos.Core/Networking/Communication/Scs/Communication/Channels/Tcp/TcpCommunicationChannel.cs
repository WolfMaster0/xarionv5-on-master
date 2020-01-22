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

using OpenNos.Core.ConcurrencyExtensions;
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints;
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global

namespace OpenNos.Core.Networking.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// This class is used to communicate with a remote application over TCP/IP protocol.
    /// </summary>
    public sealed class TcpCommunicationChannel : CommunicationChannelBase, IDisposable
    {
        #region Members

        /// <summary>
        /// Size of the buffer that is used to receive bytes from TCP socket.
        /// </summary>
        private const int ReceiveBufferSize = 4 * 1024; // 4KB

        private const ushort PingRequest = 0x0779;

        private const ushort PingResponse = 0x0988;

        /// <summary>
        /// This buffer is used to receive bytes
        /// </summary>
        private readonly byte[] _buffer;

        /// <summary>
        /// Socket object to send/reveice messages.
        /// </summary>
        private readonly Socket _clientSocket;

        private readonly ConcurrentQueue<byte[]> _highPriorityBuffer;

        private readonly ConcurrentQueue<byte[]> _lowPriorityBuffer;

        private readonly ScsTcpEndPoint _remoteEndPoint;

        private readonly CancellationTokenSource _sendCancellationToken = new CancellationTokenSource();

        private bool _disposed;

        /// <summary>
        /// A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new TcpCommunicationChannel object.
        /// </summary>
        /// <param name="clientSocket">
        /// A connected Socket object that is used to communicate over network
        /// </param>
        public TcpCommunicationChannel(Socket clientSocket)
        {
            _clientSocket = clientSocket;
            _clientSocket.NoDelay = true;
            IPEndPoint ipEndPoint = (IPEndPoint)_clientSocket.RemoteEndPoint;
            _remoteEndPoint = new ScsTcpEndPoint(ipEndPoint.Address, ipEndPoint.Port);
            _buffer = new byte[ReceiveBufferSize];
            _highPriorityBuffer = new ConcurrentQueue<byte[]>();
            _lowPriorityBuffer = new ConcurrentQueue<byte[]>();
            CancellationToken cancellationToken = _sendCancellationToken.Token;

            // initialize lagging mode
            bool isLagMode = string.Equals(ConfigurationManager.AppSettings["LagMode"], "true", StringComparison.CurrentCultureIgnoreCase);
            Observable.Interval(new TimeSpan(0, 0, 0, 0, isLagMode ? 1000 : 10))
                .Subscribe(s => SendInterval(), cancellationToken);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the endpoint of remote application.
        /// </summary>
        public override ScsEndPoint RemoteEndPoint => _remoteEndPoint;

        #endregion

        #region Methods

        /// <summary>
        /// Duplicates the client socket and closes.
        /// </summary>
        /// <param name="processId">The process identifier.</param>
        /// <returns></returns>
        /// <summary>The callee should dispose anything relying on this channel immediately.</summary>
        public SocketInformation DuplicateSocketAndClose(int processId)
        {
            // request ping from host to kill our async BeginReceive
            _clientSocket.Send(BitConverter.GetBytes(PingRequest));

            // wait for response
            while (_running)
            {
                Thread.Sleep(20);
            }

            return _clientSocket.DuplicateAndClose(processId);
        }

        public static async Task StartSendingAsync(Action action, TimeSpan period, CancellationToken sendCancellationToken)
        {
            while (!sendCancellationToken.IsCancellationRequested)
            {
                await Task.Delay(period, sendCancellationToken).ConfigureAwait(false);
                if (!sendCancellationToken.IsCancellationRequested)
                {
                    action?.Invoke();
                }
            }
        }

        public override Task ClearLowPriorityQueueAsync()
        {
            _lowPriorityBuffer.Clear();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disconnects from remote application and closes channel.
        /// </summary>
        public override void Disconnect()
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            _running = false;
            try
            {
                _sendCancellationToken.Cancel();
                if (_clientSocket.Connected)
                {
                    _clientSocket.Close();
                }

                _clientSocket.Dispose();
            }
            catch
            {
                // do nothing
            }
            finally
            {
                _sendCancellationToken.Dispose();
            }

            CommunicationState = CommunicationStates.Disconnected;
            OnDisconnected();
        }

        /// <summary>
        /// Calls Disconnect method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void SendInterval()
        {
            try
            {
                if (WireProtocol != null)
                {
                    SendByPriority(_highPriorityBuffer);
                    SendByPriority(_lowPriorityBuffer);
                }
            }
            catch
            {
                // Disconnect();
            }
            if (!_clientSocket.Connected)
            {
                // do nothing
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disconnect();
                    // we most likely dont have to dispose but lets try for any case that might be an exception
                    _sendCancellationToken.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Sends a message to the remote application.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="priority">Priority of message to send</param>
        protected override void SendMessagePublic(IScsMessage message, byte priority)
        {
            if (priority > 5)
            {
                _highPriorityBuffer.Enqueue(WireProtocol.GetBytes(message));
            }
            else
            {
                _lowPriorityBuffer.Enqueue(WireProtocol.GetBytes(message));
            }
        }

        /// <summary>
        /// Starts the thread to receive messages from socket.
        /// </summary>
        protected override void StartPublic()
        {
            _running = true;
            _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, 0, ReceiveCallback, null);
        }

        private static void SendCallback(IAsyncResult result)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)result.AsyncState;

                if (!client.Connected)
                {
                    return;
                }

                // Complete sending the data to the remote device.
                client.EndSend(result);
            }
            catch
            {
                // Disconnect();
            }
        }

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method. It
        /// reveives bytes from socker.
        /// </summary>
        /// <param name="result">Asyncronous call result</param>
        private void ReceiveCallback(IAsyncResult result)
        {
            if (!_running)
            {
                return;
            }

            try
            {
                // Get received bytes count
                int bytesRead = _clientSocket.EndReceive(result);

                if (bytesRead > 0)
                {
                    switch (BitConverter.ToUInt16(_buffer, 0))
                    {
                        case PingRequest:
                            _clientSocket.Send(BitConverter.GetBytes(PingResponse));
                            goto CONT_RECEIVE;

                        case PingResponse:
                            _running = false;
                            return;
                    }

                    LastReceivedMessageTime = DateTime.UtcNow;

                    // Copy received bytes to a new byte array
                    byte[] receivedBytes = new byte[bytesRead];
                    Array.Copy(_buffer, receivedBytes, bytesRead);

                    // Read messages according to current wire protocol and raise MessageReceived
                    // event for all received messages
                    foreach (IScsMessage message in WireProtocol.CreateMessages(receivedBytes))
                    {
                        OnMessageReceived(message, DateTime.UtcNow);
                    }
                }
                else
                {
                    Logger.Info(Language.Instance.GetMessageFromKey("CLIENT_DISCONNECTED"));
                    Disconnect();
                }

                CONT_RECEIVE:
                // Read more bytes if still running
                if (_running)
                {
                    _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, 0, ReceiveCallback, null);
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        private void SendByPriority(ConcurrentQueue<byte[]> buffer)
        {
            List<byte> outgoingPacket = new List<byte>();

            // send max 30 packets at once
            for (int i = 0; i < 30; i++)
            {
                if (buffer.TryDequeue(out byte[] message) && message != null)
                {
                    outgoingPacket = outgoingPacket.Concat(message).ToList();
                }
                else
                {
                    break;
                }
            }

            if (outgoingPacket.Count > 0)
            {
                _clientSocket.BeginSend(outgoingPacket.ToArray(), 0, outgoingPacket.Count, SocketFlags.None,
                SendCallback, _clientSocket);
            }
        }

        #endregion
    }
}