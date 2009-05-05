using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

using NModel.Internals;
using NModel.Terms;
using NModel.Algorithms;
using NModel;

using Vertex = System.Int32;

namespace NModel.Tests
{
    [TestFixture]
    public class SymmetriesTest
    {
        [Test]
        public void IsoSingleNode()
        {
            Vertex root1 = 1;//new ObjectId(new Symbol("Root"), 1);
            VertexData vd1 = new VertexData();
            Vertex root2 = 1;// new ObjectId(new Symbol("Root"), 1);
            VertexData vd2 = new VertexData();
            Dictionary<Vertex, VertexData> vr1 = new Dictionary<Vertex, VertexData>();
            vr1.Add(root1, vd1);
            Dictionary<Vertex, VertexData> vr2 = new Dictionary<Vertex, VertexData>();
            vr2.Add(root2, vd2);
            RootedLabeledDirectedGraph g1 = new RootedLabeledDirectedGraph(root1, vr1);
            RootedLabeledDirectedGraph g2 = new RootedLabeledDirectedGraph(root2, vr2);
            Map<Vertex, Vertex> iso1 = GraphIsomorphism.ComputeIsomorphism1(g1, g2);
            Map<Vertex, Vertex> iso2 = GraphIsomorphism.ComputeIsomorphism2(g1, g2);
            Assert.AreEqual(1, iso1.Count);
            Assert.AreEqual(1, iso2.Count);
        }

        [Test]
        public void IsoSingleNodeUllmann()
        {
            Vertex root1 = 1;// new ObjectId(new Symbol("Root"), 1);
            VertexData vd1 = new VertexData();
            Vertex root2 = 1;// new ObjectId(new Symbol("Root"), 1);
            VertexData vd2 = new VertexData();
            Dictionary<Vertex, VertexData> vr1 = new Dictionary<Vertex, VertexData>();
            vr1.Add(root1, vd1);
            Dictionary<Vertex, VertexData> vr2 = new Dictionary<Vertex, VertexData>();
            vr2.Add(root1, vd2);
            RootedLabeledDirectedGraph g1 = new RootedLabeledDirectedGraph(root1, vr1);
            RootedLabeledDirectedGraph g2 = new RootedLabeledDirectedGraph(root2, vr2);
            Map<Vertex, Vertex> iso = GraphIsomorphism.ComputeIsomorphismUllmann(g1, g2);
            Assert.AreEqual(1, iso.Count);
        }


        [Test]
        public void IsoOnlyFunctional()
        {
            GraphBuilder gh1 = new GraphBuilder();
            GraphBuilder gh2 = new GraphBuilder();
            Vertex root1 = gh1.InitGraph("Root", "label");
            Vertex root2 = gh2.InitGraph("Root", "label");
            Vertex[] g1nodes = new Vertex[2];//new ObjectId[2];
            Vertex[] g2nodes = new Vertex[2];//new ObjectId[2];
            g1nodes[0] = gh1.AddVertex("NodeType1", "a");
            g1nodes[1] = gh1.AddVertex("NodeType2", "b");
            g2nodes[1] = gh2.AddVertex("NodeType1", "a");
            g2nodes[0] = gh2.AddVertex("NodeType2", "b");
            gh1.AddFunctionalTransition(root1, g1nodes[0], "l0");
            gh1.AddFunctionalTransition(root1, g1nodes[1], "l1");
            gh2.AddFunctionalTransition(root2, g2nodes[1], "l0");
            gh2.AddFunctionalTransition(root2, g2nodes[0], "l1");
            RootedLabeledDirectedGraph g1 = gh1.GetGraph();
            RootedLabeledDirectedGraph g2 = gh2.GetGraph();
            Map<Vertex, Vertex> iso1 = GraphIsomorphism.ComputeIsomorphism1(g1, g2);
            Map<Vertex, Vertex> iso2 = GraphIsomorphism.ComputeIsomorphism2(g1, g2);
            Map<Vertex, Vertex> isoU = GraphIsomorphism.ComputeIsomorphismUllmann(g1, g2);
            Assert.AreEqual(iso1.Count, iso2.Count);
            Assert.AreEqual(iso1.Count, isoU.Count);
            Assert.AreEqual(3, iso1.Count);
        }

        [Test]
        public void IsoOnlyFunctionalNotIso()
        {
            GraphBuilder gh1 = new GraphBuilder();
            GraphBuilder gh2 = new GraphBuilder();
            Vertex root1 = gh1.InitGraph("Root", "label");
            Vertex root2 = gh2.InitGraph("Root", "label");
            Vertex[] g1nodes = new Vertex[2];
            Vertex[] g2nodes = new Vertex[2];
            g1nodes[0] = gh1.AddVertex("NodeType1", "a");
            g1nodes[1] = gh1.AddVertex("NodeType2", "b");
            g2nodes[1] = gh2.AddVertex("NodeType1", "a");
            g2nodes[0] = gh2.AddVertex("NodeType2", "b");
            gh1.AddFunctionalTransition(root1, g1nodes[0], "l0");
            gh1.AddFunctionalTransition(root1, g1nodes[1], "l1");
            gh2.AddFunctionalTransition(root2, g2nodes[1], "l0");
            gh2.AddFunctionalTransition(root2, g2nodes[0], "l2");
            RootedLabeledDirectedGraph g1 = gh1.GetGraph();
            RootedLabeledDirectedGraph g2 = gh2.GetGraph();
            Map<Vertex, Vertex> iso1 = GraphIsomorphism.ComputeIsomorphism1(g1, g2);
            Map<Vertex, Vertex> iso2 = GraphIsomorphism.ComputeIsomorphism2(g1, g2);
            Map<Vertex, Vertex> isoU = GraphIsomorphism.ComputeIsomorphismUllmann(g1, g2);
            Assert.IsNull(iso1);
            Assert.IsNull(iso2);
            Assert.IsNull(isoU);
        }


