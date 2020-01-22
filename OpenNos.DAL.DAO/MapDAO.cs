// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.
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
    public class MapDAO : IMapDAO
    {
        #region Methods

        public void Insert(List<MapDTO> maps)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (MapDTO item in maps)
                    {
                        Map entity = new Map();
                        Mapper.Mappers.MapMapper.ToMap(item, entity);
                        context.Map.Add(entity);
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

        public MapDTO Insert(MapDTO map)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    if (context.Map.FirstOrDefault(c => c.MapId.Equals(map.MapId)) == null)
                    {
                        Map entity = new Map();
                        Mapper.Mappers.MapMapper.ToMap(map, entity);
                        context.Map.Add(entity);
                        context.SaveChanges();
                        if (Mapper.Mappers.MapMapper.ToMapDTO(entity, map))
                        {
                            return map;
                        }

                        return null;
                    }
                    return new MapDTO();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<MapDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<MapDTO> result = new List<MapDTO>();
                foreach (Map map in context.Map.AsNoTracking())
                {
                    MapDTO dto = new MapDTO();
                    Mapper.Mappers.MapMapper.ToMapDTO(map, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public MapDTO LoadById(short mapId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    MapDTO dto = new MapDTO();
                    if (Mapper.Mappers.MapMapper.ToMapDTO(context.Map.AsNoTracking().FirstOrDefault(c => c.MapId.Equals(mapId)), dto))
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