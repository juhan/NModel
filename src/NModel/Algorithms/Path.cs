//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NModel.Algorithms
{
    /// <summary>
    /// Represents a path of edges
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class Path : IEdge, IComparable<Path>, IComparable<IEdge>, IEnumerable<IEdge>
    {
        /// <summary>
        /// Empty path
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Path Empty = new Path();

        private readonly int count;
        private readonly Path/*?*/ previous;
        private readonly int cost;
        private readonly bool optional;
        private readonly IVertex/*?*/ source;
        private readonly IVertex/*?*/ target;
        private readonly IEdge/*?*/ edge;

        private Path()
        {
            optional = true;
        }

        /// <summary>
        /// Path containing a single vertex
        /// </summary>
        public Path(IVertex vertex)
        {
            optional = true;
            source = vertex;
            target = vertex;
        }

        /// <summary>
        /// Path containing a single edge
        /// </summary>
        public Path(IEdge edge)
        {
            if (edge == null)
                throw new ArgumentNullException("edge");

            count = 1;
            cost = edge.Cost;
            optional = edge.Optional;
            source = edge.Source;
            target = edge.Target;
            this.edge = edge;
        }

        private Path(Path previous, IEdge edge)
        {
            if (previous == null)
                throw new ArgumentNullException("previous");
            if (edge == null)
                throw new ArgumentNullException("edge");

            count = previous.count + 1;
            cost = previous.cost + edge.Cost;
            optional = previous.optional && edge.Optional;
            source = previous.source;
            target = edge.Target;
            this.previous = previous;
            this.edge = edge;
        }

        /// <summary>
        /// Path containing the given edges
        /// </summary>
        public Path(IEnumerable<IEdge> edges)
        {
            if (edges == null)
                throw new ArgumentNullException("edges");

            Path path = Path.Empty + edges;
            count = path.count;
            cost = path.cost;
            optional = path.optional;
            source = path.source;
            target = path.target;
            previous = path.previous;
            this.edge = path.edge;
        }

        /// <summary>
        /// The length of the path
        /// </summary>
        public int Count
        {
            get
            {
                return count;
            }
        }

        /// <summary>
        /// Add the edge at the end of the path
        /// </summary>
        public static Path operator +(Path path, IEdge edge)
        {
            return path != null && path.source != null ? new Path(path, edge) : new Path(edge);
        }

        /// <summary>
        /// Add the edges at the end of the path
        /// </summary>
        public static Path operator +(Path path, IEnumerable<IEdge> edges)
        {
            if (edges != null)
                foreach (IEdge edge in edges)
                    path += edge;
            return path;
        }

        /// <summary>
        /// Add the edge at the end of the path
        /// </summary>
        public static Path Add(Path path, IEdge edge)
        {
            return path + edge;
        }

        /// <summary>
        /// Add the edges at the end of the path
        /// </summary>
        public static Path Add(Path path, IEnumerable<IEdge> edges)
        {
            return path + edges;
        }

        #region IEdge Members

        /// <summary>
        /// The start vertex of the path
        /// </summary>
        public IVertex Source
        {
            get
            {
                if (source == null)
                    throw new InvalidOperationException("The path is empty.");

                return source;
            }
        }

        /// <summary>
        /// The end vertex of the path
        /// </summary>
        public IVertex Target
        {
            get
            {
                if (target == null)
                    throw new InvalidOperationException("The path is empty.");

                return target;
            }
        }

        /// <summary>
        /// The cost of the path
        /// </summary>
        public int Cost
        {
            get
            {
                return cost;
            }
        }

        /// <summary>
        /// True if the path is optional
        /// </summary>
        public bool Optional
        {
            get
            {
                return optional;
            }
        }

        #endregion

        #region IComparable<Path> Members

        /// <summary>
        /// Compares the costs of the paths
        /// </summary>
        public int CompareTo(Path other)
        {
            return cost.CompareTo(other.cost);
        }

        #endregion

        #region IComparable<IEdge> Members

        /// <summary>
        /// Compares the path cost to the edge cost
        /// </summary>
        public int CompareTo(IEdge other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            return cost.CompareTo(other.Cost);
        }

        #endregion

        #region IEnumerable<IEdge> Members

        /// <summary>
        /// Enumerates the edges along the path
        /// </summary>
        public IEnumerator<IEdge> GetEnumerator()
        {
            Stack<IEdge> edges = new Stack<IEdge>();

            for (Path/*?*/ parent = this; parent != null; parent = parent.previous)

                if (parent.edge != null)
                    edges.Push(parent.edge);

            return edges.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Returns a string description of the path
        /// </summary>
        public override string ToString()
        {
            return string.Format((IFormatProvider)null, "Path of length {0} from {1} to {2} with cost {3} {4}", count, source, target, cost, (optional ? " [Optional]" : ""));
        }
    }
}
