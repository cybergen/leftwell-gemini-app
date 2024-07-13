using System;
using System.Collections.Generic;

namespace BriLib
{
  public class Quadtree<T> where T : class
  {
    public enum Quadrant
    {
      NorthWest = 0,
      NorthEast = 1,
      SouthEast = 2,
      SouthWest = 3,
    }

    public TwoDimensionalBoundingBox BoundingBox { get; private set; }
    public IEnumerable<TwoDimensionalBoundingBox> RecursiveBounds
    {
      get
      {
        yield return BoundingBox;
        for (int i = 0; i < _subtrees.Length; i++)
        {
          if (_subtrees[i] != null)
          {
            for (var enumerator = _subtrees[i].RecursiveBounds.GetEnumerator(); enumerator.MoveNext();)
            {
              yield return enumerator.Current;
            }
          }
        }
      }
    }

    private Quadtree<T>[] _subtrees;
    private int _maxObjectsPerNode;
    private List<TwoDimensionalPoint<T>> _children;
    private Dictionary<T, TwoDimensionalPoint<T>> _childMap;

    public Quadtree(TwoDimensionalBoundingBox boundingBox, int maxObjectsPerNode)
        : this(boundingBox, maxObjectsPerNode, new Dictionary<T, TwoDimensionalPoint<T>>()) { }

    public Quadtree(float centerX, float centerY, float radius, int maxObjectsPerNode)
        : this(new TwoDimensionalBoundingBox(centerX, centerY, radius), maxObjectsPerNode) { }

    private Quadtree(TwoDimensionalBoundingBox boundingBox, int maxObj, Dictionary<T, TwoDimensionalPoint<T>> childMap)
    {
      if (maxObj <= 0)
      {
        throw new System.ArgumentOutOfRangeException("Must allow at least 1 object per quadtree node");
      }

      _maxObjectsPerNode = maxObj;
      BoundingBox = boundingBox;
      _children = new List<TwoDimensionalPoint<T>>();
      _subtrees = new Quadtree<T>[4];
      _childMap = childMap;
    }

    public Quadrant GetQuadrant(float x, float y)
    {
      if (x < BoundingBox.X && y <= BoundingBox.Y) return Quadrant.NorthEast;
      if (x >= BoundingBox.X && y <= BoundingBox.Y) return Quadrant.NorthWest;
      if (x >= BoundingBox.X && y > BoundingBox.Y) return Quadrant.SouthWest;
      return Quadrant.SouthEast;
    }

    public void Insert(float x, float y, T obj)
    {
      if (_childMap.ContainsKey(obj))
      {
        throw new System.ArgumentOutOfRangeException("Cannot add object already in tree: " + obj);
      }

      var hasSubTrees = _subtrees[0] != null;

      if (_children.Count >= _maxObjectsPerNode)
      {
        Subdivide();
        _subtrees[(int)GetQuadrant(x, y)].Insert(x, y, obj);
      }
      else if (hasSubTrees)
      {
        _subtrees[(int)GetQuadrant(x, y)].Insert(x, y, obj);
      }
      else
      {
        var child = new TwoDimensionalPoint<T>(x, y, obj);
        _children.Add(child);
        _childMap.Add(obj, child);
      }
    }

    public IEnumerable<T> GetRange(float x, float y, float radius)
    {
      return GetRange(new TwoDimensionalBoundingBox(x, y, radius));
    }

    public IEnumerable<T> GetRange(TwoDimensionalBoundingBox bounds)
    {
      for (var enumerator = GetPointRange(bounds).GetEnumerator(); enumerator.MoveNext();)
      {
        yield return enumerator.Current.StoredObject;
      }
    }

    public IEnumerable<TwoDimensionalPoint<T>> GetPointRange(float x, float y, float radius)
    {
      return GetPointRange(new TwoDimensionalBoundingBox(x, y, radius));
    }

    public IEnumerable<TwoDimensionalPoint<T>> GetPointRange(TwoDimensionalBoundingBox bounds)
    {
      for (int i = 0; i < _children.Count; i++)
      {
        if (bounds.Intersects(_children[i].X, _children[i].Y))
        {
          yield return _children[i];
        }
      }

      for (int i = 0; i < _subtrees.Length; i++)
      {
        if (_subtrees[i] != null && _subtrees[i].BoundingBox.Intersects(bounds))
        {
          for (var enumerator = _subtrees[i].GetPointRange(bounds).GetEnumerator(); enumerator.MoveNext();)
          {
            yield return enumerator.Current;
          }
        }
      }
    }

    public bool Remove(T obj)
    {
      if (!_childMap.ContainsKey(obj))
      {
        throw new System.ArgumentOutOfRangeException("Cannot remove element that was not in tree: " + obj);
      }

      for (int i = 0; i < _children.Count; i++)
      {
        if (_children[i].StoredObject == obj)
        {
          _children.RemoveAt(i);
          _childMap.Remove(obj);
          return true;
        }
      }

      var point = _childMap[obj];
      if (_subtrees[(int)GetQuadrant(point.X, point.Y)].Remove(obj))
      {
        EvaluateSubtrees();
      }
      return false;
    }

