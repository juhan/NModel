//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Terms;
using EdgeLabel = NModel.Terms.CompoundTerm;
//using Vertex = System.IComparable;
using Vertex = System.Int32;

namespace NModel.Internals
{

    /// <summary>
    /// Rooted labeled directed graph is a data structure that is used
    /// as an alternative representation of state.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
    public class RootedLabeledDirectedGraph : IComparable
    {
        /// <summary>
        /// Root vertex.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public Vertex root;
        
        /// <summary>
        /// Vertex records that contain data the label and structures of functional and relational
        /// outgoing edges and incoming edges.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public Dictionary<Vertex, VertexData> vertexRecords;   // invariant (instances[i].sourceId == i)
        
        /// <summary>
        /// Next ID.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public int nextId = 0;

        /// <summary>
        /// Hash code of the graph
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public int hashCode = 0;

        /// <summary>
        /// Constructor of the rooted labeled directed graph.
        /// </summary>
        /// <param name="root">Root vertex</param>
        /// <param name="vertexRecords">Dictionary of vertex records</param>
        public RootedLabeledDirectedGraph(Vertex root, Dictionary<Vertex, VertexData> vertexRecords)
        {
            this.root = root;
            this.vertexRecords = vertexRecords;

               foreach (KeyValuePair<Vertex, VertexData> kv in vertexRecords)
                {
                    Vertex vertex = kv.Key;
                    VertexData vertexData = kv.Value;

                    // backlinks for ordered outgoing edges
                    foreach (Pair<EdgeLabel, Vertex> e1 in vertexData.orderedOutgoingEdges)
                    {
                        Vertex target = e1.Second;
                        VertexData targetData = EnsureVertex(target); //This will trigger a "Collection was modified; enumeration operation may not execute." 
                                                                      // error if a vertex is not in the current enumeration, thus EnsureVertex should actually raise an exception  
                        targetData.incomingEdges = targetData.incomingEdges.Add(new Pair<EdgeLabel, Vertex>(e1.First, vertex));
                    }

                    // backlinks for unordered outgoing edges
                   foreach (Pair<EdgeLabel,Set<Vertex>> e2 in vertexData.unorderedOutgoingEdges)
                        foreach (Vertex v2 in e2.Second)
                        {
                            Vertex target = v2;
                            VertexData targetData = EnsureVertex(target);
                            targetData.incomingEdges = targetData.incomingEdges.Add(new Pair<EdgeLabel, Vertex>(e2.First, vertex));
                        }
                }


        }

        /// <summary>
        /// Make sure that a vertexData record has been initialized for the vertex.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns>VertexData record corresponding to the argument.</returns>
        VertexData EnsureVertex(Vertex vertex)
        {
            VertexData result;
            if (!this.vertexRecords.TryGetValue(vertex, out result))
            {
                result = new VertexData(vertex, Set<Pair<CompoundTerm, IComparable>>.EmptySet,
                                        Map<EdgeLabel, Vertex>.EmptyMap, Map<EdgeLabel, Set<Vertex>>.EmptyMap);
                this.vertexRecords.Add(vertex, result);
            }
            return result;
        }

        /// <summary>
        /// Returns the label of the vertex v.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public virtual IComparable LabelOf(Vertex v)
        {
            return this.vertexRecords[v].label; 
        }

        #region IComparable Members
        
        /// <summary>
        /// This method throws a NotImplementedException.
        /// (This method is never called in NModel and should not be called.
        /// Graphs are stored in sets, thus the method is needed for IComparable.)
        /// </summary>
        public int CompareTo(object obj)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #endregion
    }

    /// <summary>
    /// Vertex data records containing vertex label, functional outgoing edges, relational outgoing edges and incoming
    /// edges of the vertex.
    /// </summary>
    public class VertexData //: IComparable
    {
        /// <summary>
        /// The vertex.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public Vertex vertex;

        /// <summary>
        /// Vertex label. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Set<Pair<CompoundTerm, IComparable>> label;       // fields that do not contain object ids; fully ordered
        
        /// <summary>
        /// Functional outgoing edges.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public Map<EdgeLabel, Vertex> orderedOutgoingEdges;

        
        //public Set<Pair<EdgeLabel, Vertex>> unorderedOutgoingEdges;
        /// <summary>
        /// Relational outgoing edges.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Map<EdgeLabel, Set<Vertex>> unorderedOutgoingEdges;
        
        /// <summary>
        /// Incoming edges.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Set<Pair<EdgeLabel, Vertex>> incomingEdges;

        //// dictionary of hash codes computed for different graph extensions. hashMap[0] is the hash
        //// code of this node only; hashMap[1] hashes this vertex plus all vertices at most one edge away, etc.
        //// THis is a cache that is computed as needed to discover a partial order of vertices that appear 
        //// in unordered edges of the graph.
        ////Dictionary<int, int> hashMap;

        //int linearizedId;            // if nonnegative, the assigned id from the linearization walk. Fully disambiguates this element.

        /// <summary>
        /// Constructor that initialises the data structure.
        /// </summary>
        public VertexData()
        { 
            label = new Set<Pair<CompoundTerm, IComparable>>();
            orderedOutgoingEdges = new Map<EdgeLabel, Vertex>();
            //unorderedOutgoingEdges = new Set<Pair<EdgeLabel, Vertex>>();
            unorderedOutgoingEdges = new Map<EdgeLabel, Set<Vertex>>();
            incomingEdges = new Set<Pair<EdgeLabel, Vertex>>();
        }

        /// <summary>
        /// Constructor that initializes the record with previously initialized data structures.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="label"></param>
        /// <param name="orderedOutgoingEdges"></param>
        /// <param name="unorderedOutgoingEdges"></param>
        public VertexData(Vertex vertex,
                        Set<Pair<EdgeLabel, IComparable>> label,
                        Map<EdgeLabel, Vertex> orderedOutgoingEdges,
                        Map<EdgeLabel, Set<Vertex>> unorderedOutgoingEdges)
                        //Set<Pair<EdgeLabel, Vertex>> unorderedOutgoingEdges)
        {
            this.vertex = vertex;
            this.label = label;
            this.orderedOutgoingEdges = orderedOutgoingEdges;            
            this.unorderedOutgoingEdges = unorderedOutgoingEdges;
            
            //this.hashMap = new Dictionary<int, int>();
            //this.linearizedId = -1;

            this.incomingEdges = Set<Pair<EdgeLabel, Vertex>>.EmptySet;
        }
    }
}
