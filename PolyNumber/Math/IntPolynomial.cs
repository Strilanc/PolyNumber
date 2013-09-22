using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Numerics;
using Strilanc.LinqToCollections;
using Strilanc.Value;
using Int = System.Numerics.BigInteger;

namespace Math {
    [DebuggerDisplay("{ToString()}")]
    public struct IntPolynomial<TTerm> : IEquatable<IntPolynomial<TTerm>> where TTerm : ITerm<TTerm> {
        public static IntPolynomial<TTerm> Zero { get { return new IntPolynomial<TTerm>(); } }
        private readonly Dictionary<TTerm, Int> _coefficients;
        public IReadOnlyDictionary<TTerm, Int> Coefficients { get { return _coefficients ?? ReadOnlyDictionary.Empty<TTerm, Int>(); } }
        public Int Coefficient(TTerm power) {
            return _coefficients.MayGetValue(power).Else(0);
        }

        public IntPolynomial(IEnumerable<KeyValuePair<TTerm, Int>> coefficients) {
            if (coefficients == null) throw new ArgumentNullException("coefficients");
            _coefficients =
                coefficients
                    .KeyReduce(Int.Add)
                    .Where(e => e.Value != 0)
                    .ToDictionary();
        }

        public static IntPolynomial<TTerm> Add(IntPolynomial<TTerm> value1, IntPolynomial<TTerm> value2) {
            return value1 + value2;
        }
        public static IntPolynomial<TTerm> Multiply(IntPolynomial<TTerm> value1, IntPolynomial<TTerm> value2) {
            return value1*value2;
        }

        public static implicit operator IntPolynomial<TTerm>(int value) {
            return (Int)value;
        }
        public static implicit operator IntPolynomial<TTerm>(Int value) {
            return new IntPolynomial<TTerm>(new[] {default(TTerm).KeyVal(value)});
        }
        public static implicit operator IntPolynomial<TTerm>(KeyValuePair<TTerm, Int> value) {
            return new IntPolynomial<TTerm>(new[] {value});
        }
        public static implicit operator IntPolynomial<TTerm>(TTerm term) {
            return new IntPolynomial<TTerm>(new[] {term.KeyVal(Int.One)});
        }
        public static IntPolynomial<TTerm> operator +(IntPolynomial<TTerm> value1, IntPolynomial<TTerm> value2) {
            var keys = value1.Coefficients.Keys.Union(value2.Coefficients.Keys);
            var keyVals = keys.SelectKeyValue(e => value1.Coefficient(e) + value2.Coefficient(e));
            return new IntPolynomial<TTerm>(keyVals);
        }
        public static IntPolynomial<TTerm> operator -(IntPolynomial<TTerm> value1, IntPolynomial<TTerm> value2) {
            var keys = value1.Coefficients.Keys.Union(value2.Coefficients.Keys);
            var keyVals = keys.SelectKeyValue(e => value1.Coefficient(e) - value2.Coefficient(e));
            return new IntPolynomial<TTerm>(keyVals);
        }
        public static IntPolynomial<TTerm> operator -(IntPolynomial<TTerm> value) {
            return new IntPolynomial<TTerm>(value.Coefficients.Select(e => e.Key.KeyVal(-e.Value)));
        }
        public static IntPolynomial<TTerm> operator *(IntPolynomial<TTerm> value1, IntPolynomial<TTerm> value2) {
            var keyVals = from c1 in value1.Coefficients
                          from c2 in value2.Coefficients
                          let key = c1.Key.Times(c2.Key)
                          let val = c1.Value*c2.Value
                          select key.KeyVal(val);
            return new IntPolynomial<TTerm>(keyVals);
        }
        public IntPolynomial<TTerm> RaisedTo(Int power) {
            if (power < 0) throw new ArgumentException();
            var curSquarePower = this;
            var total = (IntPolynomial<TTerm>)1;
            while (power > 0) {
                if (!power.IsEven) total *= curSquarePower;
                curSquarePower *= curSquarePower;
                power >>= 1;
            }
            return total;
        }

        public static bool operator ==(IntPolynomial<TTerm> value1, IntPolynomial<TTerm> value2) {
            return value1.Coefficients.DictionaryEqual(value2.Coefficients);
        }
        public static bool operator !=(IntPolynomial<TTerm> value1, IntPolynomial<TTerm> value2) {
            return !value1.Coefficients.DictionaryEqual(value2.Coefficients);
        }
        public bool Equals(IntPolynomial<TTerm> other) {
            return this == other;
        }
        public override bool Equals(object obj) {
            return obj is IntPolynomial<TTerm> && this == (IntPolynomial<TTerm>)obj;
        }
        public override int GetHashCode() {
            return Coefficients.Aggregate(Coefficients.Count.GetHashCode(), (a, e) => a ^ e.GetHashCode());
        }

        public override string ToString() {
            if (Coefficients.Count == 0) return "0";
            return string.Join(" + ",
                               Coefficients.OrderBy(e => e.Key).Reverse().Select(e => {
                                   var k = e.Key.ToString();
                                   var u = k == "" ? "1" : k;
                                   if (e.Value == 1) return u;
                                   if (e.Value == -1) return "-" + u;
                                   return e.Value + k;
                               }));
        }
    }
}
