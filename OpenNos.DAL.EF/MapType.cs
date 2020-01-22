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
using System.Collections.Generic;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace OpenNos.DAL.EF
{
    public sealed class MapType
    {
        #region Instantiation

        public MapType()
        {
            MapTypeMap = new HashSet<MapTypeMap>();
            Drops = new HashSet<Drop>();
        }

        #endregion

        #region Properties

        public ICollection<Drop> Drops { get; }

        public short MapTypeId { get; set; }

        public ICollection<MapTypeMap> MapTypeMap { get; }

        public string MapTypeName { get; set; }

        public short PotionDelay { get; set; }

        public RespawnMapType RespawnMapType { get; set; }

        public long? RespawnMapTypeId { get; set; }

        public RespawnMapType ReturnMapType { get; set; }

        public long? ReturnMapTypeId { get; set; }

        #endregion
    }
}