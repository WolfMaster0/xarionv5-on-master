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
    public class ShopItemDAO : IShopItemDAO
    {
        #region Methods

        public DeleteResult DeleteById(int itemId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    ShopItem item = context.ShopItem.FirstOrDefault(i => i.ShopItemId.Equals(itemId));

                    if (item != null)
                    {
                        context.ShopItem.Remove(item);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return DeleteResult.Error;
            }
        }

        public ShopItemDTO Insert(ShopItemDTO item)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    ShopItem entity = new ShopItem();
                    Mapper.Mappers.ShopItemMapper.ToShopItem(item, entity);
                    context.ShopItem.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.ShopItemMapper.ToShopItemDTO(entity, item))
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

        public void Insert(List<ShopItemDTO> items)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (ShopItemDTO item in items)
                    {
                        ShopItem entity = new ShopItem();
                        Mapper.Mappers.ShopItemMapper.ToShopItem(item, entity);
                        context.ShopItem.Add(entity);
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

        public IEnumerable<ShopItemDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<ShopItemDTO> result = new List<ShopItemDTO>();
                foreach (ShopItem entity in context.ShopItem.AsNoTracking())
                {
                    ShopItemDTO dto = new ShopItemDTO();
                    Mapper.Mappers.ShopItemMapper.ToShopItemDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public ShopItemDTO LoadById(int itemId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    ShopItemDTO dto = new ShopItemDTO();
                    if (Mapper.Mappers.ShopItemMapper.ToShopItemDTO(context.ShopItem.AsNoTracking().FirstOrDefault(i => i.ShopItemId.Equals(itemId)), dto))
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

        public IEnumerable<ShopItemDTO> LoadByShopId(int shopId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<ShopItemDTO> result = new List<ShopItemDTO>();
                foreach (ShopItem shopItem in context.ShopItem.AsNoTracking().Where(i => i.ShopId.Equals(shopId)))
                {
                    ShopItemDTO dto = new ShopItemDTO();
                    Mapper.Mappers.ShopItemMapper.ToShopItemDTO(shopItem, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        #endregion
    }
}