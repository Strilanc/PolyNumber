using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

public static class TestingUtilities {
    public struct SimilarEquater<T> : IEqualityComparer<T> {
        private static bool DynamicEquals(dynamic x, dynamic y) {
            if (Object.Equals(x, y)) return true;
            
            if ((x == null) != (y == null)) return false;

            var obj1 = (object)x;
            var obj2 = (object)y;
            if (obj1.GetType().IsGenericTypeCreatedFromDefinition(typeof(Dictionary<,>))
                && obj2.GetType().IsGenericTypeCreatedFromDefinition(typeof(Dictionary<,>))) {
                foreach (var key in Enumerable.Distinct(Enumerable.Concat(x.Keys, y.Keys))) {
                    if (!x.ContainsKey(key)) return false;
                    if (!y.ContainsKey(key)) return false;
                    if (!DynamicEquals(x[key], y[key])) return false;
                }
                return true;
            }
            
            if (x is IEnumerable && y is IEnumerable) {
                return TestingUtilities.IsSequenceSimilar(x, y);
            }

            if (obj1.GetType().IsGenericTypeCreatedFromDefinition(typeof(Tuple<,>))
                && obj2.GetType().IsGenericTypeCreatedFromDefinition(typeof(Tuple<,>))) {
                return DynamicEquals(x.Item1, y.Item1) && DynamicEquals(x.Item2, y.Item2);
            }

            return false;
        }
        private static int DynamicGetHashCode(dynamic d) {
            if (d == null) return 0;
            var obj = (object)d;

            if (obj.GetType().IsGenericTypeCreatedFromDefinition(typeof(Dictionary<,>))) {
                return ((IEnumerable<object>)Enumerable.Cast<object>(d))
                    .Select(DynamicGetHashCode)
                    .Aggregate(0, (a, e) => (a ^ e) + 1);
            }

            if (obj is IEnumerable) {
                return ((IEnumerable<object>)Enumerable.Cast<object>(d))
                    .Select(DynamicGetHashCode)
                    .Aggregate(0, (a, e) => (a * 53 ^ e) + 1);
            }

            if (obj.GetType().IsGenericTypeCreatedFromDefinition(typeof(Tuple<,>))) {
                return DynamicGetHashCode(d.Item1) ^ (DynamicGetHashCode(d.Item2) * 3);
            }

            return d.GetHashCode();
        }
        public bool Equals(T x, T y) {
            return DynamicEquals(x, y);
        }
        public int GetHashCode(T obj) {
            return DynamicGetHashCode(obj);
        }
    }
    public static T AssertInlineFail<T>(this T value) {
        Assert.Fail(value.ToStringTryHarder());
        return value;
    }
    public static void AssertHasSimilarItemsIgnoringOrder<T>(this IEnumerable<T> items1, IEnumerable<T> items2) {
        var eq = new SimilarEquater<T>();
        var g1 = items1.GroupBy(e => e, eq).ToDictionary(e => e.Key, e => e.Count(), eq);
        var g2 = items2.GroupBy(e => e, eq).ToDictionary(e => e.Key, e => e.Count(), eq);
        var b = new SimilarEquater<IReadOnlyDictionary<T, int>>().Equals(g1, g2);
        if (!b)
            Assert.Fail(
                "{0} should have the same items (ignoring order) as {1}",
                items1.ToStringTryHarder(),
                items2.ToStringTryHarder());
    }
    public static bool IsGenericTypeCreatedFromDefinition(this Type type, Type typeDefinition) {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeDefinition;
    }
    [DebuggerStepThrough]
    public static void AssertTrue(this bool value) {
        Assert.IsTrue(value);
    }
    [DebuggerStepThrough]
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
    [DebuggerStepThrough]
    public static void AssertEquals<T1, T2>(this T1 actual, T2 expected) {
        Assert.AreEqual(expected, actual);
    }
    public static void AssertSequenceEquals<T>(this IEnumerable<T> value1, IEnumerable<T> value2) {
        var r1 = value1.ToArray();
        var r2 = value2.ToArray();
        if (!r1.SequenceEqual(r2)) {
            Assert.Fail(
                "{0} should be equal to {1}",
                value1.ToStringTryHarder(),
                value2.ToStringTryHarder());
        }
    }
    public static void AssertSequenceEquals<T>(this IEnumerable<T> value1, params T[] value2) {
        value1.AssertSequenceEquals(value2.AsEnumerable());
    }
    public static void AssertListEquals<T>(this IReadOnlyList<T> value1, IReadOnlyList<T> value2) {
        value1.Count.AssertEquals(value2.Count);
        for (var i = 0; i < value1.Count; i++) {
            if (!Equals(value1[i], value2[i])) {
                Assert.Fail(
                    "{0} should be equal to {1}", 
                    value1.ToStringTryHarder(), 
                    value2.ToStringTryHarder());
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

    public static string ToStringTryHarder(this object value) {
        if (value == null) return "null";

        if (value is string) return (string)value;
        
        if (value is IEnumerable) {
            return "{" + string.Join(", ", 
                ((IEnumerable<object>)Enumerable.Cast<object>((dynamic)value))
                .Select(ToStringTryHarder)) + "}";
        }

        if (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(Tuple<,>)) {
            var d = (dynamic)value;
            return Tuple.Create(ToStringTryHarder(d.Item1), ToStringTryHarder(d.Item2)).ToString();
        }

        return "" + value;
    }
    public static void AssertSequenceSimilar<T>(this IEnumerable<T> value1, params T[] value2) {
        AssertSequenceSimilar(value1, value2.AsEnumerable());
    }
    public static bool IsSequenceSimilar<T>(this IEnumerable<T> value1, IEnumerable<T> value2) {
        using (var e1 = value1.GetEnumerator()) {
            using (var e2 = value2.GetEnumerator()) {
                while (true) {
                    var v1 = e1.MoveNext();
                    var v2 = e2.MoveNext();
                    if (v1 != v2) return false;
                    if (!v1) break;
                    if (!e1.Current.IsSimilarTo(e2.Current)) return false;
                }
            }
        }
        return true;
    }
    public static void AssertSequenceSimilar<T>(this IEnumerable<T> value1, IEnumerable<T> value2) {
        if (!value1.IsSequenceSimilar(value2)) {
            Assert.Fail("Expected {0} to be equal to {1}", value1.ToStringTryHarder(), value2.ToStringTryHarder());
        }
    }
    public static void AssertNotEqualTo<T1, T2>(this T1 value1, T2 value2) {
        Assert.AreNotEqual(value1, value2);
    }
    public static bool IsSimilarTo<T>(this T value1, T value2) {
        if ((value1 == null) != (value2 == null)) return false;
        return Equals(value1, value2)
            || (value1.GetType().GetInterfaces().Contains(typeof(IEnumerable)) && Enumerable.SequenceEqual((dynamic)value1, (dynamic)value2));        
    }
    public static void AssertSimilar<T>(this T value1, T value2) {
        if (!value1.IsSimilarTo(value2)) {
            Assert.Fail("Expected {0} to be equal to {1}", value1.ToStringTryHarder(), value2.ToStringTryHarder());
        }
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
