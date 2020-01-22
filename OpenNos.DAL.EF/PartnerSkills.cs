using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.DAL.EF
{
    public class PartnerSkills
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short PartnerVnum { get; set; }

        public short FirstSkill { get; set; }

        public short SecondSkill { get; set; }

        public short ThirdSkill { get; set; }

        public short SpecialBuffId { get; set; }

        public string IdentifierKey { get; set; }
    }

}
