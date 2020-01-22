// This file is part of the OpenNos NosTale Emulator Project.

// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.
using OpenNos.Domain;
using System;

namespace OpenNos.XMLModel.Quest.Objects
{
    [Serializable]
    public class Objective
    {
        #region Properties

        public int Param1 { get; set; }

        public int Param2 { get; set; }

        public int Param3 { get; set; }

        public int Param4 { get; set; }

        public int Param5 { get; set; }

        public int CurrentAmount { get; set; }

        public int GoalAmount { get; set; }

        public bool IsFinished { get; set; }

        public ObjectiveType Type { get; set; }

        #endregion
    }
}