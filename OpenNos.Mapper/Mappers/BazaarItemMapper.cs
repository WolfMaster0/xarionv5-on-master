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
    public static class BazaarItemMapper
    {
        #region Methods

        public static bool ToBazaarItem(BazaarItemDTO input, BazaarItem output)
        {
            if (input == null)
            {
                return false;
            }
            output.Amount = input.Amount;
            output.BazaarItemId = input.BazaarItemId;
            output.DateStart = input.DateStart;
            output.Duration = input.Duration;
            output.IsPackage = input.IsPackage;
            output.ItemInstanceId = input.ItemInstanceId;
            output.MedalUsed = input.MedalUsed;
            output.Price = input.Price;
            output.SellerId = input.SellerId;
            return true;
        }

        public static bool ToBazaarItemDTO(BazaarItem input, BazaarItemDTO output)
        {
            if (input == null)
            {
                return false;
            }
            output.Amount = input.Amount;
            output.BazaarItemId = input.BazaarItemId;
            output.DateStart = input.DateStart;
            output.Duration = input.Duration;
            output.IsPackage = input.IsPackage;
            output.ItemInstanceId = input.ItemInstanceId;
            output.MedalUsed = input.MedalUsed;
            output.Price = input.Price;
            output.SellerId = input.SellerId;
            return true;
        }

        #endregion
    }
}