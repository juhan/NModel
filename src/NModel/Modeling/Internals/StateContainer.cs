//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using SC = System.Collections;
using System.Diagnostics.CodeAnalysis;
using NModel.Internals;
using NModel.Terms;
using NModel.Execution;
using NModel.Algorithms;
//using Vertex = System.IComparable;
using Vertex = System.Int32;


namespace NModel.Internals
{
    /// <summary>
    /// A mutable type for containing IStates during exploration. Has support for
    /// state isomorphism checking.
    /// </summary>    
    /// <typeparam name="T">The sort of element that implements <see cref="IState"/>.</typeparam>    
    public sealed class StateContainer<T> where T : IState //: CachedHashCollectionValue<T>
    {
        Dictionary<T, RootedLabeledDirectedGraph> objects;
        Dictionary<int, Set<RootedLabeledDirectedGraph>> graphHashes;
        Dictionary<T, RootedLabeledDirectedGraph> recentCache;
        Dictionary<RootedLabeledDirectedGraph, T> graphsToStates;
        Dictionary<Symbol, Dictionary<Literal, Vertex>> abstractObjectIds;
        bool preserveBuiltInIds = false;
        Dictionary<Vertex, Symbol> builtInIds; // used for storing ids of Sets, Maps, Sequences etc which are otherwise added to the label
        // is used for visualisation
        int count;

        // we actually assume LibraryModelProgram here.
        ModelProgram mp;

        #region Constructors

        /// <summary>
        /// A constructor that creates and initializes a state container. ModelProgram is used for extracting
        /// abstractObjectIds and storing them internally.
        /// </summary>
        /// <param name="mp">The relevant model program. Currently a <see cref="LibraryModelProgram"/> is assumed.</param>
        /// <param name="elements">States to be added to the container upon creation</param>
        public StateContainer(ModelProgram mp, params T[] elements)
        {
            objects = new Dictionary<T, RootedLabeledDirectedGraph>();
            graphHashes = new Dictionary<int, Set<RootedLabeledDirectedGraph>>();
            recentCache = new Dictionary<T, RootedLabeledDirectedGraph>();
            abstractObjectIds = new Dictionary<Symbol, Dictionary<Literal, int>>();
            graphsToStates = new Dictionary<RootedLabeledDirectedGraph, T>();
            builtInIds = new Dictionary<Vertex, Symbol>();
            this.mp = mp;
            falseMatches = 0;
            trueMatches = 0;
            //errorCount = 0;
            foreach (T o in elements)
                this.Add(o);
        }

        #endregion


        /// <summary>
        /// Removes an element from the container. Note that this member is mutable.
        /// </summary>
        /// <param name="value">The value to be deleted.</param>
        public void Remove(T value)
        {
            if (count > 0)
            {
                RootedLabeledDirectedGraph graph;
                if (objects.TryGetValue(value, out graph))
                {
                    objects.Remove(value);
                    Set<RootedLabeledDirectedGraph> graphs;
                    graphHashes.TryGetValue(graph.hashCode, out graphs);
                    graphHashes.Remove(graph.hashCode);
                    graphs = graphs.Remove(graph);
                    graphsToStates.Remove(graph);
                    if (!graphs.IsEmpty) graphHashes.Add(graph.hashCode, graphs);
                    count--;
                }
            }
        }

        /// <summary>
        /// Adds an element to the container.
        /// </summary>
        /// <param name="value">The value to add</param>
        public void Add(T value)
        {
            RootedLabeledDirectedGraph graph;
            // It would be nice if the isohash mechanism were built into state as then we would not have to calculate hash each time.
            // additional fields can not be easily added to the state because states are CompoundValues with readonly fields.
            if (objects.TryGetValue(value, out graph))
            {
                return;
            }
            else
            {
                if (recentCache.TryGetValue(value, out graph)) recentCache.Remove(value);
                else graph = ExtractGraph(value);

                objects.Add(value, graph);
                graphsToStates.Add(graph, value);
                Set<RootedLabeledDirectedGraph> similarGraphs;
                if (graphHashes.TryGetValue(graph.hashCode, out similarGraphs))
                {
                    graphHashes.Remove(graph.hashCode);
                    graphHashes.Add(graph.hashCode, similarGraphs.Add(graph));
                }
                else
                {
                    similarGraphs = new Set<RootedLabeledDirectedGraph>(graph);
                    graphHashes.Add(graph.hashCode, similarGraphs);
                }
                count++;
            }
            return;
        }

        /// <summary>
        /// Check if an element is contained in the container.
        /// </summary>
        /// <param name="value">A state</param>
        /// <returns>True if the state is in the container</returns>
        public bool Contains(T value)
        {
            return objects.ContainsKey(value);
        }

        internal bool PreserveBuiltInIds
        {
            get { return preserveBuiltInIds; }
            set { this.preserveBuiltInIds = value; }
        }

        internal const string BUILTIN_MAP = "Map";
        internal const string BUILTIN_SET = "Set";
        internal const string BUILTIN_BAG = "Bag";
        internal const string BUILTIN_SEQUENCE = "Sequence";
        internal const string BUILTIN_PAIR = "Pair";
        internal const string BUILTIN_TRIPLE = "Triple";

        // used for creating the nodes of the graph
        internal static int objectIdCounter = 0;

