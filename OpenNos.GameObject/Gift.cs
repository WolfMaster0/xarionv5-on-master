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
namespace OpenNos.GameObject
{
    public class Gift
    {
        #region Instantiation

        public Gift()
        {
            // do nothing
        }

        public Gift(short vnum, byte amount, short design = 0, bool isRareRandom = false)
        {
            VNum = vnum;
            Amount = amount;
            IsRandomRare = isRareRandom;
            Design = design;
        }

        #endregion

        #region Properties

        public byte Amount { get; set; }

        public short Design { get; set; }

        public bool IsRandomRare { get; set; }

        public short VNum { get; set; }

        #endregion
    }
}