        [Test]
        public void IsoSingletonBuckets()
        {
            GraphBuilder gh1 = new GraphBuilder();
            GraphBuilder gh2 = new GraphBuilder();
            Vertex root1 = gh1.InitGraph("Root", "label");
            Vertex root2 = gh2.InitGraph("Root", "label");
            Vertex[] g1nodes = new Vertex[2];
            Vertex[] g2nodes = new Vertex[2];
            g1nodes[0] = gh1.AddVertex("NodeType1", "a");
            g1nodes[1] = gh1.AddVertex("NodeType2", "b");
            g2nodes[1] = gh2.AddVertex("NodeType1", "a");
            g2nodes[0] = gh2.AddVertex("NodeType2", "b");
            gh1.AddFunctionalTransition(root1, g1nodes[0], "l0");
            gh1.AddRelationalTransition(root1, g1nodes[1], "l1");
            gh2.AddFunctionalTransition(root2, g2nodes[1], "l0");
            gh2.AddRelationalTransition(root2, g2nodes[0], "l1");
            RootedLabeledDirectedGraph g1 = gh1.GetGraph();
            RootedLabeledDirectedGraph g2 = gh2.GetGraph();
            Map<Vertex, Vertex> iso1 = GraphIsomorphism.ComputeIsomorphism1(g1, g2);
            Map<Vertex, Vertex> iso2 = GraphIsomorphism.ComputeIsomorphism2(g1, g2);
            Map<Vertex, Vertex> isoU = GraphIsomorphism.ComputeIsomorphismUllmann(g1, g2);
            Assert.AreEqual(iso1.Count, iso2.Count);
            Assert.AreEqual(iso1.Count, isoU.Count);
            Assert.AreEqual(3, iso1.Count);
        }

        [Test]
        public void IsoSingletonBuckets2()
        {
            GraphBuilder gh1 = new GraphBuilder();
            GraphBuilder gh2 = new GraphBuilder();
            Vertex root1 = gh1.InitGraph("Root", "label");
            Vertex root2 = gh2.InitGraph("Root", "label");
            Vertex[] g1nodes = new Vertex[2];
            Vertex[] g2nodes = new Vertex[2];
            g1nodes[0] = gh1.AddVertex("NodeType1", "a");
            g1nodes[1] = gh1.AddVertex("NodeType1", "b");
            g2nodes[1] = gh2.AddVertex("NodeType1", "a");
            g2nodes[0] = gh2.AddVertex("NodeType1", "b");
            gh1.AddRelationalTransition(root1, g1nodes[0], "l0");
            gh1.AddRelationalTransition(root1, g1nodes[1], "l0");
            gh2.AddRelationalTransition(root2, g2nodes[1], "l0");
            gh2.AddRelationalTransition(root2, g2nodes[0], "l0");
            RootedLabeledDirectedGraph g1 = gh1.GetGraph();
            RootedLabeledDirectedGraph g2 = gh2.GetGraph();
            Map<Vertex, Vertex> iso1 = GraphIsomorphism.ComputeIsomorphism1(g1, g2);
            Map<Vertex, Vertex> iso2 = GraphIsomorphism.ComputeIsomorphism2(g1, g2);
            Map<Vertex, Vertex> isoU = GraphIsomorphism.ComputeIsomorphismUllmann(g1, g2);
            Assert.AreEqual(iso1.Count, iso2.Count);
            Assert.AreEqual(iso1.Count, isoU.Count);
            Assert.AreEqual(3, iso2.Count);
        }


        [Test]
        public void IsoNonSingletonBuckets()
        {
            GraphBuilder gh1 = new GraphBuilder();
            GraphBuilder gh2 = new GraphBuilder();
            Vertex root1 = gh1.InitGraph("Root", "label");
            Vertex root2 = gh2.InitGraph("Root", "label");
            Vertex[] g1nodes = new Vertex[2];
            Vertex[] g2nodes = new Vertex[2];
            g1nodes[0] = gh1.AddVertex("NodeType1", "a");
            g1nodes[1] = gh1.AddVertex("NodeType1", "a");
            g2nodes[1] = gh2.AddVertex("NodeType1", "a");
            g2nodes[0] = gh2.AddVertex("NodeType1", "a");
            gh1.AddRelationalTransition(root1, g1nodes[0], "l0");
            gh1.AddRelationalTransition(root1, g1nodes[1], "l0");
            gh2.AddRelationalTransition(root2, g2nodes[1], "l0");
            gh2.AddRelationalTransition(root2, g2nodes[0], "l0");
            RootedLabeledDirectedGraph g1 = gh1.GetGraph();
            RootedLabeledDirectedGraph g2 = gh2.GetGraph();
            Map<Vertex, Vertex> iso1 = GraphIsomorphism.ComputeIsomorphism1(g1, g2);
            Map<Vertex, Vertex> iso2 = GraphIsomorphism.ComputeIsomorphism2(g1, g2);
            Map<Vertex, Vertex> isoU = GraphIsomorphism.ComputeIsomorphismUllmann(g1, g2);
            Assert.AreEqual(iso1.Count, iso2.Count);
            Assert.AreEqual(iso1.Count, isoU.Count);
            Assert.AreEqual(3, iso2.Count);
        }

