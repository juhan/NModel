//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;




namespace NModel.Algorithms.GraphTraversals
{
	/// <summary>
	/// In graph theory, the single-source shortest path problem is 
	/// the problem of finding a path between two vertices such that the sum of the
	///  weights of its constituent edges is minimized. More formally, given a 
	///  weighted graph (that is, a set V of vertices, a set E of edges, 
	///  and a real-valued weight function f : E -> R), and given 
	///  further two elements n, n' of N, find a path P from n to n' so that 
	///  the sum of {f(p):p in P}	is minimal among all paths connecting n to n'.
	///  Here we use Dijkstra algorithm to find a path.
	/// </summary>
	internal class SingleSourceShortestPaths 
	{


		IGraph graph;
		int source;
		Edge[]pred;

		int []dist;

		
		/// <summary>
		/// The array of predecessors.
		/// </summary>
		internal Edge[] Pred {
			get{
				return pred;
			}
	
		}

		/// <summary>
		/// Performs the main calculation.
		/// </summary>
		internal void Calculate()
		{
			IntBinaryHeapPriorityQueue pq=new IntBinaryHeapPriorityQueue(graph.NumberOfVertices);
			pq.insert(source,0);
			while(!pq.isEmpty())
			{
				int u=pq.del_min();
			
	
				foreach(Edge l in graph.EdgesAtVertex(u))		
				{
					int v=l.target;
					int len=l.weight;
					int c=dist[u]+len;
			
					if(pred[v]==null && v !=source)
						pq.insert(v,c);
					else if (c<dist[v])
						pq.decrease_priority(v,c);
					else 
						continue;

					dist[v]=c; pred[v]=l;                    
				}
			}
			
		}


/// <summary>
/// Gets a path to the target.
/// </summary>
/// <param name="target"></param>
/// <returns></returns>
		internal Edge[] GetPathTo(int target)
		{
			

			int count=0;
			int v=target;
			while(v!=source)
			{
				v=pred[v].source;
				count++;
			}
		
			Edge[] thePath=new Edge[count];
			
			v=target;
			while(v!=source)
			{
				thePath[--count]=pred[v];
				v=pred[v].source;				
			}
			
			
			return thePath;

		}
/// <summary>
/// Returns the distance from the source to the target.
/// </summary>
/// <param name="target"></param>
/// <returns></returns>
		internal int GetDistTo(int target)
		{
			return this.dist[target];
		}

/// <summary>
/// </summary>
/// <param name="graph">any object implementing IGraph</param>
/// <param name="source">the source of a path</param>
		internal SingleSourceShortestPaths(IGraph graph,int source)
		{
			this.graph=graph;
			this.source=source;
			pred=new Edge[graph.NumberOfVertices];
			dist=new int[graph.NumberOfVertices];

			for(int i=0;i<dist.Length;i++)
				dist[i]=Graph.INF;

			dist[source]=0;

		}

	}
}
