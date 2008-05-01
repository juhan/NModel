//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections;

//using System.Runtime.InteropServices;
//using System.ComponentModel;
//using System.Threading;


namespace NModel.Algorithms.GraphTraversals
{

    //internal  class HiPerfTimer
    //{
    //    [DllImport("Kernel32.dll")]
    //        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

    //    [DllImport("Kernel32.dll")]
    //        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

    //    private long startTime, stopTime;
    //    private long freq;

    //    // Constructor
    //    internal HiPerfTimer()
    //    {
    //        startTime = 0;
    //        stopTime  = 0;

    //        if (QueryPerformanceFrequency(out freq) == false)
    //            {
    //                // high-performance counter not supported
    //                throw new Win32Exception();
    //            }
    //    }

    //    // Start the timer
    //    internal void Start()
    //    {
    //        // lets do the waiting threads there work
    //        Thread.Sleep(0);

    //        QueryPerformanceCounter(out startTime);
    //    }

    //    // Stop the timer
    //    internal void Stop()
    //    {
    //        QueryPerformanceCounter(out stopTime);
    //    }

    //    // Returns the duration of the timer (in seconds)
    //    internal double Duration
    //    {
    //        get
    //            {
    //                return (double)(stopTime - startTime) / (double) freq;
    //            }
    //    }
    //}




	/// <summary>
	/// calculates a tour in O(n(n+m)log(n)) time
	/// </summary>
	internal class CHPP_N_M_logN:IGraph
	{
		int n;	//number of nodes	
		int initialVertex;
		ArrayList[] linksByVertex;
    
		int [] d;

        //the array of nodes with excess of incoming edges
		int[] neg;

        //array of nodes with excess of outcoming edges
		int[] pos;

        int[] flatNeg;//every endex is repeated here the negative degree times
 		int[] flatPos; //every endex is repeated here the positive degree times

		int []nodeToNeg;
		int []nodeToPos;

		int adjoinedPathLabel;//this will label the adjoined paths
		Edge[] mustEdges;
		internal Edge[] MustEdges {get {return this.mustEdges;}}
		Edge[] optionalEdges;
		internal Edge[] OptionalEdges {get {return this.optionalEdges;}}
		
		Edge[] theTour;    
		int D;  //sum of positive degrees - equals to abs of sum of negative ones

		//IntBinaryHeapPriorityQueue pq;

		//if pathStart is different from -1 the the path can be obtaine from the tour 
		//by taking pathLength elements starting from pathStart

        int pathStart;
		int pathLength; 
        


