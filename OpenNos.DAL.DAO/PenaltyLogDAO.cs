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
    public class PenaltyLogDAO : IPenaltyLogDAO
    {
        #region Methods

        public DeleteResult Delete(int penaltyLogId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    PenaltyLog penaltyLog = context.PenaltyLog.FirstOrDefault(c => c.PenaltyLogId.Equals(penaltyLogId));

                    if (penaltyLog != null)
                    {
                        context.PenaltyLog.Remove(penaltyLog);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_PENALTYLOG_ERROR"), penaltyLogId, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref PenaltyLogDTO log)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    int id = log.PenaltyLogId;
                    PenaltyLog entity = context.PenaltyLog.FirstOrDefault(c => c.PenaltyLogId.Equals(id));

                    if (entity == null)
                    {
                        log = Insert(log, context);
                        return SaveResult.Inserted;
                    }

                    log = Update(entity, log, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_PENALTYLOG_ERROR"), log.PenaltyLogId, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<PenaltyLogDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<PenaltyLogDTO> result = new List<PenaltyLogDTO>();
                foreach (PenaltyLog entity in context.PenaltyLog.AsNoTracking())
                {
                    PenaltyLogDTO dto = new PenaltyLogDTO();
                    Mapper.Mappers.PenaltyLogMapper.ToPenaltyLogDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<PenaltyLogDTO> LoadByAccount(long accountId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<PenaltyLogDTO> result = new List<PenaltyLogDTO>();
                foreach (PenaltyLog penaltyLog in context.PenaltyLog.AsNoTracking().Where(s => s.AccountId.Equals(accountId)))
                {
                    PenaltyLogDTO dto = new PenaltyLogDTO();
                    Mapper.Mappers.PenaltyLogMapper.ToPenaltyLogDTO(penaltyLog, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public PenaltyLogDTO LoadById(int penaltyLogId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    PenaltyLogDTO dto = new PenaltyLogDTO();
                    if (Mapper.Mappers.PenaltyLogMapper.ToPenaltyLogDTO(context.PenaltyLog.AsNoTracking().FirstOrDefault(s => s.PenaltyLogId.Equals(penaltyLogId)), dto))
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

        private static PenaltyLogDTO Insert(PenaltyLogDTO penaltylog, OpenNosContext context)
        {
            PenaltyLog entity = new PenaltyLog();
            Mapper.Mappers.PenaltyLogMapper.ToPenaltyLog(penaltylog, entity);
            context.PenaltyLog.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.PenaltyLogMapper.ToPenaltyLogDTO(entity, penaltylog))
            {
                return penaltylog;
            }

            return null;
        }

        private static PenaltyLogDTO Update(PenaltyLog entity, PenaltyLogDTO penaltylog, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.PenaltyLogMapper.ToPenaltyLog(penaltylog, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.PenaltyLogMapper.ToPenaltyLogDTO(entity, penaltylog))
            {
                return penaltylog;
            }

            return null;
        }

        #endregion
    }
}