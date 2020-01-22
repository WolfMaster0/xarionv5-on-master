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
    public class TeleporterDAO : ITeleporterDAO
    {
        #region Methods

        public TeleporterDTO Insert(TeleporterDTO teleporter)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Teleporter entity = new Teleporter();
                    Mapper.Mappers.TeleporterMapper.ToTeleporter(teleporter, entity);
                    context.Teleporter.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.TeleporterMapper.ToTeleporterDTO(entity, teleporter))
                    {
                        return teleporter;
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

        public IEnumerable<TeleporterDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<TeleporterDTO> result = new List<TeleporterDTO>();
                foreach (Teleporter entity in context.Teleporter.AsNoTracking())
                {
                    TeleporterDTO dto = new TeleporterDTO();
                    Mapper.Mappers.TeleporterMapper.ToTeleporterDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public TeleporterDTO LoadById(short teleporterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    TeleporterDTO dto = new TeleporterDTO();
                    if (Mapper.Mappers.TeleporterMapper.ToTeleporterDTO(context.Teleporter.AsNoTracking().FirstOrDefault(i => i.TeleporterId.Equals(teleporterId)), dto))
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

        public IEnumerable<TeleporterDTO> LoadFromNpc(int npcId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<TeleporterDTO> result = new List<TeleporterDTO>();
                foreach (Teleporter entity in context.Teleporter.AsNoTracking().Where(c => c.MapNpcId.Equals(npcId)))
                {
                    TeleporterDTO dto = new TeleporterDTO();
                    Mapper.Mappers.TeleporterMapper.ToTeleporterDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        #endregion
    }
}