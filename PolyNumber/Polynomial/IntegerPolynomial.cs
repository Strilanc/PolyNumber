using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Strilanc.LinqToCollections;
using Strilanc.Value;

[DebuggerDisplay("{ToString()}")]
public struct IntegerPolynomial<TTerm> : IEquatable<IntegerPolynomial<TTerm>> where TTerm : IPolynomialTerm<TTerm> {
    public static IntegerPolynomial<TTerm> Zero { get { return new IntegerPolynomial<TTerm>(); } }
    private readonly Dictionary<TTerm, BigInteger> _coefficients;
    public IReadOnlyDictionary<TTerm, BigInteger> Coefficients { get { return _coefficients ?? ReadOnlyDictionary.Empty<TTerm, BigInteger>(); } }
    public BigInteger Coefficient(TTerm power) {
        return _coefficients.MayGetValue(power).Else(0);
    }

    public IntegerPolynomial(IEnumerable<KeyValuePair<TTerm, BigInteger>> coefficients) {
        if (coefficients == null) throw new ArgumentNullException("coefficients");
        _coefficients = 
            coefficients
                .KeyReduce(BigInteger.Add)
                .Where(e => e.Value != 0)
                .ToDictionary();
    }

    public static implicit operator IntegerPolynomial<TTerm>(int value) {
        return (BigInteger)value;
    }
    public static implicit operator IntegerPolynomial<TTerm>(BigInteger value) {
        return new IntegerPolynomial<TTerm>(new[] { default(TTerm).KeyVal(value) });
    }
    public static implicit operator IntegerPolynomial<TTerm>(KeyValuePair<TTerm, BigInteger> value) {
        return new IntegerPolynomial<TTerm>(new[] { value });
    }
    public static implicit operator IntegerPolynomial<TTerm>(TTerm term) {
        return new IntegerPolynomial<TTerm>(new[] { term.KeyVal(BigInteger.One) });
    }
    public static IntegerPolynomial<TTerm> operator +(IntegerPolynomial<TTerm> value1, IntegerPolynomial<TTerm> value2) {
        var keys = value1.Coefficients.Keys.Union(value2.Coefficients.Keys);
        var keyVals = keys.SelectValue(e => value1.Coefficient(e) + value2.Coefficient(e));
        return new IntegerPolynomial<TTerm>(keyVals);
    }
    public static IntegerPolynomial<TTerm> operator -(IntegerPolynomial<TTerm> value1, IntegerPolynomial<TTerm> value2) {
        var keys = value1.Coefficients.Keys.Union(value2.Coefficients.Keys);
        var keyVals = keys.SelectValue(e => value1.Coefficient(e) - value2.Coefficient(e));
        return new IntegerPolynomial<TTerm>(keyVals);
    }
    public static IntegerPolynomial<TTerm> operator -(IntegerPolynomial<TTerm> value) {
        return new IntegerPolynomial<TTerm>(value.Coefficients.Select(e => e.Key.KeyVal(-e.Value)));
    }
    public static IntegerPolynomial<TTerm> operator *(IntegerPolynomial<TTerm> value1, IntegerPolynomial<TTerm> value2) {
        var keyVals = from c1 in value1.Coefficients
                      from c2 in value2.Coefficients
                      let key = c1.Key.Times(c2.Key)
                      let val = c1.Value*c2.Value
                      select key.KeyVal(val);
        return new IntegerPolynomial<TTerm>(keyVals);
    }
    public IntegerPolynomial<TTerm> RaisedTo(BigInteger power) {
        if (power < 0) throw new ArgumentException();
        var curSquarePower = this;
        var total = (IntegerPolynomial<TTerm>)1;
        while (power > 0) {
            if (!power.IsEven) total *= curSquarePower;
            curSquarePower *= curSquarePower;
            power >>= 1;
        }
        return total;
    }

    public static bool operator ==(IntegerPolynomial<TTerm> value1, IntegerPolynomial<TTerm> value2) {
        return value1.Coefficients.DictionaryEqual(value2.Coefficients);
    }
    public static bool operator !=(IntegerPolynomial<TTerm> value1, IntegerPolynomial<TTerm> value2) {
        return !value1.Coefficients.DictionaryEqual(value2.Coefficients);
    }
    public bool Equals(IntegerPolynomial<TTerm> other) {
        return this == other;
    }
    public override bool Equals(object obj) {
        return obj is IntegerPolynomial<TTerm> && this == (IntegerPolynomial<TTerm>)obj;
    }
    public override int GetHashCode() {
        return Coefficients.Aggregate(Coefficients.Count.GetHashCode(), (a, e) => a ^ e.GetHashCode());
    }

    public override string ToString() {
        if (Coefficients.Count == 0) return "0";
        return string.Join(" + ", Coefficients.OrderBy(e => e.Key).Reverse().Select(e => {
            var k = e.Key.ToString();
            var u = k == "" ? "1" : k;
            if (e.Value == 1) return u;
            if (e.Value == -1) return "-" + u;
            return e.Value + k;
        }));
    }
}