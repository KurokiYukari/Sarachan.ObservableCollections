using System.Collections.Specialized;
using CommunityToolkit.HighPerformance.Buffers;

namespace Sarachan.ObservableCollections.Linq.Internal
{
    class ReverseCollectionViewEmitter<T> : CollectionViewEventEmitterBase<T, T>
    {
        protected override void Emit(CollectionViewEventEmitterHelper<T> emitHelper, NotifyCollectionChangedEventArgs<T> e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        using var reversedItems = BuildReversedSpan(e.NewItems);
                        var viewIndex = GetViewIndex(e.NewStartingIndex, emitHelper.Count) + 1;
                        emitHelper.Insert(viewIndex, reversedItems.Span);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        var viewIndex = GetViewIndex(e.OldStartingIndex, emitHelper.Count);
                        var length = e.OldItems.Length;
                        emitHelper.RemoveAt(viewIndex - length + 1, length);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        int viewIndex = GetViewIndex(e.NewStartingIndex, emitHelper.Count);
                        emitHelper.Replace(viewIndex, e.NewItems[0]);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        int oldViewIndex = GetViewIndex(e.OldStartingIndex, emitHelper.Count);
                        int newViewIndex = GetViewIndex(e.NewStartingIndex, emitHelper.Count);
                        emitHelper.Move(oldViewIndex, newViewIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        using var reversedItems = BuildReversedSpan(e.NewItems);
                        emitHelper.Reset(reversedItems.Span);
                    }
                    break;
                default:
                    break;
            }
        }

        private static SpanOwner<T> BuildReversedSpan(ReadOnlySpan<T> span)
        {
            var spanOwner = SpanOwner<T>.Allocate(span.Length);
            for (int i = 0; i < span.Length; i++)
            {
                spanOwner.Span[i] = span[^(i + 1)];
            }
            return spanOwner;
        }

        private static int GetViewIndex(int originalIndex, int count)
        {
            return count - 1 - originalIndex;
        }
    }
}
