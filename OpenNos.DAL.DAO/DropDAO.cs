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
    public class DropDAO : IDropDAO
    {
        #region Methods

        public void Insert(List<DropDTO> drops)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (DropDTO drop in drops)
                    {
                        Drop entity = new Drop();
                        Mapper.Mappers.DropMapper.ToDrop(drop, entity);
                        context.Drop.Add(entity);
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

        public DropDTO Insert(DropDTO drop)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Drop entity = new Drop();
                    context.Drop.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.DropMapper.ToDropDTO(entity, drop))
                    {
                        return drop;
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

        public List<DropDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<DropDTO> result = new List<DropDTO>();
                foreach (Drop entity in context.Drop.AsNoTracking())
                {
                    DropDTO dto = new DropDTO();
                    Mapper.Mappers.DropMapper.ToDropDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<DropDTO> LoadByMonster(short monsterVNum)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<DropDTO> result = new List<DropDTO>();

                foreach (Drop drop in context.Drop.AsNoTracking().Where(s => s.MonsterVNum == monsterVNum || s.MonsterVNum == null))
                {
                    DropDTO dto = new DropDTO();
                    Mapper.Mappers.DropMapper.ToDropDTO(drop, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        #endregion
    }
}