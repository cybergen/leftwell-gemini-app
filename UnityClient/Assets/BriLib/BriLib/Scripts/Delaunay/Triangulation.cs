/*
 * Copyright (c) 2005, 2007 by L. Paul Chew.
 *
 * Permission is hereby granted, without written agreement and without
 * license or royalty fees, to use, copy, modify, and distribute this
 * software and its documentation for any purpose, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Linq;
using System.Collections.Generic;

/**
 * A 2D Delaunay Triangulation (DT) with incremental site insertion.
 *
 * This is not the fastest way to build a DT, but it's a reasonable way to build
 * a DT incrementally and it makes a nice interactive display. There are several
 * O(n log n) methods, but they require that the sites are all known initially.
 *
 * A Triangulation is a Set of Triangles. A Triangulation is unmodifiable as a
 * Set; the only way to change it is to add sites (via delaunayPlace).
 *
 * @author Paul Chew
 *
 * Created July 2005. Derived from an earlier, messier version.
 *
 * Modified November 2007. Rewrote to use AbstractSet as parent class and to use
 * the Graph class internally. Tried to make the DT algorithm clearer by
 * explicitly creating a cavity.  Added code needed to find a Voronoi cell.
 *
 */
namespace BriLib.Delaunay
{
  public class Triangulation
  {

    public IEnumerable<Triangle> Triangles { get { return triGraph.nodeSet(); } }

    private Triangle mostRecent = null;      // Most recently "active" triangle
    private Graph<Triangle> triGraph;        // Holds triangles for navigation

    /**
     * All sites must fall within the initial triangle.
     * @param triangle the initial triangle
     */
    public Triangulation(Triangle triangle)
    {
      triGraph = new Graph<Triangle>();
      triGraph.add(triangle);
      mostRecent = triangle;
    }

    public Triangulation()
    {
      triGraph = new Graph<Triangle>();
    }

    /* The following two methods are required by AbstractSet */
    public int size()
    {
      return triGraph.nodeSet().Count;
    }

    public override string ToString()
    {
      return "Triangulation with " + size() + " triangles";
    }

    /**
     * True iff triangle is a member of this triangulation.
     * This method isn't required by AbstractSet, but it improves efficiency.
     * @param triangle the object to check for membership
     */
    public bool Contains(Object triangle)
    {
      return triGraph.nodeSet().Contains((Triangle)triangle);
    }

    /**
     * Report neighbor opposite the given vertex of triangle.
     * @param site a vertex of triangle
     * @param triangle we want the neighbor of this triangle
     * @return the neighbor opposite site in triangle; null if none
     * @throws ArgumentException if site is not in this triangle
     */
    public Triangle neighborOpposite(Pnt site, Triangle triangle)
    {
      if (!triangle.Contains(site))
        throw new ArgumentException("Bad vertex; not in triangle");
      foreach (var neighbor in triGraph.neighbors(triangle))
      {
        if (!neighbor.Contains(site)) return neighbor;
      }
      return null;
    }

    /**
     * Return the set of triangles adjacent to triangle.
     * @param triangle the triangle to check
     * @return the neighbors of triangle
     */
    public List<Triangle> neighbors(Triangle triangle)
    {
      return triGraph.neighbors(triangle);
    }

    /**
     * Report triangles surrounding site in order (cw or ccw).
     * @param site we want the surrounding triangles for this site
     * @param triangle a "starting" triangle that has site as a vertex
     * @return all triangles surrounding site in order (cw or ccw)
     * @throws ArgumentException if site is not in triangle
     */
    public List<Triangle> surroundingTriangles(Pnt site, Triangle triangle)
    {
      if (!triangle.Contains(site))
        throw new ArgumentException("Site not in triangle");
      List<Triangle> list = new List<Triangle>();
      Triangle start = triangle;
      Pnt guide = triangle.getVertexButNot(site);        // Affects cw or ccw
      while (true)
      {
        list.AddIfNotContains(triangle);
        Triangle previous = triangle;
        triangle = neighborOpposite(guide, triangle); // Next triangle
        guide = previous.getVertexButNot(site, guide);     // Update guide
        if (triangle == start) break;
      }
      return list;
    }

    /**
     * Locate the triangle with point inside it or on its boundary.
     * @param point the point to locate
     * @return the triangle that holds point; null if no such triangle
     */
    public Triangle locate(Pnt point)
    {
      Triangle triangle = mostRecent;
      if (!this.Contains(triangle)) triangle = null;

      // Try a directed walk (this works fine in 2D, but can fail in 3D)
      List<Triangle> visited = new List<Triangle>();
      while (triangle != null)
      {
        if (visited.Contains(triangle))
        { // This should never happen
          //System.out.println("Warning: Caught in a locate loop");
          break;
        }
        visited.AddIfNotContains(triangle);
        // Corner opposite point
        Pnt corner = point.isOutside(triangle.ToArray());
        if (corner == null) return triangle;
        triangle = this.neighborOpposite(corner, triangle);
      }
      // No luck; try brute force
      //System.out.println("Warning: Checking all triangles for " + point);
      foreach (var tri in Triangles)
      {
        if (point.isOutside(tri.ToArray()) == null) return tri;
      }
      // No such triangle
      //System.out.println("Warning: No triangle holds " + point);
      return null;
    }

