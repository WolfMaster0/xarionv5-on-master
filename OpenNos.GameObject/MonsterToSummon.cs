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
using OpenNos.GameObject.Event;

namespace OpenNos.GameObject
{
    public class MonsterToSummon
    {
        #region Instantiation

        public MonsterToSummon(short vnum, MapCell spawnCell, long target, bool move, bool isTarget = false, bool isBonus = false, bool isHostile = true, bool isBoss = false)
        {
            VNum = vnum;
            SpawnCell = spawnCell;
            Target = target;
            IsMoving = move;
            IsTarget = isTarget;
            IsBonus = isBonus;
            IsBoss = isBoss;
            IsHostile = isHostile;
            DeathEvents = new List<EventContainer>();
            NoticingEvents = new List<EventContainer>();
        }

        #endregion

        #region Properties

        public List<EventContainer> DeathEvents { get; set; }

        public bool IsBonus { get; set; }

        public bool IsBoss { get; set; }

        public bool IsHostile { get; set; }

        public bool IsMoving { get; set; }

        public bool IsTarget { get; set; }

        public byte NoticeRange { get; internal set; }

        public List<EventContainer> NoticingEvents { get; set; }

        public MapCell SpawnCell { get; set; }

        public long Target { get; set; }

        public short VNum { get; set; }

        #endregion
    }
}