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
    public class MapNpcDAO : IMapNpcDAO
    {
        #region Methods

        public DeleteResult DeleteById(int mapNpcId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    MapNpc npc = context.MapNpc.First(i => i.MapNpcId.Equals(mapNpcId));

                    if (npc != null)
                    {
                        context.MapNpc.Remove(npc);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return DeleteResult.Error;
            }
        }

        public int GetNextMapNpcId()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                return context.MapNpc.AsNoTracking().OrderByDescending(s => s.MapNpcId).First().MapNpcId + 1;
            }
        }

        public void Insert(List<MapNpcDTO> npcs)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (MapNpcDTO item in npcs)
                    {
                        MapNpc entity = new MapNpc();
                        Mapper.Mappers.MapNpcMapper.ToMapNpc(item, entity);
                        context.MapNpc.Add(entity);
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

        public MapNpcDTO Insert(MapNpcDTO npc)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    MapNpc entity = new MapNpc();
                    Mapper.Mappers.MapNpcMapper.ToMapNpc(npc, entity);
                    context.MapNpc.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.MapNpcMapper.ToMapNpcdto(entity, npc))
                    {
                        return npc;
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

        public IEnumerable<MapNpcDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<MapNpcDTO> result = new List<MapNpcDTO>();
                foreach (MapNpc entity in context.MapNpc.AsNoTracking())
                {
                    MapNpcDTO dto = new MapNpcDTO();
                    Mapper.Mappers.MapNpcMapper.ToMapNpcdto(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public MapNpcDTO LoadById(int mapNpcId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    MapNpcDTO dto = new MapNpcDTO();
                    if (Mapper.Mappers.MapNpcMapper.ToMapNpcdto(context.MapNpc.AsNoTracking().FirstOrDefault(i => i.MapNpcId.Equals(mapNpcId)), dto))
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

        public IEnumerable<MapNpcDTO> LoadFromMap(short mapId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<MapNpcDTO> result = new List<MapNpcDTO>();
                foreach (MapNpc npcobject in context.MapNpc.AsNoTracking().Where(c => c.MapId.Equals(mapId)))
                {
                    MapNpcDTO dto = new MapNpcDTO();
                    Mapper.Mappers.MapNpcMapper.ToMapNpcdto(npcobject, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        #endregion
    }
}