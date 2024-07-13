using System;
using System.Collections.Generic;
using UnityEngine;

namespace BriLib.Delaunay
{
  public class VoronoiDiagram
  {
    /// <summary>
    /// World-space cells
    /// </summary>
    public IEnumerable<VoronoiCell> Cells { get { return GetCells(); } }

    /// <summary>
    /// Object-space fragments
    /// </summary>
    public IEnumerable<VoronoiIntersection> Fragments { get { return GetFragments(); } }

    /// <summary>
    /// List of triangles generated by the intermediate delaunay triangulation
    /// </summary>
    public IEnumerable<Triangle> DelaunayTris { get { return _delaunay.Triangles; } }

    private Triangle _initial;
    private Triangulation _delaunay;
    private MeshFilter _mesh;
    private Transform _transform;

    public VoronoiDiagram(MeshFilter mesh, Transform transform)
    {
      _initial = new Triangle(new Pnt(-10000, -10000), new Pnt(10000, -10000), new Pnt(0, 10000));
      _delaunay = new Triangulation(_initial);
      _mesh = mesh;
      _transform = transform;
    }

    /// <summary>
    /// Object-space voronoi cell point to add to the diagram
    /// </summary>
    /// <param name="point">Object-space point</param>
    public void AddFacePoint(Vector3 point)
    {
      _delaunay.delaunayPlace(new Pnt(point.x, point.z));
    }

    private IEnumerable<VoronoiCell> GetCells()
    {
      var cellList = new List<VoronoiCell>();

      // Keep track of sites done; no drawing for initial triangles sites
      HashSet<Pnt> done = new HashSet<Pnt>(_initial);
      foreach (var triangle in _delaunay.Triangles)
      {
        foreach (var site in triangle)
        {
          if (done.Contains(site)) continue;

          done.Add(site);
          List<Triangle> list = _delaunay.surroundingTriangles(site, triangle);
          Pnt[] verts = new Pnt[list.Count];

          int i = 0;
          foreach (var tri in list) verts[i++] = tri.getCircumcenter();
          if (verts.Length < 3) continue;

          var cell = new VoronoiCell();
          var first = Vectorfy(verts[0]);
          for (int j = 2; j < verts.Length; j++) cell.Add(first, Vectorfy(verts[j - 1]), Vectorfy(verts[j]));
          cellList.Add(cell);
        }
      }

      return cellList;
    }

    private Vector3 Vectorfy(Pnt point)
    {
      return _transform.TransformPoint((float)point[0], _transform.position.y + 0.01f, (float)point[1]);
    }

    private IEnumerable<VoronoiIntersection> GetFragments()
    {
      throw new System.NotImplementedException("Not yet implemented");
    }
  }

  public struct VoronoiCell
  {
    public IEnumerable<Tuple<Vector3, Vector3, Vector3>> Triangles { get { return _triangles; } }

    private List<Tuple<Vector3, Vector3, Vector3>> _triangles;

    public void Add(Vector3 vectOne, Vector3 vectTwo, Vector3 vectThree)
    {
      if (_triangles == null) _triangles = new List<Tuple<Vector3, Vector3, Vector3>>();
      _triangles.Add(new Tuple<Vector3, Vector3, Vector3>(vectOne, vectTwo, vectThree));
    }
  }

  public struct VoronoiIntersection
  {
    public Vector3[] Vertices { get; private set; }
    public int[] Triangles { get; private set; }
    public Vector2[] UVs { get; private set; }
    public Vector3 WorldPosition { get; private set; }
  }
}
