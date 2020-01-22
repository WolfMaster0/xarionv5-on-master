using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("divorce")]
    public class DivorcePacket
    {
        #region Properties

        public bool Accept { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3) // + Amount of properties
            {
                return;
            }
            DivorcePacket packetDefinition = new DivorcePacket();
            if (true/*parsing here*/)
            {
                packetDefinition.Accept = packetSplit[2] == "1";
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(DivorcePacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Inventory.CountItem(1982) > 0 && Accept
                && session.Character.CharacterRelations.Find(s=>s.RelationType == CharacterRelationType.Spouse) is CharacterRelationDTO dto)
            {
                session.Character.DeleteRelation(session.Character.CharacterId == dto.CharacterId
                    ? dto.RelatedCharacterId
                    : dto.CharacterId);

                session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("DIVORCED")));
                session.Character.Inventory.RemoveItemAmount(1982);
            }
        }

        #endregion
    }
}