        [Test]
        public void IsoNonSingletonBucketsNonIso()
        {
            GraphBuilder gh1 = new GraphBuilder();
            GraphBuilder gh2 = new GraphBuilder();
            Vertex root1 = gh1.InitGraph("Root", "label");
            Vertex root2 = gh2.InitGraph("Root", "label");
            Vertex[] g1nodes = new Vertex[2];
            Vertex[] g2nodes = new Vertex[2];
            g1nodes[0] = gh1.AddVertex("NodeType1", "a");
            g1nodes[1] = gh1.AddVertex("NodeType1", "a");
            g2nodes[1] = gh2.AddVertex("NodeType1", "a");
            g2nodes[0] = gh2.AddVertex("NodeType1", "b");
            gh1.AddRelationalTransition(root1, g1nodes[0], "l0");
            gh1.AddRelationalTransition(root1, g1nodes[1], "l0");
            gh2.AddRelationalTransition(root2, g2nodes[1], "l0");
            gh2.AddRelationalTransition(root2, g2nodes[0], "l0");
            RootedLabeledDirectedGraph g1 = gh1.GetGraph();
            RootedLabeledDirectedGraph g2 = gh2.GetGraph();
            Map<Vertex, Vertex> iso1 = GraphIsomorphism.ComputeIsomorphism1(g1, g2);
            Map<Vertex, Vertex> iso2 = GraphIsomorphism.ComputeIsomorphism2(g1, g2);
            Map<Vertex, Vertex> isoU = GraphIsomorphism.ComputeIsomorphismUllmann(g1, g2);
            Assert.IsNull(iso1);
            Assert.IsNull(iso2);
            Assert.IsNull(isoU);
        }


        [Test]
        public void IsoBacktrack1()
        {
            GraphBuilder gh1 = new GraphBuilder();
            GraphBuilder gh2 = new GraphBuilder();
            Vertex root1 = gh1.InitGraph("Root", "label");
            Vertex root2 = gh2.InitGraph("Root", "label");
            Vertex[] g1nodes = new Vertex[5];
            Vertex[] g2nodes = new Vertex[5];
            //G1 nodes
            g1nodes[0] = gh1.AddVertex("Set", "0");
            g1nodes[1] = gh1.AddVertex("Map", "0");
            g1nodes[2] = gh1.AddVertex("NodeType1", "a");
            g1nodes[3] = gh1.AddVertex("NodeType1", "a");
            g1nodes[4] = gh1.AddVertex("NodeType1", "a");
            //G2 nodes
            g2nodes[0] = gh2.AddVertex("Set", "0");
            g2nodes[1] = gh2.AddVertex("Map", "0");
            g2nodes[2] = gh2.AddVertex("NodeType1", "a");
            g2nodes[3] = gh2.AddVertex("NodeType1", "a");
            g2nodes[4] = gh2.AddVertex("NodeType1", "a");
            //G1 transitions
            gh1.AddFunctionalTransition(root1, g1nodes[0], "l0");
            gh1.AddFunctionalTransition(root1, g1nodes[1], "l1");
            gh1.AddFunctionalTransition(g1nodes[1], g1nodes[2], "key");
            gh1.AddFunctionalTransition(g1nodes[1], g1nodes[3], "value");
            gh1.AddRelationalTransition(g1nodes[0], g1nodes[2], "in");
            gh1.AddRelationalTransition(g1nodes[0], g1nodes[3], "in");
            gh1.AddRelationalTransition(g1nodes[0], g1nodes[4], "in");
            //G2 transitions
            gh2.AddFunctionalTransition(root2, g2nodes[0], "l0");
            gh2.AddFunctionalTransition(root2, g2nodes[1], "l1");
            gh2.AddFunctionalTransition(g2nodes[1], g2nodes[4], "key");
            gh2.AddFunctionalTransition(g2nodes[1], g2nodes[2], "value");
            gh2.AddRelationalTransition(g2nodes[0], g2nodes[2], "in");
            gh2.AddRelationalTransition(g2nodes[0], g2nodes[3], "in");
            gh2.AddRelationalTransition(g2nodes[0], g2nodes[4], "in");
            RootedLabeledDirectedGraph g1 = gh1.GetGraph();
            RootedLabeledDirectedGraph g2 = gh2.GetGraph();
            Map<Vertex, Vertex> iso1 = GraphIsomorphism.ComputeIsomorphism1(g1, g2);
            Map<Vertex, Vertex> iso2 = GraphIsomorphism.ComputeIsomorphism2(g1, g2);
            Map<Vertex, Vertex> isoU = GraphIsomorphism.ComputeIsomorphismUllmann(g1, g2);
            Assert.AreEqual(iso1.Count, iso2.Count);
            Assert.AreEqual(iso1.Count, isoU.Count);
            Assert.AreEqual(6, iso2.Count);
        }

