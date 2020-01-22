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
using System;

namespace OpenNos.Data
{
    [Serializable]
    public class FamilyDTO
    {
        #region Properties

        public int FamilyExperience { get; set; }

        public GenderType FamilyHeadGender { get; set; }

        public long FamilyId { get; set; }

        public byte FamilyLevel { get; set; }

        public string FamilyMessage { get; set; }

        public long LastFactionChange { get; set; }

        public FamilyAuthorityType ManagerAuthorityType { get; set; }

        public bool ManagerCanGetHistory { get; set; }

        public bool ManagerCanInvite { get; set; }

        public bool ManagerCanNotice { get; set; }

        public bool ManagerCanShout { get; set; }

        public short MaxSize { get; set; }

        public FamilyAuthorityType MemberAuthorityType { get; set; }

        public bool MemberCanGetHistory { get; set; }

        public string Name { get; set; }

        public byte WarehouseSize { get; set; }

        #endregion
    }
}