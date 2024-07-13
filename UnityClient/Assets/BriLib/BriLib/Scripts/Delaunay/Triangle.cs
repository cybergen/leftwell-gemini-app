/*
 * Copyright (c) 2007 by L. Paul Chew.
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
 * A Triangle is an immutable Set of exactly three Pnts.
 *
 * All Set operations are available. Individual vertices can be accessed via
 * iterator() and also via triangle.get(index).
 *
 * Note that, even if two triangles have the same vertex set, they are
 * *different* triangles. Methods equals() and hashCode() are consistent with
 * this rule.
 *
 * @author Paul Chew
 *
 * Created December 2007. Replaced general simplices with geometric triangle.
 *
 */

namespace BriLib.Delaunay
{
  public class Triangle : List<Pnt>
  {

    private int idNumber;                   // The id number
    private Pnt circumcenter = null;        // The triangle's circumcenter

    private static int idGenerator = 0;     // Used to create id numbers

    public Triangle() { }

    /**
     * @param vertices the vertices of the Triangle.
     * @throws ArgumentException if there are not three distinct vertices
     */
    public Triangle(params Pnt[] vertices) : this(vertices.ToList()) { }

    /**
     * @param collection a Collection holding the Simplex vertices
     * @throws ArgumentException if there are not three distinct vertices
     */
    public Triangle(IEnumerable<Pnt> collection) : base(collection)
    {
      idNumber = idGenerator++;
      if (Count != 3)
        throw new ArgumentException("Triangle must have 3 vertices");
    }

    public override string ToString()
    {
      var s = "[Triangle" + idNumber;
      foreach (var entry in this)
      {
        s += " " + entry + ",";
      }
      return s + "]";
    }

    /**
     * Get arbitrary vertex of this triangle, but not any of the bad vertices.
     * @param badVertices one or more bad vertices
     * @return a vertex of this triangle, but not one of the bad vertices
     * @throws NoSuchElementException if no vertex found
     */
    public Pnt getVertexButNot(params Pnt[] badVertices)
    {
      var bad = badVertices.ToList();
      for (int i = 0; i < Count; i++) if (!bad.Contains(this[i])) return this[i];
      throw new MissingMemberException("No vertex found");
    }

    /**
     * True iff triangles are neighbors. Two triangles are neighbors if they
     * share a facet.
     * @param triangle the other Triangle
     * @return true iff this Triangle is a neighbor of triangle
     */
    public bool isNeighbor(Triangle triangle)
    {
      int count = 0;
      for (int i = 0; i < Count; i++) if (!triangle.Contains(this[i])) count++;
      return count == 1;
    }

    /**
     * Report the facet opposite vertex.
     * @param vertex a vertex of this Triangle
     * @return the facet opposite vertex
     * @throws ArgumentException if the vertex is not in triangle
     */
    public List<Pnt> facetOpposite(Pnt vertex)
    {
      List<Pnt> facet = new List<Pnt>(this);
      if (!facet.Remove(vertex))
        throw new ArgumentException("Vertex not in triangle");
      return facet;
    }

    /**
     * @return the triangle's circumcenter
     */
    public Pnt getCircumcenter()
    {
      if (circumcenter == null)
        circumcenter = Pnt.circumcenter(ToArray());
      return circumcenter;
    }

    /* The following two methods ensure that all triangles are different. */

    public override int GetHashCode()
    {
      return (int)(idNumber ^ ((uint)idNumber >> 32));
    }

    public override bool Equals(object o)
    {
      if (this == o) return true;
      var t = o as Triangle;
      if (t == null) return false;
      if (t.Count != Count) return false;
      for (int i = 0; i < Count; i++)
      {
        if (!t.Contains(this[i])) return false;
      }
      return true;
    }
  }
}
