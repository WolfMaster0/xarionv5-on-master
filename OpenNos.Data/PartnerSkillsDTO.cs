using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    [Serializable]
    public class PartnerSkillsDTO
    {
        public short PartnerVnum { get; set; }

        public short FirstSkill { get; set; }

        public short SecondSkill { get; set; }

        public short ThirdSkill { get; set; }

        public short SpecialBuffId { get; set; }

        public string IdentifierKey { get; set; }
    }

}
