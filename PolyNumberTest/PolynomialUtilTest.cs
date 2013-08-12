using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
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
        for (var i = 0; i < es.Count; i++) {
            for (var j = 0; j < es.Count; j++) {
                for (var k = 0; k < es.Count; k++) {
                    var list1 = es.Take(i);
                    var list2 = es.TakeLast(j);
                    var choiceCount = k;

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

                    var d2 = from choice1 in list1.Indexes().ChooseWithReplacement(choiceCount, list2.Count)
                             let y = from item1 in choice1
                                     group item1 by item1
                                     into choiceCounted
                                     let item1Value = list1[choiceCounted.Key]
                                     let item1Count = choiceCounted.Count()
                                     let t1 = BigInteger.Pow(item1Value, item1Count)
                                     select from choice2 in list2.Choose(item1Count)
                                            let t2 = choice2.Product()
                                            select t2*t1
                             from e in y.AllChoiceCombinations()
                             select e.Product();
                    var r1 = expected.Select(e => e.Select(f => f.Product()).Product()).Sum();
                    var r2 = d2.Sum();
                    r1.AssertEquals(r2);
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
