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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenNos.DAL.EF
{
    public sealed class RespawnMapType
    {
        #region Instantiation

        public RespawnMapType()
        {
            Respawn = new HashSet<Respawn>();
            MapTypes = new HashSet<MapType>();
            MapTypes1 = new HashSet<MapType>();
        }

        #endregion

        #region Properties

        public short DefaultMapId { get; set; }

        public short DefaultX { get; set; }

        public short DefaultY { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Map Map { get; set; }

        public ICollection<MapType> MapTypes { get; }

        public ICollection<MapType> MapTypes1 { get; }

        [MaxLength(255)]
        public string Name { get; set; }

        public ICollection<Respawn> Respawn { get; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RespawnMapTypeId { get; set; }

        #endregion
    }
}