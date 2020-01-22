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
using OpenNos.Core.Threading;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("select")]
    public class SelectPacket
    {
        #region Properties

        public byte Slot { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            SelectPacket selectPacket = new SelectPacket();
            if (byte.TryParse(packetSplit[2], out byte slot))
            {
                selectPacket.Slot = slot;
                selectPacket.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(SelectPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            try
            {
                Character character =
                    new Character(DAOFactory.CharacterDAO.LoadBySlot(session.Account.AccountId, Slot));
                if (session.Account != null && !session.HasSelectedCharacter)
                {
                    character.Initialize();

#if !DEBUG
                    if (session.Account.Authority > Domain.AuthorityType.Moderator)
                    {
                        character.Invisible = true;
                        character.InvisibleGm = true;
                    }
#endif

                    character.GeneralLogs = new ThreadSafeGenericList<GeneralLogDTO>();
                    character.GeneralLogs.AddRange(DAOFactory.GeneralLogDAO.LoadByAccount(session.Account.AccountId)
                        .Where(s => s.CharacterId == character.CharacterId).ToList());
                    character.MapInstanceId = ServerManager.GetBaseMapInstanceIdByMapId(character.MapId);
                    character.PositionX = character.MapX;
                    character.PositionY = character.MapY;
                    character.Authority = session.Account.Authority;
                    session.SetCharacter(character);
                    if (!session.Character.GeneralLogs.Any(s =>
                        s.Timestamp == DateTime.UtcNow && s.LogData == "World" && s.LogType == "Connection"))
                    {
                        session.Character.SpAdditionPoint += session.Character.SpPoint;
                        session.Character.SpPoint = 10000;
                    }

                    if (session.Character.Hp > session.Character.HPLoad())
                    {
                        session.Character.Hp = (int)session.Character.HPLoad();
                    }

                    if (session.Character.Mp > session.Character.MPLoad())
                    {
                        session.Character.Mp = (int)session.Character.MPLoad();
                    }

                    session.Character.Respawns =
                        DAOFactory.RespawnDAO.LoadByCharacter(session.Character.CharacterId).ToList();
                    session.Character.StaticBonusList = DAOFactory.StaticBonusDAO
                        .LoadByCharacterId(session.Character.CharacterId).ToList();
                    session.Character.LoadInventory();
                    session.Character.LoadQuicklists();
                    session.Character.GenerateMiniland();
                    Map miniland = ServerManager.GetMapInstanceByMapId(20001).Map;
                    DAOFactory.MateDAO.LoadByCharacterId(session.Character.CharacterId).ToList().ForEach(s =>
                    {
                        Mate mate = new Mate(s)
                        {
                            Owner = session.Character
                        };
                        mate.GenerateMateTransportId();
                        mate.Monster = ServerManager.GetNpcMonster(s.NpcMonsterVNum);
                        mate.IsAlive = true;
                        if (!mate.IsTeamMember && miniland.IsBlockedZone(mate.MapX, mate.MapY))
                        {
                            MapCell cell = miniland.GetRandomPosition();
                            mate.MapX = cell.X;
                            mate.MapY = cell.Y;
                        }

                        if (mate.MateType == MateType.Pet && mate.MateSlot == -1)
                        {
                            mate.MateSlot = session.Character.GetNextMateSlot(mate.MateType);
                            mate.PartnerSlot = 0;
                        }
                        else if (mate.MateType == MateType.Partner && mate.PartnerSlot == -1)
                        {
                            mate.PartnerSlot = session.Character.GetNextMateSlot(mate.MateType);
                            mate.MateSlot = 0;
                        }

                        session.Character.Mates.Add(mate);
                        mate.StartLife();
                    });
                    session.Character.CharacterLifeDisposable = Observable.Interval(TimeSpan.FromMilliseconds(300))
                        .Subscribe(x => session.Character.CharacterLife());
                    session.Character.GeneralLogs.Add(new GeneralLogDTO
                    {
                        AccountId = session.Account.AccountId,
                        CharacterId = session.Character.CharacterId,
                        IpAddress = session.IpAddress,
                        LogData = "World",
                        LogType = "Connection",
                        Timestamp = DateTime.UtcNow
                    });
                    session.SendPacket("OK");

                    // Inform everyone about connected character
                    CommunicationServiceClient.Instance.ConnectCharacter(ServerManager.Instance.WorldId,
                        character.CharacterId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Select character failed.", ex);
            }
        }

        #endregion
    }
}