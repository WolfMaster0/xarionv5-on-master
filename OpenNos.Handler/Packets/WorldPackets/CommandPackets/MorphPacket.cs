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
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Morph", Authority = AuthorityType.GameMaster)]
    public class MorphPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public int ArenaWinner { get; set; }

        public int MorphDesign { get; set; }

        public int MorphId { get; set; }

        public int Upgrade { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(' ');
                if (packetSplit.Length < 6)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                MorphPacket packetDefinition = new MorphPacket();
                if (int.TryParse(packetSplit[2], out int morphId) && int.TryParse(packetSplit[3], out int upgrade) && int.TryParse(packetSplit[4], out int morphDesign) && int.TryParse(packetSplit[5], out int arenaWinner))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.MorphId = morphId;
                    packetDefinition.Upgrade = upgrade;
                    packetDefinition.MorphDesign = morphDesign;
                    packetDefinition.ArenaWinner = arenaWinner;
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MorphPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Morph MORPHID UPGRADE WINGS ARENA";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[Morph]MorphId: {MorphId} MorphDesign: {MorphDesign} Upgrade: {Upgrade} MorphId: {ArenaWinner}");

                if (MorphId < 30 && MorphId > 0)
                {
                    session.Character.UseSp = true;
                    session.Character.Morph = MorphId;
                    session.Character.MorphUpgrade = Upgrade;
                    session.Character.MorphUpgrade2 = MorphDesign;
                    session.Character.ArenaWinner = ArenaWinner;
                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateCMode());
                }
                else if (MorphId > 30)
                {
                    session.Character.IsVehicled = true;
                    session.Character.Morph = MorphId;
                    session.Character.ArenaWinner = ArenaWinner;
                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateCMode());
                }
                else
                {
                    session.Character.IsVehicled = false;
                    session.Character.UseSp = false;
                    session.Character.ArenaWinner = 0;
                    session.SendPacket(session.Character.GenerateCond());
                    session.SendPacket(session.Character.GenerateLev());
                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateCMode());
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