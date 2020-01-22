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
    public class ScriptedInstanceDAO : IScriptedInstanceDAO
    {
        #region Methods

        public void Insert(List<ScriptedInstanceDTO> scriptedInstances)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (ScriptedInstanceDTO scriptedInstance in scriptedInstances)
                    {
                        ScriptedInstance entity = new ScriptedInstance();
                        Mapper.Mappers.ScriptedInstanceMapper.ToScriptedInstance(scriptedInstance, entity);
                        context.ScriptedInstance.Add(entity);
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

        public ScriptedInstanceDTO Insert(ScriptedInstanceDTO scriptedInstance)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    ScriptedInstance entity = new ScriptedInstance();
                    Mapper.Mappers.ScriptedInstanceMapper.ToScriptedInstance(scriptedInstance, entity);
                    context.ScriptedInstance.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.ScriptedInstanceMapper.ToScriptedInstanceDTO(entity, scriptedInstance))
                    {
                        return scriptedInstance;
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

        public IEnumerable<ScriptedInstanceDTO> LoadByMap(short mapId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<ScriptedInstanceDTO> result = new List<ScriptedInstanceDTO>();
                foreach (ScriptedInstance timespaceObject in context.ScriptedInstance.AsNoTracking().Where(c => c.MapId.Equals(mapId)))
                {
                    ScriptedInstanceDTO dto = new ScriptedInstanceDTO();
                    Mapper.Mappers.ScriptedInstanceMapper.ToScriptedInstanceDTO(timespaceObject, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        #endregion
    }
}