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
using OpenNos.Data;

namespace OpenNos.GameObject
{
    public class MinilandObject : MinilandObjectDTO
    {
        #region Members

        public ItemInstance ItemInstance;

        #endregion

        #region Instantiation

        public MinilandObject()
        {

        }

        public MinilandObject(MinilandObjectDTO input)
        {
            CharacterId = input.CharacterId;
            ItemInstanceId = input.ItemInstanceId;
            Level1BoxAmount = input.Level1BoxAmount;
            Level2BoxAmount = input.Level2BoxAmount;
            Level3BoxAmount = input.Level3BoxAmount;
            Level4BoxAmount = input.Level4BoxAmount;
            Level5BoxAmount = input.Level5BoxAmount;
            MapX = input.MapX;
            MapY = input.MapY;
            MinilandObjectId = input.MinilandObjectId;
        }

        #endregion

        #region Methods

        public string GenerateMinilandEffect(bool removed) => $"eff_g {ItemInstance.Item.EffectValue} {MapX.ToString("00")}{MapY.ToString("00")} {MapX} {MapY} {(removed ? 1 : 0)}";

        public string GenerateMinilandObject(bool deleted) => $"mlobj {(deleted ? 0 : 1)} {ItemInstance.Slot} {MapX} {MapY} {ItemInstance.Item.Width} {ItemInstance.Item.Height} 0 {ItemInstance.DurabilityPoint} 0 {(ItemInstance.Item.IsMinilandObject ? 1 : 0)}";

        #endregion
    }
}