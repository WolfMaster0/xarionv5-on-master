using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("marry")]
    public class MarriagePacket
    {
        #region Properties

        public bool Accept { get; set; }

        public long CharacterId { get; set; }

        public bool IsInvoker { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            MarriagePacket packetDefinition = new MarriagePacket();
            if (long.TryParse(packetSplit[3], out long charId))
            {
                packetDefinition.Accept = packetSplit[2] == "1";
                packetDefinition.IsInvoker = packetSplit[2] == "2";
                packetDefinition.CharacterId = charId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MarriagePacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (CharacterId == session.Character.CharacterId)
            {
                return;
            }
            if (IsInvoker)
            {
                if (session.Character.Inventory.CountItem(1981) < 1)
                {
                    return;
                }

                if (session.Character.CharacterRelations.Any(s => s.RelationType == CharacterRelationType.Spouse))
                {
                    session.SendPacket($"info {Language.Instance.GetMessageFromKey("ALREADY_MARRIED")}");
                    return;
                }

                if (session.Character.IsFriendOfCharacter(CharacterId))
                {
                    ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterId(CharacterId);
                    if (otherSession != null)
                    {
                        otherSession.SendPacket(UserInterfaceHelper.GenerateDialog(
                            $"#marry^1^{session.Character.CharacterId} #marry^0^{session.Character.CharacterId} " +
                            string.Format(Language.Instance.GetMessageFromKey("MARRY_REQUEST"), session.Character.Name)));
                        session.Character.MarriageRequestCharacters.Add(CharacterId);
                        session.Character.Inventory.RemoveItemAmount(1981);
                    }
                }
                else
                {
                    session.SendPacket($"info {Language.Instance.GetMessageFromKey("NOT_FRIEND")}");
                }
            }
            else
            {
                if (session.Character.CharacterRelations.Any(s => s.RelationType == CharacterRelationType.Spouse))
                {
                    session.SendPacket($"info {Language.Instance.GetMessageFromKey("ALREADY_MARRIED")}");
                    return;
                }
                if (ServerManager.Instance.GetSessionByCharacterId(CharacterId) is ClientSession targetsSession
                    && targetsSession.Character.MarriageRequestCharacters.Any(s => s == session.Character.CharacterId))
                {
                    if (Accept)
                    {
                        session.Character.DeleteRelation(CharacterId);
                        session.Character.AddRelation(CharacterId, CharacterRelationType.Spouse);

                        ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("MARRIAGE_ACCEPT"),
                                session.Character.Name,
                                targetsSession.Character.Name), 0));
                        session.CurrentMapInstance.Broadcast(
                            StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 850));
                        session.CurrentMapInstance.Broadcast(
                            StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 850));
                    }
                    else
                    {
                        ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("MARRIAGE_REJECT"),
                                session.Character.Name,
                                targetsSession.Character.Name), 0));
                    }

                    targetsSession.Character.MarriageRequestCharacters.Remove(session.Character.CharacterId);
                }
            }
        }

        #endregion
    }
}