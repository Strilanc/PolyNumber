using System;
using System.Diagnostics;
using Int = System.Numerics.BigInteger;

namespace Math {
    [DebuggerDisplay("{ToString()}")]
    public struct XTerm : ITerm<XTerm>, IComparable<XTerm> {
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
        private static string PowerFactorString(string var, Int power) {
            if (power == 0) return "";
            if (power == 1) return var;
            if (power == 2) return var + "²";
            if (power == 3) return var + "³";
            return var + "^" + power;
        }
        public static IntPolynomial<XTerm> operator *(XTerm term, Int factor) {
            return term.KeyVal(factor);
        }
        public int CompareTo(XTerm other) {
            return XPower.CompareTo(other.XPower);
        }
        public override string ToString() {
            return PowerFactorString("x", XPower);
        }
    }
}