		public int GetVertexWeight(int i){return 1;}
		public int NumberOfVertices{get{return n;}}
		internal int NumberEdgesExitingTheVertex(int node)
		{
			return this.linksByVertex[node].Count;
		}
		public double EdgeProbability(Edge l){return 1;}
		public IEnumerable InitialVertices(){return new int[] {this.initialVertex};}
		public IEnumerable EdgesAtVertex(int node){ return linksByVertex[node];}
		//careful: node indices define the number of nodes - n is set to the maximal index plus one
		internal CHPP_N_M_logN(
                             int initialVertex,
                             Edge[] mustEdges,
                             Edge[] optionalEdges,int n)
		{	
			//pq=new IntBinaryHeapPriorityQueue(n);
			this.optionalEdges=optionalEdges;
			this.pathStart=Graph.NONE;

			this.initialVertex=initialVertex;
			this.n=n;
			this.mustEdges=mustEdges;
    
			nodeToNeg=new int[n];

			nodeToPos=new int[n];

			d=new int[n];
			this.linksByVertex=new ArrayList[n];
			
			for(int i=0;i<n;i++)
				linksByVertex[i]=new ArrayList();


            if(mustEdges==null){
                mustEdges=new Edge[0];
            }
			foreach(Edge l in mustEdges)
                {
                    int u=l.source;
                    int v=l.target;
                    linksByVertex[u].Add(l);
                    d[u]++;
                    d[v]--;
                    if(adjoinedPathLabel<=l.label)
                        adjoinedPathLabel=l.label+1;
                }

			if(optionalEdges!=null)
                foreach(Edge l in optionalEdges)
                    //I don't care about degree and uniqueLabel here
                    //optional edges do not change the degree
                    linksByVertex[l.source].Add(l);

			//prepare neg,pos,flatPos and flatNeg
			int nn=0,np=0;
			for(int i=0;i<n;i++)
                {
                    int deg=d[i];
                    if (deg>0)
                        {
                            nodeToPos[i]=np++;		
                            nodeToNeg[i]=Graph.NONE;
                            D+=deg;					
                        }
                    else if (deg<0)
                        {
                            nodeToNeg[i]=nn++;
                            nodeToPos[i]=Graph.NONE;
                        }
                    else 
                        {
                            nodeToNeg[i]=Graph.NONE;
                            nodeToPos[i]=Graph.NONE;
                        }
                }

			this.neg=new int[nn];
			this.pos=new int[np];
			nn=0;np=0;
		
			int fncount=0;
			int fpcount=0;
			flatNeg=new int[D];
			flatPos=new int[D];
			

            //
			// 
			//
			// 

			for(int i=0;i<n;i++)
                {
                    int deg=d[i];
                    if (deg>0)
                        {
                            pos[np]=i;												
                            while(deg-- >0)
                                flatPos[fpcount++]=np;
		
                            np++;
                        }
                    else if (deg<0)
                        {
                            neg[nn]=i;
                            while(deg++ <0)
                                flatNeg[fncount++]=nn;

                            nn++;
                        }
                }
			
		}


