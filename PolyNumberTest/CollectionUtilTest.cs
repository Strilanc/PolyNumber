using System;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Strilanc.LinqToCollections;

[TestClass]
public class CollectionUtilTest {
    [TestMethod]
    public void TestChoose() {
        0.Range().Choose(0)
         .AssertSequenceSimilar(new[] { 0.Range() });
        0.Range().Choose(1)
         .AssertSequenceSimilar();

        1.Range().Choose(0)
         .AssertSequenceSimilar(new[] { 0.Range() });
        1.Range().Choose(1)
         .AssertSequenceSimilar(new[] { 1.Range() });
        1.Range().Choose(2)
         .AssertSequenceSimilar();

        3.Range().Choose(0)
         .AssertSequenceSimilar(new[] { 0.Range() });
        3.Range().Choose(1)
         .AssertSequenceSimilar(new[] { 0 }, new[] { 1 }, new[] { 2 });
        3.Range().Choose(2)
         .AssertSequenceSimilar(new[] { 0, 1 }, new[] { 0, 2 }, new[] { 1, 2 });
        3.Range().Choose(3)
         .AssertSequenceSimilar(new[] { 3.Range() });
        3.Range().Choose(4)
         .AssertSequenceSimilar();
    }

    [TestMethod]
    public void TestChooseWithReplacement() {
        0.Range().ChooseWithReplacement(0, Int32.MaxValue)
         .AssertSequenceSimilar(new[] { 0.Range() });
        0.Range().ChooseWithReplacement(1, Int32.MaxValue)
         .AssertSequenceSimilar();

        1.Range().ChooseWithReplacement(0, Int32.MaxValue)
         .AssertSequenceSimilar(new[] { 0.Range() });
        1.Range().ChooseWithReplacement(1, Int32.MaxValue)
         .AssertSequenceSimilar(new[] { 1.Range() });
        1.Range().ChooseWithReplacement(2, Int32.MaxValue)
         .AssertSequenceSimilar(new[] {Enumerable.Repeat(0, 2) });

        3.Range().ChooseWithReplacement(0, Int32.MaxValue)
         .AssertSequenceSimilar(new[] { 0.Range() });
        3.Range().ChooseWithReplacement(1, Int32.MaxValue)
         .AssertSequenceSimilar(new[] { 0 }, new[] { 1 }, new[] { 2 });
        3.Range().ChooseWithReplacement(2, Int32.MaxValue)
         .AssertSequenceSimilar(
            new[] { 0, 0 },
            new[] { 0, 1 }, 
            new[] { 0, 2 },
            new[] { 1, 1 },
            new[] { 1, 2 },
            new[] { 2, 2 });
        3.Range().ChooseWithReplacement(3, Int32.MaxValue)
         .AssertSequenceSimilar(
            new[] { 0, 0, 0 },
            new[] { 0, 0, 1 },
            new[] { 0, 0, 2 },
            new[] { 0, 1, 1 },
            new[] { 0, 1, 2 },
            new[] { 0, 2, 2 },
            new[] { 1, 1, 1 },
            new[] { 1, 1, 2 },
            new[] { 1, 2, 2 },
            new[] { 2, 2, 2 });
        3.Range().ChooseWithReplacement(3, 2)
         .AssertSequenceSimilar(
            new[] { 0, 0, 1 },
            new[] { 0, 0, 2 },
            new[] { 0, 1, 1 },
            new[] { 0, 1, 2 },
            new[] { 0, 2, 2 },
            new[] { 1, 1, 2 },
            new[] { 1, 2, 2 });
    }

    [TestMethod]
    public void TestConcat() {
        Enumerable.Empty<int[]>()
            .Concat()
            .AssertSequenceEquals(new int[0]);
        
        Enumerable.Repeat(new int[0], 10)
            .Concat()
            .AssertSequenceEquals(new int[0]);
        
        ReadOnlyList.Singleton(3.Range())
            .Concat()
            .AssertSequenceEquals(3.Range());

        new[] {5.Range(), 10.Range().Skip(5), 15.Range().Skip(10)}
            .Concat()
            .AssertSequenceEquals(15.Range());
    }

