// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.
using OpenNos.XMLModel.ScriptedInstance.Events;
using System;
using System.Xml.Serialization;

namespace OpenNos.XMLModel.ScriptedInstance.Objects
{
    [Serializable]
    public class CreateMap
    {
        #region Properties

        [XmlElement]
        public GenerateClock GenerateClock { get; set; }

        [XmlAttribute]
        public byte IndexX { get; set; }

        [XmlAttribute]
        public byte IndexY { get; set; }

        [XmlAttribute]
        public int Map { get; set; }

        [XmlElement]
        public OnAreaEntry[] OnAreaEntry { get; set; }

        [XmlElement]
        public OnCharacterDiscoveringMap OnCharacterDiscoveringMap { get; set; }

        [XmlElement]
        public OnLockerOpen OnLockerOpen { get; set; }

        [XmlElement]
        public OnMoveOnMap[] OnMoveOnMap { get; set; }

        [XmlElement]
        public SetButtonLockers SetButtonLockers { get; set; }

        [XmlElement]
        public SetMonsterLockers SetMonsterLockers { get; set; }

        [XmlElement]
        public SpawnButton[] SpawnButton { get; set; }

        [XmlElement]
        public SpawnPortal[] SpawnPortal { get; set; }

        [XmlElement]
        public object StartClock { get; set; }

        [XmlElement]
        public SummonMonster[] SummonMonster { get; set; }

        [XmlElement]
        public SummonNpc[] SummonNpc { get; set; }

        [XmlAttribute]
        public short VNum { get; set; }

        #endregion
    }
}