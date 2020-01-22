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
using OpenNos.Domain;
using System;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public static class MapInstancePortalHandler
    {
        #region Methods

        public static List<Portal> GenerateMinilandEntryPortals(short entryMap, Guid exitMapinstanceId)
        {
            List<Portal> list = new List<Portal>();
            switch (entryMap)
            {
                case 1:
                    list.Add(new Portal
                    {
                        SourceX = 48,
                        SourceY = 132,
                        DestinationX = 5,
                        DestinationY = 8,
                        Type = (short)PortalType.Miniland,
                        SourceMapId = 1,
                        DestinationMapInstanceId = exitMapinstanceId
                    });
                    break;

                case 145:
                    list.Add(new Portal
                    {
                        SourceX = 9,
                        SourceY = 171,
                        DestinationX = 5,
                        DestinationY = 8,
                        Type = (short)PortalType.Miniland,
                        SourceMapId = 145,
                        DestinationMapInstanceId = exitMapinstanceId
                    });
                    break;
            }
            return list;
        }

        #endregion
    }
}