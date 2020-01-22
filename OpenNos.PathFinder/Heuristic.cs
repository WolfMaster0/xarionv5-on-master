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
using System;

namespace OpenNos.PathFinder
{
    public static class Heuristic
    {
        #region Members

        public static readonly double Sqrt2 = Math.Sqrt(2);

        #endregion

        #region Methods

        public static double Chebyshev(int iDx, int iDy) => Math.Max(iDx, iDy);

        public static double Euclidean(int iDx, int iDy)
        {
            float tFdx = iDx;
            float tFdy = iDy;
            return Math.Sqrt((tFdx * tFdx) + (tFdy * tFdy));
        }

        public static double Manhattan(int iDx, int iDy) => iDx + iDy;

        public static double Octile(int iDx, int iDy)
        {
            int min = Math.Min(iDx, iDy);
            int max = Math.Max(iDx, iDy);
            return (min * Sqrt2) + max - min;
        }

        #endregion
    }
}