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
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public static class PortalMapper
    {
        #region Methods

        public static bool ToPortal(PortalDTO input, Portal output)
        {
            if (input == null)
            {
                return false;
            }
            output.DestinationMapId = input.DestinationMapId;
            output.DestinationX = input.DestinationX;
            output.DestinationY = input.DestinationY;
            output.IsDisabled = input.IsDisabled;
            output.PortalId = input.PortalId;
            output.SourceMapId = input.SourceMapId;
            output.SourceX = input.SourceX;
            output.SourceY = input.SourceY;
            output.Type = input.Type;
            return true;
        }

        public static bool ToPortalDTO(Portal input, PortalDTO output)
        {
            if (input == null)
            {
                return false;
            }
            output.DestinationMapId = input.DestinationMapId;
            output.DestinationX = input.DestinationX;
            output.DestinationY = input.DestinationY;
            output.IsDisabled = input.IsDisabled;
            output.PortalId = input.PortalId;
            output.SourceMapId = input.SourceMapId;
            output.SourceX = input.SourceX;
            output.SourceY = input.SourceY;
            output.Type = input.Type;
            return true;
        }

        #endregion
    }
}