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
    public class RecipeDAO : IRecipeDAO
    {
        #region Methods

        public RecipeDTO Insert(RecipeDTO recipe)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Recipe entity = new Recipe();
                    Mapper.Mappers.RecipeMapper.ToRecipe(recipe, entity);
                    context.Recipe.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.RecipeMapper.ToRecipeDTO(entity, recipe))
                    {
                        return recipe;
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

        public IEnumerable<RecipeDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<RecipeDTO> result = new List<RecipeDTO>();
                foreach (Recipe recipe in context.Recipe.AsNoTracking())
                {
                    RecipeDTO dto = new RecipeDTO();
                    Mapper.Mappers.RecipeMapper.ToRecipeDTO(recipe, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public RecipeDTO LoadById(short recipeId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    RecipeDTO dto = new RecipeDTO();
                    if (Mapper.Mappers.RecipeMapper.ToRecipeDTO(context.Recipe.AsNoTracking().SingleOrDefault(s => s.RecipeId.Equals(recipeId)), dto))
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

        public RecipeDTO LoadByItemVNum(short itemVNum)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    RecipeDTO dto = new RecipeDTO();
                    if (Mapper.Mappers.RecipeMapper.ToRecipeDTO(context.Recipe.AsNoTracking().SingleOrDefault(s => s.ItemVNum.Equals(itemVNum)), dto))
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

        public void Update(RecipeDTO recipe)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Recipe result = context.Recipe.FirstOrDefault(c => c.ItemVNum == recipe.ItemVNum);
                    if (result != null)
                    {
                        recipe.RecipeId = result.RecipeId;
                        Mapper.Mappers.RecipeMapper.ToRecipe(recipe, result);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        #endregion
    }
}