        internal void AnalyzeTerm(Term t, int fieldIndex, bool ordered, /* bool rootLevel, Vertex currentVertex, */ VertexData currentData, Dictionary<Vertex, VertexData> vertexRecords)
        {
            Type tt = t.GetType();
            if (tt == typeof(Literal))
            {
                currentData.label = currentData.label.Add(new Pair<CompoundTerm, IComparable>(new CompoundTerm(new Symbol(fieldIndex.ToString()), new Sequence<Term>()), t));
            }
            else if (tt == typeof(CompoundTerm))
            {
                CompoundTerm ct = (CompoundTerm)t;
                String symbolName = ct.Symbol.ShortName;

                //int obj;

                switch (symbolName)
                {
                    case BUILTIN_MAP:
                        {
                            if (ct.Arguments.Count > 0) // the number of arguments should always be even
                            {
                                Vertex map;
                                VertexData mapData = null;

                                IEnumerator<Term> argEnumerator = ct.Arguments.GetEnumerator();
                                //map = new ObjectId(new Symbol(BUILTIN_MAP), mapCounter++);
                                //mapData = new VertexData();
                                Set<Vertex> maplets = Set<Vertex>.EmptySet;
                                while (argEnumerator.MoveNext())
                                {
                                    //map = new ObjectId(new Symbol(BUILTIN_MAP), mapCounter++);
                                    map = ++objectIdCounter;
                                    if (preserveBuiltInIds)
                                        builtInIds.Add(map, new Symbol(symbolName));
                                    mapData = new VertexData();
                                    vertexRecords.Add(map, mapData);
                                    AnalyzeTerm(argEnumerator.Current, 0, true, /* false, map, */ mapData, vertexRecords);
                                    argEnumerator.MoveNext();
                                    AnalyzeTerm(argEnumerator.Current, 1, true, /* false, map, */ mapData, vertexRecords);
                                    maplets = maplets.Add(map);
                                    mapData.label = mapData.label.Add(new Pair<CompoundTerm, IComparable>((CompoundTerm)ct.Symbol.AsTerm, 0));
                                    mapData.vertex = map;
                                    //vertexRecords[map]=mapData;
                                }
                                CompoundTerm edgeLabel = new CompoundTerm(new Symbol(fieldIndex.ToString()), new Sequence<Term>(new Literal(ct.Arguments.Count >> 1)));
                                currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Add(edgeLabel, maplets);


                                //foreach (Term t1 in ct.Arguments.FieldValues())
                                //{
                                //    //if (rootLevel)
                                //    //    // if we have a root level map, then we we can say that
                                //    //    // the maplets are edges between the nodes the map points to.
                                //    //    // The labels of the edges are made up of the fieldIndex of the root field.
                                //    //    // Labels are distinguished from other labels by including the letter "R" in the label.
                                //    //{
                                //    //    if (fst)
                                //    //    {
                                //    //        map = new ObjectId(new Symbol("__DUMMY"),0);
                                //    //        mapData = new VertexData();
                                //    //        AnalyzeTerm(t1, (fst ? 0 : 1), true, false, map, mapData, vertexRecords);

                                //    //    }
                                //    //    else
                                //    //    {
                                //    //        VertexData vd;
                                //    //        mapData2 = new VertexData();
                                //    //        AnalyzeTerm(t1, (fst ? 0 : 1), true, false, map, mapData2, vertexRecords);
                                //    //        Pair<CompoundTerm, IComparable> sourcePair=mapData.orderedOutgoingEdges.Choose();
                                //    //        Pair<CompoundTerm, IComparable> targetPair;
                                //    //        vertexRecords.TryGetValue(sourcePair.Second, out vd);
                                //    //        if (mapData2.orderedOutgoingEdges.IsEmpty)
                                //    //        {
                                //    //            // If the target object is not abstract and if it is an enum, then we do not have a node for it.
                                //    //            // There is a theoretical possibility of name clashes, thus "__R" is reserved.
                                //    //            vd.label = vd.label.Add(new Pair<CompoundTerm, IComparable>(new CompoundTerm(new Symbol("__R" + fieldIndex), new Sequence<Term>()), t1));
                                //    //        }
                                //    //        else
                                //    //        {
                                //    //            targetPair = mapData2.orderedOutgoingEdges.Choose();
                                //    //            vd.unorderedOutgoingEdges = vd.unorderedOutgoingEdges.Add(new Pair<CompoundTerm, IComparable>(new CompoundTerm(new Symbol("R" + fieldIndex), new Sequence<Term>()), targetPair.Second));
                                //    //        }
                                //    //    }
                                //    //    fst = !fst;
                                //    //}
                                //    //else
                                //    //{

                                //    if (fst) map = new ObjectId(new Symbol(BUILTIN_MAP), mapCounter++);
                                //    if (fst) mapData = new VertexData();
                                //    AnalyzeTerm(t1, (fst ? 0 : 1), true, false, map, mapData, vertexRecords);
                                //    fst = !fst;
                                //    //Console.WriteLine("BM1: " + fieldIndex.ToString() + "," + ct.Arguments.Count);
                                //    //                            ordered = false; //we are creating maplets which are by definition unordered.
                                //    //                            if (ordered)
                                //    //                                currentData.orderedOutgoingEdges = (fst ? currentData.orderedOutgoingEdges.Add(new Pair<CompoundTerm, IComparable>(new CompoundTerm(new Symbol(fieldIndex.ToString()), new Sequence<Term>(new Literal(ct.Arguments.Count>>1))), map))
                                //    //                                : currentData.orderedOutgoingEdges);
                                //    //                            else
                                //    if (fst)
                                //    {
                                //        CompoundTerm edgeLabel = new CompoundTerm(new Symbol(fieldIndex.ToString()), new Sequence<Term>(new Literal(ct.Arguments.Count >> 1)));
                                //        if (currentData.unorderedOutgoingEdges.ContainsKey(edgeLabel))
                                //            currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Override(edgeLabel, currentData.unorderedOutgoingEdges[edgeLabel].Add(map));
                                //        else
                                //        {
                                //            currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Add(edgeLabel, new Set<Vertex>(map));
                                //        }
                                //    }
                                //    else
                                //    {
                                //    }
                                //    //currentData.unorderedOutgoingEdges = (fst ? currentData.unorderedOutgoingEdges.Add(new Pair<CompoundTerm, IComparable>(new CompoundTerm(new Symbol(fieldIndex.ToString()), new Sequence<Term>(new Literal(ct.Arguments.Count >> 1))), map))
                                //    //: currentData.unorderedOutgoingEdges);
                                //    if (!fst)
                                //    {
                                //        mapData.label = mapData.label.Add(new Pair<CompoundTerm, IComparable>((CompoundTerm)ct.FunctionSymbol1.AsTerm, 0));
                                //        mapData.vertex = map;
                                //        vertexRecords.Add(map, mapData);
                                //    }
                                //    //}
                                //    // Perhaps it is possible to add a special case here to make maps ordered if the contents of the maps is orderable.

                                //}

                            }
                            else
                            {
                                currentData.label = currentData.label.Add(new Pair<CompoundTerm, IComparable>(new CompoundTerm(new Symbol(fieldIndex.ToString()), new Sequence<Term>()), new Literal("Empty")));
                            }
                            break;
                        }
                    case BUILTIN_SET:
                        {
                            if (ct.Arguments.Count > 0)
                            {
                                // we need to create a new node
                                //Vertex set = new ObjectId(new Symbol(BUILTIN_SET), setCounter++);
                                Vertex set = ++objectIdCounter;
                                if (preserveBuiltInIds)
                                    builtInIds.Add(set, new Symbol(symbolName));
                                VertexData setData = new VertexData();
                                vertexRecords.Add(set, setData);
                                foreach (Term t1 in ct.Arguments.FieldValues())
                                {
                                    AnalyzeTerm(t1, 0, false, /* false, set,*/ setData, vertexRecords);
                                    // Perhaps it is possible to add a special case here to make maps ordered if the contents of the maps is orderable.

                                }
                                //Console.WriteLine("BS1: " + fieldIndex.ToString() + "," + ct.Arguments.Count);
                                CompoundTerm edgeLabel = new CompoundTerm(new Symbol(fieldIndex.ToString()), new Literal(ct.Arguments.Count));
                                if (ordered)
                                    currentData.orderedOutgoingEdges = currentData.orderedOutgoingEdges.Add(edgeLabel, set);
                                else
                                    if (currentData.unorderedOutgoingEdges.ContainsKey(edgeLabel))
                                        currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Override(edgeLabel, currentData.unorderedOutgoingEdges[edgeLabel].Add(set));
                                    else
                                    {
                                        currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Add(edgeLabel, new Set<Vertex>(set));
                                    }

                                setData.label = setData.label.Add(new Pair<CompoundTerm, IComparable>((CompoundTerm)ct.Symbol.AsTerm, 0));
                                setData.label = setData.label.Add(new Pair<CompoundTerm, IComparable>(edgeLabel, setData.unorderedOutgoingEdges.Count));
                                setData.vertex = set;
                            }
                            else
                            {
                                currentData.label = currentData.label.Add(new Pair<CompoundTerm, IComparable>(new CompoundTerm(new Symbol(fieldIndex.ToString()), new Sequence<Term>()), new Literal("Empty")));
                            }
                            break;
                        }
                    case BUILTIN_BAG:
                        {
                            if (ct.Arguments.Count > 0) // the number of arguments is always even.
                            {
                                // we need to create a new node
                                //Vertex bag = new ObjectId(new Symbol(BUILTIN_BAG), bagCounter++);
                                Vertex bag = ++objectIdCounter;
                                if (preserveBuiltInIds)
                                    builtInIds.Add(bag, new Symbol(symbolName));
                                VertexData bagData = new VertexData();
                                vertexRecords.Add(bag, bagData);
                                Bag<Term> bagCounts = Bag<Term>.EmptyBag;

                                IEnumerator<Term> argEnumerator = ct.Arguments.GetEnumerator();
                                Term t1;
                                while (argEnumerator.MoveNext())
                                {
                                    t1 = argEnumerator.Current;
                                    argEnumerator.MoveNext();
                                    int itemCount = int.Parse(argEnumerator.Current.ToString());
                                    bagCounts = bagCounts.Add(argEnumerator.Current);
                                    AnalyzeTerm(t1, itemCount, false, /* false, bag, */ bagData, vertexRecords);
                                }

                                Sequence<Term> edgeLabelArg = Sequence<Term>.EmptySequence;
                                foreach (Term tbc in bagCounts.Keys)
                                {
                                    edgeLabelArg = edgeLabelArg.AddFirst(tbc);
                                    edgeLabelArg = edgeLabelArg.AddFirst(new Literal(bagCounts.CountItem(tbc)));
                                }
                                CompoundTerm edgeLabel = new CompoundTerm(new Symbol(fieldIndex.ToString()), edgeLabelArg);
                                if (ordered)
                                    currentData.orderedOutgoingEdges = currentData.orderedOutgoingEdges.Add(edgeLabel, bag);
                                else
                                    if (currentData.unorderedOutgoingEdges.ContainsKey(edgeLabel))
                                        currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Override(edgeLabel, currentData.unorderedOutgoingEdges[edgeLabel].Add(bag));
                                    else
                                    {
                                        currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Add(edgeLabel, new Set<Vertex>(bag));
                                    }
                                bagData.label = bagData.label.Add(new Pair<CompoundTerm, IComparable>((CompoundTerm)ct.Symbol.AsTerm, 0));
                                bagData.label = bagData.label.Add(new Pair<CompoundTerm, IComparable>(edgeLabel, 0));
                                bagData.vertex = bag;
                            }
                            else
                            {
                                currentData.label = currentData.label.Add(new Pair<CompoundTerm, IComparable>(new CompoundTerm(new Symbol(fieldIndex.ToString()), new Sequence<Term>()), new Literal("Empty")));
                            }
                            break;
                        }

                    case BUILTIN_SEQUENCE:
                        {
                            if (ct.Arguments.Count > 0)
                            {
                                // we need to create a new node
                                //Vertex set = new ObjectId(new Symbol(BUILTIN_SET), setCounter++);
                                Vertex sequence = ++objectIdCounter;
                                if (preserveBuiltInIds)
                                    builtInIds.Add(sequence, new Symbol(symbolName));
                                VertexData sequenceData = new VertexData();
                                vertexRecords.Add(sequence, sequenceData);
                                int sequenceIndex = 0;
                                IEnumerator<Term> argEnumerator = ct.Arguments.GetEnumerator();

                                //// What if we just distinguish the head and think of the tail as a set?
                                //argEnumerator.MoveNext();
                                //AnalyzeTerm(argEnumerator.Current, 0, true, false, sequence, sequenceData, vertexRecords);
                                //while (argEnumerator.MoveNext())
                                //    AnalyzeTerm(argEnumerator.Current, 1, false, false, sequence, sequenceData, vertexRecords);

                                foreach (Term t1 in ct.Arguments.FieldValues())
                                {
                                    AnalyzeTerm(t1, sequenceIndex, true, /* false, sequence, */ sequenceData, vertexRecords);
                                    sequenceIndex++;
                                    // Perhaps it is possible to add a special case here to make maps ordered if the contents of the maps is orderable.

                                }
                                CompoundTerm edgeLabel = new CompoundTerm(new Symbol(fieldIndex.ToString()), new Literal(ct.Arguments.Count));
                                if (ordered)
                                    currentData.orderedOutgoingEdges = currentData.orderedOutgoingEdges.Add(edgeLabel, sequence);
                                else
                                    if (currentData.unorderedOutgoingEdges.ContainsKey(edgeLabel))
                                        currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Override(edgeLabel, currentData.unorderedOutgoingEdges[edgeLabel].Add(sequence));
                                    else
                                    {
                                        currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Add(edgeLabel, new Set<Vertex>(sequence));
                                    }
                                sequenceData.label = sequenceData.label.Add(new Pair<CompoundTerm, IComparable>((CompoundTerm)ct.Symbol.AsTerm, 0));
                                sequenceData.label = sequenceData.label.Add(new Pair<CompoundTerm, IComparable>(edgeLabel, sequenceData.orderedOutgoingEdges.Count));
                                sequenceData.vertex = sequence;
                            }
                            else
                            {
                                currentData.label = currentData.label.Add(new Pair<CompoundTerm, IComparable>(new CompoundTerm(new Symbol(fieldIndex.ToString()), new Sequence<Term>()), new Literal("Empty")));
                            }
                            break;
                        }
                    case BUILTIN_PAIR:
                        {
                            if (ct.Arguments.Count > 0)
                            {
                                // we need to create a new node
                                //Vertex set = new ObjectId(new Symbol(BUILTIN_SET), setCounter++);
                                Vertex pair = ++objectIdCounter;
                                if (preserveBuiltInIds)
                                    builtInIds.Add(pair, new Symbol(symbolName));
                                VertexData pairData = new VertexData();
                                vertexRecords.Add(pair, pairData);
                                IEnumerator<Term> argEnumerator = ct.Arguments.GetEnumerator();
                                argEnumerator.MoveNext();
                                AnalyzeTerm(argEnumerator.Current, 0, true, /* false, pair, */ pairData, vertexRecords);
                                argEnumerator.MoveNext();
                                AnalyzeTerm(argEnumerator.Current, 1, true, /* false, pair, */ pairData, vertexRecords);
                                CompoundTerm edgeLabel = new CompoundTerm(new Symbol(fieldIndex.ToString()), new Literal(ct.Arguments.Count));
                                if (ordered)
                                    currentData.orderedOutgoingEdges = currentData.orderedOutgoingEdges.Add(edgeLabel, pair);
                                else
                                    if (currentData.unorderedOutgoingEdges.ContainsKey(edgeLabel))
                                        currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Override(edgeLabel, currentData.unorderedOutgoingEdges[edgeLabel].Add(pair));
                                    else
                                    {
                                        currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Add(edgeLabel, new Set<Vertex>(pair));
                                    }
                                pairData.label = pairData.label.Add(new Pair<CompoundTerm, IComparable>((CompoundTerm)ct.Symbol.AsTerm, 0));
                                pairData.label = pairData.label.Add(new Pair<CompoundTerm, IComparable>(edgeLabel, pairData.orderedOutgoingEdges.Count));
                                pairData.vertex = pair;
                            }
                            else
                            {
                                currentData.label = currentData.label.Add(new Pair<CompoundTerm, IComparable>(new CompoundTerm(new Symbol(fieldIndex.ToString()), new Sequence<Term>()), new Literal("Empty")));
                            }
                            break;
                        }
                    case BUILTIN_TRIPLE:
                        {
                            if (ct.Arguments.Count > 0)
                            {
                                // we need to create a new node
                                //Vertex set = new ObjectId(new Symbol(BUILTIN_SET), setCounter++);
                                Vertex triple = ++objectIdCounter;
                                if (preserveBuiltInIds)
                                    builtInIds.Add(triple, new Symbol(symbolName));
                                VertexData tripleData = new VertexData();
                                vertexRecords.Add(triple, tripleData);
                                IEnumerator<Term> argEnumerator = ct.Arguments.GetEnumerator();
                                argEnumerator.MoveNext();
                                AnalyzeTerm(argEnumerator.Current, 0, true, /* false, triple, */ tripleData, vertexRecords);
                                argEnumerator.MoveNext();
                                AnalyzeTerm(argEnumerator.Current, 1, true, /* false, triple, */ tripleData, vertexRecords);
                                argEnumerator.MoveNext();
                                AnalyzeTerm(argEnumerator.Current, 2, true, /* false, triple, */ tripleData, vertexRecords);
                                CompoundTerm edgeLabel = new CompoundTerm(new Symbol(fieldIndex.ToString()), new Literal(ct.Arguments.Count));
                                if (ordered)
                                    currentData.orderedOutgoingEdges = currentData.orderedOutgoingEdges.Add(edgeLabel, triple);
                                else
                                    if (currentData.unorderedOutgoingEdges.ContainsKey(edgeLabel))
                                        currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Override(edgeLabel, currentData.unorderedOutgoingEdges[edgeLabel].Add(triple));
                                    else
                                    {
                                        currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Add(edgeLabel, new Set<Vertex>(triple));
                                    }
                                tripleData.label = tripleData.label.Add(new Pair<CompoundTerm, IComparable>((CompoundTerm)ct.Symbol.AsTerm, 0));
                                tripleData.label = tripleData.label.Add(new Pair<CompoundTerm, IComparable>(edgeLabel, tripleData.orderedOutgoingEdges.Count));
                                tripleData.vertex = triple;
                            }
                            else
                            {
                                currentData.label = currentData.label.Add(new Pair<CompoundTerm, IComparable>(new CompoundTerm(new Symbol(fieldIndex.ToString()), new Sequence<Term>()), new Literal("Empty")));
                            }
                            break;
                        }
                    default:
                        if (mp.IsSortAbstract(ct.Symbol))
                        {

                            IEnumerator<Term> argEnumerator = ct.Arguments.GetEnumerator();
                            argEnumerator.MoveNext();
                            Literal absLiteral = (Literal)argEnumerator.Current;
                            //getAbstractOidIdentifierObjectId(ct.FunctionSymbol1, absLiteral, ref abstractObjectIds, out literalDict, out obj);
                            //ObjectId objId = new ObjectId(ct.FunctionSymbol1, obj);
                            //Console.WriteLine("Getting objId for " + ct.FunctionSymbol1.Name + " absLiteral " + absLiteral);
                            Vertex objId;
                            objId = getAbstractOidIdentifier(ct.Symbol, absLiteral, ref abstractObjectIds);
                            //Console.WriteLine(objId);
                            VertexData vd;
                            bool newVertex = false;
                            if (!vertexRecords.TryGetValue(objId, out vd))
                            {
                                vd = new VertexData();
                                newVertex = true;
                            }

                            //the first field contains the object id, thus we start from 1.
                            int fieldCounter = 1;
                            while (argEnumerator.MoveNext())
                            {
                                // The abstract node has fields which have to be dealt with
                                AnalyzeTerm(argEnumerator.Current, fieldCounter, true, /* false, objId, */ vd, vertexRecords);
                                fieldCounter++;
                            }
                            //Console.WriteLine("DO1: " + fieldIndex.ToString() + ",(),"+t);
                            CompoundTerm edgeLabel = new CompoundTerm(new Symbol(fieldIndex.ToString()), new Sequence<Term>());
                            if (ordered)
                                currentData.orderedOutgoingEdges = currentData.orderedOutgoingEdges.Add(edgeLabel, objId);
                            else
                                if (currentData.unorderedOutgoingEdges.ContainsKey(edgeLabel))
                                    currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Override(edgeLabel, currentData.unorderedOutgoingEdges[edgeLabel].Add(objId));
                                else
                                {
                                    currentData.unorderedOutgoingEdges = currentData.unorderedOutgoingEdges.Add(edgeLabel, new Set<Vertex>(objId));
                                }
                            if (newVertex)
                            {
                                vd.label = vd.label.Add(new Pair<CompoundTerm, IComparable>(CompoundTerm.Create(ct.Symbol.ToString()), 0));
                                vd.vertex = objId;
                                vertexRecords.Add(objId, vd);
                            }
                        }
                        else // we have a non-abstract compound value or an enum 
                        {
                            // We should collapse all non abstract part into the currentData and only create ordered outgoing edges for
                            // fields containing abstract objects.
                            //Console.WriteLine("Adding this to currentData.label: " + ct.FunctionSymbol1.Name + ct.ToCompactString());
                            currentData.label = currentData.label.Add(new Pair<CompoundTerm, IComparable>(new CompoundTerm(new Symbol(fieldIndex.ToString()), new Sequence<Term>()), t));
                        }
                        break;
                }
            }
            else
            {
                // There should be no other cases
                throw new InvalidOperationException("State contained unsupported terms: " + tt.ToString());
            }

        }

