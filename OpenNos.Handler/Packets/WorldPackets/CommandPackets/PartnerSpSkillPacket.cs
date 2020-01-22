using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$PspSkill", Authority = AuthorityType.GameMaster)]
    public class PartnerSpSkillPacket
    {
        #region Properties

        public byte Level { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            PartnerSpSkillPacket packetDefinition = new PartnerSpSkillPacket();
            if (byte.TryParse(packetSplit[2], out var level))
            {
                packetDefinition.Level = level;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(PartnerSpSkillPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (Level <= 0 || Level > 7)
            {
                session.SendPacket(session.Character.GenerateSay("Usage: $PspSkill [Level]", 10));
                return;
            }

            var partnerInTeam = session.Character.Mates.FirstOrDefault(s => s.IsTeamMember && s.MateType == MateType.Partner);

            if (partnerInTeam == null)
            {
                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PARTNER_NOT_IN_TEAM"), 10));
                return;
            }

            if (partnerInTeam.SpInstance == null)
            {
                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PARTNER_DOESNT_WEAR_SP"), 10));
                return;
            }

            if (partnerInTeam.IsUsingSp)
            {
                session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("MUST_REMOVE_PARTNER_SP"), 1));
                return;
            }

            var skillList = ServerManager.Instance.PartnerSkills.FirstOrDefault(s => s.PartnerVnum == partnerInTeam.SpInstance.ItemVNum);

            if (skillList == null)
            {
                return;
            }

            partnerInTeam.SpInstance.FirstPartnerSkill = skillList.FirstSkill;
            partnerInTeam.SpInstance.FirstPartnerSkillRank = PartnerSkillRankType.SRank;
            partnerInTeam.SpInstance.SecondPartnerSkill = skillList.SecondSkill;
            partnerInTeam.SpInstance.SecondPartnerSkillRank = PartnerSkillRankType.SRank;
            partnerInTeam.SpInstance.ThirdPartnerSkill = skillList.ThirdSkill;
            partnerInTeam.SpInstance.ThirdPartnerSkillRank = PartnerSkillRankType.SRank;
            session.SendPacket(partnerInTeam.GenerateScPacket());
            session.SendPacket(partnerInTeam.GeneratePski());
            session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("PARTNER_LEARNT_SKILL"), 1));
        }

        #endregion
    }
}
