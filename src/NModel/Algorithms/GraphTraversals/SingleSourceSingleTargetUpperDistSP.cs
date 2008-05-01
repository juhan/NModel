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
	///  and a real-valued weight function f : E ? R), and given 
	///  further two elements n, n' of N, find a path P from n to n' so that 
	///  the sum of {f(p):p in P}	is minimal among all paths connecting n to n'.
	///  Here we use Dijkstra algorithm to find the path, and use the
  ///  given upper bound for the speed up.
	/// </summary>

	internal class SingleSourceSingleTargetUpperDistSP
	{
		IGraph graph;
		int source;
		int target;
		int udist;
		Edge[]pred;
		int []dist;
		Edge[] thePath;

		/// <summary>
		/// Returns  a  path
		/// </summary>
		/// <returns></returns>
		internal Edge[] GetPath()
		{
			if(thePath!=null)return thePath;

			IntBinaryHeapPriorityQueue pq=new IntBinaryHeapPriorityQueue(graph.NumberOfVertices);
			pq.insert(source,0);
			while(!pq.isEmpty())
			{
				int u=pq.del_min();
				if(u==target)				
					break;
				
				foreach(Edge l in graph.EdgesAtVertex(u))		
				{
					int v=l.target;
					int len=l.weight;
					int c=dist[u]+len;
					if(c > udist)continue;
                     
					if(pred[v]==null && v !=source)
						pq.insert(v,c);
					else if (c<dist[v])
						pq.decrease_priority(v,c);
					else 
						continue;

					dist[v]=c; pred[v]=l;                    
				}
			}
			if(dist[target]==Graph.INF)
				throw new InvalidOperationException("target is not reached");

		{
			int count=0;
			int v=target;
			while(v!=source)
			{
				v=pred[v].source;
				count++;
			}
			thePath=new Edge[count];
			
			v=target;
			while(v!=source)
			{
				thePath[--count]=pred[v];
				v=pred[v].source;				
			}
			
			
		}
			
			return thePath;
		}

		/// <summary>
		/// </summary>
		/// <param name="graph">the graph</param>
		/// <param name="source">the source of a path</param>
		/// <param name="target">the target of a path</param>
		/// <param name="udist">the upper distance; it is given here that the distance from source to target is no more than udist</param>
		internal SingleSourceSingleTargetUpperDistSP(IGraph graph,int source,
			int target,
			int udist)
		{
			this.graph=graph;
			this.source=source;
			this.target=target;
			this.udist=udist;
			pred=new Edge[graph.NumberOfVertices];
			dist=new int[graph.NumberOfVertices];

			for(int i=0;i<dist.Length;i++)
				dist[i]=Graph.INF;

			dist[source]=0;

		}
	}
}