        // // This member was used when vertices were ObjectIds.
        //private static void getAbstractOidIdentifier(Symbol sym, Literal absLiteral, ref Dictionary<Symbol,Dictionary<Literal,int>> absOids, out Dictionary<Literal, int> literalDict, out Vertex obj)
        //{
        //    if (absOids.TryGetValue(sym, out literalDict))
        //    {
        //        if (!literalDict.TryGetValue(absLiteral, out obj))
        //        {
        //            obj = literalDict.Count + 1;
        //            literalDict.Add(absLiteral, obj);
        //        }
        //    }
        //    else
        //    {
        //        literalDict = new Dictionary<Literal, int>();
        //        absOids.Add(sym, literalDict);
        //        obj = literalDict.Count + 1;
        //        literalDict.Add(absLiteral, obj);
        //    }
        //}


        /// <summary>
        /// Returns an integer that corresponds to previously seen object ID or returns a fresh id and stores the object ID.
        /// Has the side effect of increasing objectIdCounter when a fresh ID is generated.
        /// </summary>
        /// <param name="sym">Symbol of the sort</param>
        /// <param name="absLiteral">The ID or item, in case of enums</param>
        /// <param name="absOids">reference to the data structure mapping abstract oids to graph nodes</param>
        /// <returns></returns>
        private static int getAbstractOidIdentifier(Symbol sym, Literal absLiteral, ref Dictionary<Symbol, Dictionary<Literal, int>> absOids)
        {
            Vertex obj;
            Dictionary<Literal, int> literalDict;
            if (absOids.TryGetValue(sym, out literalDict))
            {
                if (!literalDict.TryGetValue(absLiteral, out obj))
                {
                    obj = ++objectIdCounter;
                    literalDict.Add(absLiteral, obj);
                }
            }
            else
            {
                literalDict = new Dictionary<Literal, int>();
                absOids.Add(sym, literalDict);
                obj = ++objectIdCounter;
                literalDict.Add(absLiteral, obj);
            }
            //return objectIdCounter1;
            return obj;
        }


