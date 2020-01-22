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
    public class RollGeneratedItemDAO : IRollGeneratedItemDAO
    {
        #region Methods

        public RollGeneratedItemDTO Insert(RollGeneratedItemDTO item)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    RollGeneratedItem entity = new RollGeneratedItem();
                    Mapper.Mappers.RollGeneratedItemMapper.ToRollGeneratedItem(item, entity);
                    context.RollGeneratedItem.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.RollGeneratedItemMapper.ToRollGeneratedItemDTO(entity, item))
                    {
                        return item;
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

        public IEnumerable<RollGeneratedItemDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<RollGeneratedItemDTO> result = new List<RollGeneratedItemDTO>();
                foreach (RollGeneratedItem item in context.RollGeneratedItem.AsNoTracking())
                {
                    RollGeneratedItemDTO dto = new RollGeneratedItemDTO();
                    Mapper.Mappers.RollGeneratedItemMapper.ToRollGeneratedItemDTO(item, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public RollGeneratedItemDTO LoadById(short id)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    RollGeneratedItemDTO dto = new RollGeneratedItemDTO();
                    if (Mapper.Mappers.RollGeneratedItemMapper.ToRollGeneratedItemDTO(context.RollGeneratedItem.AsNoTracking().FirstOrDefault(i => i.RollGeneratedItemId.Equals(id)), dto))
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

        public IEnumerable<RollGeneratedItemDTO> LoadByItemVNum(short vnum)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<RollGeneratedItemDTO> result = new List<RollGeneratedItemDTO>();
                foreach (RollGeneratedItem item in context.RollGeneratedItem.AsNoTracking().Where(s => s.OriginalItemVNum == vnum))
                {
                    RollGeneratedItemDTO dto = new RollGeneratedItemDTO();
                    Mapper.Mappers.RollGeneratedItemMapper.ToRollGeneratedItemDTO(item, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        #endregion
    }
}