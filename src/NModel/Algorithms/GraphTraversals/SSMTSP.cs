//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;



namespace NModel.Algorithms.GraphTraversals 
{
/*
	public class Link 
	{
		public int u;
		public int v;
		public int length;
		public int label;
		public Link(int u,int v,int length,int label)
		{
			this.source=u;this.target=v;
			this.label=label;this.length=length;
		}
	}*/
	/// <summary>
	/// Single Source Multiple Target Shortest Path.
	/// The algorithm follows the paper
	/// http://www.mpi-sb.mpg.de/~mehlhorn/ftp/wbm-heur.ps.gz
	/// </summary>
	/// 
	internal class SSMTSP
	{
		//IntBinaryHeapPriorityQueue pq;
		const int NONE=-1;
		IGraph graph;
		int source;
		bool [] freeVertices;//characteristic function of free nodes
		/// <summary>
		/// The array of distances.
		/// </summary>
		internal int [] dist;
		Edge [] pred;
		int foundFreeVertex;
		bool calcIsCalled;

		//internal int numOfEdges; //only for reporting to the stopper
		//must be set or will not communicate with the stopper

        //internal int numberOfProcessedEdges;

        const int INF=0x7fFFffFF;

        internal SSMTSP(IGraph gr, int source, bool[] fnodes /*, IntBinaryHeapPriorityQueue pq */)
        {
            //this.pq = pq; 
            init(gr, source, fnodes);
        }
		void init(IGraph gr, int src,bool [] fnodes)
		{
			this.graph=gr;
			this.source=src;
			int n=gr.NumberOfVertices;
			this.freeVertices=fnodes; 
			
			pred=new Edge[n]; 			
			dist=new int[n]; 
           
			for(int i=0;i<n;i++)
				dist[i]=INF; //maximal int
			
		
			dist[src]=0;
	
			foundFreeVertex=-1; //to signal that we haven't found it

			calcIsCalled=false;

			//numOfEdges=NONE;
			//numberOfProcessedEdges=0;
		}

		/// <summary>
		/// Single Source Multiple Target Shortest Path
		/// </summary>
		/// <param name="graph">an instance of IGraph</param>
		/// <param name="source">the source vertex</param>
		/// <param name="targets">an array of target vertices</param>
		internal SSMTSP(IGraph graph, int source,int [] targets)
		{
			bool []fVertices=new bool[graph.NumberOfVertices]; //initialized by falses I suppose

			foreach(int i in targets)
				fVertices[i]=true;

			init(graph,source, fVertices);
		}

		 void calc()
		 {
			 if(calcIsCalled)
				 return;

			 calcIsCalled=true;

			 int upperBound=0; //the init is not needed but the compiler produces an error
			 bool upperBoundIsSet=false;

			 IntBinaryHeapPriorityQueue pq1=new IntBinaryHeapPriorityQueue(graph.NumberOfVertices);
			 
			 pq1.insert(source,0);
			 while( ! pq1.isEmpty())
			 {
				 int u=pq1.del_min();
				 if(freeVertices[u])
				 {
					 foundFreeVertex=u;
					 break;
				 }

				 foreach(Edge l in graph.EdgesAtVertex(u))		
				 {

           int v=l.target;
					 int len=l.weight;
					 int c=dist[u]+len;
					 if(upperBoundIsSet && c >= upperBound)
						 continue;
                     
					 if(freeVertices[v]){upperBound=c;upperBoundIsSet=true;}

					 if(pred[v]==null && v !=source)
						 pq1.insert(v,c);
					 else if (c<dist[v])
						 pq1.decrease_priority(v,c);
					 else 
						 continue;

					 dist[v]=c; pred[v]=l;
                   
				 }
			 }
		 }


        

		/// <summary>
		/// Returns a path as a sequence of vertices.
		/// </summary>
		/// <returns></returns>
		internal int [] GetPath()
		{
			calc(); 
			if (foundFreeVertex==-1)
				return null;

			int count=0;

			int v=foundFreeVertex;
			while(v!=source)
			{
				v=pred[v].source;			
				count++;
			}

			int[] ret=new int[count+1];
			
			v=foundFreeVertex;			
			ret[count]=v;
			while(v!=source)
			{   
				count--;
				v=pred[v].source;
				ret[count]=v;			
			}
			
			return ret;
		}

		/// <summary>
		/// Returns a path as a sequence of edges.
		/// </summary>
		/// <returns></returns>
		public Edge [] GetPathAsEdges()
		{
			calc(); 
			if (foundFreeVertex==-1 )
				return null;

			int count=0;

			int v=foundFreeVertex;
			while(v!=source)
			{
				v=pred[v].source;			
				count++;
			}

			Edge[] ret=new Edge[count];
			
			v=foundFreeVertex;			
		
			while(count-- > 0)
			{   
				ret[count]= pred[v];
				v=pred[v].source;
			}
			
			return ret;
		}
	}
}