        /// <summary>
        /// Extracts graph from the state. Currently supports only <see cref="SimpleState"/>, but adding
        /// <see cref="PairState"/> support is easy.
        /// </summary>
        /// <param name="state"></param>
        /// <returns>The graph corresponding to the state</returns>
        internal RootedLabeledDirectedGraph ExtractGraph(T state)
        {

            objectIdCounter = 0;
            abstractObjectIds = new Dictionary<Symbol, Dictionary<Literal, int>>();
            builtInIds = new Dictionary<Vertex, Symbol>();
            SimpleState simple = state as SimpleState;
            //Vertex root = new ObjectId(new Symbol("Root"), 1);
            Vertex root = ++objectIdCounter;
            if (preserveBuiltInIds)
                builtInIds.Add(root, new Symbol("Root"));
            Dictionary<Vertex, VertexData> vertexRecords = new Dictionary<Vertex, VertexData>();

            if (simple != null)
            {
                VertexData rootData = new VertexData();
                rootData.label = rootData.label.Add(new Pair<CompoundTerm, IComparable>(CompoundTerm.Create("root"), 0));

                for (int i = 0; i < simple.LocationValuesCount; i++)
                {
                    Term t = simple.GetLocationValue(i);
                    AnalyzeTerm(t, i, true, /* true, root, */ rootData, vertexRecords); // It is possible that a set is attached to an ordered outgoing edge. This is currently broken.

                }
                rootData.vertex = root;
                vertexRecords.Add(root, rootData);
            }
            //StringBuilder dot;
            //String[] fsm=printGraph(root, vertexRecords, out dot);
            RootedLabeledDirectedGraph g = new RootedLabeledDirectedGraph(root, vertexRecords);
            //g.fsm = fsm;
            //g.dot = dot;
            g.hashCode = computeHash(g);
            //Console.WriteLine(g.hashCode);
            return g;
        }

