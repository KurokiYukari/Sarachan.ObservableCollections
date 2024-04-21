using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sarachan.ObservableCollections
{
    /// <summary>
    /// A dictionary object that allows rapid hash lookups using keys, but also
    /// maintains the key insertion order so that values can be retrieved by
    /// key index.
    /// </summary>
    public class OrderedDictionary<TKey, TValue> : 
        IDictionary<TKey, TValue>,
        IOrderedDictionary,
        IList<KeyValuePair<TKey, TValue>>, 
        IReadOnlyList<KeyValuePair<TKey, TValue>>
        where TKey : notnull
    {

        #region Fields/Properties

        private readonly StorageCollection _keyedCollection;

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key associated with the value to get or set.</param>
        public TValue this[TKey key]
        {
            get => GetValue(key);
            set => SetValue(key, value);
        }

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="index">The index of the value to get or set.</param>
        public KeyValuePair<TKey, TValue> this[int index]
        {
            get => _keyedCollection[index];
            set => _keyedCollection[index] = value;
        }

        public int Count => _keyedCollection.Count;

        public ICollection<TKey> Keys => _keyedCollection.Select(x => x.Key).ToList();

        public ICollection<TValue> Values => _keyedCollection.Select(x => x.Value).ToList();

        public IEqualityComparer<TKey> Comparer => _keyedCollection.Comparer;

        #endregion

        #region Constructors
        public OrderedDictionary() : this(EqualityComparer<TKey>.Default)
        {
        }

        public OrderedDictionary(IEqualityComparer<TKey> comparer)
        {
            _keyedCollection = new StorageCollection(comparer);
        }
        #endregion

        #region Methods

        public void Add(TKey key, TValue value)
        {
            _keyedCollection.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public void Clear()
        {
            _keyedCollection.Clear();
        }

        public void Insert(int index, TKey key, TValue value)
        {
            Insert(index, new KeyValuePair<TKey, TValue>(key, value));
        }

        public void Insert(int index, KeyValuePair<TKey, TValue> value)
        {
            _keyedCollection.Insert(index, value);
        }

        public int IndexOf(TKey key)
        {
            if (_keyedCollection.Contains(key))
            {
                return _keyedCollection.IndexOf(_keyedCollection[key]);
            }
            else
            {
                return -1;
            }
        }

        public int IndexOf(KeyValuePair<TKey, TValue> value)
        {
            return _keyedCollection.IndexOf(value);
        }

        public bool ContainsKey(TKey key)
        {
            return _keyedCollection.Contains(key);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            if (_keyedCollection.TryGetValue(key, out var pair))
            {
                value = pair.Value;
                return true;
            }

            value = default;
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _keyedCollection.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            return _keyedCollection.Remove(key);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _keyedCollection.Count)
            {
                throw new IndexOutOfRangeException($"The index was outside the bounds of the dictionary: {index}");
            }
            _keyedCollection.RemoveAt(index);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key associated with the value to get.</param>
        public TValue GetValue(TKey key)
        {
            if (!_keyedCollection.Contains(key))
            {
                throw new ArgumentException($"The given key is not present in the dictionary: {key}");
            }
            var kvp = _keyedCollection[key];
            return kvp.Value;
        }

        /// <summary>
        /// Sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key associated with the value to set.</param>
        /// <param name="value">The the value to set.</param>
        public void SetValue(TKey key, TValue value)
        {
            var kvp = new KeyValuePair<TKey, TValue>(key, value);
            var idx = IndexOf(key);
            if (idx > -1)
            {
                _keyedCollection[idx] = kvp;
            }
            else
            {
                _keyedCollection.Add(kvp);
            }
        }
        #endregion

        #region sorting
        public void SortKeys()
        {
            SortKeys(Comparer<TKey>.Default.Compare);
        }

        public void SortKeys(Comparison<TKey> comparison)
        {
            _keyedCollection.Sort((x, y) => comparison(x.Key, y.Key));
        }

        public void SortValues()
        {
            SortValues(Comparer<TValue>.Default.Compare);
        }

        public void SortValues(Comparison<TValue> comparison)
        {
            _keyedCollection.Sort((x, y) => comparison(x.Value, y.Value));
        }
        #endregion

        #region ICollection<KeyValuePair<TKey, TValue>>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            _keyedCollection.Add(item);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return _keyedCollection.Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _keyedCollection.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return _keyedCollection.Remove(item);
        }
        #endregion

        #region IEnumerable
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IOrderedDictionary
        object? IOrderedDictionary.this[int index]
        {
            get => this[index].Value;
            set => this[index] = KeyValuePair.Create(this[index].Key, (TValue)value!);
        }

        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator(this);
        }

        void IOrderedDictionary.Insert(int index, object key, object? value)
        {
            Insert(index, (TKey)key, (TValue)value!);
        }
        #endregion

        #region IDictionary
        bool IDictionary.IsFixedSize => false;
        bool IDictionary.IsReadOnly => false;
        ICollection IDictionary.Keys => (ICollection)Keys;
        ICollection IDictionary.Values => (ICollection)Values;

        object? IDictionary.this[object key]
        {
            get => this[(TKey)key];
            set => this[(TKey)key] = (TValue)value!;
        }

        void IDictionary.Add(object key, object? value)
        {
            Add((TKey)key, (TValue)value!);
        }

        void IDictionary.Remove(object key)
        {
            Remove((TKey)key);
        }

        bool IDictionary.Contains(object key)
        {
            return _keyedCollection.Contains((TKey)key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator(this);
        }
        #endregion

        #region ICollection
        bool ICollection.IsSynchronized => ((ICollection)_keyedCollection).IsSynchronized;
        object ICollection.SyncRoot => ((ICollection)_keyedCollection).SyncRoot;

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_keyedCollection).CopyTo(array, index);
        }
        #endregion

        sealed class StorageCollection : KeyedCollection<TKey, KeyValuePair<TKey, TValue>>
        {
            public StorageCollection(IEqualityComparer<TKey> comparer)
                : base(comparer)
            {
            }

            protected override TKey GetKeyForItem(KeyValuePair<TKey, TValue> item)
            {
                return item.Key;
            }

            public void Sort()
            {
                var comparer = Comparer<KeyValuePair<TKey, TValue>>.Default;
                Sort(comparer.Compare);
            }

            public void Sort(Comparison<KeyValuePair<TKey, TValue>> comparison)
            {
                var list = (List<KeyValuePair<TKey, TValue>>)Items;
                list.Sort(comparison);
            }
        }

        sealed class DictionaryEnumerator : IDictionaryEnumerator, IDisposable
        {
            readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

            public DictionaryEnumerator(IDictionary<TKey, TValue> value)
            {
                _enumerator = value.GetEnumerator();
            }
            public void Reset() { _enumerator.Reset(); }
            public bool MoveNext() { return _enumerator.MoveNext(); }
            public DictionaryEntry Entry
            {
                get
                {
                    var pair = _enumerator.Current;
                    return new DictionaryEntry(pair.Key, pair.Value);
                }
            }
            public object Key { get { return _enumerator.Current.Key; } }
            public object? Value { get { return _enumerator.Current.Value; } }
            public object Current { get { return Entry; } }

            public void Dispose() { _enumerator.Dispose(); }
        }
    }
}
