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

namespace OpenNos.DAL.EF
{
    public sealed class Shop
    {
        #region Instantiation

        public Shop()
        {
            ShopItem = new HashSet<ShopItem>();
            ShopSkill = new HashSet<ShopSkill>();
        }

        #endregion

        #region Properties

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public MapNpc MapNpc { get; set; }

        public int MapNpcId { get; set; }

        public byte MenuType { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public int ShopId { get; set; }

        public ICollection<ShopItem> ShopItem { get; }

        public ICollection<ShopSkill> ShopSkill { get; }

        public byte ShopType { get; set; }

        #endregion
    }
}