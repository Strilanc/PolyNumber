using System;
using System.Collections.Generic;
using System.Linq;
using Numerics;
using Strilanc.LinqToCollections;

public struct Vector {
    private readonly BigRational[] _values;
    public IReadOnlyList<BigRational> Values { get { return _values ?? new BigRational[0]; } }
    
    public Vector(IEnumerable<BigRational> values) {
        if (values == null) throw new ArgumentNullException("values");
        this._values = values.ToArray();
    }
    public static implicit operator Vector(BigRational[] value) {
        return new Vector(value);
    }
    public static implicit operator Vector(BigRational value) {
        return new Vector(ReadOnlyList.Singleton(value));
    }
    public static implicit operator Vector(int value) {
        return (BigRational)value;
    }

    public static Vector operator -(Vector vector) {
        return new Vector(vector.Values.Select(e => -e));
    }
    public static Vector operator *(Vector vector, BigRational factor) {
        return new Vector(vector.Values.Select(e => e * factor));
    }
    public static Vector operator /(Vector vector, BigRational factor) {
        return new Vector(vector.Values.Select(e => e / factor));
    }
    public static Vector operator +(Vector vector1, Vector vector2) {
        if (vector1.Values.Count != vector2.Values.Count) throw new ArgumentException();
        return new Vector(vector1.Values.Zip(vector2.Values, BigRational.Add));
    }
    public static Vector operator -(Vector vector1, Vector vector2) {
        if (vector1.Values.Count != vector2.Values.Count) throw new ArgumentException();
        return new Vector(vector1.Values.Zip(vector2.Values, BigRational.Subtract));
    }

    public Vector CancelIndexWith(int index, Vector other) {
        var e = Values[index];
        var r = other.Values[index];
        return this*r - other*e;
    }
    public Vector Reduce() {
        var d = Values.FirstOrDefault(e => e != 0);
        if (d != 0) return this / d;
        return this;
    }

    public static bool operator ==(Vector vector1, Vector vector2) {
        return vector1.Equals(vector2);
    }
    public static bool operator !=(Vector vector1, Vector vector2) {
        return !vector1.Equals(vector2);
    }

    public bool Equals(Vector other) {
        return Values.SequenceEqual(other.Values);
    }
    public override bool Equals(object obj) {
        return obj is Vector && Equals((Vector)obj);
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
