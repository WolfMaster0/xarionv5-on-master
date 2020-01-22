using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.DAL.EF;

namespace OpenNos.Mapper.Mappers
{
    public static class PartnerSkillsMapper
    {
        public static bool ToPartnerSkillsDTO(PartnerSkills input, PartnerSkillsDTO output)
        {
            if (input == null)
            {
                output = null;
                return false;
            }

            output.PartnerVnum = input.PartnerVnum;
            output.FirstSkill = input.FirstSkill;
            output.SecondSkill = input.SecondSkill;
            output.ThirdSkill = input.ThirdSkill;
            output.SpecialBuffId = input.SpecialBuffId;
            output.IdentifierKey = input.IdentifierKey;
            return true;
        }

        public static bool ToPartnerSkills(PartnerSkillsDTO input, PartnerSkills output)
        {
            if (input == null)
            {
                output = null;
                return false;
            }

            output.PartnerVnum = input.PartnerVnum;
            output.FirstSkill = input.FirstSkill;
            output.SecondSkill = input.SecondSkill;
            output.ThirdSkill = input.ThirdSkill;
            output.SpecialBuffId = input.SpecialBuffId;
            output.IdentifierKey = input.IdentifierKey;
            return true;
        }
    }

}
