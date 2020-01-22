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
    public class NpcMonsterSkillDAO : INpcMonsterSkillDAO
    {
        #region Methods

        public NpcMonsterSkillDTO Insert(ref NpcMonsterSkillDTO npcMonsterSkill)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    NpcMonsterSkill entity = new NpcMonsterSkill();
                    Mapper.Mappers.NpcMonsterSkillMapper.ToNpcMonsterSkill(npcMonsterSkill, entity);
                    context.NpcMonsterSkill.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.NpcMonsterSkillMapper.ToNpcMonsterSkillDTO(entity, npcMonsterSkill))
                    {
                        return npcMonsterSkill;
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

        public void Insert(List<NpcMonsterSkillDTO> skills)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (NpcMonsterSkillDTO skill in skills)
                    {
                        NpcMonsterSkill entity = new NpcMonsterSkill();
                        Mapper.Mappers.NpcMonsterSkillMapper.ToNpcMonsterSkill(skill, entity);
                        context.NpcMonsterSkill.Add(entity);
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

        public List<NpcMonsterSkillDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<NpcMonsterSkillDTO> result = new List<NpcMonsterSkillDTO>();
                foreach (NpcMonsterSkill npcMonsterSkillobject in context.NpcMonsterSkill.AsNoTracking())
                {
                    NpcMonsterSkillDTO dto = new NpcMonsterSkillDTO();
                    Mapper.Mappers.NpcMonsterSkillMapper.ToNpcMonsterSkillDTO(npcMonsterSkillobject, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<NpcMonsterSkillDTO> LoadByNpcMonster(short npcId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<NpcMonsterSkillDTO> result = new List<NpcMonsterSkillDTO>();
                foreach (NpcMonsterSkill npcMonsterSkillobject in context.NpcMonsterSkill.AsNoTracking().Where(i => i.NpcMonsterVNum == npcId))
                {
                    NpcMonsterSkillDTO dto = new NpcMonsterSkillDTO();
                    Mapper.Mappers.NpcMonsterSkillMapper.ToNpcMonsterSkillDTO(npcMonsterSkillobject, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        #endregion
    }
}