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
using OpenNos.XMLModel.Shared;

namespace OpenNos.XMLModel.ScriptedInstance.Objects
{
    [Serializable]
    public class Globals
    {
        #region Properties

        public Item[] DrawItems { get; set; }

        public Item[] GiftItems { get; set; }

        public Gold Gold { get; set; }

        public Id Id { get; set; }

        public Label Label { get; set; }

        public Name Name { get; set; }

        public Level LevelMaximum { get; set; }

        public Level LevelMinimum { get; set; }

        public Lives Lives { get; set; }

        public Reputation Reputation { get; set; }

        public Item[] RequiredItems { get; set; }

        public Item[] SpecialItems { get; set; }

        public StartPosition StartX { get; set; }

        public StartPosition StartY { get; set; }

        #endregion
    }
}