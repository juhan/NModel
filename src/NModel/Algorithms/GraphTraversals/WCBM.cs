//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;


namespace NModel.Algorithms.GraphTraversals 
{
	/// <summary>
	/// Weighted Bipartite Matching
	/// This is a special version when the bipartite graph 
	/// is complete.
	/// We do matching between nodes [0..n-1] and [n..2n-1]
	/// </summary>
	internal class WeightedCompleteBipartiteMatching
	{
	
		internal const int NONE=-1;
		int []flatA;
		int [] flatB;
		int[] P;
		int N; //number of nodes on one side	
		internal int [] M; //current matching 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        int [,] w;//weights
		//if M[j]> -1 then the edge (M[j],n+j) is in the matching

		int W(int i,int j)
		{
			return w[flatA[i],flatB[j]];
		}

		internal WeightedCompleteBipartiteMatching(int [,]w,int[] flatA,int[]flatB)
		{
			
			this.w=w;
			this.flatA=flatA;
			this.flatB=flatB;
			N=flatA.Length;
			P=new int[2*N];//zeroes from n to 2n-1
			M=new int[N];
			//calculating potentials and init matching to empty
			int k=0;
			for(int i=0;i<w.GetLength(0);i++)
			{
				int m=w[i,0];

				for(int j=1;j<w.GetLength(1);j++)
					if(w[i,j]>m)
						m=w[i,j];

				int l=flatA[k];
				do 
				{
					P[k++]=m;
				} while(k<N && flatA[k]==l);			
			}

			for(int i=0;i<N;i++)
				M[i]=NONE;
			
		}

//N-stages
		internal void stages()
		{
			bool[] fnodes=new bool[2*N]; //init with trues the second half
			for(int i=0;i<N;i++)
			{
				fnodes[N+i]=true;
			}
			GraphWithMatching graph=new GraphWithMatching(M,w,P,flatA,flatB);
			//IntBinaryHeapPriorityQueue pq=new IntBinaryHeapPriorityQueue(graph.NumberOfVertices);
			for(int i=0;i<N;i++)
			{	
				
        //match i-th node
				SSMTSP spa=new SSMTSP(graph,i, fnodes /*,pq */);
				int[] path=spa.GetPath();
				int theClosestVertex=path[path.Length-1];
				int db0=spa.dist[theClosestVertex];
				fnodes[theClosestVertex]=false;
				//db0 gives the shortest reduced price cost path
                //fix potential
				for(int j=0;j<N;j++)
				{
					int m=db0-spa.dist[j];
					if (m>0)
						P[j]-=m;

					m=db0-spa.dist[N+j];
					if(m>0)
						P[N+j]+=m;
				}

				for(int j=1;j<path.Length;j+=2)
				{
					M[path[j]-N]=path[j-1];
				}

			}
		}

	}
}
