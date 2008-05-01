//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;

namespace NModel.Algorithms
{
    /// <summary>
    /// Edge of a graph with source and destination vertices.
    /// The source and destination may be the same in the case of self-loops.
    /// Also, source and destination may be freely swapped in the case of undirected graphs.
    /// </summary>
    public interface IEdge
    {
        /// <summary>
        /// Source vertex of the edge.  This is the vertex the edge travels from in directed graphs.
        /// </summary>
        IVertex Source { get; }

        /// <summary>
        /// Target vertex of the edge.  This is the vertex the edge travels to in directed graphs.
        /// </summary>
        IVertex Target { get; }

        /// <summary>
        /// Positive integral cost associated with traversing the edge, relative to other edges in the graph.
        /// Note: The name "Weight" is not used here since some applications such as MDE associate Weight as a Reward.
        /// </summary>
        int Cost { get; }

        /// <summary>
        /// Whether the edge is optional for inclusion in the graph and/or graph traversals.
        /// </summary>
        bool Optional { get; }
    }
}
