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

using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$SearchItem", Authority = AuthorityType.GameMaster)]
    public class SearchItemPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string Name { get; set; }

        public byte Page { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(new[] { ' ' }, 3);
                if (packetSplit.Length < 3)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                SearchItemPacket packetDefinition = new SearchItemPacket();
                if (!string.IsNullOrWhiteSpace(packetSplit[2]))
                {
                    packetDefinition._isParsed = true;
                    string[] searchSplit = packetSplit[2].Split(' ');
                    packetDefinition.Page =
                        byte.TryParse(searchSplit[searchSplit.Length - 1], out byte pg) ? pg : (byte)0;
                    packetDefinition.Name = searchSplit[searchSplit.Length - 1] != packetDefinition.Page.ToString()
                        ? packetSplit[2]
                        : string.Join(" ", searchSplit.Take(searchSplit.Length - 1));
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(SearchItemPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$SearchItem NAME PAGE(?)";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[SearchItem]Name: {Name} Page: {Page}");

                IEnumerable<Item> itemlist = ServerManager.GetItemsByName(Name).OrderBy(s => s.VNum)
                    .Skip(Page * 200).Take(200).ToList();
                if (itemlist.Any())
                {
                    int i = 0;
                    foreach (Item item in itemlist)
                    {
                        session.SendPacket(session.Character.GenerateSay(
                            $"[{i}][Page: {Page}]Item: {(string.IsNullOrEmpty(item.Name) ? "none" : item.Name)} VNum: {item.VNum}",
                            12));
                        i++;
                    }
                }
                else
                {
                    session.SendPacket(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_NOT_FOUND"), 11));
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