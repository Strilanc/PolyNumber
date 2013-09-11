using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Numerics;
using Strilanc.LinqToCollections;
using Strilanc.Value;
using MoreLinq;

public struct IntegerVector {
    private readonly IReadOnlyList<BigInteger> _values;
    public IReadOnlyList<BigInteger> Values { get { return _values ?? ReadOnlyList.Empty<BigInteger>(); } }
    
    public IntegerVector(IEnumerable<BigInteger> values) {
        if (values == null) throw new ArgumentNullException("values");
        this._values = values.ToArray();
    }
    public static implicit operator IntegerVector(BigInteger[] value) {
        return new IntegerVector(value);
    }
    public static implicit operator IntegerVector(BigInteger value) {
        return new IntegerVector(ReadOnlyList.Singleton(value));
    }
    public static implicit operator IntegerVector(int value) {
        return (BigInteger)value;
    }

    public static IntegerVector operator -(IntegerVector vector) {
        return new IntegerVector(vector.Values.Select(e => -e));
    }
    public static IntegerVector operator *(IntegerVector vector, BigInteger factor) {
        return new IntegerVector(vector.Values.Select(e => e * factor));
    }
    public static IntegerVector operator +(IntegerVector vector1, IntegerVector vector2) {
        if (vector1.Values.Count != vector2.Values.Count) throw new ArgumentException();
        return new IntegerVector(vector1.Values.Zip(vector2.Values, BigInteger.Add));
    }
    public static IntegerVector operator -(IntegerVector vector1, IntegerVector vector2) {
        if (vector1.Values.Count != vector2.Values.Count) throw new ArgumentException();
        return new IntegerVector(vector1.Values.Zip(vector2.Values, BigInteger.Subtract));
    }

    public IntegerVector CancelIndexWith(int index, IntegerVector other) {
        var e = Values[index];
        var r = other.Values[index];
        var gcd = BigInteger.GreatestCommonDivisor(e, r);
        return this*(r/gcd) - other*(e/gcd);
    }
    public IntegerVector Reduce() {
        var gcd = Values.Where(e => e != 0)
                        .Select(BigInteger.Abs)
                        .Aggregate(BigInteger.Zero, BigInteger.GreatestCommonDivisor);
        if (gcd == 0) gcd = 1;
        var sign = Values.FirstOrDefault(e => e != 0).Sign;
        if (sign < 0) gcd *= -1;
        return new IntegerVector(Values.Select(e => e /  gcd));
    }

    public override string ToString() {
        return string.Format("<{0}>", string.Join(", ", Values));
    }
}

public struct IntegerMatrix {
    private readonly IReadOnlyList<IReadOnlyList<BigInteger>> _columns;
    private IntegerMatrix(IEnumerable<IEnumerable<BigInteger>> columns) {
        if (columns == null) throw new ArgumentNullException("columns");
        this._columns = columns.Select(e => e.ToArray()).ToArray();
    }

    public static implicit operator IntegerMatrix(BigInteger value) {
        return new IntegerMatrix(ReadOnlyList.Singleton(ReadOnlyList.Singleton(value)));
    }
    public static implicit operator IntegerMatrix(int value) {
        return (BigInteger)value;
    }

    public static IntegerMatrix FromColumns(IEnumerable<IEnumerable<BigInteger>> columns) {
        return new IntegerMatrix(columns);
    }
    public static IntegerMatrix FromRows(IReadOnlyList<IReadOnlyList<BigInteger>> rows) {
        return FromColumns(rows.Transpose());
    }
    public static IntegerMatrix FromRows(params int[][] rowData) {
        return FromRows(rowData.Select(e => e.Select(f => (BigInteger)f)));
    }

    public IReadOnlyList<IReadOnlyList<BigInteger>> Columns { get { return _columns ?? ReadOnlyList.Empty<IReadOnlyList<BigInteger>>(); } }
    public IReadOnlyList<IReadOnlyList<BigInteger>> Rows { get { return Columns.Transpose(); } }
    public IntegerMatrix Transpose() {
        return FromColumns(Columns.Transpose());
    }

    public int Width { get { return Columns.Count; } }
    public int Height { get { return Columns.MayFirst().Select(e => e.Count).ElseDefault(); } }

    public IntegerMatrix Reduced() {
        var rows = Rows.Select(row => new IntegerVector(row)).ToArray();
        var w = Width;
        var h = Height;

        var usableRows = new HashSet<int>(h.Range());
        foreach (var c in w.Range()) {
            var nonZeroRow =
                usableRows
                .Where(r => rows[r].Values[c] != 0)
                .MayMinBy(r => rows[r].Values[c] == 1 ? 1 : 0); // prefer rows with 1 in the target column

            nonZeroRow.IfHasValueThenDo(r => {
                usableRows.Remove(r);

                var row = rows[r];
                rows = rows.Select((row2, r2) => r == r2 ? row2 : row2.CancelIndexWith(c, row)).ToArray();
            });
        }

        return FromRows(
            rows
            .OrderBy(row => row.Values.TakeWhile(e => e == 0).Count())
            .Select(e => e.Reduce().Values)
            .ToArray());
    }

    public override string ToString() {
        return string.Join(Environment.NewLine, Rows.Select(e => new IntegerVector(e)));
    }
}