    public T GetNearestObject(float x, float y)
    {
      //If we have leaf nodes, check for closest and return it
      if (_children.Count > 0)
      {
        var nearestDistance = float.MaxValue;
        T nearestChild = null;

        for (int i = 0; i < _children.Count; i++)
        {
          var child = _children[i];
          var distance = MathHelpers.Distance(x, y, child.X, child.Y);
          if (distance < nearestDistance)
          {
            nearestDistance = distance;
            nearestChild = child.StoredObject;
          }
        }
        return nearestChild;
      }

      //If we don't have children, return
      var hasSubTrees = _subtrees[0] != null;
      if (!hasSubTrees) return null;

      //First check for neighbor in the octant point is currently at. If we find something, store distance.
      var startQuadrant = (int)GetQuadrant(x, y);
      var distanceToBest = float.MaxValue;
      var best = _subtrees[startQuadrant].GetNearestObject(x, y);
      if (best != null)
      {
        var loc = _childMap[best];
        distanceToBest = MathHelpers.Distance(x, y, loc.X, loc.Y);
      }

      //Only search other octants that have distance at nearest edge less than distance to current best point.
      //Start by sorting the list of octants by distance, excluding ones that are too far or already visited.
      List<Tuple<int, float>> subtreeList = new List<Tuple<int, float>>();
      for (var enumerator = System.Enum.GetValues(typeof(Quadrant)).GetEnumerator(); enumerator.MoveNext();)
      {
        //Skip octant if it is the start octant
        var quadrantIndex = (int)enumerator.Current;
        if (quadrantIndex == startQuadrant) continue;

        //If distance to nearest point of octant is greater than current best, skip
        var distanceToQuadrant = _subtrees[quadrantIndex].BoundingBox.BoundsDistance(x, y);
        if (distanceToBest <= distanceToQuadrant) continue;

        //Otherwise, add the octant to the list, sorted in order of ascending distance
        var quadrantEntry = new Tuple<int, float>(quadrantIndex, distanceToQuadrant);
        if (subtreeList.Count == 0)
        {
          subtreeList.Add(quadrantEntry);
        }
        else
        {
          for (int i = 0; i < subtreeList.Count; i++)
          {
            if (quadrantEntry.Item2 < subtreeList[i].Item2)
            {
              subtreeList.Insert(i, quadrantEntry);
              break;
            }
            else if (i == subtreeList.Count - 1)
            {
              subtreeList.Add(quadrantEntry);
              break;
            }
          }
        }
      }

      //Go through sorted quadrant list and try to better our best
      for (int i = 0; i < subtreeList.Count; i++)
      {
        //If we have already found something closer than current quadrant, exit early
        if (distanceToBest < subtreeList[i].Item2) break;

        //Check if quadrant has a candidate for neighbor
        var candidate = _subtrees[subtreeList[i].Item1].GetNearestObject(x, y);
        if (candidate == null) continue;

        //If candidate distance is shorter than current best, replace current best
        var candidateLoc = _childMap[candidate];
        var candidateDistance = MathHelpers.Distance(x, y, candidateLoc.X, candidateLoc.Y);
        if (candidateDistance < distanceToBest)
        {
          best = candidate;
          distanceToBest = candidateDistance;
        }
      }

      return best;
    }

    protected void Subdivide()
    {
      var quarterRadius = BoundingBox.Radius / 2;
      var x = BoundingBox.X;
      var y = BoundingBox.Y;

      var neBox = new TwoDimensionalBoundingBox(x - quarterRadius, y - quarterRadius, quarterRadius);
      _subtrees[(int)Quadrant.NorthEast] = new Quadtree<T>(neBox, _maxObjectsPerNode, _childMap);

      var nwBox = new TwoDimensionalBoundingBox(x + quarterRadius, y - quarterRadius, quarterRadius);
      _subtrees[(int)Quadrant.NorthWest] = new Quadtree<T>(nwBox, _maxObjectsPerNode, _childMap);

      var seBox = new TwoDimensionalBoundingBox(x - quarterRadius, y + quarterRadius, quarterRadius);
      _subtrees[(int)Quadrant.SouthEast] = new Quadtree<T>(seBox, _maxObjectsPerNode, _childMap);

      var swBox = new TwoDimensionalBoundingBox(x + quarterRadius, y + quarterRadius, quarterRadius);
      _subtrees[(int)Quadrant.SouthWest] = new Quadtree<T>(swBox, _maxObjectsPerNode, _childMap);

      for (int i = 0; i < _children.Count; i++)
      {
        var childX = _children[i].X;
        var childY = _children[i].Y;
        _childMap.Remove(_children[i].StoredObject);
        _subtrees[(int)GetQuadrant(childX, childY)].Insert(childX, childY, _children[i].StoredObject);
      }

      _children = new List<TwoDimensionalPoint<T>>();
    }

    private void EvaluateSubtrees()
    {
      var clearChildren = true;
      for (var i = 0; i < _subtrees.Length; i++)
      {
        clearChildren &= _subtrees[i]._children.Count == 0;
      }

      if (clearChildren)
      {
        _subtrees = new Quadtree<T>[4];
      }
    }
  }
}
