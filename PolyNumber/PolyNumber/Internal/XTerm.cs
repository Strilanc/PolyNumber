using System;
using System.Diagnostics;
using Numerics;
using Int = System.Numerics.BigInteger;

namespace Strilanc.PolyNumber.Internal {
    /// <summary>
    /// One of the terms in a polynomial over an X variable, without its coefficient.
    /// For example, could be the "x^2" term in the polynomial "3x^2 + x + 2".
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    internal struct XTerm : ITerm<XTerm>, IComparable<XTerm> {
        public readonly Int XPower;
        public XTerm(Int xPower) {
            if (xPower < 0) throw new ArgumentOutOfRangeException();
            XPower = xPower;
        }
        public XTerm Times(XTerm other) {
            return new XTerm(XPower + other.XPower);
        }
        public static implicit operator XTerm(Int power) {
            return new XTerm(power);
        }
        public static Polynomial<XTerm> operator *(XTerm term, BigRational factor) {
            return term.KeyVal(factor);
        }
        public int CompareTo(XTerm other) {
            return XPower.CompareTo(other.XPower);
        }
        public override string ToString() {
            return XYTerm.PowerFactorString("x", XPower);
        }
    }
}
