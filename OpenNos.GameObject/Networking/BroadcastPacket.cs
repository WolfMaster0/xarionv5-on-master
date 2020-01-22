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

using OpenNos.Domain;

namespace OpenNos.GameObject.Networking
{
    public class BroadcastPacket
    {
        #region Instantiation

        public BroadcastPacket(ClientSession session, string packet, ReceiverType receiver, string someonesCharacterName = "", long someonesCharacterId = -1, int xCoordinate = 0, int yCoordinate = 0)
        {
            Sender = session;
            Packet = packet;
            Receiver = receiver;
            SomeonesCharacterName = someonesCharacterName;
            SomeonesCharacterId = someonesCharacterId;
            XCoordinate = xCoordinate;
            YCoordinate = yCoordinate;
        }

        #endregion

        #region Properties

        public string Packet { get; set; }

        public ReceiverType Receiver { get; set; }

        public ClientSession Sender { get; set; }

        public long SomeonesCharacterId { get; set; }

        public string SomeonesCharacterName { get; set; }

        public int XCoordinate { get; set; }

        public int YCoordinate { get; set; }

        #endregion
    }
}