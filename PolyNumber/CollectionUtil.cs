using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using MoreLinq;
using Numerics;
using Strilanc.Exceptions;
using Strilanc.LinqToCollections;
using System.Linq;
using Strilanc.Value;

public static class CollectionUtil {
    public static IEnumerable<BigInteger> RangeInclusive(this BigInteger max) {
        return Naturals.Take(max + 1);
    }
    public static IEnumerable<BigInteger> Range(this BigInteger count) {
        return Naturals.Take(count);
    }
    public static T Max<T>(this T value1, T value2) where T : IComparable<T> {
        return value1.CompareTo(value2) >= 0 ? value1 : value2;
    }
    public static T Min<T>(this T value1, T value2) where T : IComparable<T> {
        return value1.CompareTo(value2) <= 0 ? value1 : value2;
    }
    public static IEnumerable<T> Take<T>(this IEnumerable<T> sequence, BigInteger count) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        if (count <= 0) yield break;
        foreach (var e in sequence) {
            yield return e;
            count -= 1;
            if (count == 0) yield break;
        }
    }
    public static IEnumerable<BigInteger> Naturals {
        get {
            for (var i = BigInteger.Zero;; i++) {
                yield return i;
            }
        }
    }

    public static IReadOnlyList<IReadOnlyList<T>> Transpose<T>(this IReadOnlyList<IReadOnlyList<T>> rectangularListOfLists) {
        if (rectangularListOfLists == null) throw new ArgumentNullException("rectangularListOfLists");

        return new AnonymousReadOnlyList<IReadOnlyList<T>>(
            () => rectangularListOfLists.MayFirst().Select(e => e.Count).ElseDefault(),
            i => new AnonymousReadOnlyList<T>(
                () => rectangularListOfLists.Count,
                j => rectangularListOfLists[j][i]));
    }
    public static IEnumerable<IReadOnlyList<int>> DecreasingSequencesOfSize(int length, int total, int max) {
        return DecreasingSequencesOfSizeSummingToHelper(length, total, max);
    }
    private static IEnumerable<ImmutableList<int>> DecreasingSequencesOfSizeSummingToHelper(int size, int total, int max) {
        if (size == 0 && total > 0) return new ImmutableList<int>[0];
        if (size == 0 && total == 0) return new[] { ImmutableList.Create<int>() };
        return from head in total.Min(max).RangeInclusive().Reverse()
               from tail in DecreasingSequencesOfSizeSummingToHelper(size - 1, total - head, head)
               select tail.Insert(0, head);
    }

    public static IEnumerable<IReadOnlyList<T>> Permutations<T>(this IEnumerable<T> items) {
        if (items == null) throw new ArgumentNullException("items");
        return items.ToImmutableList().PermutationsHelper();
    }
    private static IEnumerable<ImmutableList<T>> PermutationsHelper<T>(this ImmutableList<T> items) {
        if (items.Count == 0) return new[] { ImmutableList.Create<T>() };
        return from i in items.Indexes()
               let head = items[i]
               from tail in items.RemoveAt(i).DistinctPermutationsHelper()
               select tail.Insert(0, head);
    }

    public static IEnumerable<IReadOnlyList<T>> DistinctPermutations<T>(this IEnumerable<T> items) {
        if (items == null) throw new ArgumentNullException("items");
        return items.ToImmutableList().DistinctPermutationsHelper();
    }
    private static IEnumerable<ImmutableList<T>> DistinctPermutationsHelper<T>(this ImmutableList<T> items) {
        if (items.Count == 0) return new[] {ImmutableList.Create<T>()};
        return from i in items.Indexes().DistinctBy(i => items[i])
               let head = items[i]
               from tail in items.RemoveAt(i).DistinctPermutationsHelper()
               select tail.Insert(0, head);
    }

    /// <summary>
    /// Enumerates all of the ways that it's possible one item from each collection in a sequence.
    /// For example, the choice combinations of [[1,2],[3,4,5]] are (in some order): {[1,3],[1,4],[1,5],[2,3],[2,4],[2,5]}.
    /// </summary>
    public static IEnumerable<IReadOnlyList<T>> AllChoiceCombinations<T>(this IEnumerable<IEnumerable<T>> sequenceOfChoices) {
        using (var e = sequenceOfChoices.GetEnumerator().AllChoiceCombinationsOfRemainder()) {
            while (e.MoveNext()) {
                yield return e.Current;
            }
        }
    }
    private static IEnumerator<ImmutableList<T>> AllChoiceCombinationsOfRemainder<T>(this IEnumerator<IEnumerable<T>> sequenceOfChoices) {
        if (!sequenceOfChoices.MoveNext()) {
            yield return ImmutableList.Create<T>();
            yield break;
        }

        var headChoices = sequenceOfChoices.Current;
        var tailChoices = sequenceOfChoices.AllChoiceCombinationsOfRemainder();
        using (var e = tailChoices) {
            while (e.MoveNext()) {
                var tailChoice = e.Current;
                foreach (var headChoice in headChoices) {
                    yield return tailChoice.Insert(0, headChoice);
                }
            }
        }
    }

    public static IReadOnlyList<TItem> MaxesBy<TItem, TCompare>(this IEnumerable<TItem> sequence, Func<TItem, TCompare> projection) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        if (projection == null) throw new ArgumentNullException("projection");

        var result = new List<TItem>();
        var maxSoFar = default(TCompare);
        var comparer = Comparer<TCompare>.Default;

        foreach (var e in sequence) {
            var p = projection(e);
            var d = result.Count == 0 ? +1 : comparer.Compare(p, maxSoFar);
            if (d > 0) {
                maxSoFar = p;
                result.Clear();
            }
            if (d >= 0) result.Add(e);
        }

        return result;
    }

    public static IEnumerable<IReadOnlyList<T>> Choose<T>(this IReadOnlyList<T> items, int numberOfItemsToChoose) {
        return items.ChooseHelper(numberOfItemsToChoose);
    }
    private static IEnumerable<ImmutableList<T>> ChooseHelper<T>(this IReadOnlyList<T> items, int numberOfItemsToChoose) {
        if (items == null) throw new ArgumentNullException("items");
        if (numberOfItemsToChoose < 0) throw new ArgumentOutOfRangeException("numberOfItemsToChoose", "numberOfItemsToChoose < 0");
        if (numberOfItemsToChoose == 0) return new[] { ImmutableList.Create<T>() };
        if (numberOfItemsToChoose > items.Count) return new ImmutableList<T>[0];
        return from iv in items.Index()
               let head = iv.Value
               let tail = items.Skip(iv.Key + 1)
               let tailChoices = tail.ChooseHelper(numberOfItemsToChoose - 1)
               from tailChoice in tailChoices
               select tailChoice.Insert(0, head);
    }

    public static IEnumerable<IReadOnlyList<T>> ChooseWithReplacement<T>(this IReadOnlyList<T> items, int numberOfItemsToDraw) {
        return items.ChooseWithReplacementHelper(numberOfItemsToDraw);
    }
    private static IEnumerable<ImmutableList<T>> ChooseWithReplacementHelper<T>(this IReadOnlyList<T> items, int numberOfItemsToDraw) {
        if (items == null) throw new ArgumentNullException("items");
        if (numberOfItemsToDraw < 0) throw new ArgumentOutOfRangeException("numberOfItemsToDraw", "numberOfItemsToDraw < 0");
        if (numberOfItemsToDraw == 0) return new[] { ImmutableList.Create<T>() };

        return from iv in items.Index()
               let head = iv.Value
               let tail = items.Skip(iv.Key)
               let tailChoices = tail.ChooseWithReplacementHelper(numberOfItemsToDraw - 1)
               from tailChoice in tailChoices
               select tailChoice.Insert(0, head);
    }

    public static IEnumerable<IReadOnlyList<T>> ChooseWithBoundedRepetition<T>(this IReadOnlyList<T> items,
                                                                               int numberOfItemsToDraw,
                                                                               int maxRepeatsOfEachItem) {
        if (items == null) throw new ArgumentNullException("items");
        if (numberOfItemsToDraw < 0) throw new ArgumentOutOfRangeException("numberOfItemsToDraw", "numberOfItemsToDraw < 0");
        if (maxRepeatsOfEachItem < 0) throw new ArgumentOutOfRangeException("maxRepeatsOfEachItem", "maxRepeatsOfEachItem < 0");
        return items.ChooseWithBoundedRepetitionHelper(numberOfItemsToDraw, maxRepeatsOfEachItem, 0);
    }
    private static IEnumerable<IImmutableList<T>> ChooseWithBoundedRepetitionHelper<T>(this IReadOnlyList<T> items,
                                                                                       int numberOfItemsToDraw,
                                                                                       int maxRepeatsOfEachItem,
                                                                                       int repeatsOfFirstItemSoFar) {
        if (numberOfItemsToDraw == 0) return new[] {ImmutableList.Create<T>()};
        if (maxRepeatsOfEachItem == 0) return new ImmutableList<T>[0];

        return from iv in items.Index()
               let index = iv.Key
               let head = iv.Value
               where index > 0 || repeatsOfFirstItemSoFar < maxRepeatsOfEachItem
               let tail = items.Skip(index)
               let tailChoices = tail.ChooseWithBoundedRepetitionHelper(
                   numberOfItemsToDraw - 1,
                   maxRepeatsOfEachItem,
                   1 + (index == 0 ? repeatsOfFirstItemSoFar : 0))
               from tailChoice in tailChoices
               select tailChoice.Insert(0, head);
    }

    public static IEnumerable<T> Concat<T>(this IEnumerable<IEnumerable<T>> sequence) {
        return sequence.SelectMany(e => e);
    }
    public static IEnumerable<Tuple<T, T>> Cross<T>(this IEnumerable<T> sequence1, IEnumerable<T> sequence2) {
        if (sequence1 == null) throw new ArgumentNullException("sequence1");
        if (sequence2 == null) throw new ArgumentNullException("sequence2");
        return from item1 in sequence1
               from item2 in sequence2
               select Tuple.Create(item1, item2);
    }
    public static IReadOnlyList<Tuple<T, T>> Cross<T>(this IReadOnlyList<T> list1, IReadOnlyList<T> list2) {
        if (list1 == null) throw new ArgumentNullException("list1");
        if (list2 == null) throw new ArgumentNullException("list2");
        return new AnonymousReadOnlyList<Tuple<T, T>>(
            () => list1.Count * list2.Count,
            i => {
                var n = list2.Count;
                var index1 = i / n;
                var index2 = i % n;
                return Tuple.Create(list1[index1], list2[index2]);
            },
            list1.AsEnumerable().Cross(list2.AsEnumerable()));
    }

    public static IEnumerable<TAggregate> Stream<TAggregate, TItem>(this IEnumerable<TItem> sequence, TAggregate seed, Func<TAggregate, TItem, TAggregate> aggregator) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        if (aggregator == null) throw new ArgumentNullException("aggregator");
        foreach (var item in sequence) {
            seed = aggregator(seed, item);
            yield return seed;
        }
    }
    public static IEnumerable<int> Indexes<T>(this IEnumerable<T> sequence) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        return sequence.Select((e, i) => i);
    }
    public static IReadOnlyList<int> Indexes<T>(this IReadOnlyList<T> list) {
        if (list == null) throw new ArgumentNullException("list");
        return list.Select((e, i) => i);
    }
    public static IReadOnlyList<int> RangeInclusive(this int max) {
        if (max < 0) throw new ArgumentNegativeException("max < 0");
        return (max + 1).Range();
    }
    public static BigInteger Product(this IEnumerable<BigInteger> sequence) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        return sequence.Aggregate(BigInteger.One, (a, e) => a * e);
    }
    public static BigInteger Sum(this IEnumerable<BigInteger> sequence) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        return sequence.Aggregate(BigInteger.Zero, (a, e) => a + e);
    }
    public static BigRational Sum(this IEnumerable<BigRational> sequence) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        return sequence.Aggregate(BigRational.Zero, (a, e) => a + e);
    }
    public static BigInteger Product(this Tuple<BigInteger, BigInteger> value) {
        if (value == null) throw new ArgumentNullException("value");
        return value.Item1*value.Item2;
    }

    public static May<TVal> MayGetValue<TKey, TVal>(this IReadOnlyDictionary<TKey, TVal> dictionary, TKey key) {
        TVal val;
        if (!dictionary.TryGetValue(key, out val)) return May.NoValue;
        return val;
    }
    public static Dictionary<TKey, TVal> ToDictionary<TKey, TVal>(this IEnumerable<KeyValuePair<TKey, TVal>> sequence) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        return sequence.ToDictionary(e => e.Key, e => e.Value);
    }
    public static KeyValuePair<TKey, TVal> KeyVal<TKey, TVal>(this TKey key, TVal val) {
        return new KeyValuePair<TKey, TVal>(key, val);
    }
    public static IEnumerable<KeyValuePair<TKey, TVal>> KeyReduce<TKey, TVal>(this IEnumerable<KeyValuePair<TKey, TVal>> sequence, Func<TVal, TVal, TVal> duplicateKeyValueReducer) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        if (duplicateKeyValueReducer == null) throw new ArgumentNullException("duplicateKeyValueReducer");
        return sequence
            .GroupBy(e => e.Key)
            .SelectKeyValue(
                g => g.Key,
                g => g.Select(e => e.Value).Aggregate(duplicateKeyValueReducer));
    }
    public static Dictionary<TKey, TVal> ToDictionary<TKey, TVal>(this IEnumerable<KeyValuePair<TKey, TVal>> sequence, Func<TVal, TVal, TVal> duplicateKeyValueReducer) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        if (duplicateKeyValueReducer == null) throw new ArgumentNullException("duplicateKeyValueReducer");
        return sequence
            .GroupBy(e => e.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Value).Aggregate(duplicateKeyValueReducer));
    }
    public static bool Contains<TKey, TVal>(this IReadOnlyDictionary<TKey, TVal> dictionary, KeyValuePair<TKey, TVal> keyValuePair) {
        if (dictionary == null) throw new ArgumentNullException("dictionary");
        return dictionary.MayGetValue(keyValuePair.Key) == keyValuePair.Value.Maybe();
    }
    public static IEnumerable<KeyValuePair<TKey, TVal>> SelectKeyValue<TKey, TVal>(this IEnumerable<TKey> keys, Func<TKey, TVal> valueSelector) {
        return keys.Select(e => e.KeyVal(valueSelector(e)));
    }
    public static IEnumerable<KeyValuePair<TKey, TVal>> SelectKeyValue<TItem, TKey, TVal>(this IEnumerable<TItem> sequence, Func<TItem, TKey> keySelector, Func<TItem, TVal> valueSelector) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        if (keySelector == null) throw new ArgumentNullException("keySelector");
        if (valueSelector == null) throw new ArgumentNullException("valueSelector");
        return sequence.Select(e => keySelector(e).KeyVal(valueSelector(e)));
    }
    public static bool DictionaryEqual<TKey, TVal>(this IReadOnlyDictionary<TKey, TVal> dictionary1, IReadOnlyDictionary<TKey, TVal> dictionary2) {
        if (dictionary1 == null) throw new ArgumentNullException("dictionary1");
        if (dictionary2 == null) throw new ArgumentNullException("dictionary2");
        return dictionary1.Count == dictionary2.Count
               && dictionary1.All(dictionary2.Contains);
    }
}