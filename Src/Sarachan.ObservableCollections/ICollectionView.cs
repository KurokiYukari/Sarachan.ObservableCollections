namespace Sarachan.ObservableCollections
{
    public interface ICollectionView<T, TView> : IReadOnlyObservableList<TView>, IDisposable
    {
        IReadOnlyObservableCollection<T> Collection { get; }

        void Refresh();
    }
}
