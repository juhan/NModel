//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;

using System.Collections;


namespace NModel.Algorithms.GraphTraversals 
{
	
	internal class GraphWithMatching:IGraph 
	{         	
		internal int[] m; //if m[j]>-1 then the edge (m[j],n+j) is in the matching
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        internal int[,] len; //lengths
		internal int[] p;//potentials
		internal int n; //number of vertices on one side
        int [] flatA;
		int [] flatB;

		public double EdgeProbability(Edge l){return 1;}
		internal class Edges: IEnumerable
		{            
			internal GraphWithMatching graph;
			internal int node;
			internal Edges(GraphWithMatching graph,int node)
			{this.node=node;this.graph=graph;}
			public IEnumerator GetEnumerator()
			{
				if(node<graph.n)
					return new EdgeEnumerator0(this);
				else
					return new EdgeEnumerator1(this);
			}
		}
		/* enumerates the links going from A to B - not in matching */
		internal class EdgeEnumerator0:IEnumerator  
		{
			int cur;
			Edges links;
			int node {get {return links.node;}}
			internal EdgeEnumerator0(Edges linksAtVertex)
			{
				links=linksAtVertex;
				cur=-1;
			}

			public bool MoveNext()
			{
				cur++;
				while(cur<links.graph.n)
				{					
					if(links.graph.m[cur]!=node)
						break;
					
					cur++;
				}
					
				return cur<links.graph.n;
			}


			public void Reset() 
			{
				cur=-1;              				
			}
			public object Current 
			{
				get { return new Edge(node,cur+links.graph.n,0,links.graph.ReducedCost(node,cur));}
			}
		}

		/* enumerates the links going from B to A - in matching */
		internal class EdgeEnumerator1:IEnumerator  
		{   int cur;
			Edges links;
			int ntag;
			int node {get {return links.node;}}
			internal EdgeEnumerator1(Edges linksAtVertex)
			{
				links=linksAtVertex;
				ntag=node-links.graph.n;
				cur=-1;				
			}

			public bool MoveNext()
			{
				cur++;				
				return cur==0 && links.graph.m[ntag]!=Graph.NONE;  //edge belongs to the matching				
			}

			public void Reset() 
			{
				cur=-1;
              				
			}
			public object Current 
			{
				get { 
					int a=links.graph.m[ntag];
					return new Edge(node,a,0,links.graph.ReducedCost(a,ntag));
				}

			}
		}

		//2n is the total number of bipartite graph

		internal int ReducedCost(int a,int b)
		{return p[a]+p[b+n]-len[flatA[a],flatB[b]];}

		public IEnumerable InitialVertices()
		{
			throw new MethodAccessException();
		}

		public IEnumerable EdgesAtVertex(int node)
		{
           return new Edges(this, node); 
		}

		internal GraphWithMatching(int [] m,int [,] len,int[]p,int[] flatA,int[]flatB)
		{
			this.m=m;
			this.len=len;
			this.n=m.Length;
			this.p=p;
			this.flatA=flatA;
			this.flatB=flatB;
		}
	
		// Returns the number of nodes.
		// Vertices should be enumerated from 0 to NumberOfVertices()-1 without

		public int NumberOfVertices{get{return 2*n;}}

		// Vertices should be enumerated from 0 to NumberOfVertices()-1 without holes
	/*	override internal int NumberOfEdgesFromVertex(int node)
		{
			if (node<n)
				return n; //don't show links going from u to v
			
			if(m[node-n]> -1)
				return 1;
			
			return 0;

		}*/

		
		public int GetVertexWeight(int node)
		{
			return 1;
		}

		/*
		//return true if the edge exists and false in the case that edge doesn't exist.
		override internal bool GetEdgeInfo(
			int node,
			int linkNumber, 
			out int linkEnd, 
			out int linkLabel,
			out int linkWeight)
		{
			if(node<n)
			{
				if(linkNumber<n)
				{
					linkEnd=linkNumber+n;
					linkLabel=0;
					linkWeight=p[node]+p[linkEnd]-len[node,linkNumber];					
				}
                else 
					throw new ArgumentOutOfRangeException("linkNumber",linkNumber,"in GetEdgeInfo");
				
			}
			else 
			{  //node>=n
				node-=n;
				if(linkNumber==0 && m[node]>=0)
				{
					linkEnd=m[node];
					linkLabel=0;
					linkWeight= p[linkEnd]+p[node+n]- len[linkEnd,node];
				}
				else			
					throw new ArgumentOutOfRangeException("linkNumber==0 && m[node]>=0",false,"in GetEdgeInfo");			
			}
			return true;
		}
*/
		/*
		//entry points in ITE terminology
		override internal int NumberOfInitialVertices(){
			throw new MissingMethodException("not implemented");
		}
*/
		/*
		// returns the index of the i-th initial node
		override internal int GetInitialVertex(int i)
		{
			throw new MissingMethodException("not implemented");	
		}
		*/
		//consider later to add methods about the links going from the node
	}
}
