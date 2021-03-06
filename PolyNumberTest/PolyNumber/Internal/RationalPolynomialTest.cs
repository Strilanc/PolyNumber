﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Strilanc.PolyNumber.Internal;
using Numerics;
using Strilanc.LinqToCollections;
using Strilanc.Value;

[TestClass]
public class RationalPolynomialTest {
    [TestMethod]
    public void AdditionTest() {
        var p1 = Polynomial.FromBigEndianCoefficients(1, 2, 3);
        var p2 = Polynomial.FromBigEndianCoefficients(2, 3, 5, 7);
        (p1 + p2).AssertEquals(Polynomial.FromBigEndianCoefficients(2, 4, 7, 10));
    }
    [TestMethod]
    public void MultiplicationTest() {
        var p1 = Polynomial.FromBigEndianCoefficients(1, 2);
        var p2 = Polynomial.FromBigEndianCoefficients(2, 3, 5);
        (p1 * p2).AssertEquals(Polynomial.FromBigEndianCoefficients(2, 7, 11, 10));
    }
    [TestMethod]
    public void RaisedToTest() {
        Polynomial.FromBigEndianCoefficients(1).RaisedTo(10000000).AssertEquals(Polynomial.FromBigEndianCoefficients(1));
        Polynomial.FromBigEndianCoefficients(1, 0).RaisedTo(10000000).AssertSimilar(new XTerm(10000000).KeyVal(BigRational.One));
        Polynomial.FromBigEndianCoefficients(2, 0).RaisedTo(50).AssertSimilar(new XTerm(50).KeyVal(BigRational.Pow(2, 50)));
        Polynomial.FromBigEndianCoefficients(1, 1).RaisedTo(5).AssertSimilar(Polynomial.FromBigEndianCoefficients(1, 5, 10, 10, 5, 1));

        Polynomial.FromBigEndianCoefficientsOverVar2Of2(1, 0).RaisedTo(10000000).AssertSimilar(new XYTerm(0, 10000000).KeyVal(BigRational.One));
    }
    [TestMethod]
    public void MultiplicationTest2() {
        var p1 = Polynomial.FromBigEndianCoefficientsOverVar1Of2(1, 2);
        var p2 = Polynomial.FromBigEndianCoefficientsOverVar2Of2(2, 3, 5);
        (p1 * p2).AssertEquals(new Polynomial<XYTerm>(new Dictionary<XYTerm, BigRational> {
            {new XYTerm(0, 0), 10},
            {new XYTerm(0, 1), 6},
            {new XYTerm(0, 2), 4},
            {new XYTerm(1, 0), 5},
            {new XYTerm(1, 1), 3},
            {new XYTerm(1, 2), 2}
        }));
    }
    [TestMethod]
    public void RootMultiplicationTest_Radicals() {
        var sqrt2 = Polynomial.FromBigEndianCoefficients(1, 0, -2);
        (sqrt2.EvaluateAt(1) < 0).AssertTrue();
        (sqrt2.EvaluateAt(2) > 0).AssertTrue();

        var sqrt2Squared = sqrt2.MultiplyRoots(sqrt2);
        sqrt2Squared.EvaluateAt(2).AssertSimilar(0);
        sqrt2Squared.EvaluateAt(-2).AssertSimilar(0); // bleh, extra solution

        var cubeRoot2 = Polynomial.FromBigEndianCoefficients(1, 0, 0, -2);
        var cubeRoot2Squared = cubeRoot2.MultiplyRoots(cubeRoot2);
        var cubeRoot2Cubed = cubeRoot2Squared.MultiplyRoots(cubeRoot2);
        cubeRoot2Cubed.EvaluateAt(2).AssertSimilar(0);
        (cubeRoot2Cubed.EvaluateAt(-2) == 0).AssertFalse();
    }
    [TestMethod]
    public void RootMultiplicationTest() {
        var p1 = Polynomial.FromBigEndianCoefficients(1, 0, -1).MultiplyRoots(Polynomial.FromBigEndianCoefficients(1, 3, 2));
        p1.AssertEquals(Polynomial.FromBigEndianCoefficients(1, 0, -5, 0, 4));

        var x1 = Polynomial.FromRoots(7, 11, 13);
        var x2 = Polynomial.FromRoots(2, 3, 5);
        var x3 = Polynomial.FromRoots(2 * 7, 2 * 11, 2 * 13, 3 * 7, 3 * 11, 3 * 13, 5 * 7, 5 * 11, 5 * 13);
        x1.MultiplyRoots(x2).AssertEquals(x3);
    }
    [TestMethod]
    public void RootMultiplicationTest2() {
        var roots1 = new BigRational[] { 1, -1 };
        var roots2 = new BigRational[] { 1, -1 };
        var x1 = Polynomial.FromRoots(roots1);
        var x2 = Polynomial.FromRoots(roots2);
        var x3 = Polynomial.FromRoots((from r1 in roots1 from r2 in roots2 select r1 * r2).Distinct());
        var x3b = x1.MultiplyRoots(x2);
        x3b.AssertEquals(x3);
    }

