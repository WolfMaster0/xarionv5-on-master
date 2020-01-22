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
    public class RecipeListDAO : IRecipeListDAO
    {
        #region Methods

        public DeleteResult DeleteByMapNpcId(int mapNpcId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    IQueryable<RecipeList> recipeLists = context.RecipeList.Where(i => i.MapNpcId == mapNpcId);

                    if (recipeLists.Any())
                    {
                        context.RecipeList.RemoveRange(recipeLists);
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

        public RecipeListDTO LoadFirstByMapNpcId(int mapNpcId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    RecipeListDTO dto = new RecipeListDTO();
                    if (Mapper.Mappers.RecipeListMapper.ToRecipeListDTO(context.RecipeList.AsNoTracking().FirstOrDefault(s => s.MapNpcId == mapNpcId), dto))
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

        public RecipeListDTO Insert(RecipeListDTO recipeList)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    RecipeList entity = new RecipeList();
                    Mapper.Mappers.RecipeListMapper.ToRecipeList(recipeList, entity);
                    context.RecipeList.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.RecipeListMapper.ToRecipeListDTO(entity, recipeList))
                    {
                        return recipeList;
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

        public IEnumerable<RecipeListDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<RecipeListDTO> result = new List<RecipeListDTO>();
                foreach (RecipeList recipeList in context.RecipeList.AsNoTracking())
                {
                    RecipeListDTO dto = new RecipeListDTO();
                    Mapper.Mappers.RecipeListMapper.ToRecipeListDTO(recipeList, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public RecipeListDTO LoadById(int recipeListId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    RecipeListDTO dto = new RecipeListDTO();
                    if (Mapper.Mappers.RecipeListMapper.ToRecipeListDTO(context.RecipeList.AsNoTracking().SingleOrDefault(s => s.RecipeListId.Equals(recipeListId)), dto))
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

        public IEnumerable<RecipeListDTO> LoadByItemVNum(short itemVNum)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<RecipeListDTO> result = new List<RecipeListDTO>();
                foreach (RecipeList recipeList in context.RecipeList.AsNoTracking().Where(r => r.ItemVNum == itemVNum))
                {
                    RecipeListDTO dto = new RecipeListDTO();
                    Mapper.Mappers.RecipeListMapper.ToRecipeListDTO(recipeList, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<RecipeListDTO> LoadByMapNpcId(int mapNpcId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<RecipeListDTO> result = new List<RecipeListDTO>();
                foreach (RecipeList recipeList in context.RecipeList.AsNoTracking().Where(r => r.MapNpcId == mapNpcId))
                {
                    RecipeListDTO dto = new RecipeListDTO();
                    Mapper.Mappers.RecipeListMapper.ToRecipeListDTO(recipeList, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<RecipeListDTO> LoadByRecipeId(short recipeId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<RecipeListDTO> result = new List<RecipeListDTO>();
                foreach (RecipeList recipeList in context.RecipeList.AsNoTracking().Where(r => r.RecipeId.Equals(recipeId)))
                {
                    RecipeListDTO dto = new RecipeListDTO();
                    Mapper.Mappers.RecipeListMapper.ToRecipeListDTO(recipeList, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public void Update(RecipeListDTO recipe)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    RecipeList result = context.RecipeList.FirstOrDefault(r => r.RecipeListId.Equals(recipe.RecipeListId));
                    if (result != null)
                    {
                        Mapper.Mappers.RecipeListMapper.ToRecipeList(recipe, result);
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