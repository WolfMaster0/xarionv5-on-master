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
    public class ShopSkillDAO : IShopSkillDAO
    {
        #region Methods

        public ShopSkillDTO Insert(ShopSkillDTO shopSkill)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    ShopSkill entity = new ShopSkill();
                    Mapper.Mappers.ShopSkillMapper.ToShopSkill(shopSkill, entity);
                    context.ShopSkill.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.ShopSkillMapper.ToShopSkillDTO(entity, shopSkill))
                    {
                        return shopSkill;
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

        public void Insert(List<ShopSkillDTO> skills)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (ShopSkillDTO skill in skills)
                    {
                        ShopSkill entity = new ShopSkill();
                        Mapper.Mappers.ShopSkillMapper.ToShopSkill(skill, entity);
                        context.ShopSkill.Add(entity);
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

        public IEnumerable<ShopSkillDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<ShopSkillDTO> result = new List<ShopSkillDTO>();
                foreach (ShopSkill entity in context.ShopSkill.AsNoTracking())
                {
                    ShopSkillDTO dto = new ShopSkillDTO();
                    Mapper.Mappers.ShopSkillMapper.ToShopSkillDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<ShopSkillDTO> LoadByShopId(int shopId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<ShopSkillDTO> result = new List<ShopSkillDTO>();
                foreach (ShopSkill shopSkill in context.ShopSkill.AsNoTracking().Where(s => s.ShopId.Equals(shopId)))
                {
                    ShopSkillDTO dto = new ShopSkillDTO();
                    Mapper.Mappers.ShopSkillMapper.ToShopSkillDTO(shopSkill, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        #endregion
    }
}