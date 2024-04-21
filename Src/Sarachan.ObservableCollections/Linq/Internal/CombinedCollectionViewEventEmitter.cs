using CommunityToolkit.Diagnostics;

namespace Sarachan.ObservableCollections.Linq.Internal
{
    class CombinedCollectionViewEventEmitter<T, TRelay, TView> : CollectionView.IEventEmitter<T, TView>
    {
        private readonly CollectionView.IEventEmitter<T, TRelay> _sourceEmitter;
        private readonly CollectionView.IEventEmitter<TRelay, TView> _combinedEmitter;

        public CombinedCollectionViewEventEmitter(CollectionView.IEventEmitter<T, TRelay> sourceEmitter, CollectionView.IEventEmitter<TRelay, TView> combinedEmitter)
        {
            _sourceEmitter = sourceEmitter;
            _combinedEmitter = combinedEmitter;
        }

        private NotifyCollectionChangedEventHandler<TView>? _handler;

        public void Emit(NotifyCollectionChangedEventArgs<T> e, NotifyCollectionChangedEventHandler<TView> handler)
        {
            // don't use lambda to avoid GC 
            //_sourceEmitter.Emit(e, (_, relayArgs) =>
            //{
            //    _combinedEmitter.Emit(relayArgs, handler);
            //});

            Guard.IsNull(_handler);
            _handler = handler;

            _sourceEmitter.Emit(e, OnSourceEmitted);

            _handler = null;
        }

        private void OnSourceEmitted(object sender, NotifyCollectionChangedEventArgs<TRelay> e)
        {
            var handler = _handler;
            Guard.IsNotNull(handler);

            _combinedEmitter.Emit(e, handler);
        }
    }
}
