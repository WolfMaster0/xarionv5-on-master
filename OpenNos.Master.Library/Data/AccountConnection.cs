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

namespace OpenNos.Master.Library.Data
{
    public class AccountConnection
    {
        public AccountConnection(long accountId, int sessionId, string ipAddress)
        {
            AccountId = accountId;
            SessionId = sessionId;
            IpAddress = ipAddress;
            LastPulse = DateTime.UtcNow;
        }

        public long AccountId { get; }

        public bool CanLoginCrossServer { get; set; }

        public long CharacterId { get; set; }

        public WorldServer ConnectedWorld { get; set; }

        public string IpAddress { get; }

        public DateTime LastPulse { get; set; }

        public WorldServer OriginWorld { get; set; }

        public int SessionId { get; }
    }
}