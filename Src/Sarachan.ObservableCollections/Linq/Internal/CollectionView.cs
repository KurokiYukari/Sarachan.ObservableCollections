using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using Sarachan.ObservableCollections.Miscs;

namespace Sarachan.ObservableCollections.Linq.Internal
{
    sealed class CollectionView<T, TView> : ICollectionView<T, TView>
    {
        sealed class EventUnit
        {
            public NotifyCollectionChangedEventHandler<TView> Handler { get; }
            private readonly CollectionView.IEventEmitter<T, TView> _emitter;

            public NotifyCollectionChangedEventHandler<T> OnEventHandler { get; }

            public EventUnit(NotifyCollectionChangedEventHandler<TView> handler, CollectionView.IEventEmitter<T, TView> emitter)
            {
                Handler = handler;
                _emitter = emitter;

                OnEventHandler = OnEvent;
            }

            private void OnEvent(object sender, NotifyCollectionChangedEventArgs<T> e)
            {
                _emitter.Emit(e, Handler);
            }
        }

        public sealed class EventSource : INotifyCollectionChanged<TView>, IDisposable
        {
            public IReadOnlyObservableCollection<T> Collection { get; }
            public CollectionView.IEventEmitter<T, TView> Emitter { get; }

            private readonly List<EventUnit> _eventUnits = new();
            public event NotifyCollectionChangedEventHandler<TView>? CollectionChanged
            {
                add
                {
                    if (value == null)
                    {
                        return;
                    }

                    var unit = new EventUnit(value, Emitter);
                    _eventUnits.Add(unit);
                    Collection.CollectionChanged += unit.OnEventHandler;
                }
                remove
                {
                    var index = CollectionUtils.IndexOf(_eventUnits, unit => unit.Handler == value);
                    if (index >= 0)
                    {
                        var unit = _eventUnits[index];
                        _eventUnits.RemoveAt(index);
                        Collection.CollectionChanged -= unit.OnEventHandler;
                    }
                }
            }

            public int EventUnitCount => _eventUnits.Count;

            public EventSource(IReadOnlyObservableCollection<T> collection,
                CollectionView.IEventEmitter<T, TView> emitter)
            {
                Collection = collection;
                Emitter = emitter;
            }

            public void Dispose()
            {
                foreach (var unit in _eventUnits)
                {
                    Collection.CollectionChanged -= unit.OnEventHandler;
                }
                _eventUnits.Clear();
            }
        }

        private readonly EventSource _eventSource;

        public IReadOnlyObservableCollection<T> Collection => _eventSource.Collection;

        private readonly ObservableList<TView> _storage = new();

        public int Count
        {
            get
            {
                TryRefresh();
                return _storage.Count;
            }
        }

        public int Version { get; private set; }

        public TView this[int index]
        {
            get
            {
                TryRefresh();
                return _storage[index];
            }
        }

        public event NotifyCollectionChangedEventHandler<TView>? CollectionChanged
        {
            add
            {
                if (value == null)
                {
                    return;
                }

                if (_eventSource.EventUnitCount == 0)
                {
                    _eventSource.CollectionChanged += OnCollectionChangedEmitted;
                    Refresh();
                }
                _eventSource.CollectionChanged += value;
            }
            remove
            {
                _eventSource.CollectionChanged -= value;
                if (_eventSource.EventUnitCount == 1)
                {
                    _eventSource.CollectionChanged -= OnCollectionChangedEmitted;
                    Debug.Assert(_eventSource.EventUnitCount == 0);
                }
            }
        }

        public CollectionView(IReadOnlyObservableCollection<T> collection,
            CollectionView.IEventEmitter<T, TView> emitter)
        {
            _eventSource = new EventSource(collection, emitter);
        }

        public void Refresh()
        {
            using var spanOwner = SpanOwner.Allocate(Collection);
            var e = NotifyCollectionChangedEventArgs<T>.Reset(spanOwner.Span, default);
            _eventSource.Emitter.Emit(e, OnCollectionChangedEmitted);
        }

        private void OnCollectionChangedEmitted(object sender, NotifyCollectionChangedEventArgs<TView> e)
        {
            // Thread safe?

            Version = Collection.Version;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _storage.Insert(e.NewStartingIndex, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _storage.RemoveAt(e.OldStartingIndex, e.OldItems.Length);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    _storage[e.NewStartingIndex] = e.NewItems[0];
                    break;
                case NotifyCollectionChangedAction.Move:
                    _storage.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _storage.Reset(e.NewItems);
                    break;
                default:
                    break;
            }
        }

        private void TryRefresh()
        {
            if (Version != Collection.Version)
            {
                Refresh();
            }
        }

        public IEnumerator<TView> GetEnumerator()
        {
            TryRefresh();
            return _storage.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            _eventSource.Dispose();
            _storage.Clear();
        }
    }
}
