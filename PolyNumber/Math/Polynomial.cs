using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Numerics;
using Strilanc.LinqToCollections;
using Strilanc.Value;

namespace Math2 {
    [DebuggerDisplay("{ToString()}")]
    public struct Polynomial<TTerm> : IEquatable<Polynomial<TTerm>> where TTerm : ITerm<TTerm> {
        public static Polynomial<TTerm> Zero { get { return new Polynomial<TTerm>(); } }
        private readonly Dictionary<TTerm, BigRational> _coefficients;
        public IReadOnlyDictionary<TTerm, BigRational> Coefficients { get { return _coefficients ?? ReadOnlyDictionary.Empty<TTerm, BigRational>(); } }
        public BigRational Coefficient(TTerm power) {
            return Coefficients.MayGetValue(power).Else(0);
        }

        public Polynomial(IEnumerable<KeyValuePair<TTerm, BigRational>> coefficients) {
            if (coefficients == null) throw new ArgumentNullException("coefficients");
            _coefficients =
                coefficients
                    .KeyReduce(BigRational.Add)
                    .Where(e => e.Value != 0)
                    .ToDictionary();
        }

        public static Polynomial<TTerm> Add(Polynomial<TTerm> value1, Polynomial<TTerm> value2) {
            return value1 + value2;
        }
        public static Polynomial<TTerm> Multiply(Polynomial<TTerm> value1, Polynomial<TTerm> value2) {
            return value1 * value2;
        }

        public static implicit operator Polynomial<TTerm>(int value) {
            return (BigRational)value;
        }
        public static implicit operator Polynomial<TTerm>(BigInteger value) {
            return (BigRational)value;
        }
        public static implicit operator Polynomial<TTerm>(BigRational value) {
            return new Polynomial<TTerm>(new[] { default(TTerm).KeyVal(value) });
        }
        public static implicit operator Polynomial<TTerm>(KeyValuePair<TTerm, BigRational> value) {
            return new Polynomial<TTerm>(new[] { value });
        }
        public static implicit operator Polynomial<TTerm>(TTerm term) {
            return new Polynomial<TTerm>(new[] { term.KeyVal(BigRational.One) });
        }
        public static Polynomial<TTerm> operator +(Polynomial<TTerm> value1, Polynomial<TTerm> value2) {
            var keys = value1.Coefficients.Keys.Union(value2.Coefficients.Keys);
            var keyVals = keys.SelectKeyValue(e => value1.Coefficient(e) + value2.Coefficient(e));
            return new Polynomial<TTerm>(keyVals);
        }
        public static Polynomial<TTerm> operator -(Polynomial<TTerm> value1, Polynomial<TTerm> value2) {
            var keys = value1.Coefficients.Keys.Union(value2.Coefficients.Keys);
            var keyVals = keys.SelectKeyValue(e => value1.Coefficient(e) - value2.Coefficient(e));
            return new Polynomial<TTerm>(keyVals);
        }
        public static Polynomial<TTerm> operator -(Polynomial<TTerm> value) {
            return new Polynomial<TTerm>(value.Coefficients.Select(e => e.Key.KeyVal(-e.Value)));
        }
        public static Polynomial<TTerm> operator *(Polynomial<TTerm> value1, Polynomial<TTerm> value2) {
            var keyVals = from c1 in value1.Coefficients
                          from c2 in value2.Coefficients
                          let key = c1.Key.Times(c2.Key)
                          let val = c1.Value * c2.Value
                          select key.KeyVal(val);
            return new Polynomial<TTerm>(keyVals);
        }
        public static Polynomial<TTerm> operator /(Polynomial<TTerm> value1, BigRational denominator) {
            return value1.Coefficients.SelectKeyValue(e => e.Key, e => e.Value/denominator).ToPolynomial();
        }
        public Polynomial<TTerm> RaisedTo(BigInteger power) {
            if (power < 0) throw new ArgumentException();
            var curSquarePower = this;
            var total = (Polynomial<TTerm>)1;
            while (power > 0) {
                if (!power.IsEven) total *= curSquarePower;
                curSquarePower *= curSquarePower;
                power >>= 1;
            }
            return total;
        }

        public static bool operator ==(Polynomial<TTerm> value1, Polynomial<TTerm> value2) {
            return value1.Coefficients.DictionaryEqual(value2.Coefficients);
        }
        public static bool operator !=(Polynomial<TTerm> value1, Polynomial<TTerm> value2) {
            return !value1.Coefficients.DictionaryEqual(value2.Coefficients);
        }
        public bool Equals(Polynomial<TTerm> other) {
            return this == other;
        }
        public override bool Equals(object obj) {
            return obj is Polynomial<TTerm> && this == (Polynomial<TTerm>)obj;
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
                                   var c = e.Value.Denominator == 1 ? e.Value.Numerator.ToString() : e.Value.ToString();
                                   return c + k;
                               }));
        }
    }
}
