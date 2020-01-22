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
    public class RecipeItemDAO : IRecipeItemDAO
    {
        #region Methods

        public RecipeItemDTO Insert(RecipeItemDTO recipeItem)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    RecipeItem entity = new RecipeItem();
                    Mapper.Mappers.RecipeItemMapper.ToRecipeItem(recipeItem, entity);
                    context.RecipeItem.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.RecipeItemMapper.ToRecipeItemDTO(entity, recipeItem))
                    {
                        return recipeItem;
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

        public IEnumerable<RecipeItemDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<RecipeItemDTO> result = new List<RecipeItemDTO>();
                foreach (RecipeItem recipeItem in context.RecipeItem.AsNoTracking())
                {
                    RecipeItemDTO dto = new RecipeItemDTO();
                    Mapper.Mappers.RecipeItemMapper.ToRecipeItemDTO(recipeItem, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public RecipeItemDTO LoadById(short recipeItemId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    RecipeItemDTO dto = new RecipeItemDTO();
                    if (Mapper.Mappers.RecipeItemMapper.ToRecipeItemDTO(context.RecipeItem.AsNoTracking().FirstOrDefault(s => s.RecipeItemId.Equals(recipeItemId)), dto))
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

        public IEnumerable<RecipeItemDTO> LoadByRecipe(short recipeId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<RecipeItemDTO> result = new List<RecipeItemDTO>();
                foreach (RecipeItem recipeItem in context.RecipeItem.AsNoTracking().Where(s => s.RecipeId.Equals(recipeId)))
                {
                    RecipeItemDTO dto = new RecipeItemDTO();
                    Mapper.Mappers.RecipeItemMapper.ToRecipeItemDTO(recipeItem, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<RecipeItemDTO> LoadByRecipeAndItem(short recipeId, short itemVNum)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<RecipeItemDTO> result = new List<RecipeItemDTO>();
                foreach (RecipeItem recipeItem in context.RecipeItem.AsNoTracking().Where(s => s.ItemVNum.Equals(itemVNum) && s.RecipeId.Equals(recipeId)))
                {
                    RecipeItemDTO dto = new RecipeItemDTO();
                    Mapper.Mappers.RecipeItemMapper.ToRecipeItemDTO(recipeItem, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        #endregion
    }
}