    [TestMethod]
    public void TestCrossList() {
        0.Range().Cross(5.Range())
            .AssertListEquals();

        5.Range().Cross(0.Range())
            .AssertListEquals();

        new[] { 1 }.Cross(2.Range())
            .AssertListEquals(
                Tuple.Create(1, 0),
                Tuple.Create(1, 1));

        new[] {1,2,3}.Cross(new[] {4,5})
            .AssertListEquals(
                Tuple.Create(1, 4),
                Tuple.Create(1, 5),
                Tuple.Create(2, 4),
                Tuple.Create(2, 5),
                Tuple.Create(3, 4),
                Tuple.Create(3, 5));

        new[] { 4, 5 }.Cross(new[] { 1, 2, 3 })
            .AssertListEquals(
                Tuple.Create(4, 1),
                Tuple.Create(4, 2),
                Tuple.Create(4, 3),
                Tuple.Create(5, 1),
                Tuple.Create(5, 2),
                Tuple.Create(5, 3));
    }

    [TestMethod]
    public void TestCrossEnumerable() {
        0.Range().AsEnumerable().Cross(5.Range())
            .AssertSequenceEquals();

        5.Range().AsEnumerable().Cross(0.Range())
            .AssertSequenceEquals();

        new[] { 1 }.AsEnumerable().Cross(2.Range())
            .AssertSequenceEquals(
                Tuple.Create(1, 0),
                Tuple.Create(1, 1));

        new[] { 1, 2, 3 }.AsEnumerable().Cross(new[] { 4, 5 })
            .AssertSequenceEquals(
                Tuple.Create(1, 4),
                Tuple.Create(1, 5),
                Tuple.Create(2, 4),
                Tuple.Create(2, 5),
                Tuple.Create(3, 4),
                Tuple.Create(3, 5));

        new[] { 4, 5 }.AsEnumerable().Cross(new[] { 1, 2, 3 })
            .AssertSequenceEquals(
                Tuple.Create(4, 1),
                Tuple.Create(4, 2),
                Tuple.Create(4, 3),
                Tuple.Create(5, 1),
                Tuple.Create(5, 2),
                Tuple.Create(5, 3));
    }

    [TestMethod]
    public void TestStream() {
        0.Range()
            .Stream(0, (a, e) => a + e + 1)
            .AssertSequenceEquals();

        5.Range()
            .Stream(0, (a, e) => a + e + 1)
            .AssertSequenceEquals(1, 3, 6, 10, 15);
    }

    [TestMethod]
    public void TestIndexesList() {
        0.Range().Indexes().AssertListEquals();
        5.Range().Indexes().AssertListEquals(5.Range());
        new[] {"a", "b"}.Indexes().AssertListEquals(0, 1);
    }

    [TestMethod]
    public void TestIndexesEnumerable() {
        0.Range().AsEnumerable().Indexes().AssertSequenceEquals();
        5.Range().AsEnumerable().Indexes().AssertSequenceEquals(5.Range());
        new[] { "a", "b" }.AsEnumerable().Indexes().AssertSequenceEquals(0, 1);
    }

    [TestMethod]
    public void TestWindowList() {
        0.Range().Window(1).AssertListSimilar();
        0.Range().Window(2).AssertListSimilar();

        1.Range().Window(1).AssertListSimilar(new[] {1.Range()});
        1.Range().Window(2).AssertListSimilar();

        2.Range().Window(1).AssertListSimilar(new[] { 0 }, new[] { 1 });
        2.Range().Window(2).AssertListSimilar(new[] { 2.Range() });
        2.Range().Window(3).AssertListSimilar();

        3.Range().Window(1).AssertListSimilar(new[] { 0 }, new[] { 1 }, new[] { 2 });
        3.Range().Window(2).AssertListSimilar(new[] { 0, 1 }, new[] { 1, 2 });
        3.Range().Window(3).AssertListSimilar(new[] { 3.Range() });
        3.Range().Window(4).AssertListSimilar();
    }