        /// <summary>
        /// Extract a rooted labeled directed graph representation from the state
        /// and covert it to dot representation.
        /// </summary>
        public String[] ExtractFSM(T state)
        {
            PreserveBuiltInIds = true;
            RootedLabeledDirectedGraph g = ExtractGraph(state);
            Dictionary<Vertex, Pair<Symbol, Literal>> vertexOid = new Dictionary<Vertex, Pair<Symbol, Literal>>();
            foreach (KeyValuePair<Symbol, Dictionary<Literal, Vertex>> kv in abstractObjectIds)
            {
                foreach (KeyValuePair<Literal, Vertex> kv2 in kv.Value)
                    vertexOid.Add(kv2.Value, new Pair<Symbol, Literal>(kv.Key, kv2.Key));
            }
            foreach (KeyValuePair<Vertex, Symbol> kv in builtInIds)
            {
                vertexOid.Add(kv.Key, new Pair<Symbol, Literal>(kv.Value, new Literal(kv.Key)));
            }
            StringBuilder dot;
            String[] fsm = visualizeGraph(/* g.root, */ g.vertexRecords, vertexOid, out dot);
            //Console.WriteLine(dot);
            PreserveBuiltInIds = false;
            return fsm;
        }

        /// <summary>
        /// Computes hash of the graph.
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        internal static int computeHash(RootedLabeledDirectedGraph g)
        {
            int hashCode = 0;
            Bag<IComparable> hash = Bag<IComparable>.EmptyBag;
            foreach (KeyValuePair<Vertex, VertexData> kv in g.vertexRecords)
            {
                hash = computeHash(hash, 0, /* kv.Key, */ kv.Value /*, g.vertexRecords */);
            }
            //Console.WriteLine(hash.ToString());
            hashCode = hash.GetHashCode();
            return hashCode;
        }


