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
    public class StaticBuffDAO : IStaticBuffDAO
    {
        #region Methods

        public void Delete(short bonusToDelete, long characterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    StaticBuff bon = context.StaticBuff.FirstOrDefault(c => c.CardId == bonusToDelete && c.CharacterId == characterId);

                    if (bon != null)
                    {
                        context.StaticBuff.Remove(bon);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), bonusToDelete, e.Message), e);
            }
        }

        public SaveResult InsertOrUpdate(ref StaticBuffDTO staticBuff)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long id = staticBuff.CharacterId;
                    short cardid = staticBuff.CardId;
                    StaticBuff entity = context.StaticBuff.FirstOrDefault(c => c.CardId == cardid && c.CharacterId == id);

                    if (entity == null)
                    {
                        staticBuff = Insert(staticBuff, context);
                        return SaveResult.Inserted;
                    }
                    staticBuff.StaticBuffId = entity.StaticBuffId;
                    staticBuff = Update(entity, staticBuff, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<StaticBuffDTO> LoadByCharacterId(long characterId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<StaticBuffDTO> result = new List<StaticBuffDTO>();
                foreach (StaticBuff entity in context.StaticBuff.AsNoTracking().Where(i => i.CharacterId == characterId))
                {
                    StaticBuffDTO dto = new StaticBuffDTO();
                    Mapper.Mappers.StaticBuffMapper.ToStaticBuffDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public static StaticBuffDTO LoadById(long sbId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    StaticBuffDTO dto = new StaticBuffDTO();
                    if (Mapper.Mappers.StaticBuffMapper.ToStaticBuffDTO(context.StaticBuff.AsNoTracking().FirstOrDefault(s => s.StaticBuffId.Equals(sbId)), dto)) //who the fuck was so retarded and set it to respawn ?!?
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

        public IEnumerable<short> LoadByTypeCharacterId(long characterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    return context.StaticBuff.AsNoTracking().Where(i => i.CharacterId == characterId).Select(qle => qle.CardId).ToList();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private static StaticBuffDTO Insert(StaticBuffDTO sb, OpenNosContext context)
        {
            try
            {
                StaticBuff entity = new StaticBuff();
                Mapper.Mappers.StaticBuffMapper.ToStaticBuff(sb, entity);
                context.StaticBuff.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.StaticBuffMapper.ToStaticBuffDTO(entity, sb))
                {
                    return sb;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private static StaticBuffDTO Update(StaticBuff entity, StaticBuffDTO sb, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.StaticBuffMapper.ToStaticBuff(sb, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.StaticBuffMapper.ToStaticBuffDTO(entity, sb))
            {
                return sb;
            }

            return null;
        }

        #endregion
    }
}