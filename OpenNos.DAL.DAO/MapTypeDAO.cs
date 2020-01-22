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
    public class MapTypeDAO : IMapTypeDAO
    {
        #region Methods

        public MapTypeDTO Insert(ref MapTypeDTO mapType)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    MapType entity = new MapType();
                    Mapper.Mappers.MapTypeMapper.ToMapType(mapType, entity);
                    context.MapType.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.MapTypeMapper.ToMapTypeDTO(entity, mapType))
                    {
                        return mapType;
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

        public IEnumerable<MapTypeDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<MapTypeDTO> result = new List<MapTypeDTO>();
                foreach (MapType mapType in context.MapType.AsNoTracking())
                {
                    MapTypeDTO dto = new MapTypeDTO();
                    Mapper.Mappers.MapTypeMapper.ToMapTypeDTO(mapType, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public MapTypeDTO LoadById(short maptypeId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    MapTypeDTO dto = new MapTypeDTO();
                    if (Mapper.Mappers.MapTypeMapper.ToMapTypeDTO(context.MapType.AsNoTracking().FirstOrDefault(s => s.MapTypeId.Equals(maptypeId)), dto))
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