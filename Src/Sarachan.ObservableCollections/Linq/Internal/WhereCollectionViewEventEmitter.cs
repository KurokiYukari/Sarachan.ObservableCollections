using System.Collections.Specialized;
using CommunityToolkit.HighPerformance.Buffers;

namespace Sarachan.ObservableCollections.Linq.Internal
{
    class WhereCollectionViewEventEmitter<T> : CollectionViewEventEmitterBase<T, T>
    {
        public Predicate<T> Filter { get; }

        private readonly List<int> _originalToNewIndexMap = new();

        public WhereCollectionViewEventEmitter(Predicate<T> filter) 
        {
            Filter = filter;
        }

        // TODO: Support Range Actions
        protected override void Emit(CollectionViewEventEmitterHelper<T> emitHelper, NotifyCollectionChangedEventArgs<T> e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var newIndex = e.NewStartingIndex;
                        foreach (var item in e.NewItems)
                        {
                            if (Filter?.Invoke(item) ?? true)
                            {
                                GetViewInsertIndex(emitHelper, newIndex, out var viewIndex);
                                InsertViewItem(emitHelper, newIndex, viewIndex, item);
                                _originalToNewIndexMap.Insert(newIndex, viewIndex);
                            }
                            else
                            {
                                _originalToNewIndexMap.Insert(newIndex, -1);
                            }

                            newIndex++;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        for (int i = 0; i < e.OldItems.Length; i++)
                        {
                            var originIndex = e.OldStartingIndex;
                            var viewIndex = _originalToNewIndexMap[originIndex];
                            _originalToNewIndexMap.RemoveAt(originIndex);
                            if (viewIndex >= 0)
                            {
                                RemoveViewItem(emitHelper, viewIndex);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        var replaceNewItem = e.NewItems[0];
                        bool oldExisted = GetViewInsertIndex(emitHelper, e.NewStartingIndex, out var replaceViewIndex);
                        bool needAddNew = Filter?.Invoke(replaceNewItem) ?? true;
                        if (oldExisted && needAddNew)
                        {
                            emitHelper.Replace(replaceViewIndex, replaceNewItem);
                        }
                        else if (oldExisted)
                        {
                            _originalToNewIndexMap[e.NewStartingIndex] = -1;
                            RemoveViewItem(emitHelper, replaceViewIndex);
                        }
                        else if (needAddNew)
                        {
                            InsertViewItem(emitHelper, e.NewStartingIndex, replaceViewIndex, replaceNewItem);
                            _originalToNewIndexMap[e.NewStartingIndex] = replaceViewIndex;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        var fromViewIndex = _originalToNewIndexMap[e.OldStartingIndex];
                        if (fromViewIndex >= 0)
                        {
                            bool toViewItemExisted = GetViewInsertIndex(emitHelper, e.NewStartingIndex, out var toViewIndex);
                            if (fromViewIndex < toViewIndex)
                            {
                                if (!toViewItemExisted)
                                {
                                    toViewIndex--;
                                }
                                for (int i = 0; i < _originalToNewIndexMap.Count; i++)
                                {
                                    var mappedIndex = _originalToNewIndexMap[i];
                                    if (mappedIndex >= fromViewIndex && mappedIndex <= toViewIndex)
                                    {
                                        _originalToNewIndexMap[i] = mappedIndex - 1;
                                    }
                                }
                            }
                            else if (fromViewIndex > toViewIndex)
                            {
                                for (int i = 0; i < _originalToNewIndexMap.Count; i++)
                                {
                                    var mappedIndex = _originalToNewIndexMap[i];
                                    if (mappedIndex >= toViewIndex && mappedIndex <= fromViewIndex)
                                    {
                                        _originalToNewIndexMap[i] = mappedIndex + 1;
                                    }
                                }
                            }

                            emitHelper.Move(fromViewIndex, toViewIndex);
                            _originalToNewIndexMap[e.OldStartingIndex] = toViewIndex;
                        }
                        CollectionUtils.Move(_originalToNewIndexMap, e.OldStartingIndex, e.NewStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        using var spanOwner = SpanOwner<T>.Allocate(e.NewItems.Length);

                        int count = 0;
                        _originalToNewIndexMap.Clear();

                        foreach (var item in e.NewItems)
                        {
                            if (Filter?.Invoke(item) ?? true)
                            {
                                _originalToNewIndexMap.Add(count);
                                spanOwner.Span[count] = item;
                                count++;
                            }
                            else
                            {
                                _originalToNewIndexMap.Add(-1);
                            }
                        }

                        emitHelper.Reset(spanOwner.Span[..count]);
                    }
                    break;
                default:
                    break;
            }
        }

        private void InsertViewItem(CollectionViewEventEmitterHelper<T> emitHelper, int newIndex, int viewIndex, T item)
        {
            for (int j = 0; j < _originalToNewIndexMap.Count; j++)
            {
                var mappedIndex = _originalToNewIndexMap[j];
                if (mappedIndex >= viewIndex)
                {
                    _originalToNewIndexMap[j] = mappedIndex + 1;
                }
            }
            emitHelper.Insert(viewIndex, new(in item));
        }

        private void RemoveViewItem(CollectionViewEventEmitterHelper<T> emitHelper, int viewIndex)
        {
            emitHelper.RemoveAt(viewIndex, 1);
            for (int j = 0; j < _originalToNewIndexMap.Count; j++)
            {
                var mappedIndex = _originalToNewIndexMap[j];
                if (mappedIndex > viewIndex)
                {
                    _originalToNewIndexMap[j] = mappedIndex - 1;
                }
            }
        }

        private bool GetViewInsertIndex(CollectionViewEventEmitterHelper<T> emitHelper, int originIndex, out int viewIndex)
        {
            if (originIndex >= _originalToNewIndexMap.Count)
            {
                viewIndex = emitHelper.Count;
                return false;
            }

            viewIndex = _originalToNewIndexMap[originIndex];
            if (viewIndex >= 0)
            {
                return true;
            }

            for (int i = originIndex - 1; i >= 0; i--)
            {
                viewIndex = _originalToNewIndexMap[i];
                if (viewIndex >= 0)
                {
                    viewIndex++;
                    return false;
                }
            }

            for (int i = originIndex + 1; i < _originalToNewIndexMap.Count; i++)
            {
                viewIndex = _originalToNewIndexMap[i];
                if (viewIndex >= 0)
                {
                    return false;
                }
            }

            viewIndex = 0;
            return false;
        }
    }
}
