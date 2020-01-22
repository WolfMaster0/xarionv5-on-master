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
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core.Networking.Communication.Scs.Server;
using System;

namespace OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints
{
    /// <summary>
    /// Represents a server side end point in SCS.
    /// </summary>
    public abstract class ScsEndPoint
    {
        #region Methods

        /// <summary>
        /// Create a Scs End Point from a string. Address must be formatted as: protocol://address
        /// For example: tcp://89.43.104.179:10048 for a TCP endpoint with IP 89.43.104.179 and port 10048.
        /// </summary>
        /// <param name="endPointAddress">Address to create endpoint</param>
        /// <returns>Created end point</returns>
        public static ScsEndPoint CreateEndPoint(string endPointAddress)
        {
            // Check if end point address is null
            if (string.IsNullOrEmpty(endPointAddress))
            {
                throw new ArgumentNullException(nameof(endPointAddress));
            }

            // If not protocol specified, assume TCP.
            string endPointAddr = endPointAddress;
            if (!endPointAddr.Contains("://"))
            {
                endPointAddr = "tcp://" + endPointAddr;
            }

            // Split protocol and address parts
            string[] splittedEndPoint = endPointAddr.Split(new[] { "://" }, StringSplitOptions.RemoveEmptyEntries);
            if (splittedEndPoint.Length != 2)
            {
                throw new ApplicationException(endPointAddress + " is not a valid endpoint address.");
            }

            // Split end point, find protocol and address
            string protocol = splittedEndPoint[0].Trim().ToLower();
            string address = splittedEndPoint[1].Trim();
            switch (protocol)
            {
                case "tcp":
                    return new ScsTcpEndPoint(address);

                default:
                    throw new ApplicationException("Unsupported protocol " + protocol + " in end point " + endPointAddress);
            }
        }

        /// <summary>
        /// Creates a Scs Server that uses this end point to connect to server.
        /// </summary>
        /// <returns>Scs Client</returns>
        public abstract IScsClient CreateClient();

        /// <summary>
        /// Creates a Scs Server that uses this end point to listen incoming connections.
        /// </summary>
        /// <returns>Scs Server</returns>
        public abstract IScsServer CreateServer();

        #endregion
    }
}