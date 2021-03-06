﻿// This file is part of the OpenNos NosTale Emulator Project.
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
    public class ShopDAO : IShopDAO
    {
        #region Methods

        public DeleteResult DeleteById(int mapNpcId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Shop shop = context.Shop.First(i => i.MapNpcId.Equals(mapNpcId));
                    IEnumerable<ShopItem> shopItems = context.ShopItem.Where(s => s.ShopId.Equals(shop.ShopId));
                    IEnumerable<ShopSkill> shopSkills = context.ShopSkill.Where(s => s.ShopId.Equals(shop.ShopId));

                    if (shop != null)
                    {
                        context.ShopItem.RemoveRange(shopItems);
                        context.ShopSkill.RemoveRange(shopSkills);
                        context.Shop.Remove(shop);
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

        public void Insert(List<ShopDTO> shops)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (ShopDTO item in shops)
                    {
                        Shop entity = new Shop();
                        Mapper.Mappers.ShopMapper.ToShop(item, entity);
                        context.Shop.Add(entity);
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

        public ShopDTO Insert(ShopDTO shop)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    if (context.Shop.FirstOrDefault(c => c.MapNpcId.Equals(shop.MapNpcId)) == null)
                    {
                        Shop entity = new Shop();
                        Mapper.Mappers.ShopMapper.ToShop(shop, entity);
                        context.Shop.Add(entity);
                        context.SaveChanges();
                        if (Mapper.Mappers.ShopMapper.ToShopDTO(entity, shop))
                        {
                            return shop;
                        }

                        return null;
                    }
                    return new ShopDTO();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<ShopDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<ShopDTO> result = new List<ShopDTO>();
                foreach (Shop entity in context.Shop.AsNoTracking())
                {
                    ShopDTO dto = new ShopDTO();
                    Mapper.Mappers.ShopMapper.ToShopDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public ShopDTO LoadById(int shopId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    ShopDTO dto = new ShopDTO();
                    if (Mapper.Mappers.ShopMapper.ToShopDTO(context.Shop.AsNoTracking().FirstOrDefault(s => s.ShopId.Equals(shopId)), dto))
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

        public ShopDTO LoadByNpc(int mapNpcId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    ShopDTO dto = new ShopDTO();
                    if (Mapper.Mappers.ShopMapper.ToShopDTO(context.Shop.AsNoTracking().FirstOrDefault(s => s.MapNpcId.Equals(mapNpcId)), dto))
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