        static int IndexOfVertexInPath(Edge[] path, int vertex){

            if(path[0].source==vertex)
                return 0;
            
            for(int i=0;i<path.Length;i++)
                if(path[i].target==vertex)
                    return i+1;

            return -1;
                
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Body")]
        internal Edge[] GetRuralChinesePostmanTour()
		{
			
            //	Win.HiPerfTimer pt = new Win.HiPerfTimer();                    // create a new PerfTimer object
			//pt.Start();                 

            if(this.MustEdges==null)
                {
                    return null;
                }
			if(theTour != null)
				return theTour;
	

			int [,]dist=new int[neg.Length,pos.Length];
			//dist[i,j] will be the distance from neg[i] to pos[j]  

			int maxdist=0;
			calcDistances(dist,ref maxdist);
    
			//construct the weight matrix for matching
			int [,]w=new int[neg.Length,pos.Length];
			for(int i=0;i<neg.Length;i++)
				for(int j=0;j<pos.Length;j++)
					w[i,j]=maxdist-dist[i,j];
				
			WeightedCompleteBipartiteMatching wb=new WeightedCompleteBipartiteMatching(w,flatNeg,flatPos);
			wb.stages();

			//now the matching is calculated and we can create a balanced graph
			//for(int i=0;i<D;i++)
			//	f[neg[wm.M[i]],pos[i]]++;
			//create an array of links for a balanced graph
			Edge[] links=new Edge[ mustEdges.Length+D];
			mustEdges.CopyTo(links,D);
			for(int i=0;i<D;i++)
                {
                    int negi=neg[flatNeg[wb.M[i]]];
                    int posi=pos[flatPos[i]];
                    links[i]=new Edge(negi,posi,adjoinedPathLabel); //don't care about weight
                }
			
			EulerianTour et=new EulerianTour(initialVertex,links,n);

			Edge[] etour=et.GetTour();       
    
			ArrayList al=new ArrayList();

			//Expands adjoined path paths
			//Also calculates the maximal length adjoined path leading to the initial node:
			//will throw this path away in the GetRuralChinesePostmanPath()
			

			int maxIndexOfInitialVertexInAdjoinedPaths=-1;
			int pathToCut=0;

			foreach(Edge l in etour)
                {
                    if(l.label!=adjoinedPathLabel)
                        al.Add(l);
                    else {

                        int pathLen=dist[nodeToNeg[l.source],nodeToPos[l.target]];

                        SingleSourceSingleTargetUpperDistSP dm=
                            new SingleSourceSingleTargetUpperDistSP(this,l.source,l.target,pathLen);
						
                        Edge []path=dm.GetPath();
                        al.AddRange(path);

                        int indexOfInitVertex=IndexOfVertexInPath(path,initialVertex);
                        if(indexOfInitVertex>maxIndexOfInitialVertexInAdjoinedPaths)					
                            {
                                maxIndexOfInitialVertexInAdjoinedPaths=indexOfInitVertex;
                                pathStart=al.Count-path.Length+indexOfInitVertex;
                                pathToCut=indexOfInitVertex;
                            }
                    }
			
                }

			theTour=new Edge[al.Count];
			al.CopyTo(theTour);				
			if(pathStart!=Graph.NONE)
                this.pathLength=theTour.Length-pathToCut;			

			//pt.Stop();
			
			//System.Windows.Forms.MessageBox.Show(pt.Duration.ToString());
		
			return theTour;
			
		}


		internal Edge[] ShortestPath(int start, int end)
		{
			bool [] fnodes=new Boolean[this.NumberOfVertices];
			fnodes[end]=true;
			SSMTSP dm=new SSMTSP(this,start,fnodes /*,this.pq */);
			return dm.GetPathAsEdges();
		}
        
		
		internal Edge[] GetRuralChinesePostmanPath()
		{
			GetRuralChinesePostmanTour();
			if(pathStart==Graph.NONE)
				return theTour;
			
            Edge[] ret=new Edge[pathLength];
               				
			int j=0;
			int tl=0;
			for(int i=pathStart;j<pathLength;i++,j++)
                {
                    if(i==theTour.Length)
                        i=0;
                    ret[j]=theTour[i];
                    tl+=ret[j].weight;				
                }
			//Console.WriteLine("total length="+tl);
			return ret;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "2#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.Exception.#ctor(System.String)")]
        void calcDistancesFromVertex(int indexInNeg, 
                                   bool[] posC,
                                   int [,]dist,
                                   ref int maxdist)
		{			
		    int node=neg[indexInNeg];
			int [] ld=new int[n];//local distance array
			IntBinaryHeapPriorityQueue pq1=new IntBinaryHeapPriorityQueue(n);
			
			int posLeft=pos.Length;
			pq1.insert(node,0);
			for(int i=0;i<n;i++)
				ld[i]=Graph.INF;

			ld[node]=0;

			while( ! pq1.isEmpty())
                {
                    int u=pq1.del_min();
                    if(posC[u])
                        {
                            if (--posLeft==0)
                                break;
                        }

                    foreach(Edge l in this.linksByVertex[u])		
                        {				
                            int v=l.target;
                            int len=l.weight;
                            int c=ld[u]+len;
				     				
                            if(ld[v]==Graph.INF)
                                pq1.insert(v,c);
                            else if (c<ld[v])
                                pq1.decrease_priority(v,c);
                            else 
                                continue;

                            ld[v]=c;                   
                        }
                }
			
			if (posLeft>0)
				throw new InvalidOperationException("graph is not strongly connected");
			//put the results in dist and update maxdist
			for(int i=0;i<pos.Length;i++)
                {
                    int dis=dist[indexInNeg,i]=ld[pos[i]];
                    if (dis>maxdist)
                        maxdist=dis;
                }

		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "0#")]
        void calcDistances(int[,] dist, ref int maxdist)
		{
			bool [] posC=new bool[n];//characteristic function of positive nodes
			foreach(int i in pos)
				posC[i]=true;
			for(int i=0;i<neg.Length;i++)
				calcDistancesFromVertex(i,posC,dist,ref maxdist);
		}


	}
}
