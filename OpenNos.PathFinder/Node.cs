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

namespace OpenNos.PathFinder
{
    public class Node : GridPos, IComparable<Node>, IEquatable<Node>
    {
        #region Instantiation

        public Node(GridPos node)
        {
            Value = node.Value;
            X = node.X;
            Y = node.Y;
        }

        public Node()
        {
        }

        #endregion

        #region Properties

        public bool Closed { get; internal set; }

        public double F { get; internal set; }

        public double N { get; internal set; }

        public bool Opened { get; internal set; }

        public Node Parent { get; internal set; }

        #endregion

        #region Methods

        public int CompareTo(Node other) => F > other.F ? 1 : F < other.F ? -1 : 0;

        public bool Equals(Node other) => ReferenceEquals(this, other);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }

            return false;
        }

        // ReSharper disable once FunctionRecursiveOnAllPaths
        public override int GetHashCode() => GetHashCode();

        public static bool operator ==(Node left, Node right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(Node left, Node right) => !(left == right);

        public static bool operator <(Node left, Node right) => left is null ? !(right is null) : left.CompareTo(right) < 0;

        public static bool operator <=(Node left, Node right) => left is null || left.CompareTo(right) <= 0;

        public static bool operator >(Node left, Node right) => !(left is null) && left.CompareTo(right) > 0;

        public static bool operator >=(Node left, Node right) => left is null ? right is null : left.CompareTo(right) >= 0;

        #endregion
    }
}