        [Test]
        public void IsoBacktrack2()
        {
            GraphBuilder gh1 = new GraphBuilder();
            GraphBuilder gh2 = new GraphBuilder();
            Vertex root1 = gh1.InitGraph("Root", "label");
            Vertex root2 = gh2.InitGraph("Root", "label");
            Vertex[] g1nodes = new Vertex[7];
            Vertex[] g2nodes = new Vertex[7];
            //G1 nodes
            g1nodes[0] = gh1.AddVertex("Set", "0");
            g1nodes[1] = gh1.AddVertex("Map", "0");
            g1nodes[2] = gh1.AddVertex("NodeType1", "a");
            g1nodes[3] = gh1.AddVertex("NodeType1", "a");
            g1nodes[4] = gh1.AddVertex("NodeType1", "a");
            g1nodes[5] = gh1.AddVertex("NodeType1", "a");
            g1nodes[6] = gh1.AddVertex("Map", "0");
            //G2 nodes
            g2nodes[0] = gh2.AddVertex("Set", "0");
            g2nodes[1] = gh2.AddVertex("Map", "0");
            g2nodes[2] = gh2.AddVertex("NodeType1", "a");
            g2nodes[3] = gh2.AddVertex("NodeType1", "a");
            g2nodes[4] = gh2.AddVertex("NodeType1", "a");
            g2nodes[5] = gh2.AddVertex("NodeType1", "a");
            g2nodes[6] = gh2.AddVertex("Map", "0");
            //G1 transitions
            gh1.AddFunctionalTransition(root1, g1nodes[0], "l0");
            gh1.AddRelationalTransition(root1, g1nodes[1], "l1");
            gh1.AddRelationalTransition(root1, g1nodes[6], "l1");
            gh1.AddFunctionalTransition(g1nodes[1], g1nodes[2], "key");
            gh1.AddFunctionalTransition(g1nodes[1], g1nodes[3], "value");
            gh1.AddFunctionalTransition(g1nodes[6], g1nodes[2], "key");
            gh1.AddFunctionalTransition(g1nodes[6], g1nodes[4], "value");
            gh1.AddRelationalTransition(g1nodes[0], g1nodes[2], "in");
            gh1.AddRelationalTransition(g1nodes[0], g1nodes[3], "in");
            gh1.AddRelationalTransition(g1nodes[0], g1nodes[4], "in");
            gh1.AddRelationalTransition(g1nodes[0], g1nodes[5], "in");
            //G2 transitions
            gh2.AddFunctionalTransition(root2, g2nodes[0], "l0");
            gh2.AddRelationalTransition(root2, g2nodes[1], "l1");
            gh2.AddRelationalTransition(root2, g2nodes[6], "l1");
            gh2.AddFunctionalTransition(g2nodes[1], g2nodes[4], "key");
            gh2.AddFunctionalTransition(g2nodes[1], g2nodes[2], "value");
            gh2.AddFunctionalTransition(g2nodes[6], g2nodes[4], "key");
            gh2.AddFunctionalTransition(g2nodes[6], g2nodes[5], "value");
            gh2.AddRelationalTransition(g2nodes[0], g2nodes[2], "in");
            gh2.AddRelationalTransition(g2nodes[0], g2nodes[3], "in");
            gh2.AddRelationalTransition(g2nodes[0], g2nodes[4], "in");
            gh2.AddRelationalTransition(g2nodes[0], g2nodes[5], "in");
            RootedLabeledDirectedGraph g1 = gh1.GetGraph();
            RootedLabeledDirectedGraph g2 = gh2.GetGraph();
            Map<Vertex, Vertex> iso1 = GraphIsomorphism.ComputeIsomorphism1(g1, g2);
            Map<Vertex, Vertex> iso2 = GraphIsomorphism.ComputeIsomorphism2(g1, g2);
            Map<Vertex, Vertex> isoU = GraphIsomorphism.ComputeIsomorphismUllmann(g1, g2);
            Assert.AreEqual(iso1.Count, iso2.Count);
            Assert.AreEqual(iso1.Count, isoU.Count);
            Assert.AreEqual(8, iso2.Count);
        }

        [Test]
        public void IsoBacktrack3()
        {
            GraphBuilder gh1 = new GraphBuilder();
            GraphBuilder gh2 = new GraphBuilder();
            Vertex root1 = gh1.InitGraph("Root", "label");
            Vertex root2 = gh2.InitGraph("Root", "label");
            Vertex[] g1nodes = new Vertex[9];
            Vertex[] g2nodes = new Vertex[9];
            //G1 nodes
            g1nodes[0] = gh1.AddVertex("Set", "0");
            g1nodes[1] = gh1.AddVertex("Map", "0");
            g1nodes[2] = gh1.AddVertex("NodeType1", "a");
            g1nodes[3] = gh1.AddVertex("NodeType1", "a");
            g1nodes[4] = gh1.AddVertex("NodeType1", "a");
            g1nodes[5] = gh1.AddVertex("NodeType1", "a");
            g1nodes[6] = gh1.AddVertex("Map", "0");
            g1nodes[7] = gh1.AddVertex("Set", "1");
            g1nodes[8] = gh1.AddVertex("Set", "2");
            //G2 nodes
            g2nodes[0] = gh2.AddVertex("Set", "0");
            g2nodes[1] = gh2.AddVertex("Map", "0");
            g2nodes[2] = gh2.AddVertex("NodeType1", "a");
            g2nodes[3] = gh2.AddVertex("NodeType1", "a");
            g2nodes[4] = gh2.AddVertex("NodeType1", "a");
            g2nodes[5] = gh2.AddVertex("NodeType1", "a");
            g2nodes[6] = gh2.AddVertex("Map", "0");
            g2nodes[7] = gh2.AddVertex("Set", "1");
            g2nodes[8] = gh2.AddVertex("Set", "2");
            //G1 transitions
            gh1.AddFunctionalTransition(root1, g1nodes[0], "l0");
            gh1.AddRelationalTransition(root1, g1nodes[1], "l1");
            gh1.AddRelationalTransition(root1, g1nodes[6], "l1");
            gh1.AddFunctionalTransition(g1nodes[1], g1nodes[2], "key");
            gh1.AddFunctionalTransition(g1nodes[1], g1nodes[3], "value");
            gh1.AddFunctionalTransition(g1nodes[6], g1nodes[2], "key");
            gh1.AddFunctionalTransition(g1nodes[6], g1nodes[3], "value");
            gh1.AddRelationalTransition(g1nodes[0], g1nodes[7], "in");
            gh1.AddRelationalTransition(g1nodes[0], g1nodes[8], "in");
            gh1.AddRelationalTransition(g1nodes[7], g1nodes[4], "in");
            gh1.AddRelationalTransition(g1nodes[7], g1nodes[5], "in");
            gh1.AddRelationalTransition(g1nodes[8], g1nodes[2], "in");
            gh1.AddRelationalTransition(g1nodes[8], g1nodes[3], "in");
            //G2 transitions
            gh2.AddFunctionalTransition(root2, g2nodes[0], "l0");
            gh2.AddRelationalTransition(root2, g2nodes[1], "l1");
            gh2.AddRelationalTransition(root2, g2nodes[6], "l1");
            gh2.AddFunctionalTransition(g2nodes[1], g2nodes[4], "key");
            gh2.AddFunctionalTransition(g2nodes[1], g2nodes[2], "value");
            gh2.AddFunctionalTransition(g2nodes[6], g2nodes[4], "key");
            gh2.AddFunctionalTransition(g2nodes[6], g2nodes[2], "value");
            gh2.AddRelationalTransition(g2nodes[0], g2nodes[8], "in");
            gh2.AddRelationalTransition(g2nodes[0], g2nodes[7], "in");
            gh2.AddRelationalTransition(g2nodes[8], g2nodes[4], "in");
            gh2.AddRelationalTransition(g2nodes[8], g2nodes[2], "in");
            gh2.AddRelationalTransition(g2nodes[7], g2nodes[5], "in");
            gh2.AddRelationalTransition(g2nodes[7], g2nodes[3], "in");
            RootedLabeledDirectedGraph g1 = gh1.GetGraph();
            RootedLabeledDirectedGraph g2 = gh2.GetGraph();
            Map<Vertex, Vertex> iso1 = GraphIsomorphism.ComputeIsomorphism1(g1, g2);
            Map<Vertex, Vertex> iso2 = GraphIsomorphism.ComputeIsomorphism2(g1, g2);
            Map<Vertex, Vertex> isoU = GraphIsomorphism.ComputeIsomorphismUllmann(g1, g2);
            Assert.AreEqual(iso1.Count, iso2.Count);
            Assert.AreEqual(iso1.Count, isoU.Count);
            Assert.AreEqual(10, iso2.Count);
        }

