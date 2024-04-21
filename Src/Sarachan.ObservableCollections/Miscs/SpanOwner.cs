using System.Buffers;
using CommunityToolkit.Diagnostics;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance.Buffers;

namespace Sarachan.ObservableCollections.Miscs
{
    public static class SpanOwner
    {
        public static SpanOwner<T> Allocate<T>(IEnumerable<T> items, out int length, ArrayPool<T>? pool = null)
        {
            pool ??= ArrayPool<T>.Shared; 
            SpanOwner<T> owner;

            int index = 0;
            if (items.TryGetNonEnumeratedCount(out length))
            {
                owner = SpanOwner<T>.Allocate(length, pool);
                foreach (var item in items)
                {
                    owner.Span[index++] = item;
                }
            }
            else
            {
                owner = SpanOwner<T>.Allocate(16, pool);
                foreach (var item in items)
                {
                    if (index >= owner.Span.Length)
                    {
                        var size = owner.Span.Length * 2;
                        var oldOwner = owner;
                        owner = SpanOwner<T>.Allocate(size, pool);
                        oldOwner.Span.CopyTo(owner.Span);
                        oldOwner.Dispose();
                    }

                    owner.Span[index++] = item;
                }
                length = index;
            }

            return owner;
        }

        public static SpanOwner<T> Allocate<T>(IReadOnlyList<T> list, int index, int length, ArrayPool<T>? pool = null)
        {
            var owner = SpanOwner<T>.Allocate(length, pool ?? ArrayPool<T>.Shared);
            for (int i = 0; i < length; i++)
            {
                owner.Span[i] = list[i + index];
            }
            return owner;
        }

        public static SpanOwner<T> Allocate<T>(IReadOnlyCollection<T> collection, ArrayPool<T>? pool = null)
        {
            var owner = SpanOwner<T>.Allocate(collection.Count, pool ?? ArrayPool<T>.Shared);
            int index = 0;
            foreach (var item in collection)
            {
                owner.Span[index++] = item;
            }
            return owner;
        }
    }
}
