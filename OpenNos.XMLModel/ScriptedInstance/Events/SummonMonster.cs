﻿// This file is part of the OpenNos NosTale Emulator Project.
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
using System.Xml.Serialization;

namespace OpenNos.XMLModel.ScriptedInstance.Events
{
    [Serializable]
    public class SummonMonster
    {
        #region Properties

        [XmlAttribute]
        public bool IsBoss { get; set; }

        [XmlAttribute]
        public bool IsHostile { get; set; }

        [XmlAttribute]
        public bool IsTarget { get; set; }

        [XmlAttribute]
        public bool Move { get; set; }

        [XmlAttribute]
        public bool IsBonus { get; set; }

        [XmlElement]
        public OnDeath OnDeath { get; set; }

        [XmlElement]
        public OnNoticing OnNoticing { get; set; }

        [XmlAttribute]
        public short PositionX { get; set; }

        [XmlAttribute]
        public short PositionY { get; set; }

        [XmlElement]
        public Roam Roam { get; set; }

        [XmlAttribute]
        public short VNum { get; set; }

        #endregion
    }
}