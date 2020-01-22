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
    public static class MinilandObjectMapper
    {
        #region Methods

        public static bool ToMinilandObject(MinilandObjectDTO input, MinilandObject output)
        {
            if (input == null)
            {
                return false;
            }
            output.CharacterId = input.CharacterId;
            output.ItemInstanceId = input.ItemInstanceId;
            output.Level1BoxAmount = input.Level1BoxAmount;
            output.Level2BoxAmount = input.Level2BoxAmount;
            output.Level3BoxAmount = input.Level3BoxAmount;
            output.Level4BoxAmount = input.Level4BoxAmount;
            output.Level5BoxAmount = input.Level5BoxAmount;
            output.MapX = input.MapX;
            output.MapY = input.MapY;
            output.MinilandObjectId = input.MinilandObjectId;
            return true;
        }

        public static bool ToMinilandObjectDTO(MinilandObject input, MinilandObjectDTO output)
        {
            if (input == null)
            {
                return false;
            }
            output.CharacterId = input.CharacterId;
            output.ItemInstanceId = input.ItemInstanceId;
            output.Level1BoxAmount = input.Level1BoxAmount;
            output.Level2BoxAmount = input.Level2BoxAmount;
            output.Level3BoxAmount = input.Level3BoxAmount;
            output.Level4BoxAmount = input.Level4BoxAmount;
            output.Level5BoxAmount = input.Level5BoxAmount;
            output.MapX = input.MapX;
            output.MapY = input.MapY;
            output.MinilandObjectId = input.MinilandObjectId;
            return true;
        }

        #endregion
    }
}