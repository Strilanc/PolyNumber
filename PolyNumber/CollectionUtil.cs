using System;
using System.Collections.Generic;
using System.Numerics;
using Strilanc.LinqToCollections;
using System.Linq;

public static class CollectionUtil {
    public static IEnumerable<IReadOnlyList<T>> CombinationsOfSize<T>(this IReadOnlyList<T> items, int size) {
        if (items == null) throw new ArgumentNullException("items");
        if (size < 0) throw new ArgumentOutOfRangeException("size", "size < 0");
        if (size == 0) return new[] {new T[0]};
        if (size > items.Count) return new T[0][];
        return from i in items.Count.Range()
               let head = items[i]
               from tail in items.Skip(i + 1).CombinationsOfSize(size - 1)
               select new[] {head}.Concat(tail).ToArray();
    }
    public static IEnumerable<T> Concat<T>(this IEnumerable<IEnumerable<T>> sequence) {
        return from e in sequence
               from j in e
               select j;
    }
    public static IEnumerable<Tuple<T, T>> Cross<T>(this IEnumerable<T> list1, IEnumerable<T> list2) {
        return from e in list1
               from j in list2
               select Tuple.Create(e, j);
    }
    public static IReadOnlyList<Tuple<T, T>> Cross<T>(this IReadOnlyList<T> list1, IReadOnlyList<T> list2) {
        return new AnonymousReadOnlyList<Tuple<T, T>>(
            () => list1.Count * list2.Count,
            i => {
                var n = list1.Count;
                var r1 = i%n;
                var r2 = i/n;
                return Tuple.Create(list1[r1], list2[r2]);
            });
    }
    public static IEnumerable<IReadOnlyList<T>> ContiguousSubSequencesOfSize<T>(this IEnumerable<T> list, int size) {
        var q = new Queue<T>();
        foreach (var e in list) {
            q.Enqueue(e);
            if (q.Count >= size) {
                yield return q.ToArray();
                q.Dequeue();
            }
        }
    }
    public static IReadOnlyList<IReadOnlyList<T>> ContiguousSubListsOfSize<T>(this IReadOnlyList<T> list, int size) {
        return (list.Count - size + 1).Range().Select(i => list.Skip(i).Take(size));
    }
    public static BigInteger Product(this IEnumerable<BigInteger> sequence) {
        return sequence.Aggregate(BigInteger.One, (a, e) => a * e);
    }
    public static BigInteger Sum(this IEnumerable<BigInteger> sequence) {
        return sequence.Aggregate(BigInteger.Zero, (a, e) => a + e);
    }
    public static IReadOnlyList<int> UpTo(this int max) {
        return (max + 1).Range();
    }
    public static IReadOnlyList<int> UpTo(this int min, int max) {
        return (max + 1).Range().Skip(min);
    }
    public static BigInteger Product(this Tuple<BigInteger, BigInteger> value) {
        return value.Item1*value.Item2;
    }
}