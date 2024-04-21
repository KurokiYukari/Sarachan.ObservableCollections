using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sarachan.ObservableCollections;
using Sarachan.ObservableCollections.Miscs;
using static Sarachan.ObservableCollections.Linq.CollectionView;

namespace Sarachan.ObservableCollections.Tests
{
    [TestFixture]
    [TestFixtureSource(typeof(FixtureSource))]
    public class LinqTest
    {
        public class FixtureSource : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                object[][] source = [
                    [ new CaseArgs("Empty", [])],
                    [ new CaseArgs("SingleItem", [ I() ])],
                    [ new CaseArgs("MinMax", [ I(int.MaxValue), I(int.MinValue) ])],
                    [ new CaseArgs("Repeat", Enumerable.Repeat(0, RANDOM_COUNT).Select(I).ToArray())],
                    [ new CaseArgs("Random", GetRandomItems().ToArray()) ],
                ];
                return source.GetEnumerator();
            }
            
            static IEnumerable<Item> GetRandomItems()
            {
                for (int i = 0; i < RANDOM_COUNT; i++)
                {
                    yield return GetRandomItem();
                }
            }

            static Item I(int num = 0)
            {
                return new Item(num);
            }
        }

        const int RANDOM_COUNT = 100;
        private static Item GetRandomItem()
        {
            return new Item(Random.Shared.Next(0, RANDOM_COUNT));
        }

        private static void GetRandomItems(Span<Item> items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = GetRandomItem();
            }
        }

        public record CaseArgs(string Name, IReadOnlyList<Item> Items);

        public readonly record struct Item(int Num, Guid Id) : IComparable<Item>
        {
            public Item(int num) : this(num, Guid.NewGuid())
            {
            }

            public int CompareTo(Item other)
            {
                return Num.CompareTo(other.Num);
            }
        }


        private readonly CaseArgs _args;

        public LinqTest(CaseArgs args)
        {
            _args = args;
        }

        private void RunTestTemplate(Func<IEventEmitter<Item, Item>, IEventEmitter<Item, Item>> emitterBuilder,
            Func<IEnumerable<Item>, IEnumerable<Item>> refCollectionBuilder)
        {
            var list = new ObservableList<Item>();
            using (var owner = SpanOwner.Allocate(_args.Items))
            {
                list.Add(owner.Span);
            }

            var view = list.BuildView(emitterBuilder);

            void AssertSequenceEqual()
            {
                Assert.IsTrue(view.SequenceEqual(refCollectionBuilder(list)));
            }

            // Init
            AssertSequenceEqual();

            // Replace
            if (list.Count > 0)
            {
                for (int i = 0; i < RANDOM_COUNT; i++)
                {
                    var index = Random.Shared.Next(0, list.Count - 1);
                    var randomItem = GetRandomItem();
                    list[index] = randomItem;
                    AssertSequenceEqual();
                }
            }

            // Insert
            Span<Item> tempItems = stackalloc Item[2];
            for (int i = 0; i < RANDOM_COUNT; i++)
            {
                GetRandomItems(tempItems);
                list.Insert(Random.Shared.Next(0, list.Count), tempItems);
            }
            GetRandomItems(tempItems);
            list.Insert(0, tempItems);
            GetRandomItems(tempItems);
            list.Add(tempItems);

            AssertSequenceEqual();

            // Remove
            for (int i = 0; i < RANDOM_COUNT; i++)
            {
                int index = Random.Shared.Next(0, list.Count - 2);
                //tempItems[0] = list[index];
                //tempItems[1] = list[index + 1];
                list.RemoveAt(index, 2);
                AssertSequenceEqual();
            }

            // Move
            for (int i = 0; i < RANDOM_COUNT; i++)
            {
                list.Move(Random.Shared.Next(0, list.Count - 1), Random.Shared.Next(0, list.Count - 1));
            }

            AssertSequenceEqual();

            // Refresh
            Span<Item> resetItems = stackalloc Item[RANDOM_COUNT * 2];
            for (int i = 0; i < resetItems.Length; i++)
            {
                resetItems[i] = GetRandomItem();
            }
            list.Reset(resetItems);

            AssertSequenceEqual();
        }

        private readonly static object[][] _testSelectCases = [
            [ new Func<Item, Item>(x => x) ],
            [ new Func<Item, Item>(DoubleSelector) ]
        ];

        static Item DoubleSelector(Item x) => new(x.Num * 2, x.Id);

        [TestCaseSource(nameof(_testSelectCases))]
        public void TestSelect(Func<Item, Item> selector)
        {
            RunTestTemplate(emitter => emitter.Select(selector),
                source => source.Select(selector));
        }

        [Test]
        public void TestOrderBy()
        {
            RunTestTemplate(emitter => emitter.OrderBy(),
                source => source.OrderBy(x => x));
        }

        [Test]
        public void TestReverse()
        {
            RunTestTemplate(emitter => emitter.Reverse(),
                source => source.Reverse());
        }

        private readonly static object[][] _testWhereCases = [
            [ new Predicate<Item>(x => true) ],
            [ new Predicate<Item>(x => false) ],
            [ new Predicate<Item>(x => x.Num % 2 == 0) ],
        ];

        [TestCaseSource(nameof(_testWhereCases))]
        public void TestWhere(Predicate<Item> predicate)
        {
            RunTestTemplate(emitter => emitter.Where(predicate),
                source => source.Where(new Func<Item, bool>(predicate)));
        }

        [Test]
        public void TestOrderByAndReverse()
        {
            RunTestTemplate(emitter => emitter.OrderBy().Reverse(),
                source => source.OrderBy(x => x).Reverse());
        }

        [TestCaseSource(nameof(_testWhereCases))]
        public void TestWhereAndOrderBy(Predicate<Item> predicate)
        {
            RunTestTemplate(emitter => emitter.Where(predicate).OrderBy(),
                source => source.Where(new Func<Item, bool>(predicate)).OrderBy(x => x));
        }
    }
}