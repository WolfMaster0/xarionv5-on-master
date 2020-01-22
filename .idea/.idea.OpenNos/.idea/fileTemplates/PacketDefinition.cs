using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.GameObject;
using System;

namespace OpenNos.Handler.Packets
{
    [PacketHeader("HEADER")]
    public class ${NAME}
    {
        #region Properties
        
        // Packet Properties here

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 2) // + Amount of properties
            {
                return;
            }
            ${NAME} packetDefinition = new ${NAME}();
            if (true/*parsing here*/)
            {
                // Set Packet Properties after parsing
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(${NAME}), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            // Packet Handler Code here
        }

        #endregion
    }
}