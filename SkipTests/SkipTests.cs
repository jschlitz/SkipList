using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkipList;

namespace SkipTests
{
  [TestClass]
  public class SkipTests
  {
    /// <summary>
    /// Thin class to expose some internals for testing.
    /// </summary>
    class TestSkipList<T>:SkipList<T>
    {

       //but will get see changes in subfolders?
    #region constructors
    public TestSkipList(int seed, IComparer<T> c):base(seed, c)
    {}

    public TestSkipList():base()
    {}

    public TestSkipList(int seed): base(seed)
    {}

    public TestSkipList(IComparer<T> c):base(c)
    {}
    #endregion

      protected int GetHeightAccess()
      {
        return GetHeight();
      }

      public bool CheckInegrityAccess()
      {
        return CheckIngegrity();
      }
    }

    [TestMethod]
    public void TestMethod1()
    {
      //seeds
      BigMessOfTests(-1000, 1000);
      BigMessOfTests(int.MinValue, int.MaxValue);
    }

    private void BigMessOfTests(int minRange, int maxRange)
    {
      var listSeeds = new int[] { 33, 44, 03, 41, 57, 44 };
      var valueSeeds = new int[] { 78, 41, 12, 59, 97, 49 };

      for (int i = 0; i < listSeeds.Length; i++)
      {
        var target = new TestSkipList<int>(listSeeds[i]);
        var verf = new List<int>();

        //add some things that we'll remove later
        for (int j = -1000; j < 1000; j+=10)
        {
          verf.Add(j);
          target.Add(j);
          Assert.IsTrue(target.Contains(j));
        }

        verf.Sort();
        CheckIt(verf, target);
        
        //Some random values
        var r = new Random(valueSeeds[i]);
        for (int j = 0; j < 10000; j++)
        {
          int temp = r.Next(minRange, maxRange);
          verf.Add(temp);
          target.Add(temp);
        }
        verf.Sort();
        CheckIt(verf, target);

        //now remove items that we know to be in the list
        for (int j = -1000; j < 1000; j += 10)
        {
          Assert.IsTrue(verf.Remove(j));
          Assert.IsTrue(target.Remove(j));
        }
        Assert.AreEqual(verf.Count, target.Count);
        CheckIt(verf, target);

        //And just to be sure, add some new items.
        for (int j = 0; j < 1000; j++)
        {
          int temp = r.Next(int.MinValue, int.MaxValue);
          verf.Add(temp);
          target.Add(temp);
        }
        verf.Sort();
        CheckIt(verf, target);
      }
    }

    private void CheckIt(List<int> verf, TestSkipList<int> target)
    {
      Assert.AreEqual(verf.Count, target.Count);
      Assert.AreEqual(target.Count(), target.Count);
      Assert.AreEqual(target.Sum(x=>1), target.Count);
      
      int index = 0;
      foreach (int targetVal in target)
        Assert.AreEqual(targetVal, verf[index++]);

      target.CheckInegrityAccess();
    }
  }
}
