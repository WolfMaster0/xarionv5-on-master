using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Interface
{
    public interface IPartnerSkillsDAO
    {
        IEnumerable<PartnerSkillsDTO> LoadAll();

        IEnumerable<PartnerSkillsDTO> LoadByVnum(short vnum);

        DeleteResult DeleteById(long id);

        PartnerSkillsDTO InsertOrUpdate(PartnerSkillsDTO dto);

        IEnumerable<PartnerSkillsDTO> InsertOrUpdate(IEnumerable<PartnerSkillsDTO> dtos);
    }

}
