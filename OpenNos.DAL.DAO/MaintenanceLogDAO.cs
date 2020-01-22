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
    public class MaintenanceLogDAO : IMaintenanceLogDAO
    {
        #region Methods

        public MaintenanceLogDTO Insert(MaintenanceLogDTO maintenanceLog)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    MaintenanceLog entity = new MaintenanceLog();
                    Mapper.Mappers.MaintenanceLogMapper.ToMaintenanceLog(maintenanceLog, entity);
                    context.MaintenanceLog.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.MaintenanceLogMapper.ToMaintenanceLogDTO(entity, maintenanceLog))
                    {
                        return maintenanceLog;
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

        public IEnumerable<MaintenanceLogDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<MaintenanceLogDTO> result = new List<MaintenanceLogDTO>();
                foreach (MaintenanceLog maintenanceLog in context.MaintenanceLog.AsNoTracking())
                {
                    MaintenanceLogDTO dto = new MaintenanceLogDTO();
                    Mapper.Mappers.MaintenanceLogMapper.ToMaintenanceLogDTO(maintenanceLog, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public MaintenanceLogDTO LoadFirst()
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    MaintenanceLogDTO dto = new MaintenanceLogDTO();
                    if (Mapper.Mappers.MaintenanceLogMapper.ToMaintenanceLogDTO(context.MaintenanceLog.AsNoTracking().FirstOrDefault(m => m.DateEnd > DateTime.UtcNow), dto))
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

        #endregion
    }
}