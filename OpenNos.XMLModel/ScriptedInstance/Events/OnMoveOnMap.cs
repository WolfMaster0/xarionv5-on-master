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
using OpenNos.XMLModel.ScriptedInstance.Objects;
using System;
using System.Xml.Serialization;

namespace OpenNos.XMLModel.ScriptedInstance.Events
{
    [Serializable]
    public class OnMoveOnMap
    {
        #region Properties

        [XmlElement]
        public GenerateClock GenerateClock { get; set; }

        [XmlElement]
        public OnMapClean OnMapClean { get; set; }

        [XmlElement]
        public object RefreshRaidGoals { get; set; }

        [XmlElement]
        public object RemoveButtonLocker { get; set; }

        [XmlElement]
        public object RemoveMonsterLocker { get; set; }

        [XmlElement]
        public SendMessage SendMessage { get; set; }

        [XmlElement]
        public SendPacket SendPacket { get; set; }

        [XmlElement]
        public SetButtonLockers SetButtonLockers { get; set; }

        [XmlElement]
        public SetMonsterLockers SetMonsterLockers { get; set; }

        [XmlElement]
        public StartClock StartClock { get; set; }

        [XmlElement]
        public StartClock StartMapClock { get; set; }

        [XmlElement]
        public SummonMonster[] SummonMonster { get; set; }

        [XmlElement]
        public Wave[] Wave { get; set; }

        #endregion
    }
}