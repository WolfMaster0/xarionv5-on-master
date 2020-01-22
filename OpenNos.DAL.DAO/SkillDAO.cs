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
using OpenNos.Core;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.DAL.EF.Context;

namespace OpenNos.DAL.DAO
{
    public class SkillDAO : ISkillDAO
    {
        #region Methods

        public void Insert(List<SkillDTO> skills)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (SkillDTO skill in skills)
                    {
                        Skill entity = new Skill();
                        Mapper.Mappers.SkillMapper.ToSkill(skill, entity);
                        context.Skill.Add(entity);
                    }
                    context.Configuration.AutoDetectChangesEnabled = true;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public SkillDTO Insert(SkillDTO skill)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Skill entity = new Skill();
                    Mapper.Mappers.SkillMapper.ToSkill(skill, entity); context.Skill.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.SkillMapper.ToSkillDTO(entity, skill))
                    {
                        return skill;
                    }

                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<SkillDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<SkillDTO> result = new List<SkillDTO>();
                foreach (Skill skill in context.Skill.AsNoTracking())
                {
                    SkillDTO dto = new SkillDTO();
                    Mapper.Mappers.SkillMapper.ToSkillDTO(skill, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public SkillDTO LoadById(short skillId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    SkillDTO dto = new SkillDTO();
                    if (Mapper.Mappers.SkillMapper.ToSkillDTO(context.Skill.AsNoTracking().FirstOrDefault(s => s.SkillVNum.Equals(skillId)), dto))
                    {
                        return dto;
                    }

                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        #endregion
    }
}