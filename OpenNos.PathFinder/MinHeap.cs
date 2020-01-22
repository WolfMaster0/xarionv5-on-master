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
using System.Collections.Generic;

namespace OpenNos.PathFinder
{
    internal class MinHeap
    {
        #region Members

        private readonly List<Node> _array = new List<Node>();

        #endregion

        #region Properties

        public int Count => _array.Count;

        #endregion

        #region Methods

        public Node Pop()
        {
            Node ret = _array[0];
            _array[0] = _array[_array.Count - 1];
            _array.RemoveAt(_array.Count - 1);

            int len = 0;
            while (len < _array.Count)
            {
                int min = len;
                if ((2 * len) + 1 < _array.Count && _array[(2 * len) + 1].CompareTo(_array[min]) == -1)
                {
                    min = (2 * len) + 1;
                }
                if ((2 * len) + 2 < _array.Count && _array[(2 * len) + 2].CompareTo(_array[min]) == -1)
                {
                    min = (2 * len) + 2;
                }

                if (min == len)
                {
                    break;
                }
                Node tmp = _array[len];
                _array[len] = _array[min];
                _array[min] = tmp;
                len = min;
            }

            return ret;
        }

        public void Push(Node element)
        {
            _array.Add(element);
            int len = _array.Count - 1;
            int parent = (len - 1) >> 1;
            while (len > 0 && _array[len].CompareTo(_array[parent]) < 0)
            {
                Node tmp = _array[len];
                _array[len] = _array[parent];
                _array[parent] = tmp;
                len = parent;
                parent = (len - 1) >> 1;
            }
        }

        #endregion
    }
}