        private static Bag<IComparable> computeHash(Bag<IComparable> hash, int level, /* Vertex oid, */ VertexData vd /* , Dictionary<Vertex,VertexData> vertexRecords */)
        {
            if (level > 3) return hash;
            Set<CompoundTerm> ordered = Set<CompoundTerm>.EmptySet;
            foreach (Pair<CompoundTerm, Vertex> p in vd.orderedOutgoingEdges)
            {
                ordered = ordered.Add(p.First);
            }
            Bag<CompoundTerm> outbag = new Bag<CompoundTerm>();
            foreach (Pair<CompoundTerm, Set<Vertex>> p in vd.unorderedOutgoingEdges)
            {
                outbag = outbag.AddMultiple(p.First, p.Second.Count);
            }
            Bag<CompoundTerm> inbag = new Bag<CompoundTerm>();
            foreach (Pair<CompoundTerm, Vertex> p in vd.incomingEdges)
            {
                inbag = inbag.Add(p.First);
            }
            hash = hash.Add(new Triple<Set<CompoundTerm>, Set<Pair<CompoundTerm, IComparable>>, Pair<Bag<CompoundTerm>, Bag<CompoundTerm>>>(ordered, vd.label, new Pair<Bag<CompoundTerm>, Bag<CompoundTerm>>(outbag, inbag)));
            return hash;
        }

