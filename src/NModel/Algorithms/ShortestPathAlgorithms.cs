//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NModel.Algorithms
{
    /// <summary>
    /// Contains a set of shortest path graph algorithms.
    /// </summary>
    public static partial class GraphAlgorithms
    {
        /// <summary>
        /// Returns true if there exists a path from source to target.
        /// Implemented using single-source single-target version of Dijkstra's shortest path algorithm, 
        /// </summary>
        /// <param name="source">Source vertex to start searching from.</param>
        /// <param name="target">Target vertex to find a path to.</param>
        /// <returns>True if a path was found; false otherwise.</returns>
        /// <remarks>
        /// The graph attached to <paramref name="source"/> must not contain any negative cycles.
        /// </remarks>
        public static bool HasPath(IVertex source, IVertex target)
        {
            IDictionary<IVertex, Path> paths = ShortestPaths(source, new IVertex[] { target }, 1);
            return paths.ContainsKey(target);
        }

        /// <summary>
        /// Single-source single-target version of Dijkstra's shortest path algorithm.
        /// </summary>
        /// <param name="source">Source vertex to start searching from.</param>
        /// <param name="target">Target verticex to find the shortest path to.</param>
        /// <returns>Shortest path from the source to the target.</returns>
        /// <remarks>
        /// The graph attached to <paramref name="source"/> must not contain any negative cycles.
        /// </remarks>
        public static Path ShortestPath(IVertex source, IVertex target)
        {
            IDictionary<IVertex, Path> paths = ShortestPaths(source, new IVertex[] { target }, 1);
            Path path;
            if (paths.TryGetValue(target, out path))
                return path;
            throw new PathNotFoundException("No path found from the source to any target.");
        }

        /// <summary>
        /// Single-source multiple-target version of Dijkstra's shortest path algorithm that returns a single shortest path.
        /// </summary>
        /// <param name="source">Source vertex to start searching from.</param>
        /// <param name="targets">Target vertices to find shortest paths to.</param>
        /// <returns>Shortest path from the source to the closest target.</returns>
        /// <remarks>
        /// The graph attached to <paramref name="source"/> must not contain any negative cycles.
        /// </remarks>
        public static Path ShortestPath(IVertex source, ICollection<IVertex> targets)
        {
            IDictionary<IVertex, Path>/*?*/ paths;
            return ShortestPath(source, targets, out paths);
        }

        /// <summary>
        /// Single-source multiple-target version of Dijkstra's shortest path algorithm that returns a single shortest path.
        /// </summary>
        /// <param name="source">Source vertex to start searching from.</param>
        /// <param name="targets">Target vertices to find shortest paths to.</param>
        /// <param name="inclusive">Output of all paths shorter than (and including) the shortest path to the closest target.</param>
        /// <returns>Shortest path from the source to the closest target.</returns>
        /// <remarks>
        /// The graph attached to <paramref name="source"/> must not contain any negative cycles.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public static Path ShortestPath(IVertex source, ICollection<IVertex> targets, out IDictionary<IVertex, Path> inclusive)
        {
            IDictionary<IVertex, Path> paths = inclusive = ShortestPaths(source, targets, 1);
            Path path;
            foreach (IVertex target in targets)
                if (paths.TryGetValue(target, out path))
                    return path;
            throw new PathNotFoundException("No path found from the source to any target.");
        }

        /// <summary>
        /// Single-source multiple-target version of Dijkstra's shortest path algorithm that returns shortest paths to a specified number of targets.
        /// </summary>
        /// <param name="source">Source vertex to start searching from.</param>
        /// <param name="targets">Target vertices to find shortest paths to.</param>
        /// <param name="count">Number of targets to find.</param>
        /// <returns>Mapping of all shortest paths from the source to all vertices up to <paramref name="count"/> targets.</returns>
        /// <remarks>
        /// The graph attached to <paramref name="source"/> should not contain any negative cycles.
        /// </remarks>
        public static IDictionary<IVertex, Path> ShortestPaths(IVertex source, ICollection<IVertex> targets, int count)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (targets == null)
                throw new ArgumentNullException("targets");
            if (count < 1)
                throw new ArgumentOutOfRangeException("count");

            // Create a priority queue where target vertices are prioritized by shortest path weight, starting with the source.
            PriorityQueue<IVertex, int> queue = new PriorityQueue<IVertex, int>();
            IDictionary<IVertex, IEdge/*?*/> edges = new Dictionary<IVertex, IEdge/*?*/>();

            // Initialize the source with a path of 0 length.
            queue[source] = 0;
            edges[source] = null;

            // Keep track of the explored vertices with known minimal length.
            IDictionary<IVertex, Path> shortestPaths = new Dictionary<IVertex, Path>();

            // The number of explored targets.
            int targetsFound = 0;

            // Keep track of the upper bound on the distance to the closest 'count' nodes.
            // See "A Heuristic for Dijkstra's Algorithm" for details.
            PriorityQueue<IVertex, int> bounds = new PriorityQueue<IVertex, int>(true);
            foreach (IVertex target in targets)
            {
                bounds.Enqueue(target, int.MaxValue);
                if (bounds.Count == count)
                    break;
            }

            // Traverse the queue.
            while (queue.Count > 0)
            {
                // Get the next closest vertex and copy it to the list of shortest paths.
                KeyValuePair<IVertex, int> item = queue.Dequeue();
                source = item.Key;

                // We've found the shortest path to this vertex.
                IEdge/*?*/ shortest = edges[source];
                if (shortest == null)
                    shortestPaths[source] = new Path(source);
                else
                    shortestPaths[source] = shortestPaths[shortest.Source] + shortest;

                // If we've explored the requested number of targets then break early.
                if (targets.Contains(source))
                    if (++targetsFound >= count)
                        break;

                // Relax all outgoing edges from this vertex.
                foreach (IEdge edge in source.Edges)
                {
                    // If we haven't explored the target yet.
                    IVertex target = edge.Target;
                    if (!shortestPaths.ContainsKey(target))
                    {
                        // Calculate the new cost via this edge.
                        int newCost = item.Value + edge.Cost;

                        // Only relax the edge if the cost is an improvement (i.e. less than) the shortest paths seen so far.
                        // See "A Heuristic for Dijkstra's Algorithm" for details.
                        if (newCost < bounds.Peek().Value)
                        {
                            // Compare the new path with the current paths.
                            int currentCost;
                            if (!queue.TryGetValue(target, out currentCost) || newCost < currentCost)
                            {
                                // If no current path exists or the new path is shorter, then update the shortest path in the queue.
                                queue[target] = newCost;
                                edges[target] = edge;

                                // If the target is a final target then update the cost in the list of upper bounds.
                                if (targets.Contains(target))
                                {
                                    bounds[target] = newCost;

                                    // We only need to keep track of the closest 'count' targets.
                                    if (bounds.Count > count)
                                        bounds.Dequeue();
                                }
                            }
                        }
                    }
                }
            }

            return shortestPaths;
        }
    }
}
