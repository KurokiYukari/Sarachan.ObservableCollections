namespace Sarachan.ObservableCollections
{
    public static class ObservableCollectionExtensions
    {
        public static IStandardListView<T> CreateStandardView<T>(this IReadOnlyObservableList<T> self,
            bool supportRangeAction)
        {
            return new StandardListView<T, IReadOnlyObservableList<T>>(self, supportRangeAction);
        }
    }
}
