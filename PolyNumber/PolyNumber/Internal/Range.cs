using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Strilanc.PolyNumber.Internal {
    /// <summary>
    /// A contiguous set of values, bounded by a minimum and a maximum.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    internal struct Range<T> where T : struct, IComparable<T> {
        public readonly T Min;
        public readonly T Max;
        public readonly bool ExcludeMin;
        public readonly bool ExcludeMax;
        public Range(T min, T max, bool excludeMin, bool excludeMax) {
            var compareMinToMax = Comparer<T>.Default.Compare(min, max);
            if (compareMinToMax > 0) throw new ArgumentException("min > max");
            if (compareMinToMax == 0 && (excludeMax || excludeMin)) throw new ArgumentException("strict bound but min == max");
            Min = min;
            Max = max;
            ExcludeMin = excludeMin;
            ExcludeMax = excludeMax;
        }
        public static implicit operator Range<T>(T value) {
            return new Range<T>(value, value, false, false);
        }
        public override string ToString() {
            if (Equals(Min, Max)) return "" + Min;

            return string.Format(
                "{0}{1}, {2}{3}",
                ExcludeMin ? "(" : "[",
                Min,
                Max,
                ExcludeMax ? ")" : "]");
        }
        public bool Contains(T value) {
            var cmin = Comparer<T>.Default.Compare(value, Min);
            var cmax = Comparer<T>.Default.Compare(value, Max);
            if (cmin < 0) return false;
            if (cmin == 0 && ExcludeMin) return false;
            if (cmax > 0) return false;
            if (cmax == 0 && ExcludeMax) return false;
            return true;
        }
        public Range<TOut> Cast<TOut>() where TOut : struct, IComparable<TOut> {
            return new Range<TOut>((TOut)(object)Min, (TOut)(object)Max, ExcludeMin, ExcludeMax);
        }
    }
}