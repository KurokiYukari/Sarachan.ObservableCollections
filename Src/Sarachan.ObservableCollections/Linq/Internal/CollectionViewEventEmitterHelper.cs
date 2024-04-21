using System.Collections;
using CommunityToolkit.Diagnostics;
using Sarachan.ObservableCollections.Miscs;

namespace Sarachan.ObservableCollections.Linq.Internal
{
    public class CollectionViewEventEmitterHelper<T>
    {
        private readonly DummyObservableList _dummyList = new();

        private bool _handlerRegistered;

        public int Count => _dummyList.Count;

        public readonly ref struct EmitScope
        {
            private readonly CollectionViewEventEmitterHelper<T> _helper;
            private readonly NotifyCollectionChangedEventHandler<T> _handler;

            public EmitScope(CollectionViewEventEmitterHelper<T> helper, NotifyCollectionChangedEventHandler<T> handler)
            {
                _helper = helper;
                _handler = handler;

                Guard.IsFalse(_helper._handlerRegistered);

                _helper._handlerRegistered = true;
                _helper._dummyList.CollectionChanged += handler;
            }

            public void Dispose()
            {
                Guard.IsTrue(_helper._handlerRegistered);

                _helper._dummyList.CollectionChanged -= _handler;
                _helper._handlerRegistered = false;
            }
        }

        public EmitScope BeginEmitScope(NotifyCollectionChangedEventHandler<T> handler)
        {
            return new EmitScope(this, handler);
        }

        private void CheckHandlerRegistered()
        {
            Guard.IsTrue(_handlerRegistered);
        }

        public void Insert(int index, ReadOnlySpan<T> items)
        {
            CheckHandlerRegistered();
            _dummyList.Insert(index, items);
        }

        public void RemoveAt(int index, int length)
        {
            CheckHandlerRegistered();
            _dummyList.RemoveAt(index, length);
        }

        public void Move(int fromIndex, int toIndex)
        {
            CheckHandlerRegistered();
            _dummyList.Move(fromIndex, toIndex);
        }

        public void Replace(int index, T item)
        {
            CheckHandlerRegistered();
            _dummyList[index] = item;
        }

        public void Reset(ReadOnlySpan<T> items)
        {
            CheckHandlerRegistered();
            _dummyList.Reset(items);
        }

        class DummyObservableList : ObservableListBase<T, CollectionViewEventEmitterHelper<T>.DummyList>
        {
            public DummyObservableList() : base(new())
            {
            }

            public override void Reset(ReadOnlySpan<T> items)
            {
                using var oldItems = SpanOwner.Allocate(Storage);
                Storage.Clear();
                foreach (var item in items)
                {
                    Storage.Add(item);
                }
                OnCollectionChanged(NotifyCollectionChangedEventArgs<T>.Reset(items, oldItems.Span));
            }
        }

        class DummyList : IList<T>, IReadOnlyList<T>
        {
            public T this[int index]
            {
                get => default!;
                set { }
            }

            public int Count { get; private set; }

            public bool IsReadOnly => false;

            public void Add(T item)
            {
                Count++;
            }

            public void Clear()
            {
                Count = 0;
            }

            public bool Contains(T item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int IndexOf(T item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, T item)
            {
                Count++;
            }

            public bool Remove(T item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                Count--;
            }

            public IEnumerator<T> GetEnumerator()
            {
                var count = Count;
                for (int i = 0; i < count; i++)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
