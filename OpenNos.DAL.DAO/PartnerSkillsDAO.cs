using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Context;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.DAO
{
    public class PartnerSkillsDAO : IPartnerSkillsDAO
    {
        public IEnumerable<PartnerSkillsDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<PartnerSkillsDTO> result = new List<PartnerSkillsDTO>();
                foreach (PartnerSkills characterHome in context.PartnerSkills)
                {
                    PartnerSkillsDTO dto = new PartnerSkillsDTO();
                    Mapper.Mappers.PartnerSkillsMapper.ToPartnerSkillsDTO(characterHome, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<PartnerSkillsDTO> LoadByVnum(short vnum)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<PartnerSkillsDTO> result = new List<PartnerSkillsDTO>();
                foreach (PartnerSkills characterHome in context.PartnerSkills.Where(s => s.PartnerVnum == vnum))
                {
                    PartnerSkillsDTO dto = new PartnerSkillsDTO();
                    Mapper.Mappers.PartnerSkillsMapper.ToPartnerSkillsDTO(characterHome, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public DeleteResult DeleteById(long id)
        {
            return Delete(id);
        }

        public PartnerSkillsDTO InsertOrUpdate(PartnerSkillsDTO dto)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    return InsertOrUpdate(context, dto);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Message: {e.Message}", e);
                return null;
            }
        }

        public IEnumerable<PartnerSkillsDTO> InsertOrUpdate(IEnumerable<PartnerSkillsDTO> dtos)
        {
            try
            {
                IList<PartnerSkillsDTO> results = new List<PartnerSkillsDTO>();
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    foreach (var dto in dtos)
                    {
                        results.Add(InsertOrUpdate(context, dto));
                    }
                }

                return results;
            }
            catch (Exception e)
            {
                Logger.Error($"Message: {e.Message}", e);
                return null;
            }
        }

        #region InnerMethods

        protected static PartnerSkillsDTO InsertOrUpdate(OpenNosContext context, PartnerSkillsDTO dto)
        {
            try
            {
                PartnerSkills entity = context.PartnerSkills.FirstOrDefault(c => c.PartnerVnum == dto.PartnerVnum);
                return entity == null ? Insert(dto, context) : Update(entity, dto, context);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public virtual DeleteResult Delete(long id)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                PartnerSkills entity = context.Set<PartnerSkills>().FirstOrDefault(i => i.PartnerVnum == id);
                if (entity != null)
                {
                    context.Set<PartnerSkills>().Remove(entity);
                    context.SaveChanges();
                }
                return DeleteResult.Deleted;
            }
        }

        protected static PartnerSkillsDTO Insert(PartnerSkillsDTO dto, OpenNosContext context)
        {
            PartnerSkills entity = new PartnerSkills();
            map(dto, entity);
            context.Set<PartnerSkills>().Add(entity);
            context.SaveChanges();
            if (map(entity, dto))
            {
                return dto;
            }

            return null;
        }

        protected static PartnerSkillsDTO Update(PartnerSkills entity, PartnerSkillsDTO inventory, OpenNosContext context)
        {
            if (entity != null)
            {
                map(inventory, entity);
                context.SaveChanges();
            }
            if (map(entity, inventory))
            {
                return inventory;
            }
            return null;
        }

        #endregion

        #region Mapping

        private static bool map(PartnerSkillsDTO input, PartnerSkills output)
        {
            if (input == null)
            {
                output = null;
                return false;
            }
            Mapper.Mappers.PartnerSkillsMapper.ToPartnerSkills(input, output);
            return true;
        }

        private static bool map(PartnerSkills input, PartnerSkillsDTO output)
        {
            if (input == null)
            {
                output = null;
                return false;
            }
            Mapper.Mappers.PartnerSkillsMapper.ToPartnerSkillsDTO(input, output);
            return true;
        }

        #endregion
    }

}
