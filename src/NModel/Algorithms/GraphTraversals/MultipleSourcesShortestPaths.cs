//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;




namespace NModel.Algorithms.GraphTraversals
{
	/// <summary>
	/// Summary description for MultipleSourcesShortestPaths.
	/// </summary>
	internal class MultipleSourcesShortestPaths {


		IGraph graph;
		int[] source;
		Edge[]pred;

		int []dist;
    HSet sourceSet;
		
		internal Edge[] Pred {
			get{
				return pred;
			}
		}

		internal void Calculate()
		{
			IntBinaryHeapPriorityQueue pq=new IntBinaryHeapPriorityQueue(graph.NumberOfVertices);
      foreach(int s in source)
        pq.insert(s,0);
            
			while(!pq.isEmpty())
        {
          int u=pq.del_min();
			
	
          foreach(Edge l in graph.EdgesAtVertex(u))		
            {
              int v=l.target;
              int len=l.weight;
              int c=dist[u]+len;
			
              if(pred[v]==null && !sourceSet.Contains(v))
                pq.insert(v,c);
              else if (c<dist[v])
                pq.decrease_priority(v,c);
              else 
                continue;

              dist[v]=c; pred[v]=l;                    
            }
        }
			
		}


		internal Edge[] GetPathTo(int target)
		{
			

			int count=0;
			int v=target;
			while(!sourceSet.Contains(v))
        {
          v=pred[v].source;
          count++;
        }
		
			Edge[] thePath=new Edge[count];
			
			v=target;
			while(!sourceSet.Contains(v))
        {
          thePath[--count]=pred[v];
          v=pred[v].source;				
        }
			
			
			return thePath;

		}

		internal int GetDistTo(int target)
		{
			return this.dist[target];
		}


		internal MultipleSourcesShortestPaths(IGraph graph,int[] source)
		{
			this.graph=graph;
			this.source=source;
			pred=new Edge[graph.NumberOfVertices];
			dist=new int[graph.NumberOfVertices];

			for(int i=0;i<dist.Length;i++)
				dist[i]=Graph.INF;

      foreach(int s in source)
        dist[s]=0;

      sourceSet=new HSet(source);

		}

	}
}
