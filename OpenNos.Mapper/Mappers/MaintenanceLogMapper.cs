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
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public static class MaintenanceLogMapper
    {
        #region Methods

        public static bool ToMaintenanceLog(MaintenanceLogDTO input, MaintenanceLog output)
        {
            if (input == null)
            {
                return false;
            }
            output.DateEnd = input.DateEnd;
            output.DateStart = input.DateStart;
            output.LogId = input.LogId;
            output.Reason = input.Reason;
            return true;
        }

        public static bool ToMaintenanceLogDTO(MaintenanceLog input, MaintenanceLogDTO output)
        {
            if (input == null)
            {
                return false;
            }
            output.DateEnd = input.DateEnd;
            output.DateStart = input.DateStart;
            output.LogId = input.LogId;
            output.Reason = input.Reason;
            return true;
        }

        #endregion
    }
}