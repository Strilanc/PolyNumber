using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Numerics;
using Strilanc.LinqToCollections;
using Strilanc.Value;

[DebuggerDisplay("{ToString()}")]
public struct Matrix {
    private readonly BigRational[][] _columns;
    private Matrix(IEnumerable<IEnumerable<BigRational>> columns) {
        if (columns == null) throw new ArgumentNullException("columns");
        this._columns = columns.Select(e => e.ToArray()).ToArray();
    }

    public static Matrix FromColumns(IEnumerable<IEnumerable<BigRational>> columns) {
        return new Matrix(columns);
    }
    public static Matrix FromRows(IReadOnlyList<IReadOnlyList<BigRational>> rows) {
        return FromColumns(rows.Transpose());
    }
    public static Matrix FromRows(params int[][] rowData) {
        return FromRows(rowData.Select(e => e.Select(f => (BigRational)f)));
    }

    public IReadOnlyList<IReadOnlyList<BigRational>> Columns { get { return _columns ?? ReadOnlyList.Empty<IReadOnlyList<BigRational>>(); } }
    public IReadOnlyList<IReadOnlyList<BigRational>> Rows { get { return Columns.Transpose(); } }

    public int Width { get { return Columns.Count; } }
    public int Height { get { return Columns.MayFirst().Select(e => e.Count).ElseDefault(); } }

    public Matrix Reduced() {
        var rows = Rows.Select(row => new Vector(row)).ToArray();
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
        return string.Join(Environment.NewLine, Rows.Select(e => new Vector(e)));
    }
}
