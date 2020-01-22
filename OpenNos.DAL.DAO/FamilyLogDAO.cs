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
    public class FamilyLogDAO : IFamilyLogDAO
    {
        #region Methods

        public DeleteResult Delete(long familyLogId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    FamilyLog famlog = context.FamilyLog.FirstOrDefault(c => c.FamilyLogId.Equals(familyLogId));

                    if (famlog != null)
                    {
                        context.FamilyLog.Remove(famlog);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), familyLogId, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref FamilyLogDTO familyLog)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long famLogId = familyLog.FamilyLogId;
                    FamilyLog entity = context.FamilyLog.FirstOrDefault(c => c.FamilyLogId.Equals(famLogId));

                    if (entity == null)
                    {
                        familyLog = Insert(familyLog, context);
                        return SaveResult.Inserted;
                    }

                    familyLog = Update(entity, familyLog, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_FAMILYLOG_ERROR"), familyLog.FamilyLogId, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<FamilyLogDTO> LoadByFamilyId(long familyId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<FamilyLogDTO> result = new List<FamilyLogDTO>();
                foreach (FamilyLog familylog in context.FamilyLog.AsNoTracking().Where(fc => fc.FamilyId.Equals(familyId)))
                {
                    FamilyLogDTO dto = new FamilyLogDTO();
                    Mapper.Mappers.FamilyLogMapper.ToFamilyLogDTO(familylog, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        private static FamilyLogDTO Insert(FamilyLogDTO famlog, OpenNosContext context)
        {
            FamilyLog entity = new FamilyLog();
            Mapper.Mappers.FamilyLogMapper.ToFamilyLog(famlog, entity);
            context.FamilyLog.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.FamilyLogMapper.ToFamilyLogDTO(entity, famlog))
            {
                return famlog;
            }

            return null;
        }

        private static FamilyLogDTO Update(FamilyLog entity, FamilyLogDTO famlog, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.FamilyLogMapper.ToFamilyLog(famlog, entity);
                context.SaveChanges();
            }

            if (Mapper.Mappers.FamilyLogMapper.ToFamilyLogDTO(entity, famlog))
            {
                return famlog;
            }

            return null;
        }

        #endregion
    }
}