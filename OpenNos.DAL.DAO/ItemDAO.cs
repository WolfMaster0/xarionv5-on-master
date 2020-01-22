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
    public class ItemDAO : IItemDAO
    {
        #region Methods

        public void Insert(IEnumerable<ItemDTO> items)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    foreach (ItemDTO item in items)
                    {
                        Item entity = new Item();
                        Mapper.Mappers.ItemMapper.ToItem(item, entity);
                        context.Item.Add(entity);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public ItemDTO Insert(ItemDTO item)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Item entity = new Item();
                    Mapper.Mappers.ItemMapper.ToItem(item, entity);
                    context.Item.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.ItemMapper.ToItemDTO(entity, item))
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

        public IEnumerable<ItemDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<ItemDTO> result = new List<ItemDTO>();
                foreach (Item item in context.Item.AsNoTracking())
                {
                    ItemDTO dto = new ItemDTO();
                    Mapper.Mappers.ItemMapper.ToItemDTO(item, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public ItemDTO LoadById(short vNum)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    ItemDTO dto = new ItemDTO();
                    if (Mapper.Mappers.ItemMapper.ToItemDTO(context.Item.AsNoTracking().FirstOrDefault(i => i.VNum.Equals(vNum)), dto))
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