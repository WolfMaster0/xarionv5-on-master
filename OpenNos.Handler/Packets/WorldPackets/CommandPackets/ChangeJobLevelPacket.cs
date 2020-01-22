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
    [PacketHeader("$JLvl", Authority = AuthorityType.GameMaster)]
    public class ChangeJobLevelPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public byte JobLevel { get; set; }

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

            ChangeJobLevelPacket packetDefinition = new ChangeJobLevelPacket();
            if (byte.TryParse(packetSplit[2], out byte jobLevel))
            {
                packetDefinition._isParsed = true;
                packetDefinition.JobLevel = jobLevel;
            }

            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ChangeJobLevelPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$JLvl JOBLEVEL";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[JLvl]JobLevel: {JobLevel}");

                if (((session.Character.Class == 0 && JobLevel <= 20)
                     || (session.Character.Class != 0 && JobLevel <= 255))
                    && JobLevel > 0)
                {
                    session.Character.JobLevel = JobLevel;
                    session.Character.JobLevelXp = 0;
                    session.Character.Skills.ClearAll();
                    session.SendPacket(session.Character.GenerateLev());
                    session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("JOBLEVEL_CHANGED"), 0));
                    session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn(),
                        ReceiverType.AllExceptMe);
                    session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx(),
                        ReceiverType.AllExceptMe);
                    session.CurrentMapInstance?.Broadcast(
                        StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 8),
                        session.Character.PositionX, session.Character.PositionY);
                    session.Character.Skills[(short)(200 + (20 * (byte)session.Character.Class))] = new CharacterSkill
                    {
                        SkillVNum = (short)(200 + (20 * (byte)session.Character.Class)),
                        CharacterId = session.Character.CharacterId
                    };
                    session.Character.Skills[(short)(201 + (20 * (byte)session.Character.Class))] = new CharacterSkill
                    {
                        SkillVNum = (short)(201 + (20 * (byte)session.Character.Class)),
                        CharacterId = session.Character.CharacterId
                    };
                    session.Character.Skills[236] = new CharacterSkill
                    {
                        SkillVNum = 236,
                        CharacterId = session.Character.CharacterId
                    };
                    if (!session.Character.UseSp)
                    {
                        session.SendPacket(session.Character.GenerateSki());
                    }

                    session.Character.LearnAdventurerSkill();
                }
                else
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
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