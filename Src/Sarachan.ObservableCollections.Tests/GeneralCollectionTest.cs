using System.Collections.Specialized;
using System.Reflection;
using Sarachan.ObservableCollections.Linq;

namespace Sarachan.ObservableCollections.Tests
{
    [TestFixture(typeof(ObservableList<>))]
    //[TestFixture(typeof(ObservableDictionary<,>))]
    public class GeneralCollectionTest
    {
        private readonly Type _collectionType;

        public GeneralCollectionTest(Type collectionType) 
        {
            _collectionType = collectionType;
        }

        [Test]
        public void TestEventMemoryLeak()
        {
            var collection = CreateCollection<int>();
            foreach (var item in Enumerable.Range(0, 10))
            {
                collection.Add(item);
            }

            var view = collection.BuildView(emitter =>
            {
                return emitter.Select(x => x).Where(x => true).Reverse();
            }).CreateStandardView(true);

            var handler = GetCollectionChangedDelegate(collection);
            Assert.That(handler, Is.Null);

            NotifyCollectionChangedAction? lastAction = null;
            var standardHandler = new NotifyCollectionChangedEventHandler((sender, e) =>
            {
                lastAction = e.Action;
            });

            view.CollectionChanged += standardHandler;

            handler = GetCollectionChangedDelegate(collection);
            Assert.That(handler, Is.Not.Null);

            view.CollectionChanged -= standardHandler;

            handler = GetCollectionChangedDelegate(collection);
            Assert.That(handler, Is.Null);
        }

        private IObservableCollection<T> CreateCollection<T>()
        {
            var t = _collectionType.MakeGenericType(typeof(T));
            return (IObservableCollection<T>)Activator.CreateInstance(t)!;
        }

        private NotifyCollectionChangedEventHandler<T>? GetCollectionChangedDelegate<T>(IReadOnlyObservableCollection<T> collection)
        {
            var type = collection.GetType();
            var field = GetAllFields(type).First(f => f.FieldType == typeof(NotifyCollectionChangedEventHandler<T>));
            return (NotifyCollectionChangedEventHandler<T>?)field.GetValue(collection);
        }

        private static IEnumerable<FieldInfo> GetAllFields(Type? type)
        {
            while (type != null)
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    yield return field;
                }
                type = type.BaseType;
            }
        }
    }
}