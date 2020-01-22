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

namespace OpenNos.DAL.EF
{
    public sealed class Recipe
    {
        #region Instantiation

        public Recipe()
        {
            RecipeItem = new HashSet<RecipeItem>();
            RecipeList = new HashSet<RecipeList>();
        }

        #endregion

        #region Properties

        public byte Amount { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Item Item { get; set; }

        public short ItemVNum { get; set; }

        public short RecipeId { get; set; }

        public ICollection<RecipeItem> RecipeItem { get; }

        public ICollection<RecipeList> RecipeList { get; }

        #endregion
    }
}