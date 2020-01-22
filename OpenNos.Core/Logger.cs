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
using log4net;
using System;
using System.Runtime.CompilerServices;

namespace OpenNos.Core
{
    public static class Logger
    {
        #region Properties

        public static ILog Log { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Wraps up the error message with the CallerMemberName
        /// </summary>
        /// <param name="data"></param>
        /// <param name="memberName"></param>
        public static void Debug(string data, [CallerMemberName]string memberName = "") => Log?.Debug($"[{memberName}]: {data}");

        /// <summary>
        /// Wraps up the error message with the CallerMemberName
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="ex"></param>
        public static void Error(Exception ex, [CallerMemberName]string memberName = "") => Log?.Error($"[{memberName}]: {ex.Message}", ex);

        /// <summary>
        /// Wraps up the error message with the CallerMemberName
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ex"></param>
        /// <param name="memberName"></param>
        public static void Error(string data, Exception ex = null, [CallerMemberName]string memberName = "")
        {
            if (ex != null)
            {
                Log?.Error($"[{memberName}]: {data} {ex.InnerException}", ex);
            }
            else
            {
                Log?.Error($"[{memberName}]: {data}");
            }
        }

        /// <summary>
        /// Wraps up the fatal message with the CallerMemberName
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ex"></param>
        /// <param name="memberName"></param>
        public static void Fatal(string data, Exception ex = null, [CallerMemberName]string memberName = "")
        {
            if (ex != null)
            {
                Log?.Fatal($"[{memberName}]: {data} {ex.InnerException}", ex);
            }
            else
            {
                Log?.Fatal($"[{memberName}]: {data}");
            }
        }

        /// <summary>
        /// Wraps up the info message with the CallerMemberName
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="memberName"></param>
        public static void Info(string message, Exception ex = null, [CallerMemberName]string memberName = "")
        {
            if (ex != null)
            {
                Log?.Info($"[{memberName}]: {message}", ex);
            }
            else
            {
                Log?.Info($"[{memberName}]: {message}");
            }
        }

        public static void InitializeLogger(ILog log) => Log = log;

        /// <summary>
        /// Wraps up the error message with the Logging Event
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="data"></param>
        /// <param name="ex"></param>
        /// <param name="memberName"></param>
        public static void LogEventError(string logEvent, string data, Exception ex = null, [CallerMemberName]string memberName = "")
        {
            if (ex != null)
            {
                Log?.Error($"[{memberName}]: [{logEvent}]{data}", ex);
            }
            else
            {
                Log?.Error($"[{memberName}]: [{logEvent}]{data}");
            }
        }

        /// <summary>
        /// Wraps up the error message with the Logging Event
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="caller"></param>
        /// <param name="data"></param>
        public static void LogUserEvent(string logEvent, string caller, string data) => Log?.Info($"[{logEvent}][{caller}]{data}");

        /// <summary>
        /// Wraps up the error message with the Logging Event
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="caller"></param>
        /// <param name="data"></param>
        /// <param name="ex"></param>
        public static void LogUserEventError(string logEvent, string caller, string data, Exception ex) => Log?.Error($"[{logEvent}][{caller}]{data}", ex);

        /// <summary>
        /// Wraps up the warn message with the CallerMemberName
        /// </summary>
        /// <param name="data"></param>
        /// <param name="innerException"></param>
        /// <param name="memberName"></param>
        public static void Warn(string data, Exception innerException = null, [CallerMemberName]string memberName = "")
        {
            if (innerException != null)
            {
                Log?.Warn($"[{memberName}]: {data} {innerException.InnerException}", innerException);
            }
            else
            {
                Log?.Warn($"[{memberName}]: {data}");
            }
        }

        #endregion
    }
}