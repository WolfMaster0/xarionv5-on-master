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
using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Helpers
{
    public class MateHelper
    {
        #region Instantiation

        private MateHelper()
        {
            LoadXpData();
            LoadPrimaryMpData();
            LoadSecondaryMpData();
            LoadHpData();
            LoadMateBuffs();
            LoadPetSkills();
            LoadPartnerSkills();
            LoadConcentrate();
            LoadMinDamageData();
            LoadMaxDamageData();
            LoadStats();
        }

        #endregion

        #region Members

        #endregion

        #region Properties

        public int[,] HitRateData { get; set; }

        public int[,] MeleeDefenseData { get; set; }

        public int[,] MeleeDefenseDodgeData { get; set; }

        public int[,] RangeDefenseData { get; set; }

        public int[,] RangeDefenseDodgeData { get; set; }

        public int[,] MagicDefenseData { get; set; }

        public short[,] Concentrate { get; set; }

        public short[,] MinDamageData { get; set; }

        public short[,] MaxDamageData { get; set; }

        public int[] HpData { get; private set; }

        // Race == 0
        public int[] PrimaryMpData { get; private set; }

        // Race == 2
        public int[] SecondaryMpData { get; private set; }

        public double[] XpData { get; private set; }

        public Dictionary<int, int> MateBuffs { get; set; }

        public List<int> PetSkills { get; set; }

        public List<short> PartnerSpBuffs { get; set; }

        #endregion

        #region Methods

        #region ListLoading

        private void LoadPetSkills()
        {
            PetSkills = new List<int>
            {
                1513, // Purcival 
                1514, // Baron scratch ?
                1515, // Amiral (le chat chelou) 
                1516, // roi des pirates pussifer 
                1524, // Miaou fou
                1575, // Marié Bouhmiaou 
                1576, // Marie Bouhmiaou 
                1601, // Mechamiaou
                1627 // Boris the polar bear
            };
        }

        private void LoadPartnerSkills()
        {
            PartnerSpBuffs = new List<short>
            {
                3000,
                3001,
                3002,
                3003,
                3004,
                3005,
                3006,
                3007,
                3008,
                3009,
                3010,
                3011,
                3012,
                3013,
                3014,
                3015,
                3016,
                3017,
                3018,
                3019,
                3020,
                3021,
                3022,
                3023,
                3024,
                3025,
                3026,
                3027,
                3028
            };
        }

        private void LoadMateBuffs()
        {
            MateBuffs = new Dictionary<int, int>
            {
                { 178, 108 }, // LUCKY PIG 
                { 670, 374 }, // FIBI 
                { 830, 377 }, // RUDY LOUBARD 
                { 836, 381 }, // PADBRA
                { 838, 385 }, // RATUFU NAVY 
                { 840, 442 }, // LEO LE LACHE 
                { 841, 394 }, // RATUFU NINJA 
                { 842, 399 }, // RATUFU INDIEN 
                { 843, 403 }, // RATUFU VIKING 
                { 844, 391 }, // RATUFU COWBOY 
                { 2105, 383 }, // INFERNO 
                { 671, 687 } // SMALL FIRE DEVIL
            };
        }

        #endregion

        #region Buffs

        public void RemovePartnerBuffs(ClientSession session, MateType mateType)
        {
            if (session == null || mateType == MateType.Pet)
            {
                return;
            }

            foreach (var val in PartnerSpBuffs)
            {
                session.Character.RemoveBuff(val, true);
            }
        }

        public void AddPartnerBuffs(ClientSession session, Mate mate)
        {
            if (session == null || mate == null || mate.MateType == MateType.Pet || mate.SpInstance == null || !mate.IsUsingSp)
            {
                return;
            }

            var skillList = ServerManager.Instance.PartnerSkills.FirstOrDefault(s => s.PartnerVnum == mate.SpInstance.ItemVNum);

            if (skillList == null || skillList.SpecialBuffId <= 0)
            {
                return;
            }

            var sum = ((byte)mate.SpInstance.FirstPartnerSkillRank + (byte)mate.SpInstance.SecondPartnerSkillRank + (byte)mate.SpInstance.ThirdPartnerSkillRank) / 3;
            if (sum < 1)
            {
                sum = 1;
            }
            session.Character.AddBuff(new Buff((short)(skillList.SpecialBuffId + (sum - 1)), isPermaBuff: true));
        }

        public void AddPetBuff(ClientSession session, Mate mate)
        {
            if (session == null || mate == null)
            {
                return;
            }

            if (MateBuffs.TryGetValue(mate.NpcMonsterVNum, out var cardId) &&
                session.Character.Buff.All(b => b.Card.CardId != cardId))
            {
                session.Character.AddBuff(new Buff((short)cardId, isPermaBuff: true));
            }

            if (mate.MateType != MateType.Pet)
            {
                return;
            }

            foreach (NpcMonsterSkill skill in mate.Monster.Skills.Where(sk => PetSkills.Contains(sk.SkillVNum)))
            {
                session.SendPacket(session.Character.GeneratePetSkill(skill.SkillVNum));
            }
        }

        public void RemovePetBuffs(ClientSession session, MateType mateType)
        {
            if (session == null || mateType == MateType.Partner)
            {
                return;
            }

            foreach (Buff mateBuff in session.Character.Buff.Where(b =>
                MateBuffs.Values.Any(v => v == b.Card.CardId)))
            {
                session.Character.RemoveBuff(mateBuff.Card.CardId, true);
            }

            session.SendPacket(session.Character.GeneratePetSkill());
        }

        #endregion

        #region Stats

        private void LoadConcentrate()
        {
            Concentrate = new short[2, 256];

            short baseConcentrate = 27;
            short baseUp = 6;

            Concentrate[0, 0] = baseConcentrate;

            for (var i = 1; i < Concentrate.GetLength(1); i++)
            {
                Concentrate[0, i] = baseConcentrate;
                baseConcentrate += (short)((i % 5) == 2 ? 5 : baseUp);
            }

            baseConcentrate = 70;

            Concentrate[1, 0] = baseConcentrate;

            for (var i = 1; i < Concentrate.GetLength(1); i++)
            {
                Concentrate[1, i] = baseConcentrate;
            }
        }

        private void LoadMinDamageData()
        {
            MinDamageData = new short[2, 256];

            short baseDamage = 37;
            short baseUp = 4;

            MinDamageData[0, 0] = baseDamage;

            for (var i = 1; i < MinDamageData.GetLength(1); i++)
            {
                MinDamageData[0, i] = baseDamage;
                baseDamage += (short)((i % 5) == 0 ? 5 : baseUp);
            }

            baseDamage = 23;
            baseUp = 6;

            MinDamageData[1, 0] = baseDamage;

            for (var i = 1; i < MinDamageData.GetLength(1); i++)
            {
                MinDamageData[1, i] = baseDamage;
                baseDamage += (short)((i % 5) == 0 ? 5 : baseUp);
                baseDamage += (short)((i % 2) == 0 ? 1 : 0);
            }
        }

        private void LoadMaxDamageData()
        {
            MaxDamageData = new short[2, 256];

            short baseDamage = 40;
            short baseUp = 6;

            MaxDamageData[0, 0] = baseDamage;

            for (var i = 1; i < MaxDamageData.GetLength(1); i++)
            {
                MaxDamageData[0, i] = baseDamage;
                baseDamage += (short)((i % 5) == 0 ? 5 : baseUp);
            }

            MaxDamageData[1, 0] = baseDamage;

            baseDamage = 38;
            baseUp = 8;

            for (var i = 1; i < MaxDamageData.GetLength(1); i++)
            {
                MaxDamageData[1, i] = baseDamage;
                baseDamage += (short)((i % 5) == 0 ? 5 : baseUp);
            }
        }

        private void LoadPrimaryMpData()
        {
            PrimaryMpData = new int[256];
            PrimaryMpData[0] = 10;
            PrimaryMpData[1] = 10;
            PrimaryMpData[2] = 15;

            var baseUp = 5;
            byte count = 0;
            var isStable = true;
            var isDouble = false;

            for (var i = 3; i < PrimaryMpData.Length; i++)
            {
                if ((i % 10) == 1)
                {
                    PrimaryMpData[i] += PrimaryMpData[i - 1] + baseUp * 2;
                    continue;
                }

                if (!isStable)
                {
                    baseUp++;
                    count++;

                    if (count == 2)
                    {
                        if (isDouble)
                        {
                            isDouble = false;
                        }
                        else
                        {
                            isStable = true;
                            isDouble = true;
                            count = 0;
                        }
                    }

                    if (count == 4)
                    {
                        isStable = true;
                        count = 0;
                    }
                }
                else
                {
                    count++;
                    if (count == 2)
                    {
                        isStable = false;
                        count = 0;
                    }
                }

                PrimaryMpData[i] = PrimaryMpData[i - ((i % 10) == 2 ? 2 : 1)] + baseUp;
            }
        }

        private void LoadSecondaryMpData()
        {
            SecondaryMpData = new int[256];
            SecondaryMpData[0] = 60;
            SecondaryMpData[1] = 60;
            SecondaryMpData[2] = 78;

            var baseUp = 18;
            var boostUp = false;

            for (var i = 3; i < SecondaryMpData.Length; i++)
            {
                if ((i % 10) == 1)
                {
                    SecondaryMpData[i] += SecondaryMpData[i - 1] + i + 10;
                    continue;
                }

                if (boostUp)
                {
                    baseUp += 3;
                    boostUp = false;
                }
                else
                {
                    baseUp++;
                    boostUp = true;
                }

                SecondaryMpData[i] = SecondaryMpData[i - ((i % 10) == 2 ? 2 : 1)] + baseUp;
            }
        }

        private void LoadHpData()
        {
            HpData = new int[256];
            var baseHp = 150;
            var hpBaseUp = 40;
            for (var i = 0; i < HpData.Length; i++)
            {
                HpData[i] = baseHp;
                hpBaseUp += 5;
                baseHp += hpBaseUp;
            }
        }

        private void LoadStats()
        {
            HitRateData = new int[4, 256];
            MeleeDefenseData = new int[4, 256];
            MeleeDefenseDodgeData = new int[4, 256];
            RangeDefenseData = new int[4, 256];
            RangeDefenseDodgeData = new int[4, 256];
            MagicDefenseData = new int[4, 256];

            for (var i = 0; i < 256; i++)
            {
                // Default(0)
                HitRateData[0, i] = i + 9; // approx
                MeleeDefenseData[0, i] = i + (9 / 2); // approx
                MeleeDefenseDodgeData[0, i] = i + 9; // approx
                RangeDefenseData[0, i] = (i + 9) / 2; // approx
                RangeDefenseDodgeData[0, i] = i + 9; // approx
                MagicDefenseData[0, i] = (i + 9) / 2; // approx

                // Melee-Type Premium Pets
                MeleeDefenseDodgeData[1, i] = i + 12; // approx
                RangeDefenseDodgeData[1, i] = i + 12; // approx
                MagicDefenseData[1, i] = (i + 9) / 2; // approx
                HitRateData[1, i] = i + 27; // approx
                MeleeDefenseData[1, i] = i + 2; // approx

                RangeDefenseData[1, i] = i; // approx

                // Magic-Type Premium Pets
                HitRateData[2, i] = 0; // sure
                MeleeDefenseData[2, i] = (i + 11) / 2; // approx
                MagicDefenseData[2, i] = i + 4; // approx
                MeleeDefenseDodgeData[2, i] = 24 + i; // approx
                RangeDefenseDodgeData[2, i] = 14 + i; // approx
                RangeDefenseData[2, i] = 20 + i; // approx

                // Range-Type Premium Pets
                HitRateData[3, 1] = 41 + (i % 2 == 0 ? 2 : 4);
                MeleeDefenseData[3, i] = i; // approx
                MagicDefenseData[3, i] = i + 2; // approx
                MeleeDefenseDodgeData[3, i] = 41 + i; // approx
                RangeDefenseDodgeData[3, i] = i + 2; // approx
                RangeDefenseData[3, i] = i; // approx
            }
        }


        private void LoadXpData()
        {
            // Load XpData
            XpData = new double[256];
            double[] v = new double[256];
            double var = 1;
            v[0] = 540;
            v[1] = 960;
            XpData[0] = 300;
            for (var i = 2; i < v.Length; i++)
            {
                v[i] = v[i - 1] + 420 + (120 * (i - 1));
            }
            for (var i = 1; i < XpData.Length; i++)
            {
                if (i < 79)
                {
                    switch (i)
                    {
                        case 14:
                            var = 6 / 3d;
                            break;
                        case 39:
                            var = 19 / 3d;
                            break;
                        case 59:
                            var = 70 / 3d;
                            break;
                    }
                    XpData[i] = Convert.ToInt64(XpData[i - 1] + (var * v[i - 1]));
                }
                if (i < 79)
                {
                    continue;
                }
                switch (i)
                {
                    case 79:
                        var = 5000;
                        break;
                    case 82:
                        var = 9000;
                        break;
                    case 84:
                        var = 13000;
                        break;
                }
                XpData[i] = Convert.ToInt64(XpData[i - 1] + (var * (i + 2) * (i + 2)));
            }
        }

        #endregion

        #endregion

        #region Singleton

        private static MateHelper _instance;

        public static MateHelper Instance => _instance ?? (_instance = new MateHelper());

        #endregion
    }
}