        [Test]
        public void IsoBacktrack4()
        {
            GraphBuilder gh1 = new GraphBuilder();
            GraphBuilder gh2 = new GraphBuilder();
            Vertex root1 = gh1.InitGraph("Root", "label");
            Vertex root2 = gh2.InitGraph("Root", "label");
            Vertex[] g1nodes = new Vertex[12];
            Vertex[] g2nodes = new Vertex[12];
            //G1 nodes
            g1nodes[0] = gh1.AddVertex("Side", "a");
            g1nodes[1] = gh1.AddVertex("Side", "a");
            g1nodes[2] = gh1.AddVertex("Side", "a");
            g1nodes[3] = gh1.AddVertex("Side", "a");
            g1nodes[4] = gh1.AddVertex("Color", "b");
            g1nodes[5] = gh1.AddVertex("Map", "map");
            g1nodes[6] = gh1.AddVertex("Set", "set");
            g1nodes[7] = gh1.AddVertex("Set", "set");
            g1nodes[8] = gh1.AddVertex("Set", "set");
            g1nodes[9] = gh1.AddVertex("Set", "set");
            g1nodes[10] = gh1.AddVertex("Set", "setofsets");
            //G2 nodes
            g2nodes[0] = gh2.AddVertex("Side", "a");
            g2nodes[1] = gh2.AddVertex("Side", "a");
            g2nodes[2] = gh2.AddVertex("Side", "a");
            g2nodes[3] = gh2.AddVertex("Side", "a");
            g2nodes[4] = gh2.AddVertex("Color", "b");
            g2nodes[5] = gh2.AddVertex("Map", "map");
            g2nodes[6] = gh2.AddVertex("Set", "set");
            g2nodes[7] = gh2.AddVertex("Set", "set");
            g2nodes[8] = gh2.AddVertex("Set", "set");
            g2nodes[9] = gh2.AddVertex("Set", "set");
            g2nodes[10] = gh2.AddVertex("Set", "setofsets");            //G1 transitions

            gh1.AddFunctionalTransition(root1, g1nodes[5], "l0");
            gh1.AddFunctionalTransition(root1, g1nodes[10], "l1");
            gh1.AddFunctionalTransition(g1nodes[5], g1nodes[0], "key");
            gh1.AddFunctionalTransition(g1nodes[5], g1nodes[4], "value");
            gh1.AddRelationalTransition(g1nodes[6], g1nodes[0], "in");
            gh1.AddRelationalTransition(g1nodes[6], g1nodes[1], "in");
            gh1.AddRelationalTransition(g1nodes[7], g1nodes[3], "in");
            gh1.AddRelationalTransition(g1nodes[7], g1nodes[2], "in");
            gh1.AddRelationalTransition(g1nodes[8], g1nodes[2], "in");
            gh1.AddRelationalTransition(g1nodes[8], g1nodes[1], "in");
            gh1.AddRelationalTransition(g1nodes[9], g1nodes[3], "in");
            gh1.AddRelationalTransition(g1nodes[9], g1nodes[0], "in");
            gh1.AddRelationalTransition(g1nodes[10], g1nodes[6], "in");
            gh1.AddRelationalTransition(g1nodes[10], g1nodes[7], "in");
            gh1.AddRelationalTransition(g1nodes[10], g1nodes[8], "in");
            gh1.AddRelationalTransition(g1nodes[10], g1nodes[9], "in");
            //G2 transitions
            gh2.AddFunctionalTransition(root1, g2nodes[5], "l0");
            gh2.AddFunctionalTransition(root1, g2nodes[10], "l1");
            gh2.AddFunctionalTransition(g2nodes[5], g2nodes[0], "key");
            gh2.AddFunctionalTransition(g2nodes[5], g2nodes[4], "value");
            gh2.AddRelationalTransition(g2nodes[9], g2nodes[0], "in");
            gh2.AddRelationalTransition(g2nodes[9], g2nodes[1], "in");
            gh2.AddRelationalTransition(g2nodes[7], g2nodes[3], "in");
            gh2.AddRelationalTransition(g2nodes[7], g2nodes[2], "in");
            gh2.AddRelationalTransition(g2nodes[8], g2nodes[2], "in");
            gh2.AddRelationalTransition(g2nodes[8], g2nodes[1], "in");
            gh2.AddRelationalTransition(g2nodes[6], g2nodes[3], "in");
            gh2.AddRelationalTransition(g2nodes[6], g2nodes[0], "in");
            gh2.AddRelationalTransition(g2nodes[10], g2nodes[9], "in");
            gh2.AddRelationalTransition(g2nodes[10], g2nodes[7], "in");
            gh2.AddRelationalTransition(g2nodes[10], g2nodes[8], "in");
            gh2.AddRelationalTransition(g2nodes[10], g2nodes[6], "in");
            RootedLabeledDirectedGraph g1 = gh1.GetGraph();
            RootedLabeledDirectedGraph g2 = gh2.GetGraph();
            Map<Vertex, Vertex> iso1 = GraphIsomorphism.ComputeIsomorphism1(g1, g2);
            Map<Vertex, Vertex> iso2 = GraphIsomorphism.ComputeIsomorphism2(g1, g2);
            Map<Vertex, Vertex> isoU = GraphIsomorphism.ComputeIsomorphismUllmann(g1, g2);
            Assert.AreEqual(iso1.Count, iso2.Count);
            Assert.AreEqual(iso1.Count, isoU.Count);
            Assert.AreEqual(12, iso2.Count);
        }

