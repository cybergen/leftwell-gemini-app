using System;
using System.Collections.Generic;

namespace BriLib
{
    public class Octree<T> where T : class
    {
        public enum Octant
        {
            TopLeftBack = 0,
            TopLeftFront = 1,
            BottomLeftBack = 2,
            BottomLeftFront = 3,
            TopRightBack = 4,
            TopRightFront = 5,
            BottomRightBack = 6,
            BottomRightFront = 7,
        }

        public Tuple<float, float, float> Center { get { return new Tuple<float, float, float>(_bounds.X, _bounds.Y, _bounds.Z); } }

        private int _maxObjectsPerNode;
        private ThreeDimensionalBoundingBox _bounds;
        private List<ThreeDimensionalPoint<T>> _children;
        private Octree<T>[] _subtrees;
        private Dictionary<T, ThreeDimensionalPoint<T>> _childMap;

        public Octree(ThreeDimensionalBoundingBox bounds, int maxObjsPerNode)
            : this(bounds, maxObjsPerNode, new Dictionary<T, ThreeDimensionalPoint<T>>()) { }

        public Octree(float centerX, float centerY, float centerZ, float radius, int maxObjs)
            : this(new ThreeDimensionalBoundingBox(centerX, centerY, centerZ, radius), maxObjs) { }

        private Octree(ThreeDimensionalBoundingBox bounds, int maxObjs, Dictionary<T, ThreeDimensionalPoint<T>> childMap)
        {
            if (maxObjs <= 0)
            {
                throw new System.ArgumentOutOfRangeException("Must allow at least 1 object per octree node");
            }

            _maxObjectsPerNode = maxObjs;
            _bounds = bounds;
            _children = new List<ThreeDimensionalPoint<T>>();
            _subtrees = new Octree<T>[8];
            _childMap = childMap;
        }

        public Octant GetOctant(float x, float y, float z)
        {
            var isUp = y >= _bounds.Y;
            var isLeft = x < _bounds.X;
            var isBack = z >= _bounds.Z;

            if (isUp && isLeft && isBack) return Octant.TopLeftBack;
            if (isUp && !isLeft && isBack) return Octant.TopRightBack;
            if (isUp && isLeft && !isBack) return Octant.TopLeftFront;
            if (isUp && !isLeft && !isBack) return Octant.TopRightFront;
            if (!isUp && isLeft && isBack) return Octant.BottomLeftBack;
            if (!isUp && !isLeft && isBack) return Octant.BottomRightBack;
            if (!isUp && isLeft && !isBack) return Octant.BottomLeftFront;
            return Octant.BottomRightFront;
        }

        public void Insert(float x, float y, float z, T obj)
        {
            if (_childMap.ContainsKey(obj))
            {
                throw new System.ArgumentOutOfRangeException("Cannot add object already in tree: " + obj);
            }

            if (!_bounds.Intersects(x, y, z))
            {
                var vectorString = string.Format("[x={0}, y={1}, z={2}]", x, y, z);
                var str = string.Format("Attempted to add point {0} outside of range of bounding box {1}", vectorString, _bounds.ToString());
                throw new System.ArgumentOutOfRangeException(str);
            }

            if (_children.Count >= _maxObjectsPerNode)
            {
                Subdivide();
            }

            var hasSubTrees = _subtrees[0] != null;            
            if (hasSubTrees)
            {
                _subtrees[(int)GetOctant(x, y, z)].Insert(x, y, z, obj);
            }
            else
            {
                var point = new ThreeDimensionalPoint<T>(x, y, z, obj);
                _children.Add(point);
                _childMap.Add(obj, point);
            }
        }

        public IEnumerable<T> GetRange(float x, float y, float z, float radius)
        {
            return GetRange(new ThreeDimensionalBoundingBox(x, y, z, radius));
        }

        public IEnumerable<T> GetRange(ThreeDimensionalBoundingBox bounds)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                if (bounds.Intersects(_children[i].X, _children[i].Y, _children[i].Z))
                {
                    yield return _children[i].StoredObject;
                }
            }

