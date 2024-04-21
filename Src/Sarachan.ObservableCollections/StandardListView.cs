using System.Collections;
using System.Collections.Specialized;

namespace Sarachan.ObservableCollections
{

    public interface IStandardListView<T> : IReadOnlyList<T>, INotifyCollectionChanged
    {
        IReadOnlyObservableList<T> Collection { get; }
    }

    class StandardListView<T, TList> : IStandardListView<T> where TList : IReadOnlyObservableList<T>
    {
        IReadOnlyObservableList<T> IStandardListView<T>.Collection => Collection;
        public TList Collection { get; }

        private readonly bool _supportRangeAction;

        public int Count => Collection.Count;

        public T this[int index] => Collection[index];

        class EventUnit
        {
            public object? Sender { get; }
            public NotifyCollectionChangedEventHandler Handler { get; }
            public bool SupportRangeAction { get; }
            public NotifyCollectionChangedEventHandler<T> OnEventHandler { get; }

            public EventUnit(object sender, NotifyCollectionChangedEventHandler handler, bool supportRangeAction)
            {
                Sender = sender;
                Handler = handler;
                SupportRangeAction = supportRangeAction;
                OnEventHandler = OnEvent;
            }

            private void OnEvent(object sender, NotifyCollectionChangedEventArgs<T> e)
            {
                e.RelayStandardEventArgs(SupportRangeAction, Sender, Handler);
            }
        }

        private readonly List<EventUnit> _eventUnits = new();

        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add
            {
                if (value == null)
                {
                    return;
                }
                var unit = new EventUnit(this, value, _supportRangeAction);
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

        public StandardListView(TList collection, bool supportRangeAction)
        {
            Collection = collection;
            _supportRangeAction = supportRangeAction;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
