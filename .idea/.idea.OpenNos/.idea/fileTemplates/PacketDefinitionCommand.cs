// This file is part of the OpenNos NosTale Emulator Project.
// 
// This program is licensed under a deviated version of the Fair Source License,
// granting you a non-exclusive, non-transferable, royalty-free and fully-paid-up
// license, under all of the Licensor's copyright and patent rights, to use, copy, prepare
// derivative works of, publicly perform and display the Software, subject to the
// contitions found in the LICENSE file.
// 
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR
// CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN
// AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE.
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.GameObject;
using OpenNos.Domain;
using System;

namespace OpenNos.Handler.Packets
{
    [PacketHeader("${Command}", Authority = AuthorityType.GameMaster)]
    public class ${NAME}
    {
        #region Properties
        
        private bool _isParsed;

        // Packet Properties here

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            ClientSession sess = session as ClientSession;
            if(sess != null)
            {
                string[] packetSplit = packet.Split(' ');
                if (packetSplit.Length < 2) // + Amount of properties
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                ${NAME} packetDefinition = new ${NAME}();
                if (true/*parsing here*/)
                {
                    packetDefinition._isParsed = true;
                    // Set Packet Properties after parsing
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(${NAME}), HandlePacket);
        
        public static string ReturnHelp() => "${Command} ${Help}";
                
        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                // Packet Handler Code here
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}