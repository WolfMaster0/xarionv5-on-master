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
    [PacketHeader("$AddSkill", Authority = AuthorityType.GameMaster)]
    public class AddSkillPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public short SkillVnum { get; set; }

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
            AddSkillPacket packetDefinition = new AddSkillPacket();
            if (short.TryParse(packetSplit[2], out short skillVNum))
            {
                packetDefinition._isParsed = true;
                packetDefinition.SkillVnum = skillVNum;
            }
            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(AddSkillPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$AddSkill SKILLVNUM";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[AddSkill]SkillVNum: {SkillVnum}");

                Skill skillinfo = ServerManager.GetSkill(SkillVnum);
                if (skillinfo == null)
                {
                    session.SendPacket(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SKILL_DOES_NOT_EXIST"), 11));
                    return;
                }

                if (skillinfo.SkillVNum < 200)
                {
                    foreach (CharacterSkill skill in session.Character.Skills.GetAllItems())
                    {
                        if (skillinfo.CastId == skill.Skill.CastId && skill.Skill.SkillVNum < 200)
                        {
                            session.Character.Skills.Remove(skill.SkillVNum);
                        }
                    }
                }
                else
                {
                    if (session.Character.Skills.ContainsKey(SkillVnum))
                    {
                        session.SendPacket(
                            session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SKILL_ALREADY_EXIST"),
                                11));
                        return;
                    }

                    if (skillinfo.UpgradeSkill != 0)
                    {
                        CharacterSkill oldupgrade = session.Character.Skills.FirstOrDefault(s =>
                            s.Skill.UpgradeSkill == skillinfo.UpgradeSkill
                            && s.Skill.UpgradeType == skillinfo.UpgradeType && s.Skill.UpgradeSkill != 0);
                        if (oldupgrade != null)
                        {
                            session.Character.Skills.Remove(oldupgrade.SkillVNum);
                        }
                    }
                }

                session.Character.Skills[SkillVnum] = new CharacterSkill
                {
                    SkillVNum = SkillVnum,
                    CharacterId = session.Character.CharacterId
                };
                session.SendPacket(session.Character.GenerateSki());
                session.SendPackets(session.Character.GenerateQuicklist());
                session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"),
                    0));
                session.Character.LoadPassiveSkills();
                session.SendPacket(session.Character.GenerateLev());
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}