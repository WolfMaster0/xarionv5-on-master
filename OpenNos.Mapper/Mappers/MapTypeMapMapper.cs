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
    public static class MapTypeMapMapper
    {
        #region Methods

        public static bool ToMapTypeMap(MapTypeMapDTO input, MapTypeMap output)
        {
            if (input == null)
            {
                return false;
            }
            output.MapId = input.MapId;
            output.MapTypeId = input.MapTypeId;
            return true;
        }

        public static bool ToMapTypeMapDTO(MapTypeMap input, MapTypeMapDTO output)
        {
            if (input == null)
            {
                return false;
            }
            output.MapId = input.MapId;
            output.MapTypeId = input.MapTypeId;
            return true;
        }

        #endregion
    }
}