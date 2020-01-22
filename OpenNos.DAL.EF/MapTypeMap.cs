﻿// This file is part of the OpenNos NosTale Emulator Project.
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenNos.DAL.EF
{
    public class MapTypeMap
    {
        #region Properties

        public virtual Map Map { get; set; }

        [Key, Column(Order = 0)]
        public short MapId { get; set; }

        public virtual MapType MapType { get; set; }

        [Key, Column(Order = 1)]
        public short MapTypeId { get; set; }

        #endregion
    }
}