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
using OpenNos.DAL;
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class Recipe : RecipeDTO
    {
        #region Properties

        public List<RecipeItemDTO> Items { get; set; }

        #endregion

        public Recipe()
        {
        }

        public Recipe(RecipeDTO input)
        {
            Amount = input.Amount;
            ItemVNum = input.ItemVNum;
            RecipeId = input.RecipeId;
        }

        #region Methods

        public void Initialize()
        {
            Items = new List<RecipeItemDTO>();
            foreach (RecipeItemDTO recipe in DAOFactory.RecipeItemDAO.LoadByRecipe(RecipeId).ToList())
            {
                Items.Add(recipe);
            }
        }

        #endregion
    }
}