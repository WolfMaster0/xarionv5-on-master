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
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.DAL.EF.Context;

namespace OpenNos.DAL.DAO
{
    public class NpcMonsterDAO : INpcMonsterDAO
    {
        #region Methods

        public void Insert(List<NpcMonsterDTO> npcMonsters)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (NpcMonsterDTO item in npcMonsters)
                    {
                        NpcMonster entity = new NpcMonster();
                        Mapper.Mappers.NpcMonsterMapper.ToNpcMonster(item, entity);
                        context.NpcMonster.Add(entity);
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

        public NpcMonsterDTO Insert(NpcMonsterDTO npc)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    NpcMonster entity = new NpcMonster();
                    Mapper.Mappers.NpcMonsterMapper.ToNpcMonster(npc, entity);
                    context.NpcMonster.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.NpcMonsterMapper.ToNpcMonsterDTO(entity, npc))
                    {
                        return npc;
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

        public SaveResult InsertOrUpdate(ref NpcMonsterDTO npcMonster)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    short npcMonsterVNum = npcMonster.NpcMonsterVNum;
                    NpcMonster entity = context.NpcMonster.FirstOrDefault(c => c.NpcMonsterVNum.Equals(npcMonsterVNum));

                    if (entity == null)
                    {
                        npcMonster = Insert(npcMonster, context);
                        return SaveResult.Inserted;
                    }

                    npcMonster = Update(entity, npcMonster, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_NPCMONSTER_ERROR"), npcMonster.NpcMonsterVNum, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<NpcMonsterDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<NpcMonsterDTO> result = new List<NpcMonsterDTO>();
                foreach (NpcMonster npcMonster in context.NpcMonster.AsNoTracking())
                {
                    NpcMonsterDTO dto = new NpcMonsterDTO();
                    Mapper.Mappers.NpcMonsterMapper.ToNpcMonsterDTO(npcMonster, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public NpcMonsterDTO LoadByVNum(short npcMonsterVNum)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    NpcMonsterDTO dto = new NpcMonsterDTO();
                    if (Mapper.Mappers.NpcMonsterMapper.ToNpcMonsterDTO(context.NpcMonster.AsNoTracking().FirstOrDefault(i => i.NpcMonsterVNum.Equals(npcMonsterVNum)), dto))
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

        private static NpcMonsterDTO Insert(NpcMonsterDTO npcMonster, OpenNosContext context)
        {
            NpcMonster entity = new NpcMonster();
            Mapper.Mappers.NpcMonsterMapper.ToNpcMonster(npcMonster, entity);
            context.NpcMonster.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.NpcMonsterMapper.ToNpcMonsterDTO(entity, npcMonster))
            {
                return npcMonster;
            }

            return null;
        }

        private static NpcMonsterDTO Update(NpcMonster entity, NpcMonsterDTO npcMonster, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.NpcMonsterMapper.ToNpcMonster(npcMonster, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.NpcMonsterMapper.ToNpcMonsterDTO(entity, npcMonster))
            {
                return npcMonster;
            }

            return null;
        }

        #endregion
    }
}