using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.MatePackets
{
    [PacketHeader("ps_op")]
    public class PsOpPacket
    {
        #region Properties

        public byte PetId { get; set; }

        public byte SkillSlot { get; set; }

        public byte Option { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            PsOpPacket packetDefinition = new PsOpPacket();
            if (byte.TryParse(packetSplit[2], out var petId) && byte.TryParse(packetSplit[3], out var skillSlot))
            {
                packetDefinition.PetId = petId;
                packetDefinition.SkillSlot = skillSlot;
                packetDefinition.Option = packetSplit.Length >= 5 && byte.TryParse(packetSplit[4], out byte option) ? option : (byte)0;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(PsOpPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            Mate partnerInTeam = session.Character.Mates.FirstOrDefault(s => s.IsTeamMember && s.MateType == MateType.Partner);

            if (partnerInTeam == null || PetId != partnerInTeam.PartnerSlot)
            {
                session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("PARTNER_NOT_IN_TEAM"), 1));
                return;
            }

            if (partnerInTeam.SpInstance == null)
            {
                session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("PARTNER_DOESNT_WEAR_SP"), 1));
                return;
            }

            if (partnerInTeam.SpInstance.FirstPartnerSkill > 0 && SkillSlot == 0 ||
                partnerInTeam.SpInstance.SecondPartnerSkill > 0 && SkillSlot == 1 ||
                partnerInTeam.SpInstance.ThirdPartnerSkill > 0 && SkillSlot == 2)
            {
                session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("PARTNER_ALREADY_HAS_SKILL"), 1));
                return;
            }

            if (partnerInTeam.IsUsingSp)
            {
                session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("MUST_REMOVE_PARTNER_SP"), 1));
                return;
            }

            if (partnerInTeam.SpInstance.Agility < 100 && session.Account.Authority < AuthorityType.GameMaster)
            {
                session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("NOT_ENOUGH_AGILITY"), 1));
                return;
            }

            var skillList = ServerManager.Instance.PartnerSkills.FirstOrDefault(s => s.PartnerVnum == partnerInTeam.SpInstance.ItemVNum);

            if (skillList == null)
            {
                return;
            }

            switch (Option)
            {
                case 0:
                    session.SendPacket($"pdelay 3000 12 #ps_op^{PetId}^{SkillSlot}^1");
                    session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(2, 2, partnerInTeam.MateTransportId), partnerInTeam.PositionX, partnerInTeam.PositionY);
                    break;

                default:

                    switch (SkillSlot)
                    {
                        case 0:
                            partnerInTeam.SpInstance.FirstPartnerSkill = skillList.FirstSkill;
                            partnerInTeam.SpInstance.FirstPartnerSkillRank = (PartnerSkillRankType)ServerManager.RandomNumber<byte>(1, 8);
                            break;
                        case 1:
                            partnerInTeam.SpInstance.SecondPartnerSkill = skillList.SecondSkill;
                            partnerInTeam.SpInstance.SecondPartnerSkillRank = (PartnerSkillRankType)ServerManager.RandomNumber<byte>(1, 8);
                            break;
                        case 2:
                            partnerInTeam.SpInstance.ThirdPartnerSkill = skillList.ThirdSkill;
                            partnerInTeam.SpInstance.ThirdPartnerSkillRank = (PartnerSkillRankType)ServerManager.RandomNumber<byte>(1, 8);
                            break;
                    }

                    partnerInTeam.SpInstance.Agility = 0;
                    session.SendPacket(partnerInTeam.GenerateScPacket());
                    session.SendPacket(partnerInTeam.GeneratePski());
                    session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("PARTNER_LEARNT_SKILL"), 1));
                    break;
            }
        }

        #endregion
    }
}