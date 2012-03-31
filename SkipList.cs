﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkipList
{
   //I hate git.
  /// <summary>
  /// Node from a skip list. Has a set height.
  /// </summary>
  public class SkipNode<T>
  {
    /// <summary>
    /// No parameterless constructor.
    /// </summary>
    private SkipNode(){}

    /// <summary>
    /// Create a node of a set height.
    /// </summary>
    public SkipNode(int height)
    {
      Neighbors = new SkipNode<T>[height];
    }

    /// <summary>
    /// Create a node with a set height and value.
    /// </summary>
    public SkipNode(int height, T value):this(height)
    {
      Value = value;
    }

    /// <summary>
    /// Neighbors of the node at various heights.
    /// </summary>
    private SkipNode<T>[] Neighbors { get; set; }

    /// <summary>
    /// Get the Neighbor node at a given height. Might be null.
    /// </summary>
    public SkipNode<T> this[int index]
    {
      get { return Neighbors[index]; }
      set { Neighbors[index] = value; }
    }
    
    /// <summary>
    /// How many neighbors do we keep track of anyway?
    /// </summary>
    public int Height
    {
      get { return Neighbors.Count(); }
    }

    /// <summary>
    /// The value of the node
    /// </summary>
    public T Value { get; set; }

    public override string ToString()
    {
      return "H:" + Height + "V:" + Value;
    }
  }

  /// <summary>
  /// Skiplist, a structure with log(n) add/remove/search. Probabalistically.
  /// </summary>
  public class SkipList<T> : ICollection<T>
  {
    #region constructors
    public SkipList(int seed, IComparer<T> c)
    {
      _Rand = seed > 0 ? new Random(seed) : new Random();
      Comparer = c;
      Count = 0;
      _Root = new SkipNode<T>(1);
    }

    public SkipList():this(-1, Comparer<T>.Default)
    {
    }

    public SkipList(int seed): this(seed, Comparer<T>.Default)
    {
    }

    public SkipList(IComparer<T> c): this(-1, c)
    {
    }
    #endregion

    private const double DENOMINATOR = 2.0;
    private const double FRACTION = 1/DENOMINATOR;
    private readonly Random _Rand;
    private SkipNode<T> _Root;

    /// <summary>
    /// Based on the current length + 1, get the random height for a new node
    /// </summary>
    protected int GetHeight()
    {
      double r = _Rand.NextDouble();
      int max = (int)Math.Ceiling(Math.Log(Count + 1, DENOMINATOR));
      int result = 0;
      for (int i = 0; i < max; i++)
      {
        if(r > FRACTION) break;
        result = i;
        r = r*DENOMINATOR;
      }

      return result + 1;
    }

    /// <summary>
    /// Make sure that it is monotonicly increasing at all nodeheights.
    /// </summary>
    protected virtual bool CheckIngegrity()
    {
    }

    /// <summary>
    /// Used to compare elements
    /// </summary>
    public IComparer<T> Comparer { get; protected set; }

    public override string ToString()
    {
      var result = new StringBuilder();
      var current = _Root;
      while (current[0] != null)
      {
        current = current[0];
        result.Append(current.ToString());
        result.Append(", ");
      }
      return result.ToString();
    }

    internal void Testy()
    {
    }

    #region ICollection<T> Members

    /// <summary>
    /// Add an item to the list. Log(n) operation.
    /// </summary>
    public void Add(T item)
    {
      //How high is this node?
      var height = GetHeight();

      //see if we have to readjust the root
      if (height > _Root.Height)
      {
        var newRoot = new SkipNode<T>(height);
        for (int i = 0; i < _Root.Height; i++)
          newRoot[i] = _Root[i];
        _Root = newRoot;
      }

      //find the one that it goes just after
      var predecessors = GetPredecessors(item);

      //rethread the references
      var itemNode = new SkipNode<T>(height, item);
      for (int i = 0; i < height; i++)
      {
        itemNode[i] = predecessors[i][i];
        predecessors[i][i] = itemNode;
      }

      //And now we're bigger.
      Count++;
    }

    public bool Remove(T item)
    {
      //find the one that it goes just after
      var predecessors = GetPredecessors(item);
      if (predecessors[0] == null) return false;

      var deleteMe = predecessors[0][0];
      if (deleteMe == null) return false;

      //did we not find it?
      if ( Comparer.Compare(deleteMe.Value, item) != 0) return false;

      //rethread the references. the trick is that we don't care about predecessors at a height > the deleted node's
      for (int i = 0; i < deleteMe.Height; i++)
        predecessors[i][i] = deleteMe[i];
      
      //And now we're smaller.
      Count--;
      return true;
    }

    protected SkipNode<T>[] BuildUpdateTable(T value)
    {
      SkipNode<T>[] updates = new SkipNode<T>[_Root.Height];
      SkipNode<T> current = _Root;

      // determine the nodes that need to be updated at each level
      for (int i = _Root.Height - 1; i >= 0; i--)
      {
        while (current[i] != null && Comparer.Compare(current[i].Value, value) < 0)
          current = current[i];

        updates[i] = current;
      }

      return updates;
    }

    private SkipNode<T>[] GetPredecessors(T item)
    {
      var predecessors = new SkipNode<T>[_Root.Height];
      var current = _Root;

      for (int i = current.Height - 1; i >= 0; i--)
      {

        //go either til we find it, or the last one at this level that is less than item
        while (current[i] != null &&
               //(Comparer.Compare(item, current[i].Value) >= 0))
               (Comparer.Compare(item, current[i].Value) > 0))
        {
          current = current[i];
        }
        predecessors[i] = current;

      }

      return predecessors;
    }

    /// <summary>
    /// Scotch the list.
    /// </summary>
    public void Clear()
    {
      _Root = new SkipNode<T>(1);
    }

    /// <summary>
    /// Does the list contain the item?
    /// </summary>
    public bool Contains(T item)
    {
      var current = _Root;
      for (int i = current.Height - 1; i >= 0; i--)
      {
        //go either til we find it, or the last one at this level that is less than item
        while (current[i] != null)
        {
          int comp = Comparer.Compare(item, current[i].Value);
          if (comp < 0) break;
          if (comp == 0) return true;
          if (comp > 0)
            current = current[i];
        }
      }
      
      return false;
    }

    /// <summary>
    /// Copy all of the skiplist's contents into array, starting at array[arrayIndex]
    /// </summary>
    public void CopyTo(T[] array, int arrayIndex)
    {
      var current = _Root[0];
      int i = arrayIndex;
      while (current != null)
      {
        array[i++] = current.Value;
        current = current[0];
      }
    }

    /// <summary>
    /// How many elements do we have?
    /// </summary>
    public int Count { get; protected set; }

    public bool IsReadOnly
    {
      get { return false; }
    }

    #endregion

    #region IEnumerable<T> Members

    protected class SkipEnumerator : IEnumerator<T>
    {
      public SkipEnumerator (SkipList<T> list)
      {
        _Current = list._Root;
        _TheList = list;
      }

      private SkipNode<T> _Current;
      private SkipList<T> _TheList;
      

      #region Implementation of IDisposable

      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
      /// </summary>
      /// <filterpriority>2</filterpriority>
      public void Dispose()
      {}

      #endregion

      #region Implementation of IEnumerator

      /// <summary>
      /// Advances the enumerator to the next element of the collection.
      /// </summary>
      /// <returns>
      /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
      /// </returns>
      /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
      public bool MoveNext()
      {
        bool result = (_Current[0] != null);
        if (result)
          _Current = _Current[0];
        return result;
      }

      /// <summary>
      /// Sets the enumerator to its initial position, which is before the first element in the collection.
      /// </summary>
      /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
      public void Reset()
      {
        _Current = _TheList._Root;
      }

      /// <summary>
      /// Gets the element in the collection at the current position of the enumerator.
      /// </summary>
      /// <returns>
      /// The element in the collection at the current position of the enumerator.
      /// </returns>
      public T Current
      {
        get { return _TheList._Root == _Current ? default(T) : _Current.Value; }
      }

      /// <summary>
      /// Gets the current element in the collection.
      /// </summary>
      /// <returns>
      /// The current element in the collection.
      /// </returns>
      /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception><filterpriority>2</filterpriority>
      object IEnumerator.Current
      {
        get { return Current; }
      }

      #endregion
    }

    public IEnumerator<T> GetEnumerator()
    {
      return new SkipEnumerator(this);
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new SkipEnumerator(this);
    }

    #endregion
  }
}