        [Test]
        public void IsoBacktrack5()
        {
            GraphBuilder gh1 = new GraphBuilder();
            GraphBuilder gh2 = new GraphBuilder();
            Vertex root1 = gh1.InitGraph("Root", "label");
            Vertex root2 = gh2.InitGraph("Root", "label");
            Vertex[] g1nodes = new Vertex[12];
            Vertex[] g2nodes = new Vertex[12];
            //G1 nodes
            g1nodes[0] = gh1.AddVertex("Side", "a");
            g1nodes[1] = gh1.AddVertex("Side", "a");
            g1nodes[2] = gh1.AddVertex("Side", "a");
            g1nodes[3] = gh1.AddVertex("Side", "a");
            g1nodes[4] = gh1.AddVertex("Color", "b");
            g1nodes[5] = gh1.AddVertex("Map", "map");
            g1nodes[6] = gh1.AddVertex("Set", "set");
            g1nodes[7] = gh1.AddVertex("Set", "set");
            g1nodes[8] = gh1.AddVertex("Set", "set");
            g1nodes[9] = gh1.AddVertex("Set", "set");
            g1nodes[10] = gh1.AddVertex("Set", "setofsets");

            //G1 transitions
            gh1.AddRelationalTransition(root1, g1nodes[5], "l0");
            gh1.AddFunctionalTransition(root1, g1nodes[10], "l1");
            gh1.AddFunctionalTransition(g1nodes[5], g1nodes[3], "key");
            gh1.AddFunctionalTransition(g1nodes[5], g1nodes[4], "value");
            gh1.AddRelationalTransition(g1nodes[6], g1nodes[0], "in");
            gh1.AddRelationalTransition(g1nodes[6], g1nodes[1], "in");
            gh1.AddRelationalTransition(g1nodes[7], g1nodes[3], "in");
            gh1.AddRelationalTransition(g1nodes[7], g1nodes[2], "in");
            gh1.AddRelationalTransition(g1nodes[8], g1nodes[2], "in");
            gh1.AddRelationalTransition(g1nodes[8], g1nodes[1], "in");
            gh1.AddRelationalTransition(g1nodes[9], g1nodes[3], "in");
            gh1.AddRelationalTransition(g1nodes[9], g1nodes[0], "in");
            gh1.AddRelationalTransition(g1nodes[10], g1nodes[6], "in");
            gh1.AddRelationalTransition(g1nodes[10], g1nodes[7], "in");
            gh1.AddRelationalTransition(g1nodes[10], g1nodes[8], "in");
            gh1.AddRelationalTransition(g1nodes[10], g1nodes[9], "in");

            //G2 nodes
            g2nodes[0] = gh2.AddVertex("Side", "a");
            g2nodes[1] = gh2.AddVertex("Side", "a");
            g2nodes[2] = gh2.AddVertex("Side", "a");
            g2nodes[3] = gh2.AddVertex("Side", "a");
            g2nodes[4] = gh2.AddVertex("Color", "b");
            g2nodes[5] = gh2.AddVertex("Map", "map");
            g2nodes[6] = gh2.AddVertex("Set", "set");
            g2nodes[7] = gh2.AddVertex("Set", "set");
            g2nodes[8] = gh2.AddVertex("Set", "set");
            g2nodes[9] = gh2.AddVertex("Set", "set");
            g2nodes[10] = gh2.AddVertex("Set", "setofsets");

            //G2 transitions
            gh2.AddRelationalTransition(root1, g2nodes[5], "l0");
            gh2.AddFunctionalTransition(root1, g2nodes[10], "l1");
            gh2.AddFunctionalTransition(g2nodes[5], g2nodes[0], "key");
            gh2.AddFunctionalTransition(g2nodes[5], g2nodes[4], "value");
            gh2.AddRelationalTransition(g2nodes[9], g2nodes[0], "in");
            gh2.AddRelationalTransition(g2nodes[9], g2nodes[1], "in");
            gh2.AddRelationalTransition(g2nodes[8], g2nodes[3], "in");
            gh2.AddRelationalTransition(g2nodes[8], g2nodes[2], "in");
            gh2.AddRelationalTransition(g2nodes[7], g2nodes[2], "in");
            gh2.AddRelationalTransition(g2nodes[7], g2nodes[1], "in");
            gh2.AddRelationalTransition(g2nodes[6], g2nodes[3], "in");
            gh2.AddRelationalTransition(g2nodes[6], g2nodes[0], "in");
            gh2.AddRelationalTransition(g2nodes[10], g2nodes[6], "in");
            gh2.AddRelationalTransition(g2nodes[10], g2nodes[7], "in");
            gh2.AddRelationalTransition(g2nodes[10], g2nodes[8], "in");
            gh2.AddRelationalTransition(g2nodes[10], g2nodes[9], "in");
            RootedLabeledDirectedGraph g1 = gh1.GetGraph();
            RootedLabeledDirectedGraph g2 = gh2.GetGraph();
            Map<Vertex, Vertex> iso1 = GraphIsomorphism.ComputeIsomorphism1(g1, g2);
            Map<Vertex, Vertex> iso2 = GraphIsomorphism.ComputeIsomorphism2(g1, g2);
            Map<Vertex, Vertex> isoU = GraphIsomorphism.ComputeIsomorphismUllmann(g1, g2);
            Assert.AreEqual(iso1.Count, iso2.Count);
            Assert.AreEqual(iso1.Count, isoU.Count);
            Assert.AreEqual(12, iso2.Count);
        }

