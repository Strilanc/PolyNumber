using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using MoreLinq;
using Strilanc.LinqToCollections;

static class NumUtil {
    public static BigInteger Factorial(this BigInteger n) {
        if (n == 0) return 1;
        return Factorial(n - 1)*n;
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
                    var L = CollectionUtil.DecreasingSequencesOfSize(length: list1.Count, total: choiceCount, max: list2.Count);
                    var d2 =
                        L
                        .SelectMany(f => f.Permutations())
                        .Select(x4 =>
                            x4
                            .Select((e, i) => coefs2[e] * BigInteger.Pow(list1[i], e))
                            .Product())
                        .Sum();
                    var d3 =
                        (from f in L
                         let f2 = f.Select(e => coefs2[e]).Product()
                         let y = from p in f.Permutations()
                                 select (from r in p.Index()
                                         select BigInteger.Pow(list1[r.Key], r.Value)
                                        ).Product()
                         select y.Sum()*f2
                        ).Sum();
                    var z = from y in L
                            select from x in y
                                   group x by x
                                   into g
                                   let coef = g.Key
                                   let repeat = g.Count()
                                   where repeat > 0
                                   select new {coef, repeat};
                    var d4 =
                        (from f in z
                         let f2 = f.Select(e => BigInteger.Pow(coefs2[e.coef], e.repeat)).Product()
                         let zz = f.SelectMany(e => Enumerable.Repeat(e.coef, e.repeat))
                         let y = from p in zz.Permutations()
                                 let q = p.SelectMany((e,i) => Enumerable.Repeat(i, e))
                                 let qq = q.Select(i => list1[i])
                                 select qq.Product()
                         select y.Sum() * f2
                        ).Sum();
                    var d5 =
                        (from f in z
                         let f2 = f.Select(e => BigInteger.Pow(coefs2[e.coef], e.repeat)).Product()
                         let zz = f.SelectMany(e => Enumerable.Repeat(e.coef, e.repeat))
                         let y = from p in zz.Permutations()
                                 let q = p.SelectMany((e, i) => Enumerable.Repeat(i, e))
                                 let qq = q.Select(i => list1[i])
                                 select qq.Product()
                         select y.Sum() * f2
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
