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

using System.Collections.Generic;
using System.Linq;

/**
 * Straightforward undirected graph implementation.
 * Nodes are generic type N.
 *
 * @author Paul Chew
 *
 * Created November, December 2007.  For use in Delaunay/Voronoi code.
 *
 */

namespace BriLib.Delaunay
{
  public class Graph<N>
  {

    private Dictionary<N, List<N>> theNeighbors =    // Node -> adjacent nodes
        new Dictionary<N, List<N>>();
    private List<N> theNodeSet { get { return theNeighbors.Keys.ToList(); } } // Set view of all nodes

    /**
     * Add a node.  If node is already in graph then no change.
     * @param node the node to add
     */
    public void add(N node)
    {
      if (theNeighbors.ContainsKey(node)) return;
      theNeighbors.Add(node, new List<N>());
    }

    /**
     * Add a link. If the link is already in graph then no change.
     * @param nodeA one end of the link
     * @param nodeB the other end of the link
     * @throws NullPointerException if either endpoint is not in graph
     */
    public void add(N nodeA, N nodeB)
    {
      if (!theNeighbors.ContainsKey(nodeA)) add(nodeA);
      if (!theNeighbors.ContainsKey(nodeB)) add(nodeB);

      theNeighbors[nodeA].AddIfNotContains(nodeB);
      theNeighbors[nodeB].AddIfNotContains(nodeA);
    }

    /**
     * Remove node and any links that use node. If node not in graph, nothing
     * happens.
     * @param node the node to remove.
     */
    public void remove(N node)
    {
      if (!theNeighbors.ContainsKey(node)) return;
      for (var enumerator = theNeighbors.GetEnumerator(); enumerator.MoveNext();)
      {
        var neighbor = enumerator.Current.Key;
        theNeighbors[neighbor].Remove(node);
      }                                               // Remove "to" links
      theNeighbors[node].Clear();                 // Remove "from" links
      theNeighbors.Remove(node);                      // Remove the node
    }

    /**
     * Remove the specified link. If link not in graph, nothing happens.
     * @param nodeA one end of the link
     * @param nodeB the other end of the link
     * @throws NullPointerException if either endpoint is not in graph
     */
    public void remove(N nodeA, N nodeB)
    {
      theNeighbors[nodeA].Remove(nodeB);
      theNeighbors[nodeB].Remove(nodeA);
    }

    /**
     * Report all the neighbors of node.
     * @param node the node
     * @return the neighbors of node
     * @throws NullPointerException if node does not appear in graph
     */
    public List<N> neighbors(N node)
    {
      return theNeighbors[node];
    }

    /**
     * Returns an unmodifiable Set view of the nodes contained in this graph.
     * The set is backed by the graph, so changes to the graph are reflected in
     * the set.
     * @return a Set view of the graph's node set
     */
    public List<N> nodeSet()
    {
      return theNodeSet;
    }

  }
}
