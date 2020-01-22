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
using System.Collections.Generic;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Battle;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BattlePackets
{
    [PacketHeader("mtlist")]
    public class MultiTargetListPacket
    {
        #region Properties

        public List<Tuple<int, UserType>> TargetList { get; set; }

        public byte TargetsAmount { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.TrimEnd(' ').Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }

            MultiTargetListPacket packetDefinition = new MultiTargetListPacket();
            if (byte.TryParse(packetSplit[2], out byte targetAmount))
            {
                packetDefinition.TargetList = new List<Tuple<int, UserType>>();
                for (int i = 3; i < 3 + (targetAmount * 2); i += 2)
                {
                    if (int.TryParse(packetSplit[i + 1], out int targetId)
                        && Enum.TryParse(packetSplit[i], out UserType userType))
                    {
                        packetDefinition.TargetList.Add(new Tuple<int, UserType>(targetId, userType));
                    }
                }

                packetDefinition.TargetsAmount = targetAmount;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MultiTargetListPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (!session.HasCurrentMapInstance)
            {
                return;
            }

            bool isMuted = session.Character.MuteMessage();
            if (isMuted || session.Character.IsVehicled)
            {
                session.SendPacket(StaticPacketHelper.Cancel());
                return;
            }

            if ((DateTime.UtcNow - session.Character.LastTransform).TotalSeconds < 3)
            {
                session.SendPacket(StaticPacketHelper.Cancel());
                session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_ATTACK"), 0));
                return;
            }

            if (TargetsAmount > 0
                && TargetsAmount == TargetList.Count
                && TargetList != null)
            {
                session.Character.MtListTargetQueue.Clear();
                foreach (Tuple<int, UserType> target in TargetList)
                {
                    session.Character.MtListTargetQueue.Push(new MtListHitTarget(target.Item2, target.Item1));
                }
            }
        }

        #endregion
    }
}