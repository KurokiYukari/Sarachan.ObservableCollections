using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sarachan.ObservableCollections
{
    public class ObservableOrderedDictionary<TKey, TValue> : 
        ObservableListBase<KeyValuePair<TKey, TValue>, OrderedDictionary<TKey, TValue>>,
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        public ObservableOrderedDictionary(IEqualityComparer<TKey> comparer) : base(new OrderedDictionary<TKey, TValue>(comparer))
        {
        }

        public ObservableOrderedDictionary() : this(EqualityComparer<TKey>.Default)
        {
        }

        public TValue this[TKey key]
        {
            get => Storage[key];
            set
            {
                var index = IndexOf(key);
                if (index >= 0)
                {
                    this[index] = KeyValuePair.Create(key, value);
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public ICollection<TKey> Keys => Storage.Keys;

        public ICollection<TValue> Values => Storage.Values;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public virtual void Add(TKey key, TValue value)
        {
            var index = Count;
            Storage.Add(key, value);
            OnCollectionChanged(NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Add(KeyValuePair.Create(key, value), index));
        }

        public virtual bool Remove(TKey key)
        {
            var index = IndexOf(key);
            if (index < 0)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return Storage.TryGetValue(key, out value);
        }

        public bool ContainsKey(TKey key)
        {
            return Storage.ContainsKey(key);
        }

        public int IndexOf(TKey key)
        {
            return Storage.IndexOf(key);
        }
    }
}