        [Test]
        public void IsoBacktrack6()
        {
            GraphBuilder gh1 = new GraphBuilder();
            GraphBuilder gh2 = new GraphBuilder();
            Vertex root1 = gh1.InitGraph("Root", "label");
            Vertex root2 = gh2.InitGraph("Root", "label");
            Vertex[] g1nodes = new Vertex[12];
            Vertex[] g2nodes = new Vertex[12];
            //G1 nodes
            g1nodes[0] = gh1.AddVertex("Side", "a");
            g1nodes[1] = gh1.AddVertex("Side", "a");
            g1nodes[2] = gh1.AddVertex("Side", "a");
            g1nodes[3] = gh1.AddVertex("Side", "a");
            g1nodes[4] = gh1.AddVertex("Color", "b");
            g1nodes[5] = gh1.AddVertex("Map", "map");
            g1nodes[6] = gh1.AddVertex("Map", "map");
            g1nodes[7] = gh1.AddVertex("Map", "map");
            g1nodes[8] = gh1.AddVertex("Map", "map");
            g1nodes[9] = gh1.AddVertex("Map", "map");
            g1nodes[10] = gh1.AddVertex("Set", "set");

            //G1 transitions
            gh1.AddRelationalTransition(root1, g1nodes[5], "r0");
            gh1.AddRelationalTransition(root1, g1nodes[6], "r0");
            gh1.AddRelationalTransition(root1, g1nodes[7], "r0");
            gh1.AddRelationalTransition(root1, g1nodes[8], "r1");
            gh1.AddRelationalTransition(root1, g1nodes[9], "r1");
            gh1.AddFunctionalTransition(root1, g1nodes[10], "f1");
            gh1.AddFunctionalTransition(g1nodes[5], g1nodes[3], "key");
            gh1.AddFunctionalTransition(g1nodes[5], g1nodes[4], "value");
            gh1.AddFunctionalTransition(g1nodes[6], g1nodes[0], "key");
            gh1.AddFunctionalTransition(g1nodes[6], g1nodes[1], "value");
            gh1.AddFunctionalTransition(g1nodes[7], g1nodes[3], "key");
            gh1.AddFunctionalTransition(g1nodes[7], g1nodes[2], "value");
            gh1.AddFunctionalTransition(g1nodes[8], g1nodes[2], "key");
            gh1.AddFunctionalTransition(g1nodes[8], g1nodes[1], "value");
            gh1.AddFunctionalTransition(g1nodes[9], g1nodes[3], "key");
            gh1.AddFunctionalTransition(g1nodes[9], g1nodes[0], "value");
            gh1.AddRelationalTransition(g1nodes[10], g1nodes[6], "in");
            gh1.AddRelationalTransition(g1nodes[10], g1nodes[7], "in");
            gh1.AddRelationalTransition(g1nodes[10], g1nodes[8], "in");
            gh1.AddRelationalTransition(g1nodes[10], g1nodes[9], "in");

            //G2 nodes
            g2nodes[0] = gh2.AddVertex("Side", "a");
            g2nodes[1] = gh2.AddVertex("Side", "a");
            g2nodes[2] = gh2.AddVertex("Side", "a");
            g2nodes[3] = gh2.AddVertex("Side", "a");
            g2nodes[4] = gh2.AddVertex("Color", "b");
            g2nodes[5] = gh2.AddVertex("Map", "map");
            g2nodes[6] = gh2.AddVertex("Map", "map");
            g2nodes[7] = gh2.AddVertex("Map", "map");
            g2nodes[8] = gh2.AddVertex("Map", "map");
            g2nodes[9] = gh2.AddVertex("Map", "map");
            g2nodes[10] = gh2.AddVertex("Set", "set");

            //G2 transitions
            gh2.AddRelationalTransition(root1, g2nodes[8], "r0");
            gh2.AddRelationalTransition(root1, g2nodes[6], "r0");
            gh2.AddRelationalTransition(root1, g2nodes[7], "r0");
            gh2.AddRelationalTransition(root1, g2nodes[5], "r1");
            gh2.AddRelationalTransition(root1, g2nodes[9], "r1");
            gh2.AddFunctionalTransition(root1, g2nodes[10], "f1");
            gh2.AddFunctionalTransition(g2nodes[5], g2nodes[2], "key");
            gh2.AddFunctionalTransition(g2nodes[5], g2nodes[0], "value");
            gh2.AddFunctionalTransition(g2nodes[6], g2nodes[1], "key");
            gh2.AddFunctionalTransition(g2nodes[6], g2nodes[0], "value");
            gh2.AddFunctionalTransition(g2nodes[7], g2nodes[3], "key");
            gh2.AddFunctionalTransition(g2nodes[7], g2nodes[2], "value");
            gh2.AddFunctionalTransition(g2nodes[8], g2nodes[3], "key");
            gh2.AddFunctionalTransition(g2nodes[8], g2nodes[4], "value");
            gh2.AddFunctionalTransition(g2nodes[9], g2nodes[3], "key");
            gh2.AddFunctionalTransition(g2nodes[9], g2nodes[1], "value");
            gh2.AddRelationalTransition(g2nodes[10], g2nodes[6], "in");
            gh2.AddRelationalTransition(g2nodes[10], g2nodes[7], "in");
            gh2.AddRelationalTransition(g2nodes[10], g2nodes[5], "in");
            gh2.AddRelationalTransition(g2nodes[10], g2nodes[9], "in");

            RootedLabeledDirectedGraph g1 = gh1.GetGraph();
            RootedLabeledDirectedGraph g2 = gh2.GetGraph();
            Map<Vertex, Vertex> iso1 = GraphIsomorphism.ComputeIsomorphism1(g1, g2);
            Map<Vertex, Vertex> iso2 = GraphIsomorphism.ComputeIsomorphism2(g1, g2);
            Map<Vertex, Vertex> isoU = GraphIsomorphism.ComputeIsomorphismUllmann(g1, g2);
            if (iso1 == null && iso2 == null && isoU == null) throw new Exception("None of the algorithms found the graphs isomorphic");
            Assert.AreEqual(iso1.Count, iso2.Count);
            Assert.AreEqual(iso1.Count, isoU.Count);
            Assert.AreEqual(12, iso2.Count);
        }
    }

