//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;

using System.Collections;

namespace NModel.Algorithms.GraphTraversals
{
	/// <summary>
	/// Eulerian Tour - A walk on a graph edges  
	/// which uses each graph edge exactly once. 
	/// A connected graph has an Eulerian tour iff it has at most two graph vertices of odd degree. 
	/// </summary>

	internal class EulerianTour
	{
		ArrayList[] backwardEdges;
		int u; //the initial node
		int n; //number of nodes
		Stack[] forwardEdges;
		Edge[] spanningTree;
        bool [] covered;
		Edge[] edges;

		Edge[] theTour;
        int m; //number of edges

		ArrayList[] BuildBackwordEdges(Edge[] es)
		{
			this.edges=es;
			ArrayList[] ret=new ArrayList[n];
			int c=n;
			while(c-- >0)
				ret[c]=new ArrayList();  //capacity?

			foreach(Edge l in es)
			   ret[l.target].Add(l);

			return ret;
			
		}

		/// <summary>
		/// The walk starts at u
		/// </summary>
		/// <param name="u">start of the walk</param>
		/// <param name="edges">edges of the graph</param>
		public EulerianTour(int u,Edge[] edges):this(u,edges,VertBound(edges))
		{
			int nOfVert=0;
			foreach(Edge l in edges)
			{
				if(l.source>=nOfVert)
					nOfVert++;

				if(l.target>=nOfVert)
					nOfVert++;

			}

		}
		
		static int VertBound(Edge[] edges)
		{
			int bound=0;
			foreach(Edge l in edges)
			{
				if(l.source>=bound)
					bound++;

				if(l.target>=bound)
					bound++;
			}
			return bound;
		}

		internal EulerianTour(int u/*start node*/,Edge[] edges,int n)
		{
			this.u=u;
			this.n=n;
			this.m=edges.Length;
			backwardEdges=BuildBackwordEdges(edges);

			spanningTree=new Edge[n];
			forwardEdges=new Stack[n];
			//building IN-spanning-tree

			covered=new bool[n];			
		}

		/// <summary>
		/// Returns an Euler tour over the graph.
		/// </summary>
		/// <returns>Returns an empty array if there is no tour.</returns>
		internal Edge[] GetTour()
		{

			if(theTour!=null)
				return theTour;

            if(this.n==0)
                return new Edge[0];

			int nn=n;
			int v=u;
			while(nn-- > 0)
			{
				if(!covered[v])
					DFSR(v);
				
				if(v==n-1)
					v=0;
				else
					v++;
			}
		
			forwardEdges[u]=new Stack();

			foreach(Edge l in edges){
				v=l.source;
				if(l!=spanningTree[v])
					forwardEdges[v].Push(l);
				
			}			

			theTour=new Edge[m];
			v=u;
			int i=0;
			Stack lt;
			while((lt=forwardEdges[v]).Count>0)
			{				
				theTour[i]=lt.Pop() as Edge;
						
				v=theTour[i].target;	                
				i++;
			}


			if(i!=m)
			{
        throw new InvalidOperationException("the graph does not have an Euler tour");
			}
			
			return theTour;

		}
		
		//dfs backword
		void DFSR(int v)
		{
			covered[v]=true;
		
			foreach(Edge l in backwardEdges[v])
			{
				if(covered[l.source])
					continue;

				//this edge will be in the spanning tree and last in the list of forward edges
				
				v=l.source;              

				forwardEdges[v]=new Stack();
				forwardEdges[v].Push(l);
				spanningTree[v]=l;
				
				DFSR(v);
			}
		}

	}
}