    [TestMethod]
    public void TestWindowEnumerable() {
        0.Range().AsEnumerable().Window(1).AssertSequenceSimilar();
        0.Range().AsEnumerable().Window(2).AssertSequenceSimilar();

        1.Range().AsEnumerable().Window(1).AssertSequenceSimilar(new[] { 1.Range() });
        1.Range().AsEnumerable().Window(2).AssertSequenceSimilar();

        2.Range().AsEnumerable().Window(1).AssertSequenceSimilar(new[] { 0 }, new[] { 1 });
        2.Range().AsEnumerable().Window(2).AssertSequenceSimilar(new[] { 2.Range() });
        2.Range().AsEnumerable().Window(3).AssertSequenceSimilar();

        3.Range().AsEnumerable().Window(1).AssertSequenceSimilar(new[] { 0 }, new[] { 1 }, new[] { 2 });
        3.Range().AsEnumerable().Window(2).AssertSequenceSimilar(new[] { 0, 1 }, new[] { 1, 2 });
        3.Range().AsEnumerable().Window(3).AssertSequenceSimilar(new[] { 3.Range() });
        3.Range().AsEnumerable().Window(4).AssertSequenceSimilar();
    }

    [TestMethod]
    public void RangeInclusiveTest() {
        TestingUtilities.AssertThrows(() => (-1).RangeInclusive());
        0.RangeInclusive().AssertListEquals(0);
        5.RangeInclusive().AssertListEquals(0, 1, 2, 3, 4, 5);
    }

    [TestMethod]
    public void BigIntAggTest() {
        new BigInteger[] { }.Sum().AssertSimilar(0);
        new BigInteger[] { 1 }.Sum().AssertSimilar(1);
        new BigInteger[] { 1, 2 }.Sum().AssertSimilar(3);
        new BigInteger[] { 1, 2, 3 }.Sum().AssertSimilar(6);

        new BigInteger[] { }.Product().AssertSimilar(1);
        new BigInteger[] { 1 }.Product().AssertSimilar(1);
        new BigInteger[] { 1, 2 }.Product().AssertSimilar(2);
        new BigInteger[] { 1, 2, 3 }.Product().AssertSimilar(6);

        Tuple.Create(BigInteger.One * 2, BigInteger.One * 3).Product().AssertSimilar(6);
    }

    [TestMethod]
    public void TestDecreasingSequences() {
        CollectionUtil.DecreasingSequencesOfSize(length: 3, total: 3, max: 2)
            .AssertSequenceSimilar(
                new[] { 2, 1, 0 },
                new[] { 1, 1, 1 });
     
        CollectionUtil.DecreasingSequencesOfSize(length: 3, total: 4, max: 2)
            .AssertSequenceSimilar(
                new[] { 2, 2, 0 },
                new[] { 2, 1, 1 });

        CollectionUtil.DecreasingSequencesOfSize(length: 3, total: 5, max: 3)
            .AssertSequenceSimilar(
                new[] { 3, 2, 0 },
                new[] { 3, 1, 1 },
                new[] { 2, 2, 1 });

        CollectionUtil.DecreasingSequencesOfSize(length: 3, total: 5, max: 2)
            .Single().AssertSequenceSimilar(2, 2, 1);
    }

    [TestMethod]
    public void TestPermutations() {
        0.Range().Permutations().Single().AssertSequenceSimilar(0.Range());
        1.Range().Permutations().Single().AssertSequenceSimilar(1.Range());
        2.Range().Permutations().AssertSequenceSimilar(new[] { 0, 1 }, new[] { 1, 0 });
        3.Range().Permutations().AssertSequenceSimilar(
            new[] { 0, 1, 2 },
            new[] { 0, 2, 1 },
            new[] { 1, 0, 2 },
            new[] { 1, 2, 0 },
            new[] { 2, 0, 1 },
            new[] { 2, 1, 0 });

        6.Range().Permutations().Count().AssertEquals(6*5*4*3*2);
    }
}