            for (int i = 0; i < _subtrees.Length; i++)
            {
                if (_subtrees[i] == null || !_subtrees[i]._bounds.Intersects(bounds)) continue;
                for (var enumerator = _subtrees[i].GetRange(bounds).GetEnumerator(); enumerator.MoveNext();)
                {
                    yield return enumerator.Current;
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
            if (_subtrees[(int)GetOctant(point.X, point.Y, point.Z)].Remove(obj))
            {
                EvaluateSubtrees();
            }
            return false;
        }

        public T GetNearestObject(float x, float y, float z)
        {
            //If we have leaf nodes, check for closest and return it
            if (_children.Count > 0)
            {
                var nearestDistance = float.MaxValue;
                T nearestChild = null;

                for (int i = 0; i < _children.Count; i++)
                {
                    var child = _children[i];
                    var distance = MathHelpers.Distance(x, y, z, child.X, child.Y, child.Z);
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
            var startOctant = (int)GetOctant(x, y, z);
            var distanceToBest = float.MaxValue;
            var best = _subtrees[startOctant].GetNearestObject(x, y, z);
            if (best != null)
            {
                var loc = _childMap[best];
                distanceToBest = MathHelpers.Distance(x, y, z, loc.X, loc.Y, loc.Z);
            }

            //Only search other octants that have distance at nearest edge less than distance to current best point.
            //Start by sorting the list of octants by distance, excluding ones that are too far or already visited.
            List<Tuple<int, float>> subtreeList = new List<Tuple<int, float>>();
            for (var enumerator = System.Enum.GetValues(typeof(Octant)).GetEnumerator(); enumerator.MoveNext();)
            {
                //Skip octant if it is the start octant
                var octantIndex = (int)enumerator.Current;
                if (octantIndex == startOctant) continue;

                //If distance to nearest point of octant is greater than current best, skip
                var distanceToOctant = _subtrees[octantIndex]._bounds.BoundsDistance(x, y, z);
                if (distanceToBest <= distanceToOctant) continue;

                //Otherwise, add the octant to the list, sorted in order of ascending distance
                var octantEntry = new Tuple<int, float>(octantIndex, distanceToOctant);
                if (subtreeList.Count == 0)
                {
                    subtreeList.Add(octantEntry);
                }
                else
                {
                    for (int i = 0; i < subtreeList.Count; i++)
                    {
                        if (octantEntry.Item2 < subtreeList[i].Item2)
                        {
                            subtreeList.Insert(i, octantEntry);
                            break;
                        }
                        else if (i == subtreeList.Count - 1)
                        {
                            subtreeList.Add(octantEntry);
                            break;
                        }
                    }
                }
            }

            //Go through sorted octant list and try to better our best
            for (int i = 0; i < subtreeList.Count; i++)
            {
                //If we have already found something closer than current octant, exit early
                if (distanceToBest < subtreeList[i].Item2) break;

                //Check if octant has a candidate for neighbor
                var candidate = _subtrees[subtreeList[i].Item1].GetNearestObject(x, y, z);
                if (candidate == null) continue;

                //If candidate distance is shorter than current best, replace current best
                var candidateLoc = _childMap[candidate];
                var candidateDistance = MathHelpers.Distance(x, y, z, candidateLoc.X, candidateLoc.Y, candidateLoc.Z);
                if (candidateDistance < distanceToBest)
                {
                    best = candidate;
                    distanceToBest = candidateDistance;
                }
            }

            return best;
        }

        private void Subdivide()
        {
            var rad = _bounds.Radius / 2f;
            var x = _bounds.X;
            var y = _bounds.Y;
            var z = _bounds.Z;

            var tlbBox = new ThreeDimensionalBoundingBox(x - rad, y + rad, z + rad, rad);
            var trbBox = new ThreeDimensionalBoundingBox(x + rad, y + rad, z + rad, rad);
            var tlfBox = new ThreeDimensionalBoundingBox(x - rad, y + rad, z - rad, rad);
            var trfBox = new ThreeDimensionalBoundingBox(x + rad, y + rad, z - rad, rad);
            var blbBox = new ThreeDimensionalBoundingBox(x - rad, y - rad, z + rad, rad);
            var brbBox = new ThreeDimensionalBoundingBox(x + rad, y - rad, z + rad, rad);
            var blfBox = new ThreeDimensionalBoundingBox(x - rad, y - rad, z - rad, rad);
            var brfBox = new ThreeDimensionalBoundingBox(x + rad, y - rad, z - rad, rad);

            _subtrees[(int)Octant.TopLeftBack] = new Octree<T>(tlbBox, _maxObjectsPerNode, _childMap);
            _subtrees[(int)Octant.TopRightBack] = new Octree<T>(trbBox, _maxObjectsPerNode, _childMap);
            _subtrees[(int)Octant.TopLeftFront] = new Octree<T>(tlfBox, _maxObjectsPerNode, _childMap);
            _subtrees[(int)Octant.TopRightFront] = new Octree<T>(trfBox, _maxObjectsPerNode, _childMap);
            _subtrees[(int)Octant.BottomLeftBack] = new Octree<T>(blbBox, _maxObjectsPerNode, _childMap);
            _subtrees[(int)Octant.BottomRightBack] = new Octree<T>(brbBox, _maxObjectsPerNode, _childMap);
            _subtrees[(int)Octant.BottomLeftFront] = new Octree<T>(blfBox, _maxObjectsPerNode, _childMap);
            _subtrees[(int)Octant.BottomRightFront] = new Octree<T>(brfBox, _maxObjectsPerNode, _childMap);

            for (int i = 0; i < _children.Count; i++)
            {
                var childX = _children[i].X;
                var childY = _children[i].Y;
                var childZ = _children[i].Z;
                var child = _children[i].StoredObject;

                _childMap.Remove(_children[i].StoredObject);
                _subtrees[(int)GetOctant(childX, childY, childZ)].Insert(childX, childY, childZ, child);
            }

            _children = new List<ThreeDimensionalPoint<T>>();
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
                _subtrees = new Octree<T>[8];
            }
        }
    }
}