    /**
     * Place a new site into the DT.
     * Nothing happens if the site matches an existing DT vertex.
     * @param site the new Pnt
     * @throws ArgumentException if site does not lie in any triangle
     */
    public void delaunayPlace(Pnt site)
    {
      // Uses straightforward scheme rather than best asymptotic time

      // Locate containing triangle
      Triangle triangle = locate(site);
      // Give up if no containing triangle or if site is already in DT
      if (triangle == null)
        throw new ArgumentException("No containing triangle");
      if (triangle.Contains(site)) return;

      // Determine the cavity and update the triangulation
      List<Triangle> cavity = getCavity(site, triangle);
      mostRecent = update(site, cavity);
    }

    /**
     * Determine the cavity caused by site.
     * @param site the site causing the cavity
     * @param triangle the triangle containing site
     * @return set of all triangles that have site in their circumcircle
     */
    private List<Triangle> getCavity(Pnt site, Triangle triangle)
    {
      List<Triangle> encroached = new List<Triangle>();
      Queue<Triangle> toBeChecked = new Queue<Triangle>();
      List<Triangle> marked = new List<Triangle>();
      toBeChecked.Enqueue(triangle);
      marked.AddIfNotContains(triangle);
      while (toBeChecked.Count != 0)
      {
        triangle = toBeChecked.Dequeue();
        if (site.vsCircumcircle(triangle.ToArray()) == 1)
          continue; // Site outside triangle => triangle not in cavity
        encroached.AddIfNotContains(triangle);
        // Check the neighbors
        foreach (var neighbor in triGraph.neighbors(triangle))
        {
          if (marked.Contains(neighbor)) continue;
          marked.AddIfNotContains(neighbor);
          toBeChecked.Enqueue(neighbor);
        }
      }
      return encroached;
    }

    /**
     * Update the triangulation by removing the cavity triangles and then
     * filling the cavity with new triangles.
     * @param site the site that created the cavity
     * @param cavity the triangles with site in their circumcircle
     * @return one of the new triangles
     */
    private Triangle update(Pnt site, List<Triangle> cavity)
    {
      List<List<Pnt>> boundary = new List<List<Pnt>>();
      List<Triangle> theTriangles = new List<Triangle>();

      // Find boundary facets and adjacent triangles
      foreach (var triangle in cavity)
      {
        var neighborTriangles = neighbors(triangle);
        theTriangles.AddUniques(neighborTriangles);
        foreach (var vertex in triangle)
        {
          List<Pnt> facet = triangle.facetOpposite(vertex);

          int removeIndex = -1;
          for (int i = 0; i < boundary.Count; i++)
          {
            if (boundary[i].ListsEqual(facet)) removeIndex = i;
          }

          if (removeIndex != -1) boundary.RemoveAt(removeIndex);
          else boundary.Add(facet);
        }
      }
      theTriangles.RemoveAll(cavity);        // Adj triangles only

      // Remove the cavity triangles from the triangulation
      foreach (var triangle in cavity) triGraph.remove(triangle);

      // Build each new triangle and add it to the triangulation
      List<Triangle> newTriangles = new List<Triangle>();
      foreach (var vertices in boundary)
      {
        vertices.AddIfNotContains(site);
        Triangle tri = new Triangle(vertices);
        triGraph.add(tri);
        newTriangles.AddIfNotContains(tri);
      }

      // Update the graph links for each new triangle
      theTriangles.AddAll(newTriangles);    // Adj triangle + new triangles
      foreach (var triangle in newTriangles)
        foreach (var other in theTriangles)
          if (triangle.isNeighbor(other))
            triGraph.add(triangle, other);

      // Return one of the new triangles
      var enumerator = newTriangles.GetEnumerator();
      enumerator.MoveNext();
      return enumerator.Current;
    }

    public void AddExistingTriangles(List<Triangle> triangles)
    {
      foreach (var triangle in triangles) triGraph.add(triangle);
      foreach (var triangle in triangles)
      {
        foreach (var other in triangles)
        {
          if (triangle.isNeighbor(other)) triGraph.add(triangle, other);
        }
        mostRecent = triangle;
      }
    }

    /**
     * Main program; used for testing.
     */
    //public static void main (String[] args) {
    //    Triangle tri =
    //        new Triangle(new Pnt(-10,10), new Pnt(10,10), new Pnt(0,-10));
    //    System.out.println("Triangle created: " + tri);
    //    Triangulation dt = new Triangulation(tri);
    //    System.out.println("DelaunayTriangulation created: " + dt);
    //    dt.delaunayPlace(new Pnt(0,0));
    //    dt.delaunayPlace(new Pnt(1,0));
    //    dt.delaunayPlace(new Pnt(0,1));
    //    System.out.println("After adding 3 points, we have a " + dt);
    //    Triangle.moreInfo = true;
    //    System.out.println("Triangles: " + dt.triGraph.nodeSet());
    //}
  }
}
