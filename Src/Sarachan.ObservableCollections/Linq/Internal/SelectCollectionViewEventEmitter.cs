using CommunityToolkit.HighPerformance.Buffers;
using System.Collections.Specialized;

namespace Sarachan.ObservableCollections.Linq.Internal
{
    class SelectCollectionViewEventEmitter<T, TView> : CollectionViewEventEmitterBase<T, TView>
    {
        public Func<T, TView> Selector { get; }

        public SelectCollectionViewEventEmitter(Func<T, TView> selector)
        {
            Selector = selector;
        }

        protected override void Emit(CollectionViewEventEmitterHelper<TView> emitHelper, NotifyCollectionChangedEventArgs<T> e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        using var spanOwner = CreateViewItems(e.NewItems);
                        emitHelper.Insert(e.NewStartingIndex, spanOwner.Span);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    emitHelper.RemoveAt(e.OldStartingIndex, e.OldItems.Length);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    emitHelper.Replace(e.NewStartingIndex, Selector(e.NewItems[0]));
                    break;
                case NotifyCollectionChangedAction.Move:
                    emitHelper.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        using var spanOwner = CreateViewItems(e.NewItems);
                        emitHelper.Reset(spanOwner.Span);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private SpanOwner<TView> CreateViewItems(ReadOnlySpan<T> items)
        {
            var spanOwner = SpanOwner<TView>.Allocate(items.Length);
            for (int i = 0; i < items.Length; i++)
            {
                spanOwner.Span[i] = Selector(items[i]);
            }
            return spanOwner;
        }
    }
}
