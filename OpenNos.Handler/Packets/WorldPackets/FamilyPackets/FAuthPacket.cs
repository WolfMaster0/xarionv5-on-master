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
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader("fauth")]
    public class FAuthPacket
    {
        #region Properties

        public FamilyAuthority MemberType { get; set; }

        public byte AuthorityId { get; set; }

        public byte Value { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 5)
            {
                return;
            }
            FAuthPacket packetDefinition = new FAuthPacket();
            if (Enum.TryParse(packetSplit[2], out FamilyAuthority memberType)
                && byte.TryParse(packetSplit[3], out byte authorityId) && byte.TryParse(packetSplit[4], out byte value))
            {
                packetDefinition.MemberType = memberType;
                packetDefinition.AuthorityId = authorityId;
                packetDefinition.Value = value;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(FAuthPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Family == null || session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)

            {
                return;
            }

            session.Character.Family.InsertFamilyLog(FamilyLogType.RightChanged, session.Character.Name,
                authority: MemberType, righttype: AuthorityId + 1,
                rightvalue: Value);
            switch (MemberType)
            {
                case FamilyAuthority.Manager:
                    switch (AuthorityId)
                    {
                        case 0:
                            session.Character.Family.ManagerCanInvite = Value == 1;
                            break;

                        case 1:
                            session.Character.Family.ManagerCanNotice = Value == 1;
                            break;

                        case 2:
                            session.Character.Family.ManagerCanShout = Value == 1;
                            break;

                        case 3:
                            session.Character.Family.ManagerCanGetHistory = Value == 1;
                            break;

                        case 4:
                            session.Character.Family.ManagerAuthorityType = (FamilyAuthorityType) Value;
                            break;
                    }

                    break;

                case FamilyAuthority.Member:
                    switch (AuthorityId)
                    {
                        case 0:
                            session.Character.Family.MemberCanGetHistory = Value == 1;
                            break;

                        case 1:
                            session.Character.Family.MemberAuthorityType = (FamilyAuthorityType) Value;
                            break;
                    }

                    break;
            }

            FamilyDTO fam = session.Character.Family;
            DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
            CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
            {
                DestinationCharacterId = fam.FamilyId,
                SourceCharacterId = session.Character.CharacterId,
                SourceWorldId = ServerManager.Instance.WorldId,
                Message = "fhis_stc",
                Type = MessageType.Family
            });
        }

        #endregion
    }
}