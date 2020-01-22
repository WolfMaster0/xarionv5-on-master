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
    public class RespawnMapTypeDAO : IRespawnMapTypeDAO
    {
        #region Methods

        public void Insert(List<RespawnMapTypeDTO> respawnMapTypes)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (RespawnMapTypeDTO respawnMapType in respawnMapTypes)
                    {
                        RespawnMapType entity = new RespawnMapType();
                        Mapper.Mappers.RespawnMapTypeMapper.ToRespawnMapType(respawnMapType, entity);
                        context.RespawnMapType.Add(entity);
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

        public SaveResult InsertOrUpdate(ref RespawnMapTypeDTO respawnMapType)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    short mapId = respawnMapType.DefaultMapId;
                    RespawnMapType entity = context.RespawnMapType.FirstOrDefault(c => c.DefaultMapId.Equals(mapId));

                    if (entity == null)
                    {
                        respawnMapType = Insert(respawnMapType, context);
                        return SaveResult.Inserted;
                    }

                    respawnMapType.RespawnMapTypeId = entity.RespawnMapTypeId;
                    respawnMapType = Update(entity, respawnMapType, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public RespawnMapTypeDTO LoadById(long respawnMapTypeId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    RespawnMapTypeDTO dto = new RespawnMapTypeDTO();
                    if (Mapper.Mappers.RespawnMapTypeMapper.ToRespawnMapTypeDTO(context.RespawnMapType.AsNoTracking().FirstOrDefault(s => s.RespawnMapTypeId.Equals(respawnMapTypeId)), dto))
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

        public RespawnMapTypeDTO LoadByMapId(short mapId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    RespawnMapTypeDTO dto = new RespawnMapTypeDTO();
                    if (Mapper.Mappers.RespawnMapTypeMapper.ToRespawnMapTypeDTO(context.RespawnMapType.AsNoTracking().FirstOrDefault(s => s.DefaultMapId.Equals(mapId)), dto))
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

        private static RespawnMapTypeDTO Insert(RespawnMapTypeDTO respawnMapType, OpenNosContext context)
        {
            try
            {
                RespawnMapType entity = new RespawnMapType();
                Mapper.Mappers.RespawnMapTypeMapper.ToRespawnMapType(respawnMapType, entity);
                context.RespawnMapType.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.RespawnMapTypeMapper.ToRespawnMapTypeDTO(entity, respawnMapType))
                {
                    return respawnMapType;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private static RespawnMapTypeDTO Update(RespawnMapType entity, RespawnMapTypeDTO respawnMapType, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.RespawnMapTypeMapper.ToRespawnMapType(respawnMapType, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.RespawnMapTypeMapper.ToRespawnMapTypeDTO(entity, respawnMapType))
            {
                return respawnMapType;
            }

            return null;
        }

        #endregion
    }
}