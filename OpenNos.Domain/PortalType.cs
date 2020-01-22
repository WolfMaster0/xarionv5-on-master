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
namespace OpenNos.Domain
{
    public enum PortalType : sbyte
    {
        MapPortal = -1,
        TsNormal = 0, // same over >127 - sbyte
        Closed = 1,
        Open = 2,
        Miniland = 3,
        TsEnd = 4,
        TsEndClosed = 5,
        Exit = 6,
        ExitClosed = 7,
        Raid = 8,
        Effect = 9, // same as 13 - 19 and 20 - 126
        BlueRaid = 10,
        DarkRaid = 11,
        TimeSpace = 12,
        ShopTeleport = 20
    }
}