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
using OpenNos.Core;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using OpenNos.DAL.EF.Context;

namespace OpenNos.DAL.DAO
{
    public class EventScriptDAO : IEventScriptDAO
    {
        #region Methods

        public IEnumerable<EventScriptDTO> LoadActive()
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    DateTime now = DateTime.UtcNow;
                    List<EventScriptDTO> result = new List<EventScriptDTO>();
                    List<EventScript> filtered = new List<EventScript>();
                    foreach (EventScript eventScript in context.EventScript.AsNoTracking())
                    {
                        // This is default type of event thats works in set amount of time.
                        if (DateTime.TryParseExact(eventScript.DateStart, "MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateStart)
                            && DateTime.TryParseExact(eventScript.DateEnd, "MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateEnd))
                        {
                            // thats most likely less shit than the other one, it eliminates the jump year etc.
                            // current month is bigger or equal than start month
                            // current day is bigger or equal than start day
                            // current month is less or equal than end month
                            // current day is less or equal than end day
                            if ((now.Month >= dateStart.Month && now.Month <= dateEnd.Month)
                                || (now.Month == dateEnd.Month && now.Day <= dateEnd.Day)
                                || (now.Month == dateStart.Month && now.Day >= dateStart.Day))
                            {
                                filtered.Add(eventScript);
                            }
                            //int dateStartDay = dateStart.DayOfYear;
                            //int dateEndDay = dateEnd.DayOfYear;
                            //if (dateStartDay <= day && ((dateStartDay >= dateEndDay && dateEndDay <= day) || dateEndDay >= day))
                            //{
                            //    filtered.Add(eventScript);
                            //}

                        }
                        // This should be a daily event started at set hour.
                        else if (string.IsNullOrEmpty(eventScript.DateEnd)
                                 // ReSharper disable once UnusedVariable
                                 && DateTime.TryParseExact(eventScript.DateStart, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime interval))
                        {
                            filtered.Add(eventScript);
                        }
                        // This should be a event with interval set inside of script itself.
                        else if (string.IsNullOrEmpty(eventScript.DateEnd) && string.IsNullOrEmpty(eventScript.DateStart))
                        {
                            filtered.Add(eventScript);
                        }
                    }
                    foreach (EventScript eventScript in filtered)
                    {
                        EventScriptDTO dto = new EventScriptDTO();
                        Mapper.Mappers.EventScriptMapper.ToEventScriptDTO(eventScript, dto);
                        result.Add(dto);
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<EventScriptDTO> LoadInactive()
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    DateTime now = DateTime.UtcNow;
                    List<EventScriptDTO> result = new List<EventScriptDTO>();
                    List<EventScript> filtered = new List<EventScript>();
                    foreach (EventScript eventScript in context.EventScript.AsNoTracking())
                    {
                        if (DateTime.TryParseExact(eventScript.DateStart, "MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateStart)
                            && DateTime.TryParseExact(eventScript.DateEnd, "MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateEnd)
                            && !(now.Month >= dateStart.Month && now.Month <= dateEnd.Month)
                            && !(now.Month == dateEnd.Month && now.Day <= dateEnd.Day)
                            && !(now.Month == dateStart.Month && now.Day >= dateStart.Day))
                        {
                            filtered.Add(eventScript);
                        }
                    }
                    foreach (EventScript eventScript in filtered)
                    {
                        EventScriptDTO dto = new EventScriptDTO();
                        Mapper.Mappers.EventScriptMapper.ToEventScriptDTO(eventScript, dto);
                        result.Add(dto);
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        #endregion
    }
}