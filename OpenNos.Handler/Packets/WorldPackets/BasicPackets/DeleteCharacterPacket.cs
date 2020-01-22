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

using OpenNos.Core;
using OpenNos.Core.Cryptography;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("Char_DEL")]
    public class DeleteCharacterPacket
    {
        #region Properties

        public string Password { get; set; }

        public byte Slot { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            DeleteCharacterPacket charDelPacket = new DeleteCharacterPacket();
            if (byte.TryParse(packetSplit[2], out byte slot) && !string.IsNullOrWhiteSpace(packetSplit[3]))
            {
                charDelPacket.Slot = slot;
                charDelPacket.Password = packetSplit[3];
                charDelPacket.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(DeleteCharacterPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.HasCurrentMapInstance)
            {
                return;
            }

            AccountDTO account = DAOFactory.AccountDAO.LoadById(session.Account.AccountId);
            if (account == null)
            {
                return;
            }

            if (account.Password.ToLower() == CryptographyBase.Sha512(Password))
            {
                CharacterDTO character =
                    DAOFactory.CharacterDAO.LoadBySlot(account.AccountId, Slot);
                if (character == null)
                {
                    return;
                }

                GameLogger.Instance.LogCharacterDeletion(ServerManager.Instance.ChannelId, session.Account.Name,
                    session.Account.AccountId, character);

                // this was removed. propably baguettes idea, idk.
                //DAOFactory.GeneralLogDAO.SetCharIdNull(Convert.ToInt64(character.CharacterId));
                DAOFactory.CharacterDAO.DeleteByPrimaryKey(account.AccountId, Slot);
                EntryPointPacket.HandlePacket(session, string.Empty);
            }
            else
            {
                session.SendPacket($"info {Language.Instance.GetMessageFromKey("BAD_PASSWORD")}");
            }
        }

        #endregion
    }
}