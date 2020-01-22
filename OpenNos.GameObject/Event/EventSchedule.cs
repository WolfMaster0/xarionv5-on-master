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
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using OpenNos.Domain;

namespace OpenNos.GameObject.Event
{
    public class EventSchedule : IConfigurationSectionHandler
    {
        #region Methods

        public object Create(object parent, object configContext, XmlNode section)
        {
            List<Schedule> list = new List<Schedule>();
            foreach (XmlNode aSchedule in section.ChildNodes)
            {
                list.Add(GetSchedule(aSchedule));
            }
            return list;
        }

        private static Schedule GetSchedule(XmlNode str)
        {
            if (str.Attributes != null)
            {
                return new Schedule
                {
                    Event = (EventType)Enum.Parse(typeof(EventType), str.Attributes["event"].Value),
                    Time = TimeSpan.Parse(str.Attributes["time"].Value)
                };
            }
            return null;
        }

        #endregion
    }
}