    [TestMethod]
    public void DividesTest() {
        Polynomial.FromBigEndianCoefficients(1, 1).Divides(Polynomial.FromBigEndianCoefficients(1, 0, -1)).AssertTrue();
        Polynomial.FromBigEndianCoefficients(1, -1).Divides(Polynomial.FromBigEndianCoefficients(1, 0, -1)).AssertTrue();
        Polynomial.FromRoots(2, 3).Divides(Polynomial.FromRoots(2, 3)).AssertTrue();
        Polynomial.FromRoots(2, 3).Divides(Polynomial.FromRoots(2, 3, 5)).AssertTrue();
        Polynomial.FromRoots(2, 3).Divides(Polynomial.FromRoots(2, 3, 6)).AssertTrue();

        Polynomial.FromRoots(2, 3).Divides(Polynomial.FromRoots(2, 6)).AssertFalse();
        Polynomial.FromRoots(1).Divides(Polynomial.FromRoots(2)).AssertFalse();
        Polynomial.FromRoots(1).Divides(Polynomial.FromRoots(0)).AssertFalse();

        Polynomial.FromRoots(0).Divides(Polynomial.FromRoots(0, 2)).AssertTrue();
    }

    [TestMethod]
    public void PerturbedRootMultiplicationTest() {
        var rng = new Random(2357);
        foreach (var repeat in 25.Range()) {
            var rootCount1 = rng.Next(5);
            var rootCount2 = rng.Next(5);

            var roots1 = rootCount1.Range().Select(e => (BigRational)rng.Next(-50, 50)).ToArray();
            var roots2 = rootCount2.Range().Select(e => (BigRational)rng.Next(-50, 50)).ToArray();

            var roots3Max = (from r1 in roots1 from r2 in roots2 select r1 * r2).ToArray();
            var roots3Min = (from r1 in roots1 from r2 in roots2 select r1 * r2).Distinct().ToArray();

            var poly1 = Polynomial.FromRoots(roots1);
            var poly2 = Polynomial.FromRoots(roots2);
            var actualPoly3 = poly1.MultiplyRoots(poly2);

            actualPoly3.Divides(Polynomial.FromRoots(roots3Max)).AssertTrue();
            Polynomial.FromRoots(roots3Min).Divides(actualPoly3).AssertTrue();
        }
    }

    [TestMethod]
    public void PerturbedRootMultiplicationTest_LowDegree() {
        var rng = new Random(23571113);
        foreach (var repeat in 200.Range()) {
            var rootCount1 = rng.Next(3);
            var rootCount2 = rng.Next(3);

            var roots1 = rootCount1.Range().Select(e => (BigRational)rng.Next(-50, 50)).ToArray();
            var roots2 = rootCount2.Range().Select(e => (BigRational)rng.Next(-50, 50)).ToArray();

            var roots3Max = (from r1 in roots1 from r2 in roots2 select r1 * r2).ToArray();
            var roots3Min = (from r1 in roots1 from r2 in roots2 select r1 * r2).Distinct().ToArray();

            var poly1 = Polynomial.FromRoots(roots1);
            var poly2 = Polynomial.FromRoots(roots2);
            var actualPoly3 = poly1.MultiplyRoots(poly2);

            actualPoly3.Divides(Polynomial.FromRoots(roots3Max)).AssertTrue();
            Polynomial.FromRoots(roots3Min).Divides(actualPoly3).AssertTrue();
        }
    }
}
