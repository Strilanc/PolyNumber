using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Numerics;
using Strilanc.LinqToCollections;

public struct PolyNumber {
    public readonly ImmutableArray<BigInteger> Coefficients;
    private PolyNumber(IEnumerable<BigInteger> coefficients) {
        Coefficients = 
            coefficients
            .SkipWhile(e => e == 0)
            .ToImmutableArray();
    }

    public BigInteger Coefficient(int index) {
        if (index < 0) throw new ArgumentOutOfRangeException();
        if (index >= Coefficients.Length) return 0;
        return Coefficients[index];
    }

    public int Degree { get { return Math.Max(0, Coefficients.Length - 1); }
    }

    public static PolyNumber FromCoefficients(IEnumerable<BigInteger> coefficients) {
        return new PolyNumber(coefficients);
    }
    public static PolyNumber FromRoots(params BigInteger[] roots) {
        return FromCoefficients(roots.Distinct().RootsToCoefficients());
    }
    public static PolyNumber FromValue(BigInteger value) {
        return FromRoots(new[] { value });
    }
    public static PolyNumber FromValue(int value) {
        return FromValue((BigInteger)value);
    }

    public PolyNumber Derivative() {
        return FromCoefficients(
            Coefficients.Length.Range().Reverse()
            .Zip(Coefficients, (i, e) => e*i)
            .SkipLast(1));
    }
    public BigRational EvaluateAt(BigRational x) {
        var t = BigRational.Zero;
        var f = BigRational.One;
        foreach (var coefficient in Coefficients.Reverse()) {
            t += f * coefficient;
            f *= x;
        }
        return t;
    }
    public IEnumerable<BigRational> ComputeRoots(BigRational epsilon) {
        if (epsilon <= 0) throw new ArgumentOutOfRangeException("epsilon");

        if (Degree == 0) throw new NotImplementedException();
        if (Degree == 1) {
            yield return -Coefficients[1] / (BigRational)Coefficients[0];
            yield break;
        }

        var s = this;
        var criticalPoints = Derivative()
            .ComputeRoots(epsilon)
            .Select(x => new { x, y = s.EvaluateAt(x)})
            .ToArray();
        var xx = (BigRational)500000;
        var transitions = 
            new[] {new {x=-xx,y=s.EvaluateAt(-xx)}}
            .Concat(criticalPoints)
            .Concat(new[] { new { x = xx, y = s.EvaluateAt(xx) } })
            .Window(2)
            .Where(e => e[0].y.Sign != e[1].y.Sign);
        foreach (var transition in transitions) {
            if (transition[0].y.Sign == 0) {
                yield return transition[0].x;
                continue;
            }
            if (transition[1].y.Sign == 0) {
                continue;
            }

            var left = transition[0].x;
            var right = transition[1].x;
            while (right - left > epsilon) {
                var midX = (left + right)/2;
                var midY = EvaluateAt(midX);
                if (BigRational.Abs(midY) < epsilon) {
                    yield return midX;
                    yield break;
                }
                if (midY.Sign == transition[0].y.Sign) {
                    left = midY;
                } else {
                    right = midY;
                }
            }
        }
    }
    public static PolyNumber operator +(PolyNumber value1, PolyNumber value2) {
        var degree = value1.Degree + value2.Degree;
        return FromCoefficients(
            degree.RangeInclusive()
            .Select(i => 
                i.RangeInclusive()
                .Select(j => value1.Coefficient(j)*value2.Coefficient(i - j))
                .Sum()));
    }
    public static PolyNumber operator *(PolyNumber value1, PolyNumber value2) {
        var degree = value1.Degree + value2.Degree;
        var roots1 = new BigInteger[] { 1, 2};
        var roots2 = new BigInteger[] {3, 4};
        var roots3 = roots1.Cross(roots2).Select(e => e.Product());

        var coefs3 = degree.RangeInclusive()
            .Select(i =>
                roots1.Cross(roots2)
                .Choose(i)
                .Select(e => e.Select(f => f.Product()))
                .Select(e => e.Product())
                .Sum())
            .ToArray();
        var coefs4 = degree.RangeInclusive()
            .Select(i =>
                roots1.Cross(roots2)
                .Choose(i)
                .Select(e => e.Select(f => f.Product()))
                .Select(e => e.Product())
                .Sum())
            .ToArray();
        if (!coefs3.SequenceEqual(coefs4)) throw new ArgumentOutOfRangeException(string.Join(", ", coefs4));

        return FromCoefficients(coefs3);
    }

    public override string ToString() {
        if (Coefficients.Length == 0) return "0";
        var coefs = Coefficients;
        var d = Degree;
        var terms = 
            from i in coefs.Length.Range()
            let p = d - i
            let c = coefs[i]
            where c != 0
            let factor = c == 1 ? "" : c+""
            let power = p == 0 ? ""
                      : p == 1 ? "x"
                      : "x^" + p
            select factor + power;
        var poly = string.Join(" + ", terms);
        return string.Format("Roots of {0} = 0", poly);
    }
}
