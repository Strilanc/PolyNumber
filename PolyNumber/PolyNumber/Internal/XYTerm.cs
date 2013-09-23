using System;
using System.Diagnostics;
using Int = System.Numerics.BigInteger;

namespace Strilanc.PolyNumber.Internal {
    [DebuggerDisplay("{ToString()}")]
    internal struct XYTerm : ITerm<XYTerm>, IComparable<XYTerm> {
        public readonly Int XPower;
        public readonly Int YPower;
        public XYTerm(Int xPower, Int yPower) {
            if (xPower < 0) throw new ArgumentOutOfRangeException();
            if (yPower < 0) throw new ArgumentOutOfRangeException();
            XPower = xPower;
            YPower = yPower;
        }
        public XYTerm Times(XYTerm other) {
            return new XYTerm(XPower + other.XPower, YPower + other.YPower);
        }
        public static string PowerFactorString(string var, Int power) {
            if (power == 0) return "";
            if (power == 1) return var;
            if (power == 2) return var + "²";
            if (power == 3) return var + "³";
            return var + "^" + power;
        }
        public int CompareTo(XYTerm other) {
            if (XPower != other.XPower) return XPower.CompareTo(other.XPower);
            return YPower.CompareTo(other.YPower);
        }
        public override string ToString() {
            return PowerFactorString("x", XPower) + PowerFactorString("y", YPower);
        }
    }
}
