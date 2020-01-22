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
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.ScriptedInstancePackets
{
    [PacketHeader("rsel")]
    public class RewardSelectPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            RewardSelectPacket packetDefinition = new RewardSelectPacket();
            packetDefinition.ExecuteHandler(session as ClientSession);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(RewardSelectPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Timespace?.FirstMap?.MapInstanceType == MapInstanceType.TimeSpaceInstance)
            {
                ServerManager.GetBaseMapInstanceIdByMapId(session.Character.MapId);
                if (session.Character.Timespace?.FirstMap.InstanceBag.EndState == 5)
                {
                    session.Character.SetReputation(session.Character.Timespace.Reputation);

                    session.Character.Gold =
                        session.Character.Gold + session.Character.Timespace.Gold
                        > ServerManager.Instance.Configuration.MaxGold
                            ? ServerManager.Instance.Configuration.MaxGold
                            : session.Character.Gold + session.Character.Timespace.Gold;
                    session.SendPacket(session.Character.GenerateGold());
                    session.SendPacket(session.Character.GenerateSay(
                        string.Format(Language.Instance.GetMessageFromKey("GOLD_TS_END"),
                            session.Character.Timespace.Gold), 10));

                    int rand = new Random().Next(session.Character.Timespace.DrawItems.Count);
                    string repay = "repay ";
                    session.Character.GiftAdd(session.Character.Timespace.DrawItems[rand].VNum,
                        session.Character.Timespace.DrawItems[rand].Amount);

                    for (int i = 0; i < 3; i++)
                    {
                        Gift gift = session.Character.Timespace.GiftItems.ElementAtOrDefault(i);
                        repay += gift == null ? "-1.0.0 " : $"{gift.VNum}.0.{gift.Amount} ";
                        if (gift != null)
                        {
                            session.Character.GiftAdd(gift.VNum, gift.Amount);
                        }
                    }

                    // TODO: Add HasAlreadyDone
                    for (int i = 0; i < 2; i++)
                    {
                        Gift gift = session.Character.Timespace.SpecialItems.ElementAtOrDefault(i);
                        repay += gift == null ? "-1.0.0 " : $"{gift.VNum}.0.{gift.Amount} ";
                        if (gift != null)
                        {
                            session.Character.GiftAdd(gift.VNum, gift.Amount);
                        }
                    }

                    repay +=
                        $"{session.Character.Timespace.DrawItems[rand].VNum}.0.{session.Character.Timespace.DrawItems[rand].Amount}";
                    session.SendPacket(repay);
                    session.Character.Timespace.FirstMap.InstanceBag.EndState = 6;
                    session.Character.Timespace = null;
                }
            }
        }

        #endregion
    }
}