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
    public class PortalDAO : IPortalDAO
    {
        #region Methods

        public DeleteResult DeleteById(int portalId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Portal portal = context.Portal.Single(i => i.PortalId.Equals(portalId));

                    if (portal != null)
                    {
                        context.Portal.Remove(portal);
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

        public void Insert(List<PortalDTO> portals)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (PortalDTO item in portals)
                    {
                        Portal entity = new Portal();
                        Mapper.Mappers.PortalMapper.ToPortal(item, entity);
                        context.Portal.Add(entity);
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

        public PortalDTO Insert(PortalDTO portal)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Portal entity = new Portal();
                    Mapper.Mappers.PortalMapper.ToPortal(portal, entity);
                    context.Portal.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.PortalMapper.ToPortalDTO(entity, portal))
                    {
                        return portal;
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

        public PortalDTO LoadById(int portalId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    PortalDTO dto = new PortalDTO();
                    if (Mapper.Mappers.PortalMapper.ToPortalDTO(context.Portal.AsNoTracking().SingleOrDefault(i => i.PortalId.Equals(portalId)), dto))
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

        public IEnumerable<PortalDTO> LoadByMap(short mapId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<PortalDTO> result = new List<PortalDTO>();
                foreach (Portal portalobject in context.Portal.AsNoTracking().Where(c => c.SourceMapId.Equals(mapId)))
                {
                    PortalDTO dto = new PortalDTO();
                    Mapper.Mappers.PortalMapper.ToPortalDTO(portalobject, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        #endregion
    }
}