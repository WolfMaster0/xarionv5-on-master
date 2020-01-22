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
    public class MapMonsterDAO : IMapMonsterDAO
    {
        #region Methods

        public DeleteResult DeleteById(int mapMonsterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    MapMonster monster = context.MapMonster.First(i => i.MapMonsterId.Equals(mapMonsterId));

                    if (monster != null)
                    {
                        context.MapMonster.Remove(monster);
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

        public bool DoesMonsterExist(int mapMonsterId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                return context.MapMonster.AsNoTracking().Any(i => i.MapMonsterId.Equals(mapMonsterId));
            }
        }

        public int GetNextMapMonsterId()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                return context.MapMonster.AsNoTracking().OrderByDescending(s => s.MapMonsterId).FirstOrDefault()?.MapMonsterId + 1 ?? 1;
            }
        }

        public void Insert(IEnumerable<MapMonsterDTO> mapMonsters)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (MapMonsterDTO monster in mapMonsters)
                    {
                        MapMonster entity = new MapMonster();
                        Mapper.Mappers.MapMonsterMapper.ToMapMonster(monster, entity);
                        context.MapMonster.Add(entity);
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

        public MapMonsterDTO Insert(MapMonsterDTO mapMonster)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    MapMonster entity = new MapMonster();
                    Mapper.Mappers.MapMonsterMapper.ToMapMonster(mapMonster, entity);
                    context.MapMonster.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.MapMonsterMapper.ToMapMonsterDTO(entity, mapMonster))
                    {
                        return mapMonster;
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

        public MapMonsterDTO LoadById(int mapMonsterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    MapMonsterDTO dto = new MapMonsterDTO();
                    if (Mapper.Mappers.MapMonsterMapper.ToMapMonsterDTO(context.MapMonster.AsNoTracking().FirstOrDefault(i => i.MapMonsterId.Equals(mapMonsterId)), dto))
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

        public IEnumerable<MapMonsterDTO> LoadFromMap(short mapId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<MapMonsterDTO> result = new List<MapMonsterDTO>();
                foreach (MapMonster mapMonsterobject in context.MapMonster.AsNoTracking().Where(c => c.MapId.Equals(mapId)))
                {
                    MapMonsterDTO dto = new MapMonsterDTO();
                    Mapper.Mappers.MapMonsterMapper.ToMapMonsterDTO(mapMonsterobject, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        #endregion
    }
}