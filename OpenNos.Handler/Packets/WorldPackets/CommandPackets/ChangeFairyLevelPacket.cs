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
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$FLvl", Authority = AuthorityType.GameMaster)]
    public class ChangeFairyLevelPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public short FairyLevel { get; set; }

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

            ChangeFairyLevelPacket packetDefinition = new ChangeFairyLevelPacket();
            if (short.TryParse(packetSplit[2], out short fairyLevel))
            {
                packetDefinition._isParsed = true;
                packetDefinition.FairyLevel = fairyLevel;
            }

            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ChangeFairyLevelPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$FLvl FAIRYLEVEL";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[FLvl]FairyLevel: {FairyLevel}");

                ItemInstance fairy =
                    session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Fairy, InventoryType.Wear);
                if (fairy != null)
                {
                    short fairylevel = FairyLevel;
                    fairylevel -= fairy.Item.ElementRate;
                    fairy.ElementRate = fairylevel;
                    fairy.XP = 0;
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("FAIRY_LEVEL_CHANGED"), fairy.Item.Name),
                        10));
                    session.SendPacket(session.Character.GeneratePairy());
                }
                else
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_FAIRY"),
                        10));
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