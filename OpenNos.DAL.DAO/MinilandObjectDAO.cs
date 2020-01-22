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
    public class MinilandObjectDAO : IMinilandObjectDAO
    {
        #region Methods

        public DeleteResult DeleteById(long id)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    MinilandObject item = context.MinilandObject.First(i => i.MinilandObjectId.Equals(id));

                    if (item != null)
                    {
                        context.MinilandObject.Remove(item);
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

        public SaveResult InsertOrUpdate(ref MinilandObjectDTO obj)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long id = obj.MinilandObjectId;
                    MinilandObject entity = context.MinilandObject.FirstOrDefault(c => c.MinilandObjectId.Equals(id));

                    if (entity == null)
                    {
                        obj = Insert(obj, context);
                        return SaveResult.Inserted;
                    }

                    obj.MinilandObjectId = entity.MinilandObjectId;
                    obj = Update(entity, obj, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<MinilandObjectDTO> LoadByCharacterId(long characterId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<MinilandObjectDTO> result = new List<MinilandObjectDTO>();
                foreach (MinilandObject obj in context.MinilandObject.AsNoTracking().Where(s => s.CharacterId == characterId))
                {
                    MinilandObjectDTO dto = new MinilandObjectDTO();
                    Mapper.Mappers.MinilandObjectMapper.ToMinilandObjectDTO(obj, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        private static MinilandObjectDTO Insert(MinilandObjectDTO obj, OpenNosContext context)
        {
            try
            {
                MinilandObject entity = new MinilandObject();
                Mapper.Mappers.MinilandObjectMapper.ToMinilandObject(obj, entity);
                context.MinilandObject.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.MinilandObjectMapper.ToMinilandObjectDTO(entity, obj))
                {
                    return obj;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private static MinilandObjectDTO Update(MinilandObject entity, MinilandObjectDTO respawn, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.MinilandObjectMapper.ToMinilandObject(respawn, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.MinilandObjectMapper.ToMinilandObjectDTO(entity, respawn))
            {
                return respawn;
            }

            return null;
        }

        #endregion
    }
}