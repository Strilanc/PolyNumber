using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using MoreLinq;
using Strilanc.LinqToCollections;

static class NumUtil {
    public static BigInteger Factorial(this int n) {
        return ((BigInteger)n).Factorial();
        }
    public static BigInteger Factorial(this BigInteger n) {
        if (n == 0) return 1;
        return Factorial(n - 1)*n;
    }
    public static BigInteger Choose(this int n, BigInteger i) {
        return n.Factorial()/i.Factorial()/(n - i).Factorial();
    }
}
[TestClass]
public class PolynomialUtilTest {
    [TestMethod]
    public void Tessssss() {
        var es = 5.Range().Select(e => (BigInteger)e);
        for (var i1 = 0; i1 < es.Count; i1++) {
            for (var i2 = 0; i2 < es.Count; i2++) {
                for (var i3 = 0; i3 < es.Count; i3++) {
                    var list1 = es.Take(i1);
                    var list2 = es.TakeLast(i2);
                    var choiceCount = i3;

                    var d = from choice1 in list1.Indexes().ChooseWithReplacement(choiceCount, list2.Count)
                            let y = from item1 in choice1
                                    group item1 by item1
                                    into choiceCounted
                                    let item1Value = list1[choiceCounted.Key]
                                    let item1Count = choiceCounted.Count()
                                    let choices2 = list2.Choose(item1Count)
                                    select from choice2 in choices2
                                           let pairs = from item2 in choice2
                                                       select Tuple.Create(item1Value, item2)
                                           select pairs
                            from e in y.AllChoiceCombinations()
                            select e.Concat();

                    var expected = list1.Cross(list2).Choose(choiceCount).Select(e => e.ToArray()).ToArray();
                    d.AssertHasSimilarItemsIgnoringOrder(expected);

                    var coefs2 = list2.Count.RangeInclusive()
                                      .Select(i => list2.Choose(i).Select(e => e.Product()).Sum())
                                      .ToArray();
                    var coefs1 = list1.Count.RangeInclusive()
                                      .Select(i => list1.Choose(i).Select(e => e.Product()).Sum())
                                      .ToArray();
                    var decreasings = CollectionUtil.DecreasingSequencesOfSize(
                        length: list1.Count, 
                        total: choiceCount, 
                        max: list2.Count);
                    var d2 =
                        decreasings
                        .SelectMany(f => f.DistinctPermutations())
                        .Select(x4 =>
                            x4
                            .Select((e, i) => coefs2[e] * BigInteger.Pow(list1[i], e))
                            .Product())
                        .Sum();
                    var d3 =
                        (from f in decreasings
                         let f2 = f.Select(e => coefs2[e]).Product()
                         let y = from p in f.DistinctPermutations()
                                 select (from r in p.Index()
                                         select BigInteger.Pow(list1[r.Key], r.Value)
                                        ).Product()
                         select y.Sum()*f2
                        ).Sum();
                    var coefRepeatSequences =
                        from decreasing in decreasings
                        select from count in decreasing
                               group count by count
                                   into sameCounts
                                   let coef = sameCounts.Key
                                   let repeat = sameCounts.Count()
                                   where repeat > 0
                                   select new { coef, repeat };
                    var d4 =
                        (from f in coefRepeatSequences
                         let f2 = f.Select(e => BigInteger.Pow(coefs2[e.coef], e.repeat)).Product()
                         let zz = f.SelectMany(e => Enumerable.Repeat(e.coef, e.repeat))
                         let y = from p in zz.DistinctPermutations()
                                 let q = p.SelectMany((e,i) => Enumerable.Repeat(i, e))
                                 let qq = q.Select(i => list1[i])
                                 select qq.Product()
                         select y.Sum() * f2
                        ).Sum();
                    var d5 =
                        (from coefRepeatSequence in decreasings
                         let coefs2Factor = 
                            coefRepeatSequence
                            .Select(e => coefs2[e])
                            .Product()
                         let coefs1Factor = F(coefRepeatSequence, list1, coefs1)
                         select coefs2Factor*coefs1Factor
                        ).Sum();
                    var r1 = expected.Select(e => e.Select(f => f.Product()).Product()).Sum();
                    r1.AssertEquals(d2);
                    r1.AssertEquals(d3);
                    r1.AssertEquals(d4);
                    r1.AssertEquals(d5);
                }
            }
        }
    }
    //public static PolyNumber RootTermsToCoefs(Dictionary<Term, BigInteger> terms, int degree) {
    //    var worst = terms
    //        .MaxesBy(e => e.Key.Powers.Sum())
    //        .MaxBy(e => e.Value);

