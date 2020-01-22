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
using OpenNos.XMLModel.Quest.Events;
using System;
using System.Xml.Serialization;
using OpenNos.Domain;
using OpenNos.XMLModel.Quest.Objects;

namespace OpenNos.XMLModel.Quest.Model
{
    [XmlRoot("Definition"), Serializable]
    public class QuestModel
    {
        public QuestGiver QuestGiver { get; set; }

        public long QuestId { get; set; }

        public Guid QuestProgressId { get; set; }

        public bool IsFinished { get; set; }

        public short QuestDataVNum { get; set; }

        public QuestGoalType QuestGoalType { get; set; }

        public Script Script { get; set; }

        public Reward Reward { get; set; }

        public Objective[] Objectives { get; set; }
    }
}