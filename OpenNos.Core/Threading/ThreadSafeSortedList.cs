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
using System.Threading;
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedMember.Global

namespace OpenNos.Core.Threading
{
    /// <summary>
    /// This class is used to store key-value based items in a thread safe manner.
    /// </summary>
    /// <typeparam name="TK">Key type</typeparam>
    /// <typeparam name="TV">Value type</typeparam>
    public class ThreadSafeSortedList<TK, TV> : IDisposable
    {
        #region Members

        /// <summary>
        /// private collection to store _items.
        /// </summary>
        private readonly SortedList<TK, TV> _items;

        /// <summary>
        /// Used to synchronize access to _items list.
        /// </summary>
        private readonly ReaderWriterLockSlim _lock;

        private bool _disposed;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new ThreadSafeSortedList object.
        /// </summary>
        public ThreadSafeSortedList()
        {
            _items = new SortedList<TK, TV>();
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets count of items in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                if (!_disposed)
                {
                    _lock.EnterReadLock();
                    try
                    {
                        return _items.Count;
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                }
                return 0;
            }
        }

        #endregion

        #region Indexers

        /// <summary>
        /// Gets/adds/replaces an item by key.
        /// </summary>
        /// <param name="key">Key to get/set value</param>
        /// <returns>Item associated with this key</returns>
        public TV this[TK key]
        {
            get
            {
                if (!_disposed)
                {
                    _lock.EnterReadLock();
                    try
                    {
                        return _items.ContainsKey(key) ? _items[key] : default;
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                }
                return default;
            }

            set
            {
                if (!_disposed)
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        _items[key] = value;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether all elements of a sequence satisfy a condition.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns><see cref="bool"/> True; if elements satisgy the condition</returns>
        public bool All(Func<TV, bool> predicate)
        {
            if (_disposed)
            {
                return false;
            }

            _lock.EnterReadLock();
            try
            {
                return _items.Values.All(predicate);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns><see cref="bool"/></returns>
        public bool Any(Func<TV, bool> predicate)
        {
            if (_disposed)
            {
                return false;
            }

            _lock.EnterReadLock();
            try
            {
                return _items.Values.Any(predicate);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Removes all items from list.
        /// </summary>
        public void ClearAll()
        {
            if (_disposed)
            {
                return;
            }

            _lock.EnterWriteLock();
            try
            {
                _items.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Checks if collection contains spesified key.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns><see cref="bool"/> True; if collection contains given key</returns>
        public bool ContainsKey(TK key)
        {
            if (_disposed)
            {
                return false;
            }

            _lock.EnterReadLock();
            try
            {
                return _items.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Checks if collection contains spesified item.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns><see cref="bool"/> True; if collection contains given item</returns>
        public bool ContainsValue(TV item)
        {
            if (_disposed)
            {
                return false;
            }

            _lock.EnterReadLock();
            try
            {
                return _items.ContainsValue(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns a number that represents how many elements in the specified sequence satisfy a condition.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns><see cref="int"/> number of found elements</returns>
        public int CountLinq(Func<TV, bool> predicate)
        {
            if (_disposed)
            {
                return 0;
            }

            _lock.EnterReadLock();
            try
            {
                return _items.Values.Count(predicate);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Disposes the current object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns the first element of the sequence that satisfies a condition or a default value
        /// if no such element is found.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns><see cref="TV"/> object</returns>
        public TV FirstOrDefault(Func<TV, bool> predicate)
        {
            if (_disposed)
            {
                return default;
            }

            _lock.EnterReadLock();
            try
            {
                return _items.Values.FirstOrDefault(predicate);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Performs the specified action on each element of the <see cref="List{T}"/>.
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<TV> action)
        {
            if (_disposed)
            {
                return;
            }

            _lock.EnterReadLock();
            try
            {
                _items.Values.ToList().ForEach(action);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets all items in collection.
        /// </summary>
        /// <returns><see cref="List{TV}"/></returns>
        public List<TV> GetAllItems()
        {
            if (_disposed)
            {
                return new List<TV>();
            }

            _lock.EnterReadLock();
            try
            {
                return new List<TV>(_items.Values);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets then removes all items in collection.
        /// </summary>
        /// <returns><see cref="List{TV}"/></returns>
        public List<TV> GetAndClearAllItems()
        {
            if (_disposed)
            {
                return new List<TV>();
            }

            _lock.EnterWriteLock();
            try
            {
                List<TV> list = new List<TV>(_items.Values);
                _items.Clear();
                return list;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Returns the last element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns><see cref="TV"/> object</returns>
        public TV Last(Func<TV, bool> predicate)
        {
            if (!_disposed)
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.Last(predicate);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return default;
        }

        /// <summary>
        /// Returns the last element of a sequence.
        /// </summary>
        /// <returns><see cref="TV"/> object</returns>
        public TV Last()
        {
            if (!_disposed)
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.Last();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return default;
        }

        /// <summary>
        /// Returns the last element of a sequence that satisfies a condition or a default value if
        /// no such element is found.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns><see cref="TV"/> object</returns>
        public TV LastOrDefault(Func<TV, bool> predicate)
        {
            if (!_disposed)
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.LastOrDefault(predicate);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return default;
        }

        /// <summary>
        /// Returns the last element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <returns><see cref="TV"/> object</returns>
        public TV LastOrDefault()
        {
            if (!_disposed)
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.LastOrDefault();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return default;
        }

        /// <summary>
        /// Removes an item from collection.
        /// </summary>
        /// <param name="key">Key of item to remove</param>
        /// <returns><see cref="bool"/> if removed</returns>
        public bool Remove(TK key)
        {
            if (!_disposed)
            {
                _lock.EnterWriteLock();
                try
                {
                    if (!_items.ContainsKey(key))
                    {
                        return false;
                    }

                    _items.Remove(key);
                    return true;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            return false;
        }

        /// <summary>
        /// Removes an item from collection.
        /// </summary>
        /// <param name="value">Value of item to remove</param>
        /// <returns><see cref="bool"/> if removed</returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public bool Remove(TV value)
        {
            if (!_disposed)
            {
                _lock.EnterWriteLock();
                try
                {
                    if (!_items.ContainsValue(value))
                    {
                        return false;
                    }

                    _items.RemoveAt(_items.IndexOfValue(value));
                    return true;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            return false;
        }

        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        public IEnumerable<TResult> Select<TResult>(Func<TV, TResult> selector)
        {
            if (!_disposed)
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.Select(selector);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return default;
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, and throws
        /// an exception if more than one such element exists.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns><see cref="TV"/> object</returns>
        public TV Single(Func<TV, bool> predicate)
        {
            if (!_disposed)
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.Single(predicate);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return default;
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition or a default
        /// value if no such element exists; this method throws an exception if more than one element
        /// satisfies the condition.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns><see cref="TV"/> object</returns>
        public TV SingleOrDefault(Func<TV, bool> predicate)
        {
            if (!_disposed)
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.SingleOrDefault(predicate);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return default;
        }

        /// <summary>
        /// Returns a number that represents how many elements in the specified sequence satisfy a condition.
        /// </summary>
        /// <param name="selector"></param>
        /// <returns><see cref="int"/> number of found elements</returns>
        public int Sum(Func<TV, int> selector)
        {
            if (!_disposed)
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.Sum(selector);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns a number that represents how many elements in the specified sequence satisfy a condition.
        /// </summary>
        /// <param name="selector"></param>
        /// <returns>int? number of found elements</returns>
        public int? Sum(Func<TV, int?> selector)
        {
            if (!_disposed)
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.Sum(selector);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns a number that represents how many elements in the specified sequence satisfy a condition.
        /// </summary>
        /// <param name="selector"></param>
        /// <returns><see cref="long"/> number of found elements</returns>
        public long Sum(Func<TV, long> selector)
        {
            if (!_disposed)
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.Sum(selector);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns a number that represents how many elements in the specified sequence satisfy a condition.
        /// </summary>
        /// <param name="selector"></param>
        /// <returns>long? number of found elements</returns>
        public long? Sum(Func<TV, long?> selector)
        {
            if (!_disposed)
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.Sum(selector);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns a number that represents how many elements in the specified sequence satisfy a condition.
        /// </summary>
        /// <param name="selector"></param>
        /// <returns><see cref="double"/> number of found elements</returns>
        public double Sum(Func<TV, double> selector)
        {
            if (!_disposed)
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.Sum(selector);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns a number that represents how many elements in the specified sequence satisfy a condition.
        /// </summary>
        /// <param name="selector"></param>
        /// <returns>double? number of found elements</returns>
        public double? Sum(Func<TV, double?> selector)
        {
            if (!_disposed)
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Values.Sum(selector);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return 0;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns><see cref="List{TV}"/></returns>
        public List<TV> Where(Func<TV, bool> predicate)
        {
            if (!_disposed)
            {
                _lock.EnterReadLock();
                try
                {
                    return new List<TV>(_items.Values.Where(predicate));
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return new List<TV>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ClearAll();
                    _lock.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }
}