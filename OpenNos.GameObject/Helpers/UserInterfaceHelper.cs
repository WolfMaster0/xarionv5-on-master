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
using OpenNos.Domain;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject.Helpers
{
    public class UserInterfaceHelper
    {
        #region Members

        private static UserInterfaceHelper _instance;

        #endregion

        #region Properties

        public static UserInterfaceHelper Instance => _instance ?? (_instance = new UserInterfaceHelper());

        #endregion

        #region Methods

        public static string GenerateBsInfo(byte mode, short title, short time, short text) => $"bsinfo {mode} {title} {time} {text}";

        public static string GenerateChdm(int maxhp, int angeldmg, int demondmg, int time) => $"ch_dm {maxhp} {angeldmg} {demondmg} {time}";

        public static string GenerateDelay(int delay, int type, string argument) => $"delay {delay} {type} {argument}";

        public static string GenerateDialog(string dialog) => $"dlg {dialog}";

        public static string GenerateFrank(byte type)
        {
            string packet = "frank_stc";
            int rank = 1;
            long savecount = 0;

            List<Family> familyordered = null;
            switch (type)
            {
                case 0:
                    familyordered = ServerManager.Instance.FamilyList.GetAllItems().OrderByDescending(s => s.FamilyExperience).ToList();
                    break;

                case 1:
                    familyordered = ServerManager.Instance.FamilyList.GetAllItems().OrderByDescending(s => s.FamilyLogs.Where(l => l.FamilyLogType == FamilyLogType.FamilyXP && l.Timestamp.AddDays(30) < DateTime.UtcNow).ToList().Sum(c => long.Parse(c.FamilyLogData.Split('|')[1]))).ToList();//use month instead log
                    break;

                case 2:

                    // use month instead log
                    familyordered = ServerManager.Instance.FamilyList.GetAllItems().OrderByDescending(s => s.FamilyCharacters.Sum(c => c.Character.Reputation)).ToList();
                    break;

                case 3:
                    familyordered = ServerManager.Instance.FamilyList.GetAllItems().OrderByDescending(s => s.FamilyCharacters.Sum(c => c.Character.Reputation)).ToList();
                    break;
            }
            int i = 0;
            if (familyordered != null)
            {
                foreach (Family fam in familyordered.Take(100))
                {
                    i++;
                    long sum;
                    switch (type)
                    {
                        case 0:
                            if (savecount != fam.FamilyExperience)
                            {
                                rank++;
                            }
                            else
                            {
                                rank = i;
                            }
                            savecount = fam.FamilyExperience;
                            packet += $" {rank}|{fam.Name}|{fam.FamilyLevel}|{fam.FamilyExperience}";//replace by month log
                            break;

                        case 1:
                            if (savecount != fam.FamilyExperience)
                            {
                                rank++;
                            }
                            else
                            {
                                rank = i;
                            }
                            savecount = fam.FamilyExperience;
                            packet += $" {rank}|{fam.Name}|{fam.FamilyLevel}|{fam.FamilyExperience}";
                            break;

                        case 2:
                            sum = fam.FamilyCharacters.Sum(c => c.Character.Reputation);
                            if (savecount != sum)
                            {
                                rank++;
                            }
                            else
                            {
                                rank = i;
                            }
                            savecount = sum;//replace by month log
                            packet += $" {rank}|{fam.Name}|{fam.FamilyLevel}|{savecount}";
                            break;

                        case 3:
                            sum = fam.FamilyCharacters.Sum(c => c.Character.Reputation);
                            if (savecount != sum)
                            {
                                rank++;
                            }
                            else
                            {
                                rank = i;
                            }
                            savecount = sum;
                            packet += $" {rank}|{fam.Name}|{fam.FamilyLevel}|{savecount}";
                            break;
                    }
                }
            }
            return packet;
        }

        public static string GenerateGuri(byte type, byte argument, long callerId, int value = 0, int value2 = 0)
        {
            switch (type)
            {
                case 2:
                    return $"guri 2 {argument} {callerId}";

                case 6:
                    return $"guri 6 1 {callerId} 0 0";

                case 10:
                    return $"guri 10 {argument} {value} {callerId}";

                case 15:
                    return $"guri 15 {argument} 0 0";

                case 31:
                    return $"guri 31 {argument} {callerId} {value} {value2}";

                default:
                    return $"guri {type} {argument} {callerId} {value}";
            }
        }

        public static string GenerateInbox(string value) => $"inbox {value}";

        public static string GenerateInfo(string message) => $"info {message}";

        public static string GenerateMapOut() => "mapout";

        public static string GenerateModal(string message, int type) => $"modal {type} {message}";

        public static string GenerateMsg(string message, int type) => $"msg {type} {message}";

        public static string GeneratePClear() => "p_clear";

        public static string GenerateRemovePacket(short slot) => $"{slot}.-1.0.0.0";

        public static string GenerateRl(byte type)
        {
            string str = $"rl {type}";
            ServerManager.Instance.GroupList.ToList().ForEach(s =>
            {
                if (s.CharacterCount <= 0)
                {
                    return;
                }

                ClientSession leader = s.Characters.ElementAt(0);
                str += $" {s.Raid.Id}.{s.Raid?.LevelMinimum}.{s.Raid?.LevelMaximum}.{leader.Character.Name}.{leader.Character.Level}.{(leader.Character.UseSp ? leader.Character.Morph : -1)}.{(byte)leader.Character.Class}.{(byte)leader.Character.Gender}.{s.CharacterCount}.{leader.Character.HeroLevel}";
            });
            return str;
        }

        public static string GenerateRp(int mapid, int x, int y, string param) => $"rp {mapid} {x} {y} {param}";

        public static string GenerateSay(string message, int type, long callerId = 0) => $"say 1 {callerId} {type} {message}";

        public static string GenerateShopMemo(int type, string message) => $"s_memo {type} {message}";

        public static string GenerateTeamArenaClose() => "ta_close";

        public static string GenerateTeamArenaMenu(byte mode, byte zenasScore, byte ereniaScore, int time, byte arenaType) => $"ta_m {mode} {zenasScore} {ereniaScore} {time} {arenaType}";

        public static IEnumerable<string> GenerateVb() => new[] { "vb 340 0 0", "vb 339 0 0", "vb 472 0 0", "vb 471 0 0" };

        public string GenerateFStashRemove(short slot) => $"f_stash {GenerateRemovePacket(slot)}";

        public string GenerateInventoryRemove(InventoryType type, short slot) => $"ivn {(byte)type} {GenerateRemovePacket(slot)}";

        public string GeneratePStashRemove(short slot) => $"pstash {GenerateRemovePacket(slot)}";

        public string GenerateStashRemove(short slot) => $"stash {GenerateRemovePacket(slot)}";

        #endregion
    }
}