using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Sarachan.ObservableCollections.Miscs;

namespace Sarachan.ObservableCollections
{
    public interface IReadOnlyObservableCollection<T> : INotifyCollectionChanged<T>, IReadOnlyCollection<T>
    {
        int Version { get; }
    }

    public interface IObservableCollection<T> : IReadOnlyObservableCollection<T>, ICollection<T>
    {
        new int Count { get; }

        void Reset(ReadOnlySpan<T> items);
    }

    public abstract class ObservableCollectionBase<T, TStorage> : 
        IObservableCollection<T>,
        INotifyPropertyChanged
        where TStorage : ICollection<T>, IReadOnlyCollection<T>
    {
        protected TStorage Storage { get; private set; }

        private int _version;
        public int Version => _version;
        public int Count => ((IReadOnlyCollection<T>)Storage).Count;
        public bool IsReadOnly => Storage.IsReadOnly;

        public event NotifyCollectionChangedEventHandler<T>? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollectionBase(TStorage storage)
        {
            Storage = storage;
        }

        public abstract void Add(T item);

        public abstract bool Remove(T item);

        public virtual void Clear()
        {
            if (Count == 0)
            {
                return;
            }

            using var oldItems = SpanOwner.Allocate(Storage);
            Storage.Clear();
            OnCollectionChanged(NotifyCollectionChangedEventArgs<T>.Reset(default, oldItems.Span));
        }

        public virtual void Reset(ReadOnlySpan<T> items)
        {
            using var oldItems = SpanOwner.Allocate(Storage);
            Storage.Clear();
            foreach (var item in items)
            {
                Storage.Add(item);
            }
            using var newItems = SpanOwner.Allocate(Storage);
            OnCollectionChanged(NotifyCollectionChangedEventArgs<T>.Reset(newItems.Span, oldItems.Span));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs<T> e)
        {
            Interlocked.Increment(ref _version);
            CollectionChanged?.Invoke(this, e);
            if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove || 
                e.Action is NotifyCollectionChangedAction.Reset && e.OldItems.Length == e.NewItems.Length)
            {
                OnCountPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        private static readonly PropertyChangedEventArgs _countPropertyChangedEventArgs = new(nameof(Count));
        protected virtual void OnCountPropertyChanged()
        {
            OnPropertyChanged(_countPropertyChangedEventArgs);
        }

        public bool Contains(T item)
        {
            return Storage.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Storage.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Storage.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
