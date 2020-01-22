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
using System.Threading;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BazaarPackets
{
    [PacketHeader("c_skill")]
    public class BazaarOpenPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            // empty packet no need for checks, we can add some type of packet identification check
            // in general handling
            BazaarOpenPacket packetDefinition = new BazaarOpenPacket();
            packetDefinition.ExecuteHandler(session as ClientSession);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BazaarOpenPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InBazaarRefreshMode);
            StaticBonusDTO medal = session.Character.StaticBonusList.Find(s =>
                s.StaticBonusType == StaticBonusType.BazaarMedalGold
                || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);
            if (medal != null)
            {
                byte bmedal = medal.StaticBonusType == StaticBonusType.BazaarMedalGold
                    ? (byte)MedalType.Gold
                    : (byte)MedalType.Silver;
                int time = (int)(medal.DateEnd - DateTime.UtcNow).TotalHours;
                session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOTICE_BAZAAR"),
                    0));
                session.SendPacket($"wopen 32 {bmedal} {time}");
            }
            else
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("INFO_BAZAAR")));
            }
        }

        #endregion
    }
}