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
    public class LogFileReader
    {
        public List<ChatLogEntry> ReadLogFile(string path)
        {
            FileStream stream = null;
            try
            {
                stream = File.OpenRead(path);
                using (BinaryReader br = new BinaryReader(stream))
                {
                    stream = null;
                    string header = $"{br.ReadChar()}{br.ReadChar()}{br.ReadChar()}";
                    byte version = br.ReadByte();
                    if (header == "ONC")
                    {
                        switch (version)
                        {
                            case 1:
                                return ReadVersion(br);

                            default:
                                throw new InvalidDataException("File Version invalid!");
                        }
                    }
                    else
                    {
                        throw new InvalidDataException("File Header invalid!");
                    }
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }

        private List<ChatLogEntry> ReadVersion(BinaryReader reader)
        {
            List<ChatLogEntry> result = new List<ChatLogEntry>();
            int count = reader.ReadInt32();
            while (count != 0)
            {
                DateTime timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(reader.ReadDouble());
                ChatLogType chatLogType = (ChatLogType)reader.ReadByte();
                string sender = reader.ReadString();
                long senderId = reader.ReadInt64();
                string receiver = reader.ReadString();
                long receiverId = reader.ReadInt64();
                string message = reader.ReadString();
                result.Add(new ChatLogEntry()
                {
                    Timestamp = timestamp,
                    MessageType = chatLogType,
                    Sender = sender,
                    SenderId = senderId,
                    Receiver = receiver,
                    ReceiverId = receiverId,
                    Message = message
                });
                count--;
            }
            return result;
        }
    }
}
