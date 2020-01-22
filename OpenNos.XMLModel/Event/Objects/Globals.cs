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
using OpenNos.XMLModel.Event.Events;
using System;
using System.Xml.Serialization;
using OpenNos.XMLModel.Shared;

namespace OpenNos.XMLModel.Event.Objects
{
    [Serializable]
    public class Globals
    {
        #region Properties

        [XmlElement]
        public AddDrop[] AddDrop { get; set; }

        [XmlElement]
        public FinishEvents FinishEvents { get; set; }

        [XmlArray]
        public Item[] Rewards { get; set; }

        [XmlArray]
        public Item[] Roll { get; set; }

        [XmlElement]
        public SpawnNpc [] SpawnNpc { get; set; }

        [XmlElement]
        public SpawnMonster[] SpawnMonster { get; set; }

        [XmlElement]
        public SpawnPortal[] SpawnPortal { get; set; }

        [XmlElement]
        public EventItem[] UnlockItems { get; set; }

        [XmlElement]
        public StartConditions StartConditions { get; set; }

        #endregion
    }
}