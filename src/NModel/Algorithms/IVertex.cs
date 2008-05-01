//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;

namespace NModel.Algorithms
{
    /// <summary>
    /// Vertex of a graph with a set of (outgoing for directed graphs) edges.
    /// </summary>
    public interface IVertex
    {
        /// <summary>
        /// Set of edges leaving this vertex.
        /// They may also be edges entering this vertex in the case of self-loops or undirected graphs.
        /// </summary>
        IEnumerable<IEdge> Edges { get; }

        /// <summary>
        /// Flag representing whether the vertex represents a choice point in a graph.
        /// </summary>
        bool ChoicePoint { get; }
    }
}
