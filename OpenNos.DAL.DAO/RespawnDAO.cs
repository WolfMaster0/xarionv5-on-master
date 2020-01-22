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
    public class RespawnDAO : IRespawnDAO
    {
        #region Methods

        public SaveResult InsertOrUpdate(ref RespawnDTO respawn)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long characterId = respawn.CharacterId;
                    long respawnMapTypeId = respawn.RespawnMapTypeId;
                    Respawn entity = context.Respawn.FirstOrDefault(c => c.RespawnMapTypeId.Equals(respawnMapTypeId) && c.CharacterId.Equals(characterId));

                    if (entity == null)
                    {
                        respawn = Insert(respawn, context);
                        return SaveResult.Inserted;
                    }

                    respawn.RespawnId = entity.RespawnId;
                    respawn = Update(entity, respawn, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<RespawnDTO> LoadByCharacter(long characterId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<RespawnDTO> result = new List<RespawnDTO>();
                foreach (Respawn respawnobject in context.Respawn.AsNoTracking().Where(i => i.CharacterId.Equals(characterId)))
                {
                    RespawnDTO dto = new RespawnDTO();
                    Mapper.Mappers.RespawnMapper.ToRespawnDTO(respawnobject, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public RespawnDTO LoadById(long respawnId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    RespawnDTO dto = new RespawnDTO();
                    if (Mapper.Mappers.RespawnMapper.ToRespawnDTO(context.Respawn.AsNoTracking().FirstOrDefault(s => s.RespawnId.Equals(respawnId)), dto))
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

        private static RespawnDTO Insert(RespawnDTO respawn, OpenNosContext context)
        {
            try
            {
                Respawn entity = new Respawn();
                Mapper.Mappers.RespawnMapper.ToRespawn(respawn, entity);
                context.Respawn.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.RespawnMapper.ToRespawnDTO(entity, respawn))
                {
                    return respawn;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private static RespawnDTO Update(Respawn entity, RespawnDTO respawn, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.RespawnMapper.ToRespawn(respawn, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.RespawnMapper.ToRespawnDTO(entity, respawn))
            {
                return respawn;
            }

            return null;
        }

        #endregion
    }
}