//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
//using Node = System.IComparable;
using Node = System.Int32;

using NodeLabel = System.IComparable;
using NModel;
using NModel.Terms;
using NModel.Internals;
using System.Collections.ObjectModel;

namespace NModel.Algorithms
{
    /// <summary>
    /// Includes two algorithms:
    /// 
    /// 1) Based on Ullmann's SUBGRAPH isomorphism algorithm from JACM, Vol.23(1), pp 31-42, 1976
    /// specialized as a GRAPH isomorphism algorithm for directed labelled graphs
    /// 
    /// 2) Implements a backtracking extension of the linearization algorithm for 
    /// rooted directed ordered labelled graphs.
    /// The algorithm uses optimizations that rely on strong
    /// objectid independent hashing.
    /// 
    /// Currently there are two variations of the second algorithm, one with partitioning and one without it.
    /// </summary>
    public class GraphIsomorphism
    {
        RootedLabeledDirectedGraph g1;
        RootedLabeledDirectedGraph g2;
        Dictionary<NodeLabel, Bucket> bucketOf;

        GraphIsomorphism(RootedLabeledDirectedGraph g1, RootedLabeledDirectedGraph g2)
        {
            this.g1 = g1;
            this.g2 = g2;
        }

        #region Shared by both algorithms

        /// <summary>
        /// Computes the buckets for all nodes.
        /// Returns false if this fails, in which case
        /// isomorphism from g1 to g2 is not possible.
        /// </summary>
        private bool ComputeBuckets()
        {
            this.bucketOf = new Dictionary<NodeLabel, Bucket>();

            Stack<Node> stack = new Stack<Node>();

            #region compute the bucket labels for each node in g1
            Set<Node> visited = new Set<Node>(g1.root);
            stack.Push(g1.root);
            while (stack.Count > 0)
            {
                Node x = stack.Pop();
                NodeLabel lab = g1.LabelOf(x);
                Bucket bucket;
                if (!bucketOf.TryGetValue(lab, out bucket))
                {
                    bucket = new Bucket(x);
                    bucketOf[lab] = bucket;
                }
                else
                    bucket.g1Nodes = bucket.g1Nodes.Add(x);
                VertexData xData = g1.vertexRecords[x];
                foreach (Pair<CompoundTerm, Node> edge in xData.orderedOutgoingEdges)
                {
                    if (!visited.Contains(edge.Second))
                    {
                        stack.Push(edge.Second);
                        visited = visited.Add(edge.Second);
                    }
                }
                foreach (Pair<CompoundTerm, Set<Node>> edges in xData.unorderedOutgoingEdges)
                    foreach (Node n in edges.Second)
                    {
                        if (!visited.Contains(n))
                        {
                            stack.Push(n);
                            visited = visited.Add(n);
                        }
                    }
            }
            #endregion

            #region add g2 nodes to the buckets
            visited = new Set<Node>(g2.root);
            stack.Push(g2.root);

            while (stack.Count > 0)
            {
                Node y = stack.Pop();
                NodeLabel lab = g2.LabelOf(y);

                Bucket bucket;
                if (!bucketOf.TryGetValue(lab, out bucket))
                    return false; // label of y does not appear in g1 at all

                bucket.g2Nodes = bucket.g2Nodes.Add(y);

                VertexData yData = g2.vertexRecords[y];
                foreach (Pair<CompoundTerm, Node> edge in yData.orderedOutgoingEdges)
                {
                    if (!visited.Contains(edge.Second))
                    {
                        stack.Push(edge.Second);
                        visited = visited.Add(edge.Second);
                    }
                }
                foreach (Pair<CompoundTerm, Set<Node>> edges in yData.unorderedOutgoingEdges)
                    foreach (Node n in edges.Second)
                    {
                        if (!visited.Contains(n))
                        {
                            stack.Push(n);
                            visited = visited.Add(n);
                        }
                    }

            }
            #endregion

            //Finally check that all buckets have the same nr of g1 and g2 nodes
            //or else isomorphism is not possible
            foreach (Bucket bucket in bucketOf.Values)
            {
                if (bucket.g1Nodes.Count != bucket.g2Nodes.Count)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Represents a pair of buckets of nodes
        /// from g1 and g2, both buckets have the same label and the same size
        /// </summary>
        internal class Bucket
        {
            internal Set<Node> g1Nodes;
            internal Set<Node> g2Nodes;
            internal Bucket(Node x)
            {
                this.g1Nodes = new Set<Node>(x);
                this.g2Nodes = Set<Node>.EmptySet;
            }
            internal int Size
            {
                get
                {
                    return g1Nodes.Count;
                }
            }

            public override string ToString()
            {
                return "G1:" + g1Nodes.ToString() + "\nG2:" + g2Nodes.ToString();
            }
        }
        #endregion


        /// <summary>
        /// Returns an isomorphism from g1 to g2, if g1 and g2 are isomorphic;
        /// returns null, otherwise.
        /// 
        /// Implements a backtracking extension of the linearization algorithm for 
        /// rooted directed ordered labelled graphs.
        /// The algorithm uses optimizations that rely on strong
        /// objectid independent hashing.
        /// 
        /// (This version uses partitioning)
        /// </summary>
        static public Map<Node, Node> ComputeIsomorphism1(RootedLabeledDirectedGraph g1, RootedLabeledDirectedGraph g2)
        {
            GraphIsomorphism gi = new GraphIsomorphism(g1, g2);
            return gi.ComputeIsomorphism1(); 
        }

        Map<Node, Node> ComputeIsomorphism1()
        {
            if (!ComputeBuckets())
                return null;
            IterativeIsomorphismExtensionWithBacktrackingAndPartitioning iter = new IterativeIsomorphismExtensionWithBacktrackingAndPartitioning(g1, g2, bucketOf);
            return iter.ComputeIsomorphism();

        }

        /// <summary>
        /// Returns an isomorphism from g1 to g2, if g1 and g2 are isomorphic;
        /// returns null, otherwise.
        /// 
        /// Implements a backtracking extension of the linearization algorithm for 
        /// rooted directed ordered labelled graphs.
        /// The algorithm uses optimizations that rely on strong
        /// objectid independent hashing.
        /// 
        /// (This version does not use partitioning)
        /// </summary>
        static public Map<Node, Node> ComputeIsomorphism2(RootedLabeledDirectedGraph g1, RootedLabeledDirectedGraph g2)
        {
            GraphIsomorphism gi = new GraphIsomorphism(g1, g2);
            return gi.ComputeIsomorphism2();
        }

        Map<Node, Node> ComputeIsomorphism2()
        {
            if (!ComputeBuckets())
                return null;
            IterativeIsomorphismExtensionWithBacktracking iter = new IterativeIsomorphismExtensionWithBacktracking(g1, g2, bucketOf);
            return iter.ComputeIsomorphism();

        }

        #region The initial implementation of the iterative isomorphism extension.
        //private void checkPartitions(SortedList<IComparable, NodePartition> xPartitions, out bool existSingletons, bool existOrderDependent)
        //{
        //    existSingletons = false;
        //    existOrderDependent = false;
        //    for (int i = 0; i < xPartitions.Count; i++)
        //    {
        //        NodePartition xPartition = xPartitions.Values[i];
        //        if (xPartition.independent || xPartition.Count == 1)
        //            existSingletons = true;
        //        else
        //            existOrderDependent = true;
        //    }
        //}


        ///// <summary>
        ///// Computes the extension of the isomorphism iso for all nodes 
        ///// that are reachable from x in g1 and y in g2 where x maps to y
        ///// If no such extension exists, returns false
        ///// </summary>
        //private Bijection ExtendIsomorphism(Bijection iso, Node x, Node y)
        //{
        //    if (!g1.LabelOf(x).Equals(g2.LabelOf(y)))
        //        return null; //extension is not possible
        //    if (iso.map.ContainsKey(x))
        //    {
        //        //x is already mapped to a node
        //        //this node must then be y or else isomorphism is not possible
        //        if (iso.map[x].Equals(y))
        //            return iso;
        //        else
        //            return null;
        //    }
        //    if (iso.range.Contains(y)) //y is already in the range of iso
        //        return null;

        //    VertexData xData = g1.vertexRecords[x];
        //    VertexData yData = g2.vertexRecords[y];

        //    //1) extend the isomorphism for the ordered outgoing edges 
        //    Bijection isoExt = new Bijection(iso.map.Add(x, y), iso.range.Add(y));
        //    foreach (Pair<CompoundTerm, Node> edge in xData.orderedOutgoingEdges)
        //    {
        //        if (yData.orderedOutgoingEdges.ContainsKey(edge.First))
        //            isoExt = ExtendIsomorphism(isoExt, edge.Second, yData.orderedOutgoingEdges[edge.First]);
        //        else 
        //            isoExt = null;
        //        if (isoExt == null)
        //            return null; //extension is not possible
        //    }

        //    //2) compute the partitions of the unordered outgoing edges
        //    SortedList<IComparable, NodePartition> xPartitions = GetPartitionsG1(x);
        //    SortedList<IComparable, NodePartition> yPartitions = GetPartitionsG2(y);

        //    //partitions must be consistent for an extension of iso to be possible
        //    if (!PartitionsAreConsistent(xPartitions, yPartitions))
        //        return null;

        //    //There is backtracking involved here, thus this needs to be changed.
        //    //3) extend iso for all order-independent and singleton partitions first
        //    //this does not introduce backtracking, a fixed order of mappings 
        //    //is chosen that uses the order of Nodes (object ids)
        //    //computationally this is similar to the case of ordered edges
        //    for (int i = 0; i < xPartitions.Count; i++)
        //    {
        //        NodePartition xPartition = xPartitions.Values[i];
        //        if (xPartition.independent || xPartition.Count == 1)
        //        //if (xPartition.Count == 1)
        //        {
        //            NodePartition yPartition = yPartitions.Values[i];
        //            for (int j = 0; j < xPartition.Count; j++)
        //            {
        //                isoExt = ExtendIsomorphism(isoExt, xPartition.Nodes[j], yPartition.Nodes[j]);
        //                if (isoExt == null)
        //                    return null;
        //            }
        //        }
        //    }

        //    //4) search for an isomorphism for each pair of the order-dependent partitions
        //    //this may introduce backtracking and in the worst case exponential time complexity
        //    for (int i = 0; i < xPartitions.Count; i++)
        //    {
        //        NodePartition xPartition = xPartitions.Values[i];
        //        if (!xPartition.independent && xPartition.Count > 1)
        //        //if (xPartition.Count > 1)
        //        {
        //            NodePartition yPartition = yPartitions.Values[i];
        //            Pair<Set<Node>, Set<Node>> pair =
        //                PruneCandidates(isoExt, new Set<Node>(xPartition.Nodes), new Set<Node>(yPartition.Nodes));
        //            if (pair.First == null) //pruning failed
        //                return null;
        //            if (!pair.First.IsEmpty) //not all candidates were pruned away
        //            {
        //                isoExt = SearchForIsomorphism(isoExt, pair.First, pair.Second);
        //                if (isoExt == null)
        //                    return null;
        //            }
        //        }
        //    }

        //    return isoExt; //extension of iso was found
        //}

        ///// <summary>
        ///// Returns true if both partitions have the same number of elements
        ///// and the same keys, and for each key, the corresponding partitions 
        ///// have the same size and order-idependence property
        ///// </summary>
        //static private bool PartitionsAreConsistent(SortedList<Node, NodePartition> xPartitions, SortedList<Node, NodePartition> yPartitions)
        //{
        //    if (xPartitions.Count != yPartitions.Count)
        //        return false;

        //    for (int i = 0; i < xPartitions.Count; i++)
        //    {
        //        if (xPartitions.Keys[i].CompareTo(yPartitions.Keys[i]) != 0)
        //            return false;
        //        NodePartition xPartition = xPartitions.Values[i];
        //        NodePartition yPartition = yPartitions.Values[i];
        //        if (xPartition.independent != yPartition.independent ||
        //            xPartition.Count != yPartition.Count)
        //            return false;
        //    }
        //    return true;
        //}

        ///// <summary>
        ///// Prune away xs nodes and ys nodes that already appear in iso
        ///// and check for possibility of bijectivity for the remaining nodes.
        ///// Return (null,null) if the pruning fails.
        ///// </summary>
        //static Pair<Set<Node>,Set<Node>> PruneCandidates(Bijection iso, Set<Node> xs, Set<Node> ys)
        //{
        //    Set<Node> xsPruned = xs;
        //    Set<Node> ysPruned = ys;
        //    foreach (Node x in xs)
        //    {
        //        if (iso.map.ContainsKey(x))
        //        {
        //            if (ysPruned.Contains(iso.map[x]))
        //            {
        //                xsPruned = xsPruned.Remove(x);
        //                ysPruned = ysPruned.Remove(iso.map[x]);
        //            }
        //            else
        //                return new Pair<Set<Node>, Set<Node>>(null, null);
        //        }
        //    }
        //    if (iso.range.Intersect(ysPruned).IsEmpty)
        //        return new Pair<Set<Node>, Set<Node>>(xsPruned, ysPruned);
        //    else
        //        return new Pair<Set<Node>, Set<Node>>(null, null);
        //}

        //private Bijection SearchForIsomorphism(Bijection iso, Set<Node> xs, Set<Node> ys)
        //{
        //    if (xs.Count == 1)
        //        return ExtendIsomorphism(iso, xs.Choose(), ys.Choose());

        //    //search over all the possible mappings from xs to ys
        //    foreach (Node x in xs)
        //    {
        //        foreach (Node y in ys)
        //        {
        //            Bijection isoExt = ExtendIsomorphism(iso, x, y);
        //            if (isoExt != null)
        //            {
        //                isoExt = SearchForIsomorphism(isoExt, xs.Remove(x), ys.Remove(y));
        //                if (isoExt != null)
        //                    return isoExt; 
        //            }
        //        }
        //    }
        //    return null; //no extension of iso could be found
        //}

        //private class NodePartition : SortedList<Node,object>
        //{
        //    internal bool independent;

        //    internal NodePartition(Node x, bool independent) : base()
        //    {
        //        base.Add(x, null);
        //        this.independent = independent;
        //    }

        //    internal void AddNode(Node x)
        //    {
        //        base.Add(x, null);
        //    }

        //    internal IList<Node> Nodes
        //    {
        //        get
        //        {
        //            return base.Keys;
        //        }
        //    }
        //}


        //Dictionary<Node, SortedList<IComparable, NodePartition>> g1Partitions = 
        //    new Dictionary<Node, SortedList<IComparable, NodePartition>>();

        ///// <summary>
        ///// Returns GetPartitions(g1, x), caches the result.
        ///// </summary>
        //SortedList<IComparable, NodePartition> GetPartitionsG1(Node x)
        //{
        //    SortedList<IComparable, NodePartition> partitions;
        //    if (!g1Partitions.TryGetValue(x, out partitions))
        //    {
        //        partitions = GetPartitions(g1, x);
        //        g1Partitions[x] = partitions;
        //    }
        //    return partitions;
        //}

        //Dictionary<Node, SortedList<IComparable, NodePartition>> g2Partitions =
        //    new Dictionary<Node, SortedList<IComparable, NodePartition>>();

        ///// <summary>
        ///// Returns GetPartitions(g2, x), caches the result.
        ///// </summary>
        //SortedList<IComparable, NodePartition> GetPartitionsG2(Node x)
        //{
        //    SortedList<IComparable, NodePartition> partitions;
        //    if (!g2Partitions.TryGetValue(x, out partitions))
        //    {
        //        partitions = GetPartitions(g2, x);
        //        g2Partitions[x] = partitions;
        //    }
        //    return partitions;
        //}

        ///// <summary>
        ///// Partition the set of neighbors of all outgoing unordered edges of a node x in g.
        ///// Returns all the partitions indexed by the pair (E,L) where
        ///// E is an edge label and L is the label of the neighbor.
        ///// 
        ///// A partition is marked order-independent if some node in that partition is 
        ///// order-independent, which implies that all nodes in the same partition must be 
        ///// order-independent as well (this is a property implied by the node labeling/hashing algorithm).
        ///// </summary>
        ///// <param name="g">given graph</param>
        ///// <param name="x">given node in g</param>
        //SortedList<IComparable,NodePartition> GetPartitions(RootedLabeledDirectedGraph g, Node x)
        //{
        //    SortedList<IComparable, NodePartition> partitions =
        //        new SortedList<IComparable, NodePartition>();
        //    VertexData xData = g.vertexRecords[x];

        //    foreach (Pair<CompoundTerm, Node> edge in xData.unorderedOutgoingEdges)
        //    {
        //        IComparable key = new Pair<CompoundTerm, NodeLabel>(edge.First, g.LabelOf(edge.Second));
        //        NodePartition nodePartition;
        //        if (partitions.TryGetValue(key, out nodePartition))
        //            nodePartition.AddNode(edge.Second);
        //        else
        //            partitions[key] = new NodePartition(edge.Second, IsOrderIndependent(edge.Second, g));
        //    }
        //    return partitions;
        //}

        ///// <summary>
        ///// A node x is order-independent in g if all nodes in 
        ///// its neighborhood are in singleton buckets
        ///// </summary>
        //bool IsOrderIndependent(Node x, RootedLabeledDirectedGraph g)
        //{
        //    VertexData xData = g.vertexRecords[x];
        //    foreach (Pair<CompoundTerm, Node> edge in xData.incomingEdges)
        //        if (bucketOf[g.LabelOf(edge.Second)].Size > 1)
        //            return false;
        //    foreach (Pair<CompoundTerm, Node> edge in xData.orderedOutgoingEdges)
        //        if (bucketOf[g.LabelOf(edge.Second)].Size > 1)
        //            return false;
        //    foreach (Pair<CompoundTerm, Node> edge in xData.unorderedOutgoingEdges)
        //        if (bucketOf[g.LabelOf(edge.Second)].Size > 1)
        //            return false;
        //    return true;
        //}

        #endregion

        /// <summary>
        /// Represents a one-to-one map of nodes from g1 to nodes in g2.
        /// For efficiency includes the range of the map.
        /// </summary>
        private class Bijection : IComparable
        {
            internal Map<Node, Node> map;
            internal Set<Node> range;

            internal Bijection(Map<Node, Node> map, Set<Node> range)
            {
                this.map = map;
                this.range = range;
            }

            #region IComparable Members

            public int CompareTo(object obj)
            {
                Bijection b = obj as Bijection;
                if (b!=null)
                    if (this.map.Equals(b.map) && this.range.Equals(b.range))
                        return 0;
     
                return 1;
            }

            #endregion
        }


        /// <summary>
        /// A class used to capture information that needs to be stored and retrieved in order to
        /// make backtracking work.
        /// </summary>
        private class BacktrackPoint
        {
            // outLabels denote the corresponding labels of relational edges.
            internal CompoundTerm outLabel;
            //internal int yRelationalOrder = 0;
            //internal int xRelationalOrder = 0;
            internal int index;
            internal int relationalPassIndex;
            internal Dictionary<Node, int> indexDict;
            internal Dictionary<Node, int> rangeDict;
            internal Set<Node> xtoNodes, ytoNodes;
            internal Set<CompoundTerm> relationalEdgeLabels;
            internal Permuter permuter;


            internal BacktrackPoint(Dictionary<Node, int> indexDict, Dictionary<Node, int> rangeDict, CompoundTerm outLabel, int index, int relationalPassIndex, Set<CompoundTerm> relationalEdgeLabels, Set<Node> xtoNodes, Set<Node> ytoNodes)
            {

                this.index = index;
                this.outLabel = outLabel;
                this.relationalPassIndex = relationalPassIndex;
                this.indexDict = new Dictionary<Node, int>();
                this.rangeDict = new Dictionary<Node, int>();
                this.relationalEdgeLabels = relationalEdgeLabels;
                this.xtoNodes = xtoNodes;
                this.ytoNodes = ytoNodes;
                foreach (KeyValuePair<Node, int> pair in indexDict)
                {
                    this.indexDict.Add(pair.Key, pair.Value);
                }
                foreach (KeyValuePair<Node, int> pair in rangeDict)
                {
                    this.rangeDict.Add(pair.Key, pair.Value);
                }
                this.permuter = new Permuter(xtoNodes.Count);
            }

            internal static Dictionary<Node, int> getCopy(Dictionary<Node, int> dict)
            {
                Dictionary<Node, int> newDict = new Dictionary<Node, int>();
                foreach (KeyValuePair<Node, int> pair in dict)
                {
                    newDict.Add(pair.Key, pair.Value);
                }
                return newDict;
            }

        }


        /// <summary>
        /// A class used to capture information that needs to be stored and retrieved in order to
        /// make backtracking work.
        /// </summary>
        private class BacktrackPointWithPartitions
        {
            // outLabels denote the corresponding labels of relational edges.
            internal CompoundTerm outLabel;
            //internal int yRelationalOrder = 0;
            //internal int xRelationalOrder = 0;
            internal int index;
            internal int relationalPassIndex;
            internal Dictionary<Node,int> indexDict;
            internal Dictionary<Node, int> rangeDict;
            internal Set<Node> xtoNodes, ytoNodes;
            internal Set<CompoundTerm> relationalEdgeLabels;
            internal Permuter permuter;
            internal Map<Pair<IComparable, bool>, Set<Node>> xPartitions;
            internal Map<Pair<IComparable, bool>, Set<Node>> yPartitions;


            internal BacktrackPointWithPartitions(
                Dictionary<Node,int> indexDict,
                Dictionary<Node,int> rangeDict,
                CompoundTerm outLabel,
                int index,
                int relationalPassIndex,
                Set<CompoundTerm> relationalEdgeLabels,
                Set<Node> xtoNodes,
                Set<Node> ytoNodes,
                Map<Pair<IComparable, bool>, Set<Node>> xPartitions,
                Map<Pair<IComparable, bool>, Set<Node>> yPartitions
                )
            {
                
                this.index = index;
                this.outLabel = outLabel;
                this.relationalPassIndex = relationalPassIndex;
                this.indexDict = new Dictionary<Node, int>();
                this.rangeDict = new Dictionary<Node, int>();
                this.relationalEdgeLabels = relationalEdgeLabels;
                this.xtoNodes = xtoNodes;
                this.ytoNodes = ytoNodes;
                foreach (KeyValuePair<Node,int> pair in indexDict)
                {
                    this.indexDict.Add(pair.Key, pair.Value);
                }
                foreach (KeyValuePair<Node, int> pair in rangeDict)
                {
                    this.rangeDict.Add(pair.Key, pair.Value);
                }
                this.permuter = new Permuter(xtoNodes.Count);
                this.xPartitions = xPartitions;
                this.yPartitions = yPartitions;
            }

            internal static Dictionary<Node, int> getCopy(Dictionary<Node, int> dict) {
                Dictionary<Node, int> newDict = new Dictionary<Node, int>();
                foreach (KeyValuePair<Node, int> pair in dict)
                {
                    newDict.Add(pair.Key, pair.Value);
                }
                return newDict;
            }

        }

        /// <summary>
        /// Permuter facilitates walking two collections in parallel using a permutation of indexes each time.
        /// </summary>
        class Permuter
        {

            int[] map;
            int size;

            internal Permuter(int size)
            {
                map = new int[size+1];
                this.size = size;
                for (int i = 0; i < size; i++)
                    map[i] = i;
                map[size] = int.MaxValue;
            }

            /// <summary>
            /// Makes it possible to access the <code>Permuter p;</code> in the convenient
            /// <code>int j = p[i];</code> way.
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            internal int this[int i]
            {
                // assert(i<ie.Length)
                get { return map[i]; }
            }

            //internal int CurrentPermutation(int i)
            //{
            //    return ie[i];
            //}

            private void swap(int i, int j)
            {
                int tmp = map[j];
                map[j] = map[i];
                map[i] = tmp;
            }


            ///<summary>
            /// Next() function is made along the lines of an implementation from a forthcoming book by Jörg Arndt
            /// http://www.jjj.de/fxt/#fxtbook where the implementation is transitively attributed
            /// to Glenn Rhoads and then to Edsger Dijkstra. Here we have made changes relevant to our application
            /// and C#.
            /// </summary>
            internal bool Next()
            {
                int n1 = size - 1;
                int i = n1;
                do { --i; } while (i>=0 && map[i] > map[i + 1]);
                if (i < 0) return false; // the last sequence is a decreasing sequence
                int j = n1;
                while (map[i] > map[j]) --j;
                swap(i, j);
                int r = n1;
                int s = i + 1;
                while (r > s)
                {
                    swap(r, s);
                    --r;
                    ++s;
                }
                return true;
            }
        }


        #region Iterative implementation of the Forte07 algorithm with partitioning
        /// <summary>
        /// Implementation of the backtracking isomorphism checking algorithm described in Forte07 paper
        /// with a version of on the fly partitioning.
        /// </summary>
        private class IterativeIsomorphismExtensionWithBacktrackingAndPartitioning
        {
            RootedLabeledDirectedGraph g1;
            RootedLabeledDirectedGraph g2;

            Dictionary<NodeLabel, Bucket> bucketOf;
            const int X = 0;
            const int Y = 1;

            // Set up an array that will serve 2 purposes:
            // * be a container for the isomorphism mapping
            // * contain the "call stack" and allow us to jump directly to last seen
            //   backtrackpoint when backtracking.
            // It would make sense to make it an int[,], but requires changing the graph structure.
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
            Node[,] bijection;


            #region Variables that are part of the backtrackstate.

            Dictionary<Node, int> indexDict = new Dictionary<Node, int>();
            Dictionary<Node, int> rangeDict = new Dictionary<Node, int>();

            CompoundTerm xEdgeLabel;

            int index = 0;
            int relationalPassIndex = 0;

            Node x;
            Node y;
            #endregion

            VertexData xData;
            VertexData yData;

            IEnumerator<CompoundTerm> relationalEdgeEnumerator = default(IEnumerator<CompoundTerm>);
            Set<CompoundTerm> relationalEdgeLabels=default(Set<CompoundTerm>);

            Set<Node> xtoNodes;
            Set<Node> ytoNodes;

            int numberOfNodes;

            BacktrackPointWithPartitions btp=null;

            // BacktrackStack of backtrackpoints
            Stack<BacktrackPointWithPartitions> backtrackStack = new Stack<BacktrackPointWithPartitions>();

            Dictionary<Node, bool> orderIndependent = new Dictionary<Node, bool>();
            Map<Pair<IComparable,bool>, Set<Node>> xPartitions = Map<Pair<IComparable,bool>, Set<Node>>.EmptyMap;
            Map<Pair<IComparable,bool>, Set<Node>> yPartitions = Map<Pair<IComparable,bool>, Set<Node>>.EmptyMap;

            internal IterativeIsomorphismExtensionWithBacktrackingAndPartitioning(RootedLabeledDirectedGraph g1, RootedLabeledDirectedGraph g2, Dictionary<NodeLabel, Bucket> bucketOf)
            {
                this.g1 = g1;
                this.g2 = g2;
                this.bijection = new Node[g1.vertexRecords.Count,2];
                this.x = g1.root;
                this.y = g2.root;
                this.bucketOf = bucketOf;
                this.numberOfNodes = g1.vertexRecords.Count;

            }

            /// <summary>
            /// Walk all reachable functional edges. No backtracking involved. If not possible, then return FAIL,
            /// which might get handled by backtracking upstream the stack. x and y get added to the bijection array if successful.
            /// </summary>
            /// <param name="x1">a vertex from the first graph</param>
            /// <param name="y1">a vertex from the second graph</param>
            /// <returns></returns>
            private ControlState ExtendForOnlyFunctionalEdges(Node x1, Node y1)
            {
                // we only do anything if anything useful can be done. If labels do not match
                // then we may just as well skip the step.
                if (!g1.LabelOf(x1).Equals(g2.LabelOf(y1)))
                {
                    return ControlState.Fail;
                }
                if (indexDict.ContainsKey(x1))
                {
                    //x is already mapped to a node
                    //this node must then be y or else isomorphism is not possible
                    if (bijection[indexDict[x1],Y].Equals(y1))
                        return ControlState.RelationalPass;
                    else
                        return ControlState.Fail;
                }
                 
                if (rangeDict.ContainsKey(y1)) //y is already in the range of iso
                        return ControlState.Fail;

                VertexData xDataPrime = g1.vertexRecords[x1];
                VertexData yDataPrime = g2.vertexRecords[y1];

                CompoundTerm xEdgeLabelPrime;
                bijection[index,X] = x1;
                bijection[index,Y] = y1;
                indexDict.Add(x1, index);
                rangeDict.Add(y1, index);
                index++;
                ControlState result = ControlState.RelationalPass ;
                foreach (Pair<CompoundTerm, Node> edge in xDataPrime.orderedOutgoingEdges)
                {
                    xEdgeLabelPrime = edge.First;
                    if (yDataPrime.orderedOutgoingEdges.ContainsKey(xEdgeLabelPrime))
                        result = ExtendForOnlyFunctionalEdges(edge.Second, yDataPrime.orderedOutgoingEdges[xEdgeLabelPrime]);
                    else
                        return ControlState.Fail;
                    // Break the loop if it is clear that cannot proceed.
                    if (result == ControlState.Fail)
                        return ControlState.Fail;
                }
                return result;
            }


            private ControlState Choose()
            {
                ControlState c=ControlState.Fail;
                Permuter p=btp.permuter;
                int count = xtoNodes.Count;
                do
                {
                    for (int i = 0; i < count; i++)
                    {
                        c = ExtendForOnlyFunctionalEdges(xtoNodes.Choose(i), ytoNodes.Choose(p[i]));
                        if (c == ControlState.Fail)
                            break;
                    }

                    if (c == ControlState.Fail && p.Next())
                    {
                        index = btp.index;
                        indexDict = BacktrackPointWithPartitions.getCopy(btp.indexDict);
                        rangeDict = BacktrackPointWithPartitions.getCopy(btp.rangeDict);
                        
                    }
                    else
                        break;
                } while (true);
                if (count > 1 && p.Next())
                {
                    btp.permuter = p;
                    backtrackStack.Push(btp);
                }
                btp = null;
                return c;
            }

            private ControlState ChooseOne()
            {
                ControlState c = ControlState.Fail;
                int count = xtoNodes.Count;
                for (int i = 0; i < count; i++)
                {
                    c = ExtendForOnlyFunctionalEdges(xtoNodes.Choose(i), ytoNodes.Choose(i));
                    if (c == ControlState.Fail)
                        break;
                }
                return c;
            }


            // States of the state machine used for calculating isomorphism
            internal enum ControlState
            {
                InitialFunctionalPass,
                RelationalPass,
                ProcessPartition,
                NextPartition,
//                NextRelationalEdge,
                Choose,
                ChooseOne,
                Fail,
                Done
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            internal Map<Node, Node> ComputeIsomorphism()
            {
                // We start with a functional pass.
                ControlState c = ControlState.InitialFunctionalPass;
                while (true)
                {

                    switch (c)
                    {
                        # region Find all reachable functional matches of current active nodes x and y.
                        case ControlState.InitialFunctionalPass:
                            c = ExtendForOnlyFunctionalEdges(x, y);


                            break;
                        # endregion

                        #region Find all relational matches after one or more functional pass.
                        case ControlState.RelationalPass:
                            if (xPartitions.Count > 0)
                                goto case ControlState.NextPartition;
                            if (relationalPassIndex >= index)
                            {
                                if (relationalPassIndex == numberOfNodes)
                                    c = ControlState.Done;
                                else
                                    goto case ControlState.Fail; // c = ControlState.Fail;
                                break;
                            
                            }
                            x = bijection[relationalPassIndex,X];
                            y = bijection[relationalPassIndex,Y];
                            xData = g1.vertexRecords[x];
                            yData = g2.vertexRecords[y];
                            if (relationalEdgeEnumerator == default(IEnumerator<CompoundTerm>))
                            {
                                
                                relationalEdgeLabels = Set<CompoundTerm>.EmptySet;
                                foreach (Pair<CompoundTerm, Set<Node>> pair in xData.unorderedOutgoingEdges) {
                                    relationalEdgeLabels = relationalEdgeLabels.Add(pair.First);
                                }
                                relationalEdgeEnumerator = relationalEdgeLabels.GetEnumerator();
                            }
                            if (relationalEdgeEnumerator.MoveNext())
                            {
                                xEdgeLabel = relationalEdgeEnumerator.Current;
                                //xtoNodes = xData.unorderedOutgoingEdges[xEdgeLabel];
                                if (yData.unorderedOutgoingEdges.ContainsKey(xEdgeLabel)) // Perhaps this check can be assumed true for all graphs?
                                {
                                    //ytoNodes = yData.unorderedOutgoingEdges[xEdgeLabel];
                                    xPartitions = GetPartitions(g1, x, xEdgeLabel);
                                    yPartitions = GetPartitions(g2, y, xEdgeLabel);

                                    //Console.WriteLine("Partitions: " + x + " has " + xPartitions.Count + "elements.");  
                                    if (PartitionsAreConsistent(xPartitions,yPartitions))
                                        goto case ControlState.NextPartition;
                                    else
                                        goto case ControlState.Fail;
                                    ////c = ControlState.NextRelationalEdge;
                                    //goto case ControlState.NextRelationalEdge;
                                }
                                else
                                    goto case ControlState.Fail; // c = ControlState.Fail;

                            }
                            else
                            {
                                relationalPassIndex++;
                                relationalEdgeEnumerator = default(IEnumerator<CompoundTerm>);
                                relationalEdgeLabels = default(Set<CompoundTerm>);
                                if (relationalPassIndex == numberOfNodes)
                                    c = ControlState.Done;
                            }
                            break;

                        case ControlState.NextPartition:
                            if (xPartitions.Count > 0) // the other constraints have been implied by PartitionsAreConsistent
                            {
                                Pair<IComparable, bool> label = xPartitions.Keys.Choose(0);
                                xtoNodes = xPartitions[label];
                                xPartitions = xPartitions.RemoveKey(label);
                                ytoNodes = yPartitions[label];
                                yPartitions = yPartitions.RemoveKey(label);
                                goto case ControlState.ProcessPartition;
                            }
                            else 
                                c = ControlState.RelationalPass;
                            break;
                        // It is possible that we have multiple sets of relational edges from a node. The following is done for each set.
                        case ControlState.ProcessPartition:
                            
                                // xEdgeLabel has been set 
                                // xtoNodes has been set;
                                // ytoNodes has been set;
                                Node x1, y1;
                                // remove all nodes that have already been matched. (previously implemented as PruneCandidates)
                                Set<Node> xtoNodesTmp = xtoNodes;
                                int count = xtoNodesTmp.Count;
                                for (int i = 0; i < count; i++)
                                {
                                    x1 = xtoNodesTmp.Choose(i);
                                    if (indexDict.ContainsKey(x1))
                                    {
                                        y1 = bijection[indexDict[x1], Y];
                                        if (ytoNodes.Contains(y1))
                                        {
                                            xtoNodes = xtoNodes.Remove(x1);
                                            ytoNodes = ytoNodes.Remove(y1);
                                        }
                                        else
                                        {
                                            goto case ControlState.Fail;// c = ControlState.Fail;
                                            //goto EndOfNextRelationalEdge;
                                            //break;
                                        }
                                    }
                                    //else if (IsOrderIndependent(x1,g1))
                                    //{
                                    //    xtoNodesOrderIndependent = xtoNodesOrderIndependent.Add(x1);
                                    //    bucketOf[g1.LabelOf(x1)].g2Nodes
                                    //    xtoNodes = xtoNodes.Remove(x1);
                                    //    //ytoNodesOrderIndependent = ytoNodesOrderIndependent.Add(x1);
                                    //}
                                }
                                if (xtoNodes.Count == 0)
                                {
                                    //c = ControlState.RelationalPass;
                                    c = ControlState.NextPartition;
                                    break; // out of the switch statement
                                }// we do not need to check as all nodes contained in xtoNodes were already contained.

                                //if (IsOrderIndependent(xtoNodes.Choose(0),g1))
                                //{
                                //    goto case ControlState.ChooseOne;
                                //}
                                //else
                                //{
                                    btp = new BacktrackPointWithPartitions(indexDict, rangeDict, xEdgeLabel, index, relationalPassIndex, relationalEdgeLabels.Remove(xEdgeLabel), xtoNodes, ytoNodes, xPartitions, yPartitions);
                                    goto case ControlState.Choose;
                                //}
                                //break;
                            
                        #endregion

                        #region Choose one combination of matching relational edges and add a backtrackpoint if necessary
                        case ControlState.Choose:
                            if (btp == null) // check used to stop when backtracking.
                            {
                                c = ControlState.Fail;
                                break;
                            }
                            c = Choose();
                            break;

                        #endregion

                        #region Choose one combination of matching relational edges and do not add backtrackpoint
                        case ControlState.ChooseOne:
                            c = ChooseOne();
                            break;

                        #endregion


                        # region Process a backtrack request.

                        case ControlState.Fail:
                            if (backtrackStack.Count == 0)
                                return null;
                            else
                            {
                                // Reset environment to topmost backtrackpoint and continue
                                // with the relevant Choose.
                                btp = backtrackStack.Pop();
                                
                                // Restore program state from the BacktrackPoint.
                                index = btp.index;
                                relationalPassIndex = btp.relationalPassIndex;
                                indexDict = BacktrackPointWithPartitions.getCopy(btp.indexDict);
                                xEdgeLabel = btp.outLabel;
                                rangeDict = BacktrackPointWithPartitions.getCopy(btp.rangeDict);
                                x = bijection[relationalPassIndex, X];
                                y = bijection[relationalPassIndex, Y];
                                xData = g1.vertexRecords[x];
                                yData = g2.vertexRecords[y];
                                xtoNodes = btp.xtoNodes;
                                ytoNodes = btp.ytoNodes;
                                relationalEdgeLabels = btp.relationalEdgeLabels;
                                relationalEdgeEnumerator = btp.relationalEdgeLabels.GetEnumerator();
                                xPartitions = btp.xPartitions;
                                yPartitions = btp.yPartitions;

                                c = ControlState.Choose;
                            }
                            break;
                        # endregion

                        case ControlState.Done:
                            goto Success;

                        default:
                            break;
                    }
                }
                #region Prepare result map and return.
                Success:

                    Map<Node, Node> iso = Map<Node, Node>.EmptyMap;
                    for (int i = 0; i < numberOfNodes; i++)
                    {
                        iso = iso.Add(bijection[i, X], bijection[i, Y]);
                    }
                    return iso;
                #endregion
            }

            //private class NodePartition : SortedList<Node, object>
            //{
            //    internal bool independent;

            //    internal NodePartition(Node x, bool independent)
            //        : base()
            //    {
            //        base.Add(x, null);
            //        this.independent = independent;
            //    }

            //    internal void AddNode(Node x)
            //    {
            //        base.Add(x, null);
            //    }

            //    internal IList<Node> Nodes
            //    {
            //        get
            //        {
            //            return base.Keys;
            //        }
            //    }
            //}

            /// <summary>
            /// Partition the set of neighbors of all outgoing unordered edges of a node x in g.
            /// Returns all the partitions indexed by the pair (E,L) where
            /// E is an edge label and L is the label of the neighbor.
            /// 
            /// A partition is marked order-independent if some node in that partition is 
            /// order-independent, which implies that all nodes in the same partition must be 
            /// order-independent as well (this is a property implied by the node labeling/hashing algorithm).
            /// </summary>
            /// <param name="g">given graph</param>
            /// <param name="node">given node in g</param> 
            /// <param name="edgeLabel">given edge label</param>
            static Map<Pair<IComparable,bool>, Set<Node>> GetPartitions(RootedLabeledDirectedGraph g, Node node, CompoundTerm edgeLabel)
            {
                Map<Pair<IComparable,bool>, Set<Node>> partitions =
                    Map<Pair<IComparable,bool>, Set<Node>>.EmptyMap;
                VertexData xData1 = g.vertexRecords[node]; // xData is already in context.

                foreach (Node x1 in xData1.unorderedOutgoingEdges[edgeLabel])
                {
                    Pair<IComparable, bool> key = new Pair<IComparable, bool>(g.LabelOf(x1), true);//IsOrderIndependent(x1, g));
                    Set<Node> nodePartition;
                    if (partitions.TryGetValue(key, out nodePartition))
                        partitions=partitions.Override(key,nodePartition.Add(x1));
                    else
                        partitions=partitions.Add(key, new Set<Node>(x1));

                }
                return partitions;
            }

            /// <summary>
            /// A node n is order-independent in g if all nodes in 
            /// its neighborhood are in singleton buckets
            /// </summary>
            bool IsOrderIndependent(Node n, RootedLabeledDirectedGraph g) // this is currently too general. It should rather work on a partition.
            {
                if (orderIndependent.ContainsKey(n))
                    return orderIndependent[n];
                VertexData nData = g.vertexRecords[n];
                foreach (Pair<CompoundTerm, Node> edge in nData.incomingEdges)
                    if (bucketOf[g.LabelOf(edge.Second)].Size > 1)
                    {
                        orderIndependent.Add(n, false);
                        return false;
                    }
                foreach (Pair<CompoundTerm, Node> edge in nData.orderedOutgoingEdges)
                    if (bucketOf[g.LabelOf(edge.Second)].Size > 1)
                    {
                        orderIndependent.Add(n, false);
                        return false;
                    }
                foreach (Pair<CompoundTerm, Set<Node>> edge in nData.unorderedOutgoingEdges)
                    foreach (Node x1 in edge.Second)
                        if (bucketOf[g.LabelOf(x1)].Size > 1)
                        {
                            orderIndependent.Add(n, false);
                            return false;
                        }
                orderIndependent.Add(n, true);
                return true;
            }

            /// <summary>
            /// Returns true if both partitions have the same number of elements
            /// and the same keys, and for each key, the corresponding partitions 
            /// have the same size and order-idependence property.
            /// </summary>
            static private bool PartitionsAreConsistent(Map<Pair<IComparable,bool>, Set<Node>> xPartitions, Map<Pair<IComparable,bool>, Set<Node>> yPartitions)
            {
                if (xPartitions.Count != yPartitions.Count)
                    return false;
                foreach (Pair<IComparable, bool> label in xPartitions.Keys)
                {
                    if (!yPartitions.ContainsKey(label))
                        return false;
                    Set<Node> xPartition = xPartitions[label];
                    Set<Node> yPartition = yPartitions[label];
                    if (xPartition.Count != yPartition.Count)
                        return false;
                }
                return true;
            }



        }

        #endregion

        #region Iterative implementation of the Forte07 algorithm without partitioning
        /// <summary>
        /// Implementation of the backtracking isomorphism checking algorithm described in Forte07 paper
        /// without on the fly partitioning.
        /// </summary>
        private class IterativeIsomorphismExtensionWithBacktracking
        {
            RootedLabeledDirectedGraph g1;
            RootedLabeledDirectedGraph g2;

            Dictionary<NodeLabel, Bucket> bucketOf;
            const int X = 0;
            const int Y = 1;

            // Set up an array that will serve 2 purposes:
            // * be a container for the isomorphism mapping
            // * contain the "call stack" and allow us to jump directly to last seen
            //   backtrackpoint when backtracking.
            // It would make sense to make it an int[,], but requires changing the graph structure.
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
            Node[,] bijection;


            #region Variables that are part of the backtrackstate.

            Dictionary<Node, int> indexDict = new Dictionary<Node, int>();
            Dictionary<Node, int> rangeDict = new Dictionary<Node, int>();

            CompoundTerm xEdgeLabel;

            int index = 0;
            int relationalPassIndex = 0;

            Node x;
            Node y;
            #endregion

            VertexData xData;
            VertexData yData;

            IEnumerator<CompoundTerm> relationalEdgeEnumerator = default(IEnumerator<CompoundTerm>);
            Set<CompoundTerm> relationalEdgeLabels = default(Set<CompoundTerm>);

            Set<Node> xtoNodes;
            Set<Node> ytoNodes;

            int numberOfNodes;

            BacktrackPoint btp = null;

            // BacktrackStack of backtrackpoints
            Stack<BacktrackPoint> backtrackStack = new Stack<BacktrackPoint>();

            internal IterativeIsomorphismExtensionWithBacktracking(RootedLabeledDirectedGraph g1, RootedLabeledDirectedGraph g2, Dictionary<NodeLabel, Bucket> bucketOf)
            {
                this.g1 = g1;
                this.g2 = g2;
                this.bijection = new Node[g1.vertexRecords.Count, 2];
                this.x = g1.root;
                this.y = g2.root;
                this.bucketOf = bucketOf;
                this.numberOfNodes = g1.vertexRecords.Count;

            }

            /// <summary>
            /// Walk all reachable functional edges. No backtracking involved. If not possible, then return FAIL,
            /// which might get handled by backtracking upstream the stack. x and y get added to the bijection array if successful.
            /// </summary>
            /// <param name="x1">a vertex from the first graph</param>
            /// <param name="y1">a vertex from the second graph</param>
            /// <returns></returns>
            private ControlState ExtendForOnlyFunctionalEdges(Node x1, Node y1)
            {
                // we only do anything if anything useful can be done. If labels do not match
                // then we may just as well skip the step.
                if (!g1.LabelOf(x1).Equals(g2.LabelOf(y1)))
                {
                    return ControlState.Fail;
                }
                if (indexDict.ContainsKey(x1))
                {
                    //x is already mapped to a node
                    //this node must then be y or else isomorphism is not possible
                    if (bijection[indexDict[x1], Y].Equals(y1))
                        return ControlState.RelationalPass;
                    else
                        return ControlState.Fail;
                }

                if (rangeDict.ContainsKey(y1)) //y is already in the range of iso
                    return ControlState.Fail;

                VertexData xDataPrime = g1.vertexRecords[x1];
                VertexData yDataPrime = g2.vertexRecords[y1];

                CompoundTerm xEdgeLabelPrime;
                bijection[index, X] = x1;
                bijection[index, Y] = y1;
                indexDict.Add(x1, index);
                rangeDict.Add(y1, index);
                index++;
                ControlState result = ControlState.RelationalPass;
                foreach (Pair<CompoundTerm, Node> edge in xDataPrime.orderedOutgoingEdges)
                {
                    xEdgeLabelPrime = edge.First;
                    if (yDataPrime.orderedOutgoingEdges.ContainsKey(xEdgeLabelPrime))
                        result = ExtendForOnlyFunctionalEdges(edge.Second, yDataPrime.orderedOutgoingEdges[xEdgeLabelPrime]);
                    else
                        return ControlState.Fail;
                    // Break the loop if it is clear that cannot proceed.
                    if (result == ControlState.Fail)
                        return ControlState.Fail;
                }
                return result;
            }


            private ControlState Choose()
            {
                ControlState c = ControlState.Fail;
                Permuter p = btp.permuter;
                int count = xtoNodes.Count;
                do
                {
                    for (int i = 0; i < count; i++)
                    {
                        c = ExtendForOnlyFunctionalEdges(xtoNodes.Choose(i), ytoNodes.Choose(p[i]));
                        if (c == ControlState.Fail)
                            break;
                    }

                    if (c == ControlState.Fail && p.Next())
                    {
                        index = btp.index;
                        indexDict = BacktrackPoint.getCopy(btp.indexDict);
                        rangeDict = BacktrackPoint.getCopy(btp.rangeDict);
                    }
                    else
                        break;
                } while (true);
                if (count > 1 && p.Next())
                {
                    btp.permuter = p;
                    backtrackStack.Push(btp);
                }
                btp = null;
                return c;
            }


            // States of the state machine used for calculating isomorphism
            internal enum ControlState
            {
                InitialFunctionalPass,
                RelationalPass,
                NextRelationalEdge,
                Choose,
                Fail,
                Done
            }

            internal Map<Node, Node> ComputeIsomorphism()
            {
                // We start with a functional pass.
                ControlState c = ControlState.InitialFunctionalPass;
                while (true)
                {

                    switch (c)
                    {
                        # region Find all reachable functional matches of current active nodes x and y.
                        case ControlState.InitialFunctionalPass:
                            c = ExtendForOnlyFunctionalEdges(x, y);

                            break;
                        # endregion

                        #region Find all relational matches after one or more functional pass.
                        case ControlState.RelationalPass:
                            if (relationalPassIndex >= index)
                            {
                                if (relationalPassIndex == numberOfNodes)
                                    c = ControlState.Done;
                                else
                                    c = ControlState.Fail;
                                break;

                            }
                            x = bijection[relationalPassIndex, X];
                            y = bijection[relationalPassIndex, Y];
                            xData = g1.vertexRecords[x];
                            yData = g2.vertexRecords[y];
                            if (relationalEdgeEnumerator == default(IEnumerator<CompoundTerm>))
                            {

                                relationalEdgeLabels = Set<CompoundTerm>.EmptySet;
                                foreach (Pair<CompoundTerm, Set<Node>> pair in xData.unorderedOutgoingEdges)
                                {
                                    relationalEdgeLabels = relationalEdgeLabels.Add(pair.First);
                                }
                                relationalEdgeEnumerator = relationalEdgeLabels.GetEnumerator();
                            }
                            if (relationalEdgeEnumerator.MoveNext())
                            {
                                xEdgeLabel = relationalEdgeEnumerator.Current;
                                xtoNodes = xData.unorderedOutgoingEdges[xEdgeLabel];
                                if (yData.unorderedOutgoingEdges.ContainsKey(xEdgeLabel)) // Perhaps this check can be assumed true for all graphs?
                                {
                                    ytoNodes = yData.unorderedOutgoingEdges[xEdgeLabel];
                                    c = ControlState.NextRelationalEdge;
                                }
                                else
                                    c = ControlState.Fail;

                            }
                            else
                            {
                                relationalPassIndex++;
                                relationalEdgeEnumerator = default(IEnumerator<CompoundTerm>);
                                relationalEdgeLabels = default(Set<CompoundTerm>);
                                if (relationalPassIndex == numberOfNodes)
                                    c = ControlState.Done;
                            }
                            break;

                        // It is possible that we have multiple sets of relational edges from a node. The following is done for each set.
                        case ControlState.NextRelationalEdge:

                            // xEdgeLabel has been set 
                            // xtoNodes has been set;
                            // ytoNodes has been set;
                            Node x1, y1;
                            // remove all nodes that have already been matched.
                            for (int i = 0; i < xtoNodes.Count; i++)
                            {
                                x1 = xtoNodes.Choose(i);
                                if (indexDict.ContainsKey(x1))
                                {
                                    y1 = bijection[indexDict[x1], Y];
                                    if (ytoNodes.Contains(y1))
                                    {
                                        xtoNodes = xtoNodes.Remove(x1);
                                        ytoNodes = ytoNodes.Remove(y1);
                                    }
                                    else
                                    {
                                        c = ControlState.Fail;
                                        goto EndOfNextRelationalEdge;
                                    }
                                }
                            }
                            if (xtoNodes.Count == 0)
                            {
                                c = ControlState.RelationalPass;
                                break; // out of the switch statement
                            }// we do not need to check as all nodes contained in xtoNodes were already contained.
                            //isoExt = ExtendIsomorphism4(backtrackBuckets, backtrackStack, isoExt, xtoNodes.Choose(), ytoNodes.Choose());

                            btp = new BacktrackPoint(indexDict, rangeDict, xEdgeLabel, index, relationalPassIndex, relationalEdgeLabels.Remove(xEdgeLabel), xtoNodes, ytoNodes);
                            c = ControlState.Choose;

                        //goto case ControlState.Choose;
                        EndOfNextRelationalEdge:
                            break;

                        #endregion

                        #region Choose one combination of matching relational edges
                        case ControlState.Choose:
                            if (btp == null) // check used to stop when backtracking.
                            {
                                c = ControlState.Fail;
                                break;
                            }
                            c = Choose();
                            break;

                        #endregion

                        # region Process a backtrack request.

                        case ControlState.Fail:
                            if (backtrackStack.Count == 0)
                                return null;
                            else
                            {
                                // Reset environment to topmost backtrackpoint and continue
                                // with the relevant Choose from where we left off last time.
                                btp = backtrackStack.Pop();

                                // Restore program state from the BacktrackPoint.
                                index = btp.index;
                                relationalPassIndex = btp.relationalPassIndex;
                                indexDict = BacktrackPoint.getCopy(btp.indexDict);
                                xEdgeLabel = btp.outLabel;
                                rangeDict = BacktrackPoint.getCopy(btp.rangeDict);
                                x = bijection[relationalPassIndex, X];
                                y = bijection[relationalPassIndex, Y];
                                xData = g1.vertexRecords[x];
                                yData = g2.vertexRecords[y];
                                xtoNodes = btp.xtoNodes;
                                ytoNodes = btp.ytoNodes;
                                relationalEdgeLabels = btp.relationalEdgeLabels;
                                relationalEdgeEnumerator = btp.relationalEdgeLabels.GetEnumerator();

                                c = ControlState.Choose;
                            }
                            break;
                        # endregion

                        case ControlState.Done:
                            goto Success;

                        default:
                            break;
                    }
                }
                #region Prepare result map and return.
            Success:

                Map<Node, Node> iso = Map<Node, Node>.EmptyMap;
                for (int i = 0; i < numberOfNodes; i++)
                {
                    iso = iso.Add(bijection[i, X], bijection[i, Y]);
                }
                return iso;
                #endregion
            }

        }

        #endregion



        #region Ullmann's algorithm

        /// <summary>
        /// Returns an isomorphism from g1 to g2, if g1 and g2 are isomorphic;
        /// returns null, otherwise.
        /// 
        /// Based on Ullmann's SUBGRAPH isomorphism algorithm from JACM, Vol.23(1), pp 31-42, 1976
        /// specialized as a GRAPH isomorphism algorithm for rooted directed labelled graphs.
        /// </summary>
        static public Map<Node, Node> ComputeIsomorphismUllmann(RootedLabeledDirectedGraph g1, RootedLabeledDirectedGraph g2)
        {
            GraphIsomorphism gi = new GraphIsomorphism(g1, g2);
            return gi.ComputeIsomorphismUllmann();
        }
        Map<Node, Node> ComputeIsomorphismUllmann()
        {
            if (!ComputeBuckets())
                return null;

            //create the initial candidate map M for isomorphisms
            //all nodes with the same label get the same candidates.
            Map<Node, Set<Node>> M = Map<Node, Set<Node>>.EmptyMap;
            foreach (Bucket bucket in bucketOf.Values)
            {
                foreach (Node x in bucket.g1Nodes)
                    M = M.Add(x, bucket.g2Nodes);
            }

            Set<Node> ns = M.Keys;

            Map<Node, Set<Node>> I = ComputeIsomorphismFromNode(ns, M);
            if (I == null)
                return null;
            else
                return I.Convert<Node, Node>(delegate(Pair<Node, Set<Node>> entry) { return new Pair<Node, Node>(entry.First, entry.Second.Choose(0)); });
        }

        /// <summary>
        /// Computes an isomorphism that is consistent with M.
        /// M already includes a partial isomorphism from all g1 nodes not in ns.
        /// </summary>
        Map<Node, Set<Node>> ComputeIsomorphismFromNode(Set<Node> ns, Map<Node, Set<Node>> M)
        {
            //isomorphism was found
            if (ns.IsEmpty)
                return M;

            //choose a particular node from ns
            Node x = ns.Choose(0);
            Set<Node> ns1 = ns.Remove(x);

            //TBD: use the optimization with order-idependent nodes 
            //to eliminate backtracking over orders that do not make a difference
            //this is done by choosing a subset of nodes from ns and removing them
            //all at once thus skipping several levels in the search tree rather than 
            //picking nodes one at a time

            //try with a mapping from x to to some y
            //this basically corresponds to backtracking over the y's
            foreach (Node y in M[x])
            {
                Map<Node, Set<Node>> M1 = M.Override(x, new Set<Node>(y));
                //Remove first all instances of y from the other candidates
                Map<Node, Set<Node>> M1a = RemoveUsedG2Node(M1, x, y);
                if (M1a != null)
                {
                    Map<Node, Set<Node>> M2 = Refine(M1a);
                    if (M2 != null)
                    {
                        Map<Node, Set<Node>> I = ComputeIsomorphismFromNode(ns1, M2);
                        if (I != null)
                            return I;
                    }
                }
            }
            //isomorphism was not found
            return null;
        }

        static Map<Node, Set<Node>> RemoveUsedG2Node(Map<Node, Set<Node>> M, Node x, Node y)
        {
            Map<Node, Set<Node>> M1 = M;
            foreach (Pair<Node, Set<Node>> entry in M)
            {
                if (!entry.First.Equals(x) && entry.Second.Contains(y))
                {
                    if (entry.Second.Count == 1)
                        return null; //isomorphism candidates become empty
                    else
                        M1 = M1.Override(entry.First, entry.Second.Remove(y));
                }
            }
            return M1;
        }

        /// <summary>
        /// Refinement procedure adapted from Ullmann's algorithm
        /// that eliminates candidates from M. In combination with 
        /// the main procedure it guarantees that the endresult is indeed an isomorphism
        /// </summary>
        Map<Node, Set<Node>> Refine(Map<Node, Set<Node>> M)
        {
            foreach (Pair<Node,Set<Node>> entry in M)
            {
                foreach (Node j in entry.Second)
                {
                    if (CannotBeIsomorphic(entry.First, j, M))
                    {
                        Set<Node> candidates = entry.Second.Remove(j);
                        if (candidates.IsEmpty)
                            return null;
                        else
                            return Refine(M.Override(entry.First, candidates));
                    }
                }
            }
            return M;
        }

        /// <summary>
        /// Returns true if node i from g1 and node j from g2 cannot be isomorphic
        /// Corresponds to the negation of condition (2) for directed graphs in Ullmann's paper
        /// </summary>
        bool CannotBeIsomorphic(Node i, Node j, Map<Node, Set<Node>> M)
        {
            VertexData iData = g1.vertexRecords[i];
            VertexData jData = g2.vertexRecords[j];

            //check that ordered outgoing edges match
            foreach (Pair<CompoundTerm, Node> edge in iData.orderedOutgoingEdges)
                if (!M[edge.Second].Exists(delegate(Node y){return jData.orderedOutgoingEdges.Contains(new Pair<CompoundTerm, Node>(edge.First,y));}))
                    return true;

            //check that unordered outgoing edges match
            foreach (Pair<CompoundTerm, Set<Node>> edges in iData.unorderedOutgoingEdges)
            {
                Set<Node> toNodes = jData.unorderedOutgoingEdges[edges.First];
                foreach (Node n in edges.Second)
                    if (!M[n].Exists(delegate(Node y) { return toNodes.Contains(y); }))
                        return true;
            }

            //check that incoming edges match
            foreach (Pair<CompoundTerm, Node> edge in iData.incomingEdges)
                if (!M[edge.Second].Exists(delegate(Node y) { return jData.incomingEdges.Contains(new Pair<CompoundTerm, Node>(edge.First, y)); }))
                    return true;

            return false;
        }
        #endregion
    }

 
}
