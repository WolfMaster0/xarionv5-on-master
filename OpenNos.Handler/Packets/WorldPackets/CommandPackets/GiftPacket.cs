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

using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Gift", Authority = AuthorityType.GameMaster)]
    public class GiftPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public byte Amount { get; set; }

        public string CharacterName { get; set; }

        public sbyte Rare { get; set; }

        public byte Upgrade { get; set; }

        public short VNum { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(' ');
                if (packetSplit.Length < 7)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                GiftPacket packetDefinition = new GiftPacket();
                if (!string.IsNullOrWhiteSpace(packetSplit[2]) && short.TryParse(packetSplit[3], out short vnum) && byte.TryParse(packetSplit[4], out byte amount) && sbyte.TryParse(packetSplit[5], out sbyte rare) && byte.TryParse(packetSplit[6], out byte upgrade))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.CharacterName = packetSplit[2];
                    packetDefinition.VNum = vnum;
                    packetDefinition.Amount = amount;
                    packetDefinition.Rare = rare;
                    packetDefinition.Upgrade = upgrade;
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(GiftPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Gift NAME(*) VNUM AMOUNT RARE UPGRADE";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[Gift]CharacterName: {CharacterName} ItemVNum: {VNum} Amount: {Amount} Rare: {Rare} Upgrade: {Upgrade}");

                if (CharacterName == "*")
                {
                    if (session.HasCurrentMapInstance)
                    {
                        Parallel.ForEach(session.CurrentMapInstance.Sessions,
                            sess => session.Character.SendGift(sess.Character.CharacterId, VNum,
                                Amount, Rare, Upgrade, false));
                        session.SendPacket(
                            session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GIFT_SENT"), 10));
                    }
                }
                else
                {
                    CharacterDTO chara = DAOFactory.CharacterDAO.LoadByName(CharacterName);
                    if (chara != null)
                    {
                        session.Character.SendGift(chara.CharacterId, VNum, Amount,
                            Rare, Upgrade, false);
                        session.SendPacket(
                            session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GIFT_SENT"), 10));
                    }
                    else
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"),
                                0));
                    }
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