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
using OpenNos.Core.Networking.Communication.Scs.Client;
using OpenNos.Core.Networking.Communication.Scs.Communication;
using OpenNos.Core.Networking.Communication.Scs.Communication.Channels;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messengers;
using OpenNos.Core.Networking.Communication.ScsServices.Communication;
using OpenNos.Core.Networking.Communication.ScsServices.Communication.Messages;
using System;
using System.Linq;
using System.Reflection;

namespace OpenNos.Core.Networking.Communication.ScsServices.Client
{
    /// <summary>
    /// Represents a service client that consumes a SCS service.
    /// </summary>
    /// <typeparam name="T">Type of service interface</typeparam>
    public sealed class ScsServiceClient<T> : IScsServiceClient<T> where T : class
    {
        #region Members

        /// <summary>
        /// Underlying IScsClient object to communicate with server.
        /// </summary>
        private readonly IScsClient _client;

        /// <summary>
        /// The client object that is used to call method invokes in client side. May be null if
        /// client has no methods to be invoked by server.
        /// </summary>
        private readonly object _clientObject;

        /// <summary>
        /// Messenger object to send/receive messages over _client.
        /// </summary>
        private readonly RequestReplyMessenger<IScsClient> _requestReplyMessenger;

        private bool _disposed;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new ScsServiceClient object.
        /// </summary>
        /// <param name="client">Underlying IScsClient object to communicate with server</param>
        /// <param name="clientObject">
        /// The client object that is used to call method invokes in client side. May be null if
        /// client has no methods to be invoked by server.
        /// </param>
        public ScsServiceClient(IScsClient client, object clientObject)
        {
            _client = client;
            _clientObject = clientObject;

            _client.Connected += Client_Connected;
            _client.Disconnected += Client_Disconnected;

            _requestReplyMessenger = new RequestReplyMessenger<IScsClient>(client);
            _requestReplyMessenger.MessageReceived += RequestReplyMessenger_MessageReceived;

            AutoConnectRemoteInvokeProxy<T, IScsClient> realServiceProxy = new AutoConnectRemoteInvokeProxy<T, IScsClient>(_requestReplyMessenger, this);
            ServiceProxy = (T)realServiceProxy.GetTransparentProxy();
        }

        #endregion

        #region Events

        /// <summary>
        /// This event is raised when client connected to server.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// This event is raised when client disconnected from server.
        /// </summary>
        public event EventHandler Disconnected;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public ICommunicationChannel CommunicationChannel => _client.CommunicationChannel;

        /// <summary>
        /// Gets the current communication state.
        /// </summary>
        public CommunicationStates CommunicationState => _client.CommunicationState;

        /// <summary>
        /// Timeout for connecting to a server (as milliseconds). Default value: 15 seconds (15000 ms).
        /// </summary>
        public int ConnectTimeout
        {
            get => _client.ConnectTimeout;
            set => _client.ConnectTimeout = value;
        }

        /// <summary>
        /// Reference to the service proxy to invoke remote service methods.
        /// </summary>
        public T ServiceProxy { get; }

        /// <summary>
        /// Timeout value when invoking a service method. If timeout occurs before end of remote
        /// method call, an exception is thrown. Use -1 for no timeout (wait indefinite). Default
        /// value: 60000 (1 minute).
        /// </summary>
        public int Timeout
        {
            get => _requestReplyMessenger.Timeout;
            set => _requestReplyMessenger.Timeout = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Connects to server.
        /// </summary>
        public void Connect() => _client.Connect();

        /// <summary>
        /// Disconnects from server. Does nothing if already disconnected.
        /// </summary>
        public void Disconnect() => _client.Disconnect();

        /// <summary>
        /// Calls Disconnect method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets a service proxy for the specified <typeparamref name="TServiceInterface"/>.
        /// </summary>
        /// <typeparam name="TServiceInterface">the service interface type</typeparam>
        /// <returns></returns>
        public TServiceInterface GetServiceProxy<TServiceInterface>() => (TServiceInterface)new AutoConnectRemoteInvokeProxy<TServiceInterface, IScsClient>(_requestReplyMessenger, this).GetTransparentProxy();

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disconnect();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Handles Connected event of _client object.
        /// </summary>
        /// <param name="sender">Source of object</param>
        /// <param name="e">Event arguments</param>
        private void Client_Connected(object sender, EventArgs e)
        {
            _requestReplyMessenger.Start();
            OnConnected();
        }

        /// <summary>
        /// Handles Disconnected event of _client object.
        /// </summary>
        /// <param name="sender">Source of object</param>
        /// <param name="e">Event arguments</param>
        private void Client_Disconnected(object sender, EventArgs e)
        {
            _requestReplyMessenger.Stop();
            OnDisconnected();
        }

        /// <summary>
        /// Raises Connected event.
        /// </summary>
        private void OnConnected()
        {
            EventHandler handler = Connected;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises Disconnected event.
        /// </summary>
        private void OnDisconnected()
        {
            EventHandler handler = Disconnected;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles MessageReceived event of messenger. It gets messages from server and invokes
        /// appropriate method.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void RequestReplyMessenger_MessageReceived(object sender, MessageEventArgs e)
        {
            // Cast message to ScsRemoteInvokeMessage and check it
            if (!(e.Message is ScsRemoteInvokeMessage invokeMessage))
            {
                return;
            }

            // Check client object.
            if (_clientObject == null)
            {
                SendInvokeResponse(invokeMessage, null, new ScsRemoteException("Client does not wait for method invocations by server."));
                return;
            }

            // Invoke method
            object returnValue;
            try
            {
                // reflection?
                Type type = _clientObject.GetType();
                MethodInfo method = type.GetMethod(invokeMessage.MethodName) ?? type.GetInterfaces().Select(t => t.GetMethod(invokeMessage.MethodName)).FirstOrDefault(m => m != null);
                returnValue = method?.Invoke(_clientObject, invokeMessage.Parameters);
            }
            catch (TargetInvocationException ex)
            {
                Exception innerEx = ex.InnerException;
                if (innerEx != null)
                {
                    SendInvokeResponse(invokeMessage, null, new ScsRemoteException(innerEx.Message, innerEx));
                }
                return;
            }
            catch (Exception ex)
            {
                SendInvokeResponse(invokeMessage, null, new ScsRemoteException(ex.Message, ex));
                return;
            }

            // Send return value
            SendInvokeResponse(invokeMessage, returnValue, null);
        }

        /// <summary>
        /// Sends response to the remote application that invoked a service method.
        /// </summary>
        /// <param name="requestMessage">Request message</param>
        /// <param name="returnValue">Return value to send</param>
        /// <param name="exception">Exception to send</param>
        private void SendInvokeResponse(IScsMessage requestMessage, object returnValue, ScsRemoteException exception)
        {
            try
            {
                _requestReplyMessenger.SendMessage(new ScsRemoteInvokeReturnMessage
                {
                    RepliedMessageId = requestMessage.MessageId,
                    ReturnValue = returnValue,
                    RemoteException = exception
                }, 10);
            }
            catch (Exception ex)
            {
                Logger.Error("Invoke response failed to send", ex);
            }
        }

        #endregion
    }
}