using System;
using System.Collections;
using System.Collections.Specialized;
using Sarachan.ObservableCollections.Linq.Internal;

namespace Sarachan.ObservableCollections.Linq
{

    public static class CollectionView
    {
        public interface IEventEmitter<T, TView>
        {
            void Emit(NotifyCollectionChangedEventArgs<T> e, NotifyCollectionChangedEventHandler<TView> handler);
        }

        public static ICollectionView<T, TView> BuildView<T, TView>(this IReadOnlyObservableCollection<T> self, 
            Func<IEventEmitter<T, T>, IEventEmitter<T, TView>> emitterBuilder)
        {
            var emitter = emitterBuilder(UnitCollectionViewEventEmitter<T>.Instance);
            var view = new CollectionView<T, TView>(self, emitter);
            return view;
        }

        public static IEventEmitter<T, TView> Combine<T, TRelay, TView>(this IEventEmitter<T, TRelay> self, IEventEmitter<TRelay, TView> other)
        {
            return new CombinedCollectionViewEventEmitter<T, TRelay, TView>(self, other);
        }

        public static IEventEmitter<T, TToView> Select<T, TFromView, TToView>(this IEventEmitter<T, TFromView> self, Func<TFromView, TToView> selector)
        {
            var selectEmitter = new SelectCollectionViewEventEmitter<TFromView, TToView>(selector);
            return self.Combine(selectEmitter);
        }

        public static IEventEmitter<T, TView> Where<T, TView>(this IEventEmitter<T, TView> self, Predicate<TView> predicate)
        {
            var whereEmitter = new WhereCollectionViewEventEmitter<TView>(predicate);
            return self.Combine(whereEmitter);
        }

        public static IEventEmitter<T, TView> OrderBy<T, TView>(this IEventEmitter<T, TView> self, Comparison<TView> comparison, bool reverse = false)
        {
            var actualComparison = comparison;
            if (reverse)
            {
                actualComparison = (x, y) => -comparison(x, y);
            }

            var orderByEmitter = new OrderByCollectionViewEventEmitter<TView>(actualComparison);
            return self.Combine(orderByEmitter);
        }

        public static IEventEmitter<T, TView> OrderBy<T, TView>(this IEventEmitter<T, TView> self, bool reverse = false)
        {
            var comparer = Comparer<TView>.Default;
            return OrderBy(self, comparer.Compare, reverse);
        }

        public static IEventEmitter<T, TView> Reverse<T, TView>(this IEventEmitter<T, TView> self)
        {
            var reverseEmitter = new ReverseCollectionViewEmitter<TView>();
            return self.Combine(reverseEmitter);
        }
    }
}