    //    var roots = degree.Range().Select(e => new Term(degree, e));
    //    var coef = worst.Key.Powers.Sum();

    //    var coef = new 
    //    var best = terms.MaxBy(e => e.Powers.Max());
    //}
    public struct Term {
        private readonly IReadOnlyList<int> _powers;
        public IReadOnlyList<int> Powers { get { return _powers ?? ReadOnlyList.Empty<int>(); } }
        public Term(int degree, int root) {
            if (root >= degree) throw new ArgumentException();
            if (root < 0) throw new ArgumentException();

            _powers = Enumerable.Repeat(0, root).Concat(new[] {1}).Pad(degree).ToArray();
        }
        public Term(IReadOnlyList<int> powers) {
            if (powers == null) throw new ArgumentNullException("powers");
            _powers = powers;
        }
        public bool Equals(Term other) {
            return Powers.SequenceEqual(other.Powers);
        }
        public static Term operator *(Term v1, Term v2) {
            if (v1.Powers.Count != v2.Powers.Count) throw new ArgumentException();
            return new Term(v1.Powers.Zip(v2.Powers, (e1, e2) => e1 + e2).ToArray());
        }
        public override int GetHashCode() {
            return Powers.Aggregate(Powers.Count, (a, e) => a * 31 + e.GetHashCode());
        }
        public override bool Equals(object obj) {
            return obj is Term && Equals((Term)obj);
        }
    }
    private static BigInteger F(IReadOnlyList<int> d, IReadOnlyList<BigInteger> roots, IReadOnlyList<BigInteger> coefs) {
        return (from p in d.DistinctPermutations()
                select p.Select((e, i) => BigInteger.Pow(roots[i], e)).Product()
               ).Sum();
    }
    private static BigInteger F2(IReadOnlyList<int> d, IReadOnlyList<BigInteger> roots) {
        var zeroes = d.Count(e => e == 0);
        if (zeroes == d.Count) return 1;

        var min = d.Min();
        if (min > 0) {
            var mecs = d.Where(e => e > 0).Select(e => e - min).ToArray();
            return BigInteger.Pow(roots.Product(), min) * F2(mecs, roots);
        }

        var decs = d.Where(e => e > 0).ToArray();
        //return roots.Choose(d.Count - zeroes).Select(e => e.Product()).Sum() * F2(decs, roots);
        return (from x in roots.Choose(d.Count - zeroes)
                select F2(decs, x)
                ).Sum();
    }
    [TestMethod]
    public void TestXXX() {
        var roots = new BigInteger[] {2, 3, 5, 7, 11, 13, 17, 19, 23};
        foreach (var j in roots.Count().Range().Reverse()) {
            var x = roots.Take(j).ToArray();
            foreach (var i in x.Length.Range().Reverse()) {
                foreach (var r in CollectionUtil.DecreasingSequencesOfSize(x.Length, i, x.Length)) {
                    F(r, roots, null).AssertEquals(F2(r, x));
                }
            }
        }
    }
    //[TestMethod]
    //public void TestRootsToCoefficients() {
    //    new[] { 1, 2 }.RootsToCoefficients().AssertSequenceEquals(1, 3, 2);
    //    new[] { 3, 4 }.RootsToCoefficients().AssertSequenceEquals(1, 7, 12);
    //}

    //[TestMethod]
    //public void TempTest() {
    //    var p1 = PolyNumber.FromRoots(1, 2);
    //    var p2 = PolyNumber.FromRoots(3, 4);
    //    var p3 = p1*p2;
    //}
    //[TestMethod]
    //public void TempTest2() {
    //    var p1 = PolyNumber.FromRoots(1, 2);
    //    var p2 = PolyNumber.FromRoots(3, 4);
    //    var p3 = p1 + p2;
    //    var zz = p3.ComputeRoots(0.0001).ToArray();
    //    var p4 = PolyNumber.FromRoots(4, 5, 6);
    //}
    //[TestMethod]
    //public void TempTest3() {
    //    PolyNumber.FromRoots(1).ComputeRoots(0.1).AssertSequenceEquals(1);
    //    PolyNumber.FromRoots(1,2).ComputeRoots(0.1).AssertSequenceEquals(1,2);
    //}
}