        internal static String[] visualizeGraph(/* Vertex v, */ Dictionary<Vertex, VertexData> vertexRecords, Dictionary<Vertex, Pair<Symbol, Literal>> vertexSymbols, out StringBuilder dot)
        {
            StringBuilder sbn = new StringBuilder();
            StringBuilder sbe = new StringBuilder();
            List<String> fsml = new List<String>();
            sbn.AppendLine("digraph state {");
            //sbn.AppendLine("graph [");
            sbn.AppendLine("rankdir=LR;");
            //sbn.AppendLine("];");
            // root Console.WriteLine(v);
            int mpCounter = 0; // this is to make all arcs distinguishable for the ModelProgramViewer.
            foreach (KeyValuePair<Vertex, VertexData> kv in vertexRecords)
            {
                //ObjectId oid = (ObjectId)kv.Key;
                //Console.WriteLine(kv.Key + " " + vertexSymbols.Count);
                Pair<Symbol, Literal> oid = vertexSymbols[kv.Key]; //new Pair<Symbol,Literal>(new Symbol("oid"),new Literal(kv.Key));
                sbn.AppendLine("\"" + oid.First + ":" + oid.Second + "\" [");
                //StringBuilder label = new StringBuilder();
                sbn.Append("label = \"");
                sbn.Append(oid.First + ":" + oid.Second);
                foreach (Pair<CompoundTerm, IComparable> p in kv.Value.label)
                {
                    sbn.Append("| ");
                    sbn.Append(p.First.ToCompactString().Replace('"', '\'') + ":" + p.Second.ToString().Replace('"', '\''));
                }
                sbn.AppendLine("\"");
                sbn.AppendLine("shape = \"record\"");
                sbn.AppendLine("];");


                String edgeLabel = "";
                foreach (Pair<CompoundTerm, Vertex> p in kv.Value.orderedOutgoingEdges)
                {
                    Pair<Symbol, Literal> target = vertexSymbols[p.Second]; //new Pair<Symbol,Literal>(new Symbol("oid"),new Literal(p.Second));
                    sbe.AppendLine("\"" + oid.First + ":" + oid.Second + "\" -> \"" + target.First + ":" + target.Second + "\" [");
                    sbe.AppendLine(" label = \"" + p.First.ToCompactString() + "\" ];");
                    if (p.First.Arguments.Count > 0)
                        edgeLabel = p.First.ToCompactString().Replace('(', '_').Replace(')', '_').Replace(',', '_').Replace(' ', '_');
                    else
                        edgeLabel = p.First.Symbol.ToString();
                    fsml.Add(("T(" + oid.First + "" + oid.Second + "(),l" + edgeLabel + "(" + (mpCounter++) + ")," + target.First + "" + target.Second + "())").Replace('<', '_').Replace('>', '_').Replace('"', '_'));
                    //fsml.Add("T(" + oid.First + "" + oid.Second + "(" + vertexRecords[kv.Key].label.ToString() + "),l" + edgeLabel + "(" + (mpCounter++) + ")," + target.First + "" + target.Second + "(" + vertexRecords[p.Second].label.ToString() + "))");
                }
                foreach (Pair<CompoundTerm, Set<Vertex>> p in kv.Value.unorderedOutgoingEdges)
                    foreach (Vertex vtx in p.Second)
                    {
                        Pair<Symbol, Literal> target = vertexSymbols[vtx]; //new Pair<Symbol, Literal>(new Symbol("oid"), new Literal(vtx));
                        sbe.AppendLine("\"" + oid.First + ":" + oid.Second + "\" -> \"" + target.First + ":" + target.Second + "\" [");
                        sbe.AppendLine(" label = \"" + p.First.ToCompactString() + "\" ];");
                        if (p.First.Arguments.Count > 0)
                            edgeLabel = p.First.ToCompactString().Replace('(', '_').Replace(')', '_').Replace(',', '_').Replace(' ', '_');
                        else
                            edgeLabel = p.First.Symbol.ToString();
                        fsml.Add(("T(" + oid.First + "" + oid.Second + "(),l" + edgeLabel + "(" + (mpCounter++) + ")," + target.First + "" + target.Second + "())").Replace('<', '_').Replace('>', '_').Replace('"', '_'));
                        //fsml.Add("T(" + oid.First + "" + oid.Second + "(" + vertexRecords[kv.Key].label.ToString() + "),l" + edgeLabel + "(" + (mpCounter++) + ")," + target.First + "" + target.Second + "(" + vertexRecords[vtx].label.ToString() + "))");
                        // fsml.Add("T(" + oid.First + "" + oid.Second + "(),l" + p.First.FunctionSymbol1.ToString() + "_" + p.Second.ToString().Replace('(', '<').Replace(')', '>') + "(" + (mpCounter++) + ")," + target.ObjectSort + "" + target.Id + "())");
                    }
            }
            if (fsml.Count == 0) fsml.Add("T(Root1(),l(1),Root1())"); // How would we write a graph consisting of a single node?
            String[] fsm = new String[fsml.Count];
            int i = 0;
            foreach (String s in fsml) fsm[i++] = s;
            String fst = fsm[0]; // as Root is always the last element, we switch it to be the first, as MP considers the first element the root one.
            fsm[0] = fsm[fsml.Count - 1];
            fsm[fsml.Count - 1] = fst;
            dot = sbn.Append(sbe).Append('}').AppendLine();
            //            Console.Write(sbn.ToString());
            //            Console.Write(sbe.ToString());
            //            Console.WriteLine("}");
            //foreach (String s in fsm) Console.WriteLine(s);
            //Console.WriteLine("FalseMatches: " + falseMatches + ", trueMatches: " + trueMatches);
            return fsm;

        }

        static int falseMatches = 0;
        static int trueMatches = 0;
        //static int errorCount = 0;


