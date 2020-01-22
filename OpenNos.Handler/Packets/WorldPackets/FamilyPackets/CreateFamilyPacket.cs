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
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using System.Reactive.Linq;
using OpenNos.GameLog.LogHelper;

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader("glmk")]
    public class CreateFamilyPacket
    {
        #region Properties

        public string FamilyName { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new []{' '}, 3);
            if (packetSplit.Length < 3)
            {
                return;
            }
            CreateFamilyPacket packetDefinition = new CreateFamilyPacket();
            if (!string.IsNullOrWhiteSpace(packetSplit[2]))
            {
                packetDefinition.FamilyName = packetSplit[2];
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(CreateFamilyPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Group?.GroupType == GroupType.Group && session.Character.Group.CharacterCount == 3)
            {
                foreach (ClientSession sess in session.Character.Group.Characters.GetAllItems())
                {
                    if (sess.Character.Family != null || sess.Character.FamilyCharacter != null)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("PARTY_MEMBER_IN_FAMILY")));
                        return;
                    }
                    else if (sess.Character.LastFamilyLeave > DateTime.UtcNow.AddDays(-1).Ticks)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("PARTY_MEMBER_HAS_PENALTY")));
                        return;
                    }
                }

                if (session.Character.Gold < 200000)
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                    return;
                }

                string name = FamilyName;
                if (DAOFactory.FamilyDAO.LoadByName(name) != null)
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(
                            Language.Instance.GetMessageFromKey("FAMILY_NAME_ALREADY_USED")));
                    return;
                }

                session.Character.Gold -= 200000;
                session.SendPacket(session.Character.GenerateGold());
                FamilyDTO family = new FamilyDTO
                {
                    Name = name,
                    FamilyExperience = 0,
                    FamilyLevel = 1,
                    FamilyMessage = string.Empty,
                    MaxSize = 50
                };
                DAOFactory.FamilyDAO.InsertOrUpdate(ref family);

                GameLogger.Instance.LogGuildCreate(ServerManager.Instance.ChannelId, session.Character.Name,
                    session.Character.CharacterId, name, family.FamilyId,
                    session.Character.Group.Characters.GetAllItems().Select(s => s.Character).Cast<CharacterDTO>()
                        .ToList());

                ServerManager.Instance.Broadcast(
                    UserInterfaceHelper.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("FAMILY_FOUNDED"), name), 0));
                foreach (ClientSession sess in session.Character.Group.Characters.GetAllItems())
                {
                    FamilyCharacterDTO familyCharacter = new FamilyCharacterDTO
                    {
                        CharacterId = sess.Character.CharacterId,
                        DailyMessage = string.Empty,
                        Experience = 0,
                        Authority = session.Character.CharacterId == sess.Character.CharacterId
                            ? FamilyAuthority.Head
                            : FamilyAuthority.Assistant,
                        FamilyId = family.FamilyId,
                        Rank = 0
                    };
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref familyCharacter);
                }

                ServerManager.Instance.FamilyRefresh(family.FamilyId);
                CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
                {
                    DestinationCharacterId = family.FamilyId,
                    SourceCharacterId = session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = "fhis_stc",
                    Type = MessageType.Family
                });

                void RefreshFamily(ClientSession sess)
                {
                    Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(observer =>
                        sess?.CurrentMapInstance?.Broadcast(sess.Character.GenerateGidx()));
                }

                session.Character.Group.Characters.ForEach(RefreshFamily);
            }
        }

        #endregion
    }
}