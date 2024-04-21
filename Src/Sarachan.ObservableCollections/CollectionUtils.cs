using CommunityToolkit.Diagnostics;

namespace Sarachan.ObservableCollections
{
    public static class CollectionUtils
    {
        public static void Resize<T>(IList<T> list, int count)
        {
            Guard.IsGreaterThanOrEqualTo(count, 0);

            var listCount = list.Count;

            if (count > listCount)
            {
                for (int i = 0; i < count - listCount; i++)
                {
                    list.Add(default!);
                }
            }
            else
            {
                for (int i = 0; i < listCount - count; i++)
                {
                    list.RemoveAt(listCount - i - 1);
                }
            }
        }

        public static void Move<T>(IList<T> list, int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
            {
                return;
            }

            Guard.IsInRangeFor(fromIndex, list);
            Guard.IsInRangeFor(toIndex, list);

            var movedItem = list[fromIndex];
            if (fromIndex > toIndex)
            {
                for (int i = fromIndex - 1; i >= toIndex; i--)
                {
                    list[i + 1] = list[i];
                }
            }
            else
            {
                for (int i = fromIndex + 1; i <= toIndex; i++)
                {
                    list[i - 1] = list[i];
                }
            }

            list[toIndex] = movedItem;
        }

        public static void RemoveRange<T>(IList<T> list, int fromIndex, int length)
        {
            var maxIndex = fromIndex + length;

            Guard.IsInRangeFor(fromIndex, list);
            Guard.HasSizeGreaterThanOrEqualTo(list, maxIndex);

            if (length == 0)
            {
                return;
            }

            for (int i = 0; i + maxIndex < list.Count; i++)
            {
                list[fromIndex + i] = list[maxIndex + i];
            }

            Resize(list, list.Count - length);
        }

        public static void AddRange<T>(IList<T> list, ReadOnlySpan<T> items, int index)
        {
            Guard.IsBetweenOrEqualTo(index, 0, list.Count);

            var oldCount = list.Count;
            var moveItemsCount = oldCount - index;

            Resize(list, list.Count + items.Length);

            for (int i = 0; i < moveItemsCount; i++)
            {
                list[^(i + 1)] = list[oldCount - 1 - i];
            }
            for (int i = 0; i < items.Length; i++)
            {
                list[index + i] = items[i];
            }
        }

        public static int IndexOf<T>(IReadOnlyList<T> list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (predicate(item))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
