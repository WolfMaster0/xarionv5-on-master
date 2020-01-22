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
using System.IO;

namespace OpenNos.ChatLog.Shared
{
    public class LogFileWriter
    {
        public void WriteLogFile(string path, List<ChatLogEntry> logs)
        {
            if (logs.Count > 0)
            {
                FileStream stream = null;
                try
                {
                    stream = File.Create(path);
                    using (BinaryWriter bw = new BinaryWriter(stream))
                    {
                        stream = null;
                        bw.Write((byte)0x4F);
                        bw.Write((byte)0x4E);
                        bw.Write((byte)0x43);
                        bw.Write((byte)1);
                        bw.Write(logs.Count);
                        foreach (ChatLogEntry log in logs)
                        {
                            bw.Write(log.Timestamp.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
                            bw.Write((byte)log.MessageType);
                            bw.Write(log.Sender ?? string.Empty);
                            bw.Write(log.SenderId ?? 0);
                            bw.Write(log.Receiver ?? string.Empty);
                            bw.Write(log.ReceiverId ?? 0);
                            bw.Write(log.Message ?? string.Empty);
                        }
                    }
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }
    }
}
