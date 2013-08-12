using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using MoreLinq;
using Strilanc.Exceptions;
using Strilanc.LinqToCollections;
using System.Linq;

public static class CollectionUtil {
    private static IReadOnlyList<T> AnonymousList<T>(Func<int> counter, Func<int, T> getter, IEnumerable<T> optionalEfficientIterator = null) {
        return new AnonymousReadOnlyList<T>(counter, getter, optionalEfficientIterator);
    }

    public static IEnumerable<IReadOnlyList<T>> Choose<T>(this IReadOnlyList<T> items, int size) {
        return items.ChooseHelper(size);
    }
    private static IEnumerable<IImmutableList<T>> ChooseHelper<T>(this IReadOnlyList<T> items, int size) {
        if (items == null) throw new ArgumentNullException("items");
        if (size < 0) throw new ArgumentOutOfRangeException("size", "size < 0");
        if (size == 0) return new[] { ImmutableList.Create<T>() };
        if (size > items.Count) return new ImmutableList<T>[0];
        return from iv in items.Index()
               let head = iv.Value
               let tail = items.Skip(iv.Key + 1)
               let tailChoices = tail.ChooseHelper(size - 1)
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
        return AnonymousList(
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
    public static IEnumerable<IReadOnlyList<T>> Window<T>(this IEnumerable<T> sequence, int windowSize) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        if (windowSize <= 0) throw new ArgumentNotPositiveException("windowSize");
        return sequence
            .Stream(
                ImmutableList.Create<T>(), 
                (window, item) => 
                    (window.Count < windowSize ? window : window.RemoveAt(0))
                    .Add(item))
            .Skip(windowSize - 1);
    }
    public static IReadOnlyList<IReadOnlyList<T>> Window<T>(this IReadOnlyList<T> list, int windowSize) {
        if (list == null) throw new ArgumentNullException("list");
        if (windowSize <= 0) throw new ArgumentNotPositiveException("windowSize");
        return list
            .Indexes()
            .SkipLast(windowSize - 1)
            .Select(i => list.Skip(i).Take(windowSize));
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
    public static BigInteger Product(this Tuple<BigInteger, BigInteger> value) {
        if (value == null) throw new ArgumentNullException("value");
        return value.Item1*value.Item2;
    }
}