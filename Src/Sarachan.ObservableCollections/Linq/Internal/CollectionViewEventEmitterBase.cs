namespace Sarachan.ObservableCollections.Linq.Internal
{
    abstract class CollectionViewEventEmitterBase<T, TView> : CollectionView.IEventEmitter<T, TView>
    {
        private readonly CollectionViewEventEmitterHelper<TView> _emitHelper = new();

        public void Emit(NotifyCollectionChangedEventArgs<T> e, NotifyCollectionChangedEventHandler<TView> handler)
        {
            using var scope = _emitHelper.BeginEmitScope(handler);
            Emit(_emitHelper, e);
        }

        protected abstract void Emit(CollectionViewEventEmitterHelper<TView> emitHelper, NotifyCollectionChangedEventArgs<T> e);
    }
}
