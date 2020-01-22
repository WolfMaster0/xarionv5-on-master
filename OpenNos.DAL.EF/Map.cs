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
    public sealed class Map
    {
        #region Instantiation

        public Map()
        {
            Character = new HashSet<Character>();
            MapMonster = new HashSet<MapMonster>();
            MapNpc = new HashSet<MapNpc>();
            Portal = new HashSet<Portal>();
            Portal1 = new HashSet<Portal>();
            ScriptedInstance = new HashSet<ScriptedInstance>();
            Teleporter = new HashSet<Teleporter>();
            MapTypeMap = new HashSet<MapTypeMap>();
            Respawn = new HashSet<Respawn>();
            RespawnMapType = new HashSet<RespawnMapType>();
        }

        #endregion

        #region Properties

        public ICollection<Character> Character { get; }

        public byte[] Data { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short MapId { get; set; }

        public ICollection<MapMonster> MapMonster { get; }

        public ICollection<MapNpc> MapNpc { get; }

        public ICollection<MapTypeMap> MapTypeMap { get; }

        public int Music { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public ICollection<Portal> Portal { get; }

        public ICollection<Portal> Portal1 { get; }

        public ICollection<Respawn> Respawn { get; }

        public ICollection<RespawnMapType> RespawnMapType { get; }

        public ICollection<ScriptedInstance> ScriptedInstance { get; }

        public bool ShopAllowed { get; set; }

        public ICollection<Teleporter> Teleporter { get; }

        #endregion
    }
}