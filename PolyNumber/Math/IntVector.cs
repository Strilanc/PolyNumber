using System;
using System.Collections.Generic;
using System.Linq;
using Strilanc.LinqToCollections;
using Int = System.Numerics.BigInteger;

public struct IntVector {
    private readonly IReadOnlyList<Int> _values;
    public IReadOnlyList<Int> Values { get { return _values ?? ReadOnlyList.Empty<Int>(); } }
    
    public IntVector(IEnumerable<Int> values) {
        if (values == null) throw new ArgumentNullException("values");
        this._values = values.ToArray();
    }
    public static implicit operator IntVector(Int[] value) {
        return new IntVector(value);
    }
    public static implicit operator IntVector(Int value) {
        return new IntVector(ReadOnlyList.Singleton(value));
    }
    public static implicit operator IntVector(int value) {
        return (Int)value;
    }

    public static IntVector operator -(IntVector vector) {
        return new IntVector(vector.Values.Select(e => -e));
    }
    public static IntVector operator *(IntVector vector, Int factor) {
        return new IntVector(vector.Values.Select(e => e * factor));
    }
    public static IntVector operator +(IntVector vector1, IntVector vector2) {
        if (vector1.Values.Count != vector2.Values.Count) throw new ArgumentException();
        return new IntVector(vector1.Values.Zip(vector2.Values, Int.Add));
    }
    public static IntVector operator -(IntVector vector1, IntVector vector2) {
        if (vector1.Values.Count != vector2.Values.Count) throw new ArgumentException();
        return new IntVector(vector1.Values.Zip(vector2.Values, Int.Subtract));
    }

    public IntVector CancelIndexWith(int index, IntVector other) {
        var e = Values[index];
        var r = other.Values[index];
        var gcd = Int.GreatestCommonDivisor(e, r);
        return this*(r/gcd) - other*(e/gcd);
    }
    public IntVector Reduce() {
        var gcd = Values.Where(e => e != 0)
                        .Select(Int.Abs)
                        .Aggregate(Int.Zero, Int.GreatestCommonDivisor);
        if (gcd == 0) gcd = 1;
        var sign = Values.FirstOrDefault(e => e != 0).Sign;
        if (sign < 0) gcd *= -1;
        return new IntVector(Values.Select(e => e /  gcd));
    }

    public static bool operator ==(IntVector vector1, IntVector vector2) {
        return vector1.Equals(vector2);
    }
    public static bool operator !=(IntVector vector1, IntVector vector2) {
        return !vector1.Equals(vector2);
    }

    public bool Equals(IntVector other) {
        return Values.SequenceEqual(other.Values);
    }
    public override bool Equals(object obj) {
        return obj is IntVector && Equals((IntVector)obj);
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
        return string.Format("<{0}>", string.Join(", ", Values));
    }
}
