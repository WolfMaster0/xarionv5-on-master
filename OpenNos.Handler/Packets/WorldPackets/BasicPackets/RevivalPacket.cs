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
using System.Reactive.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("revival")]
    public class RevivalPacket
    {
        #region Properties

        public byte Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            RevivalPacket packetDefinition = new RevivalPacket();
            if (byte.TryParse(packetSplit[2], out byte type))
            {
                packetDefinition.Type = type;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(RevivalPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Hp > 0)
            {
                return;
            }

            switch (Type)
            {
                case 0:
                    switch (session.CurrentMapInstance.MapInstanceType)
                    {
                        case MapInstanceType.LodInstance:
                            const int saver = 1211;
                            if (session.Character.Inventory.CountItem(saver) < 1)
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_SAVER"), 0));
                                ServerManager.Instance.ReviveFirstPosition(session.Character.CharacterId);
                            }
                            else
                            {
                                session.Character.Inventory.RemoveItemAmount(saver);
                                session.Character.Hp = (int)session.Character.HPLoad();
                                session.Character.Mp = (int)session.Character.MPLoad();
                                session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateRevive());
                                session.SendPacket(session.Character.GenerateStat());
                            }
                            break;

                        case MapInstanceType.Act4Berios:
                        case MapInstanceType.Act4Calvina:
                        case MapInstanceType.Act4Hatus:
                        case MapInstanceType.Act4Morcos:
                            if (session.Character.Reputation < session.Character.Level * 10)
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_REPUT"), 0));
                                ServerManager.Instance.ReviveFirstPosition(session.Character.CharacterId);
                            }
                            else
                            {
                                session.Character.SetReputation(session.Character.Level * -10);
                                session.Character.Hp = (int)session.Character.HPLoad();
                                session.Character.Mp = (int)session.Character.MPLoad();
                                session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateRevive());
                                session.SendPacket(session.Character.GenerateStat());
                            }
                            break;

                        default:
                            const int seed = 1012;
                            if (session.Character.Inventory.CountItem(seed) < 10 && session.Character.Level > 20)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("NOT_ENOUGH_POWER_SEED"), 0));
                                ServerManager.Instance.ReviveFirstPosition(session.Character.CharacterId);
                                session.SendPacket(
                                    session.Character.GenerateSay(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_SEED_SAY"), 0));
                            }
                            else
                            {
                                if (session.Character.Level > 20)
                                {
                                    session.SendPacket(session.Character.GenerateSay(
                                        string.Format(Language.Instance.GetMessageFromKey("SEED_USED"), 10), 10));
                                    session.Character.Inventory.RemoveItemAmount(seed, 10);
                                    session.Character.Hp = (int)(session.Character.HPLoad() / 2);
                                    session.Character.Mp = (int)(session.Character.MPLoad() / 2);
                                }
                                else
                                {
                                    session.Character.Hp = (int)session.Character.HPLoad();
                                    session.Character.Mp = (int)session.Character.MPLoad();
                                }

                                session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateTp());
                                session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateRevive());
                                session.SendPacket(session.Character.GenerateStat());
                                MateHelper.Instance.AddPetBuff(session, session.Character.Mates.Find(s => s.IsTeamMember && s.MateType == MateType.Pet));
                                MateHelper.Instance.AddPartnerBuffs(session, session.Character.Mates.Find(s => s.IsTeamMember && s.MateType == MateType.Partner));
                            }
                            break;
                    }
                    break;

                case 1:
                    ServerManager.Instance.ReviveFirstPosition(session.Character.CharacterId);
                    break;

                case 2:
                    if (session.Character.Gold >= 100)
                    {
                        session.Character.Hp = (int)session.Character.HPLoad();
                        session.Character.Mp = (int)session.Character.MPLoad();
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateTp());
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateRevive());
                        session.SendPacket(session.Character.GenerateStat());
                        session.Character.Gold -= 100;
                        session.SendPacket(session.Character.GenerateGold());
                        session.Character.LastPvpRevive = DateTime.UtcNow;
                        Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(observer =>
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PVP_ACTIVE"), 10)));
                    }
                    else
                    {
                        ServerManager.Instance.ReviveFirstPosition(session.Character.CharacterId);
                    }
                    break;
            }
        }

        #endregion
    }
}