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
    public class GeneralLogDAO : IGeneralLogDAO
    {
        #region Methods

        public bool IdAlreadySet(long id)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    return context.GeneralLog.AsNoTracking().Any(gl => gl.LogId == id);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        public GeneralLogDTO Insert(GeneralLogDTO generalLog)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    GeneralLog entity = new GeneralLog();
                    Mapper.Mappers.GeneralLogMapper.ToGeneralLog(generalLog, entity);
                    context.GeneralLog.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.GeneralLogMapper.ToGeneralLogDTO(entity, generalLog))
                    {
                        return generalLog;
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

        public IEnumerable<GeneralLogDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<GeneralLogDTO> result = new List<GeneralLogDTO>();
                foreach (GeneralLog generalLog in context.GeneralLog.AsNoTracking())
                {
                    GeneralLogDTO dto = new GeneralLogDTO();
                    Mapper.Mappers.GeneralLogMapper.ToGeneralLogDTO(generalLog, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<GeneralLogDTO> LoadByAccount(long? accountId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<GeneralLogDTO> result = new List<GeneralLogDTO>();
                foreach (GeneralLog generalLog in context.GeneralLog.AsNoTracking().Where(s => s.AccountId == accountId))
                {
                    GeneralLogDTO dto = new GeneralLogDTO();
                    Mapper.Mappers.GeneralLogMapper.ToGeneralLogDTO(generalLog, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<GeneralLogDTO> LoadByLogType(string logType, long? characterId, bool onlyToday = false)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<GeneralLogDTO> result = new List<GeneralLogDTO>();
                if (onlyToday)
                {
                    DateTime today = DateTime.UtcNow.Date;
                    foreach (GeneralLog log in context.GeneralLog.AsNoTracking().Where(c =>
                        c.Timestamp.Year == today.Year && c.Timestamp.Month == today.Month
                         && c.Timestamp.Day == today.Day && c.LogType.Equals(logType) && c.CharacterId == characterId))
                    {
                        GeneralLogDTO dto = new GeneralLogDTO();
                        Mapper.Mappers.GeneralLogMapper.ToGeneralLogDTO(log, dto);
                        result.Add(dto);
                    }
                }
                else
                {
                    foreach (GeneralLog log in context.GeneralLog.AsNoTracking().Where(c =>
                        c.LogType.Equals(logType) && c.CharacterId == characterId))
                    {
                        GeneralLogDTO dto = new GeneralLogDTO();
                        Mapper.Mappers.GeneralLogMapper.ToGeneralLogDTO(log, dto);
                        result.Add(dto);
                    }
                }

                return result;
            }
        }

        public void WriteGeneralLog(long accountId, string ipAddress, long? characterId, string logType, string logData)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    GeneralLog log = new GeneralLog
                    {
                        AccountId = accountId,
                        IpAddress = ipAddress,
                        Timestamp = DateTime.UtcNow,
                        LogType = logType,
                        LogData = logData,
                        CharacterId = characterId
                    };

                    context.GeneralLog.Add(log);
                    context.SaveChanges();
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