        /// <summary>
        /// Method that checks whether the container already contains a state that is isomorphic to 
        /// value. If such a state is found it is returned in isoState.
        /// </summary>
        public bool HasIsomorphic(T value, out T isoState)
        {
            //DateTime startTime = DateTime.Now;
            RootedLabeledDirectedGraph graph;
            if (objects.TryGetValue(value, out graph))
            {
                isoState = value;
                return true;  // if the graph is contained then done.
            }
            else
            {
                graph = ExtractGraph(value);
                //Console.WriteLine("Extracting graph: " + (DateTime.Now - startTime).ToString() + ". The graph has " + graph.vertexRecords.Count + " nodes.");

            }
            // The comments below can be removed. The code was used for evaluation and sanity checks.
            if (graphHashes.ContainsKey(graph.hashCode))
            {
                Set<RootedLabeledDirectedGraph> similarGraphs;
                graphHashes.TryGetValue(graph.hashCode, out similarGraphs);
                foreach (RootedLabeledDirectedGraph g in similarGraphs)
                {
                    //DateTime startTimeUllmann = DateTime.Now;
                    //Map<Vertex, Vertex> iso1;// = GraphIsomorphism.ComputeIsomorphismUllmann(graph,g);
                    //DateTime endTimeUllmann = DateTime.Now;
                    //DateTime startTimeUs = DateTime.Now;
                    //Map<Vertex, Vertex> iso2;// = GraphIsomorphism.ComputeIsomorphism(graph, g);
                    //DateTime endTimeUs = DateTime.Now;
                    //DateTime startTimeUsOld = DateTime.Now;
                    Map<Vertex, Vertex> iso = GraphIsomorphism.ComputeIsomorphism2(graph, g);
                    //DateTime endTimeUsOld = DateTime.Now;
                    //iso1 = iso3;
                    //iso2 = iso3;
                    //if (iso1 != null)
                    //{
                    //    //Console.WriteLine("Ullmann: " + iso1.ToString());
                    //}
                    //if (iso2 != null)
                    //{
                    //    //Console.WriteLine("We: " + iso2.ToString());
                    //}
                    //if ((iso1 != null & iso2 == null) | (iso1 == null & iso2 != null))
                    //{
                    //    errorCount++;
                    //    Console.WriteLine("CONTRADICTION: "+errorCount);
                    //    Console.WriteLine("Ullmann says "+((iso1==null?"not iso":"iso")));
                    //    Console.WriteLine("We say " + ((iso2 == null ? "not iso" : "iso")));
                    //    if (iso1 != null) {
                    //        Console.WriteLine("Ullmann: "+iso1.ToString());
                    //    }
                    //    if (iso2 != null)
                    //    {
                    //        Console.WriteLine("We: "+iso2.ToString());
                    //    }
                    //    //Console.WriteLine(g.dot);
                    //    Console.WriteLine();
                    //    //Console.WriteLine(graph.dot);
                    //    SimpleState ss = value as SimpleState;
                    //    Console.WriteLine(ss.ToString());
                    //    //throw new Exception("Ullmann and and our algorithm yielded contradicting results");
                    //}
                    ////Console.WriteLine();
                    ////Console.WriteLine(g.dot);
                    ////Console.WriteLine();
                    ////Console.WriteLine(graph.dot);
                    //Map<Vertex, Vertex> iso = iso1;
                    if (iso != null)
                    {
                        trueMatches++;
                        //Console.WriteLine("Result2: " + (iso != null));
                        //foreach (IComparable node in iso.Keys) {
                        //    IComparable node2;
                        //    iso.TryGetValue(node, out node2);
                        //    Console.WriteLine("From: " + node.ToString()+" to:"+node2.ToString());
                        //}
                        graphsToStates.TryGetValue(g, out isoState);
                        //Console.WriteLine("Found iso took: Ullmann " + (endTimeUllmann - startTimeUllmann).ToString() + " and us " + (endTimeUs - startTimeUs).ToString() + " and our old " + (endTimeUsOld - startTimeUsOld).ToString() + ". The graph had " + graph.vertexRecords.Count + " nodes.");
                        //Console.WriteLine("T: " + trueMatches + ", F: " + falseMatches);
                        return true;
                    }
                    falseMatches++;
                    //Console.WriteLine("Not isomorphic:");
                    //foreach (String s in graph.fsm) Console.WriteLine(s);
                    //Console.WriteLine("and:");
                    //foreach (String s in g.fsm) Console.WriteLine(s);
                    //Console.WriteLine("Did not find iso took: Ullmann " + (endTimeUllmann - startTimeUllmann).ToString() + " and us " + (endTimeUs - startTimeUs).ToString() + " and our old " + (endTimeUsOld - startTimeUsOld).ToString() + ". The graph had " + graph.vertexRecords.Count + " nodes.");
                    //Console.WriteLine("T: " + trueMatches + ", F: " + falseMatches);
                }
            }
            recentCache.Add(value, graph);
            isoState = default(T);
            //Console.WriteLine("HasIsomorphic returned false in: " + (DateTime.Now - startTime).ToString() + ". The graph had " + graph.vertexRecords.Count + " nodes.");
            return false;
        }

        /// <summary>
        /// Enumerator of each element of this set. If two sets are equal, then their enumerations are in the same order.
        /// (This is a fixed order with no external significance.)
        /// </summary>
        /// <returns>The enumerator of this set</returns>
        //public override IEnumerator<T> GetEnumerator()
        public IEnumerator<T> GetEnumerator()
        {
            return ((object)objects == null ? (IEnumerator<T>)(new List<T>().GetEnumerator()) : (IEnumerator<T>)objects.Keys.GetEnumerator());
        }

        /// <summary>
        /// Returns the number of states contained.
        /// </summary>
        public int Count
        {
            get { return count; }
        }

        /// <summary>
        /// Pretty printer
        /// </summary>
        /// <returns>A human-readable representation of the this set.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("StateContainer(");
            bool isFirst = true;
            if (this.objects != null)
                foreach (T val in this.objects.Keys)
                {
                    if (!isFirst) sb.Append(", ");
                    PrettyPrinter.Format(sb, val);
                    isFirst = false;
                }
            sb.Append(")");
            return sb.ToString();
        }

    }
}
