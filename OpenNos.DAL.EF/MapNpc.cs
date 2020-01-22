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
using System.ComponentModel.DataAnnotations.Schema;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace OpenNos.DAL.EF
{
    public sealed class MapNpc
    {
        #region Instantiation

        public MapNpc()
        {
            Shop = new HashSet<Shop>();
            Teleporter = new HashSet<Teleporter>();
            RecipeList = new HashSet<RecipeList>();
        }

        #endregion

        #region Properties

        public short Dialog { get; set; }

        public short Effect { get; set; }

        public short EffectDelay { get; set; }

        public bool IsDisabled { get; set; }

        public bool IsMoving { get; set; }

        public bool IsSitting { get; set; }

        public Map Map { get; set; }

        public short MapId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MapNpcId { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }

        public NpcMonster NpcMonster { get; set; }

        public short NpcVNum { get; set; }

        public byte Position { get; set; }

        public ICollection<RecipeList> RecipeList { get; }

        public ICollection<Shop> Shop { get; }

        public ICollection<Teleporter> Teleporter { get; }

        #endregion
    }
}