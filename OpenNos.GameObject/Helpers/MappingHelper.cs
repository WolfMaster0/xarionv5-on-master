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

namespace OpenNos.GameObject.Helpers
{
    public static class MappingHelper
    {
        #region Instantiation

        static MappingHelper() =>

            // intialize hardcode in waiting for better solution
            GuriItemEffects = new Dictionary<int, int>
            {
                [859] = 1343,
                [860] = 1344,
                [861] = 1344,
                [875] = 1558,
                [876] = 1559,
                [877] = 1560,
                [878] = 1560,
                [879] = 1561,
                [880] = 1561
            };

        #endregion

        // effect items aka. fireworks

        #region Properties

        public static Dictionary<int, int> GuriItemEffects { get; }

        #endregion
    }
}