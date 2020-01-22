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
using System.Data.Entity;
using System.Linq;
using OpenNos.DAL.EF.Context;

namespace OpenNos.DAL.DAO
{
    public class MinigameLogDAO : IMinigameLogDAO
    {
        #region Methods

        public SaveResult InsertOrUpdate(ref MinigameLogDTO minigameLog)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long minigameLogId = minigameLog.MinigameLogId;
                    MinigameLog entity = context.MinigameLog.FirstOrDefault(c => c.MinigameLogId.Equals(minigameLogId));

                    if (entity == null)
                    {
                        minigameLog = Insert(minigameLog, context);
                        return SaveResult.Inserted;
                    }
                    minigameLog = Update(entity, minigameLog, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<MinigameLogDTO> LoadByCharacterId(long characterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    IEnumerable<MinigameLog> minigameLog = context.MinigameLog.AsNoTracking().Where(a => a.CharacterId.Equals(characterId)).ToList();
                    {
                        List<MinigameLogDTO> result = new List<MinigameLogDTO>();
                        foreach (MinigameLog input in minigameLog)
                        {
                            MinigameLogDTO dto = new MinigameLogDTO();
                            if (Mapper.Mappers.MinigameLogMapper.ToMinigameLogDTO(input, dto))
                            {
                                result.Add(dto);
                            }
                        }
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        public MinigameLogDTO LoadById(long minigameLogId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    MinigameLog minigameLog = context.MinigameLog.AsNoTracking().FirstOrDefault(a => a.MinigameLogId.Equals(minigameLogId));
                    if (minigameLog != null)
                    {
                        MinigameLogDTO minigameLogDTO = new MinigameLogDTO();
                        if (Mapper.Mappers.MinigameLogMapper.ToMinigameLogDTO(minigameLog, minigameLogDTO))
                        {
                            return minigameLogDTO;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        private static MinigameLogDTO Insert(MinigameLogDTO account, OpenNosContext context)
        {
            MinigameLog entity = new MinigameLog();
            Mapper.Mappers.MinigameLogMapper.ToMinigameLog(account, entity);
            context.MinigameLog.Add(entity);
            context.SaveChanges();
            Mapper.Mappers.MinigameLogMapper.ToMinigameLogDTO(entity, account);
            return account;
        }

        private static MinigameLogDTO Update(MinigameLog entity, MinigameLogDTO account, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.MinigameLogMapper.ToMinigameLog(account, entity);
                context.Entry(entity).State = EntityState.Modified;
                context.SaveChanges();
            }
            if (Mapper.Mappers.MinigameLogMapper.ToMinigameLogDTO(entity, account))
            {
                return account;
            }

            return null;
        }

        #endregion
    }
}