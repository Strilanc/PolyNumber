using System;
using System.Collections.Generic;
using System.Linq;
using Numerics;
using Strilanc.LinqToCollections;

public struct RationalVector {
    private readonly BigRational[] _values;
    public IReadOnlyList<BigRational> Values { get { return _values ?? new BigRational[0]; } }
    
    public RationalVector(IEnumerable<BigRational> values) {
        if (values == null) throw new ArgumentNullException("values");
        this._values = values.ToArray();
    }
    public static implicit operator RationalVector(BigRational[] value) {
        return new RationalVector(value);
    }
    public static implicit operator RationalVector(BigRational value) {
        return new RationalVector(ReadOnlyList.Singleton(value));
    }
    public static implicit operator RationalVector(int value) {
        return (BigRational)value;
    }

    public static RationalVector operator -(RationalVector vector) {
        return new RationalVector(vector.Values.Select(e => -e));
    }
    public static RationalVector operator *(RationalVector vector, BigRational factor) {
        return new RationalVector(vector.Values.Select(e => e * factor));
    }
    public static RationalVector operator /(RationalVector vector, BigRational factor) {
        return new RationalVector(vector.Values.Select(e => e / factor));
    }
    public static RationalVector operator +(RationalVector vector1, RationalVector vector2) {
        if (vector1.Values.Count != vector2.Values.Count) throw new ArgumentException();
        return new RationalVector(vector1.Values.Zip(vector2.Values, BigRational.Add));
    }
    public static RationalVector operator -(RationalVector vector1, RationalVector vector2) {
        if (vector1.Values.Count != vector2.Values.Count) throw new ArgumentException();
        return new RationalVector(vector1.Values.Zip(vector2.Values, BigRational.Subtract));
    }

    public RationalVector CancelIndexWith(int index, RationalVector other) {
        var e = Values[index];
        var r = other.Values[index];
        return this*r - other*e;
    }
    public RationalVector Reduce() {
        var d = Values.FirstOrDefault(e => e != 0);
        if (d != 0) return this / d;
        return this;
    }

    public static bool operator ==(RationalVector vector1, RationalVector vector2) {
        return vector1.Equals(vector2);
    }
    public static bool operator !=(RationalVector vector1, RationalVector vector2) {
        return !vector1.Equals(vector2);
    }

    public bool Equals(RationalVector other) {
        return Values.SequenceEqual(other.Values);
    }
    public override bool Equals(object obj) {
        return obj is RationalVector && Equals((RationalVector)obj);
    }
    public override int GetHashCode() {
        return Values.Aggregate(
            Values.Count.GetHashCode(),
            (a, e) => {
                unchecked {
                    return a*3 + e.GetHashCode();
                }
            });
    }
    public override string ToString() {
        return string.Format("<{0}>", string.Join(", ", Values.Select(e => (double)e)));
    }
}