    /// <summary>
    /// A convenience class for constructing graphs for testing purposes.
    /// </summary>
    public class GraphBuilder
    {
        Dictionary<string, int> idCounters = new Dictionary<string, int>();
        Vertex root;
        Dictionary<Vertex, VertexData> vertexRecords;
        int idCounter = 0;

        /// <summary>
        /// Initialise the graph builder.
        /// </summary>
        /// <param name="rootName"></param>
        /// <param name="rootLabel"></param>
        /// <returns></returns>
        public Vertex InitGraph(string rootName, string rootLabel)
        {
            vertexRecords = new Dictionary<Vertex, VertexData>();
            root = AddVertex(rootName, rootLabel);
            return root;
        }

        int getNextId(String name)
        {
            int id = 0;
            if (idCounters.ContainsKey(name))
                id = idCounters[name] + 1;
            idCounters.Remove(name);
            idCounters.Add(name, id);
            return id;
        }

        /// <summary>
        /// Add a vertex into the graph being built.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public Vertex AddVertex(string name, string label)
        {
            VertexData vData = new VertexData();
            int vId = getNextId(name);
            Vertex v = idCounter++;//new ObjectId(new Symbol(name), vId);
            vData.vertex = v;
            vData.label = vData.label.Add(new Pair<CompoundTerm, IComparable>(new CompoundTerm(new Symbol(name), new Sequence<Term>()), label));
            vertexRecords.Add(v, vData);
            return v;
        }

        /// <summary>
        /// Add a functional (i.e. ordered) transition into the graph.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="label"></param>
        public void AddFunctionalTransition(Vertex from, Vertex to, String label)
        {
            VertexData vd = vertexRecords[from];
            vd.orderedOutgoingEdges = vd.orderedOutgoingEdges.Add(new Pair<CompoundTerm, Vertex>(new CompoundTerm(new Symbol(label), new Sequence<Term>()), to));
        }

        /// <summary>
        /// Add a relational (i.e. unordered) transition into the graph.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="label"></param>
        public void AddRelationalTransition(Vertex from, Vertex to, String label)
        {
            VertexData vd = vertexRecords[from];
            CompoundTerm edgeLabel = new CompoundTerm(new Symbol(label), new Sequence<Term>());
            if (vd.unorderedOutgoingEdges.ContainsKey(edgeLabel))
            {
                vd.unorderedOutgoingEdges = vd.unorderedOutgoingEdges.Override(edgeLabel, vd.unorderedOutgoingEdges[edgeLabel].Add(to));
            }
            else
            {
                vd.unorderedOutgoingEdges = vd.unorderedOutgoingEdges.Add(edgeLabel, new Set<Vertex>(to));
            }
        }

        /// <summary>
        /// When all vertices and edges have been added, get the actual graph.
        /// </summary>
        /// <returns></returns>
        public RootedLabeledDirectedGraph GetGraph()
        {
            return new RootedLabeledDirectedGraph(root, vertexRecords);
        }
    }
}
