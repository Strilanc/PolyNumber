using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

public static class TestingUtilities {
    public static void AssertTrue(this bool value) {
        Assert.IsTrue(value);
    }
    public static void AssertFalse(this bool value) {
        Assert.IsFalse(value);
    }
    public static T InvokeWithDefaultOnFail<T>(Func<T> v) {
        try {
            var r = v();
            Assert.AreNotEqual(r, default(T));
            return r;
        } catch {
            return default(T);
        }
    }
    public static void AssertEquals<T1, T2>(this T1 value1, T2 value2) {
        Assert.AreEqual(value1, value2);
    }
    public static void AssertSequenceEquals<T>(this IEnumerable<T> value1, IEnumerable<T> value2) {
        var r1 = value1.ToArray();
        var r2 = value2.ToArray();
        if (!r1.SequenceEqual(r2)) {
            Assert.Fail("{0} should be equal to {1}", string.Join(", ", r1), string.Join(", ", r2));
        }
    }
    public static void AssertSequenceEquals<T>(this IEnumerable<T> value1, params T[] value2) {
        value1.AssertSequenceEquals(value2.AsEnumerable());
    }
    public static void AssertListEquals<T>(this IReadOnlyList<T> value1, IReadOnlyList<T> value2) {
        value1.Count.AssertEquals(value2.Count);
        for (var i = 0; i < value1.Count; i++) {
            if (!Equals(value1[i], value2[i])) {
                Assert.Fail("{0} should be equal to {1}", string.Join(", ", value1), string.Join(", ", value2));
            }
        }

        // also check iterator
        value1.AssertSequenceEquals(value2.AsEnumerable());
    }
    public static void AssertListSimilar<T>(this IReadOnlyList<T> value1, params T[] value2) {
        value1.AssertListSimilar((IReadOnlyList<T>)value2);
    }

    public static void AssertListSimilar<T>(this IReadOnlyList<T> value1, IReadOnlyList<T> value2) {
        value1.Count.AssertEquals(value2.Count);
        for (var i = 0; i < value1.Count; i++) {
            if (!IsSimilarTo(value1[i], value2[i])) {
                Assert.Fail("{0} should be equal to {1}", string.Join(", ", value1), string.Join(", ", value2));
            }
        }

        // also check iterator
        value1.AssertSequenceSimilar(value2.AsEnumerable());
    }
    public static void AssertListEquals<T>(this IReadOnlyList<T> value1, params T[] value2) {
        value1.AssertListEquals((IReadOnlyList<T>)value2);
    }

    public static void AssertSequenceSimilar<T>(this IEnumerable<T> value1, params T[] value2) {
        AssertSequenceSimilar(value1, value2.AsEnumerable());
    }
    public static void AssertSequenceSimilar<T>(this IEnumerable<T> value1, IEnumerable<T> value2) {
        using (var e1 = value1.GetEnumerator()) {
            using (var e2 = value2.GetEnumerator()) {
                while (true) {
                    var v1 = e1.MoveNext();
                    var v2 = e2.MoveNext();
                    v1.AssertSimilar(v2);
                    if (!v1) break;
                    e1.Current.AssertSimilar(e2.Current);
                }
            }
        }
    }
    public static void AssertNotEqualTo<T1, T2>(this T1 value1, T2 value2) {
        Assert.AreNotEqual(value1, value2);
    }
    public static bool IsSimilarTo<T>(this T value1, T value2) {
        return Equals(value1, value2)
            || (value1.GetType().GetInterfaces().Contains(typeof(IEnumerable)) && Enumerable.SequenceEqual((dynamic)value1, (dynamic)value2));        
    }
    public static void AssertSimilar<T>(this T value1, T value2) {
        if (!value1.IsSimilarTo(value2)) Assert.AreEqual(value1, value2);
    }
    public static void AssertThrows(Action action) {
        try {
            action();
        } catch (Exception) {
            return;
        }
        Assert.Fail();
    }
    public static void AssertDoesNotThrow(Action action) {
        action();
    }
}
