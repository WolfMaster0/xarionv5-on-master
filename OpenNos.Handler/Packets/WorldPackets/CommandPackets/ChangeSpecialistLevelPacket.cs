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

using System;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$SPLvl", Authority = AuthorityType.GameMaster)]
    public class ChangeSpecialistLevelPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public byte SpecialistLevel { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (!(session is ClientSession sess))
            {
                return;
            }
            if (packetSplit.Length < 3)
            {
                sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                return;
            }
            ChangeSpecialistLevelPacket packetDefinition = new ChangeSpecialistLevelPacket();
            if (byte.TryParse(packetSplit[2], out byte specialistLevel))
            {
                packetDefinition._isParsed = true;
                packetDefinition.SpecialistLevel = specialistLevel;
            }
            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ChangeSpecialistLevelPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$SPLvl SPLEVEL";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[SPLvl]SpecialistLevel: {SpecialistLevel}");

                ItemInstance sp =
                    session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
                if (sp != null && session.Character.UseSp)
                {
                    if (SpecialistLevel <= 255
                        && SpecialistLevel > 0)
                    {
                        sp.SpLevel = SpecialistLevel;
                        sp.XP = 0;
                        session.SendPacket(session.Character.GenerateLev());
                        session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SPLEVEL_CHANGED"), 0));
                        session.Character.LearnSpSkill();
                        session.SendPacket(session.Character.GenerateSki());
                        session.SendPackets(session.Character.GenerateQuicklist());
                        session.Character.Skills.ForEach(s => s.LastUse = DateTime.UtcNow.AddDays(-1));
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn(),
                            ReceiverType.AllExceptMe);
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx(),
                            ReceiverType.AllExceptMe);
                        session.CurrentMapInstance?.Broadcast(
                            StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 8),
                            session.Character.PositionX, session.Character.PositionY);
                    }
                    else
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                    }
                }
                else
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"),
                        0));
                }
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}