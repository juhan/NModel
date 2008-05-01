//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.IO;
using System.Collections;

namespace NModel.Algorithms.GraphTraversals 
{
	/// <summary>
	/// Strategy representation.
	/// </summary>
  internal struct EdgesAndExpectations {
		/// <summary>
		/// for a vertex i edges[i] is the move at i 
		/// </summary>
    internal Edge[] edges;
		/// <summary>
		/// expectations[i] is the expected weight of a path
		/// in a game starting from i 
		/// </summary>
    internal double[] expectations;
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="ls"></param>
		/// <param name="exps"></param>
    internal EdgesAndExpectations(Edge[] ls,double[] exps){edges=ls;expectations=exps;}
  }
/// <summary>
/// Class calculating strategies for reachability games. See MSR technical report "Play to test".
/// </summary>
  internal class StaticStrategies {

    private StaticStrategies(){}
    
    static void CheckTransience(Graph graph,HSet targets){
      Queue q=new Queue();
      bool[] reached=new bool[graph.NumberOfVertices];
      foreach(int v in targets){
        reached[v]=true;
        q.Enqueue(v);        
      }
      while(q.Count>0){
        int v=(int)q.Dequeue();
        foreach(int u in new Pred(graph,v)){
          if(reached[u]==false){
            reached[u]=true;
            q.Enqueue(u);
          }
        }
      }

      foreach(bool b in reached)
        if(!b)
          throw new InvalidOperationException("some vertex has not been reached");

    }

    
        /*
      From "Play to test"
      Value iteration is the most widely used algorithm for solving discounted Markov decision
      problems (see e.g. [21]). Reachability games give rise to non-discounted Markov
      decision problems. Nevertheless the value iteration algorithm applies; this is a practical
      approach for computing strategies for transient test graphs. Test graphs, modified by inserting
      a zero-cost edge (0; 0), correspond to a subclass of negative stationary Markov
      decision processes (MDPs) with an infinite horizon, where rewards are negative and
      thus regarded as costs, strategies are stationary, i.e. time independent, and there is no
      finite upper bound on the number of steps in the process. The optimization criterion
      for our strategies corresponds to the expected total reward criterion, rather than the
      expected discounted reward criterion used in discounted Markov decision problems.
      Let G = (V;E; V a; V p; g; p; c) be a test graph modified by inserting a zero-cost
      edge (0; 0). The classical value iteration algorithm works as follows on G.

      Value iteration Let n = 0 and let M0 be the zero vector with coordinates V so that
      every M0[u] = 0. Given n and Mn, we compute Mn+1 (and then increment n):
      Mn+1[u] ={ min {c(u,v) +Mn[v]:(u,v) in E} if u is an active state}
      or sum {p(u,v)*(c(u,v) +Mn[v]); if u is a choice point

      Value iteration for negative MDPs with the expected total reward criterion, or negative
      Markov decision problems for short, does not in general converge to an optimal
      solution, even if one exists. However, if there exists a strategy for which the the expected
      cost is finite for all states [21, Assumption 7.3.1], then value iteration does converge for
      negative Markov decision problems [21, Theorem 7.3.10]. In light of lemmas 2 and 3,
      this implies that value iteration converges for transient test graphs. Let us make this
      more precise, as a corollary of Theorem 7.3.10 in [21]. 
    */

    //nStates marks the end of active states, choice points start after that
      static double[] ValueIteration(Graph graph, HSet targets, int nStates) //ValueIteration(Graph graph,int[] sources,HSet targets,int nStates)
    {

      graph.InitEdgeProbabilities();
      
      double[]v0=new double[graph.NumberOfVertices];
      double[]v1=new double[graph.NumberOfVertices];
      double eps=1.0E-6;
      double delta;
      double[] v=v0;
      //double[] vnew=v1;


      //      CheckTransience(graph,targets);

      
      int nOfIter=0;
      do{

        delta=0;
        

        for(int i=0;i<nStates&&i<graph.NumberOfVertices;i++){

          if(targets.Contains(i))
            continue;
          
          double min=Double.MaxValue;

          foreach(Edge l in graph.EdgesAtVertex(i)){              
            double r=((double)l.weight)+v[l.target];
            if(r<min)
              min=r;
          }
          if(min!=Double.MaxValue){
            v1[i]=min;
            if(delta<min-v[i])
              delta=min-v[i];
          }

          
        }

        for(int i=nStates;i<graph.NumberOfVertices;i++){
          if(targets.Contains(i))
            continue;

          double r=0;
          foreach(Edge l in graph.EdgesAtVertex(i))
            r+=graph.EdgeProbability(l)*(((double)l.weight)+v[l.target]);

          v1[i]=r;
          if(delta<r-v[i])
            delta=r-v[i];
        }


        nOfIter++;

        //swap v and v1
        double[] vtmp=v;
        v=v1;
        v1=vtmp;
      }
      while(delta>eps && nOfIter<10000);

      if(delta>eps){
        return null; //the result is erroneous
      }

      return v;
    }

    static internal double [] GetExpectations(Graph graph,/*int[] sources,*/ HSet targets, int nStates){

        double[] ret = ValueIteration(graph, targets, nStates); //ValueIteration(graph,sources,targets,nStates);
			if(ret==null)
        return graph.GetDistancesToAcceptingStates(targets.ToArray(typeof(int))as int[]);  
                //return graph.GetDistancesToAcceptingStates(sources,targets.ToArray(typeof(int))as int[]); 
      else
        return ret;

    }


		/// <summary>
		/// Calculates an optimal strategy leading from sources to targets.
		/// If the number of vertices in the graph is big, more than 500, then expected
		/// values sometimes are not correct - they just express the length of a shortest path from sources 
		/// to targets. It can happen because the value iteration process converges too slowly
    /// and we use just shortest path weights to estimate expectations.
		/// </summary>
		/// <param name="graph">the test graph</param>
		/// <param name="sources">the start vertices of the game</param>
		/// <param name="targets">the target vertices of the game</param>
		/// <param name="nStates">active states are enumerated from 0 to nStates-1,
    /// and passive from nStates to graph.NumberOfVertices-1</param>
		/// <param name="resetCost">it is the cost of a link which could be added to connect a state with the initial vertex</param>
		/// <param name="deadStates">the set of vertices which are forbidden to use in a path to targets</param>
		/// <returns>a structure containing strategy edges and expected weight of a path to the targets</returns>
    //
		//
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    static internal EdgesAndExpectations GetStaticStrategy(Graph graph,
                                                         int[] sources,
                                                         HSet targets,
                                                         int nStates,
                                                         int resetCost,
                                                         int []deadStates ){


      

      foreach(int t in targets){
        foreach(Edge l in graph.EdgesAtVertex(t))
          l.weight=0;
      }

      //fix the edges weights
      foreach(Edge l in graph.MustEdges){
        if(l.target>=nStates)
          l.weight=0;        
      }
      foreach(Edge l in graph.OptionalEdges)
        l.weight=resetCost;
      


      if(graph.NumberOfVertices>1000){//Value iteration becomes too slow
        return graph.GetStaticStrategyWithDistancesToAcceptingStates(sources, targets.ToArray(typeof(int)) as int[]);
      }

      
      HSet deadStatesSet=new HSet(deadStates);

      //create reachableGraph 
      bool []reachableVerts=new bool[graph.NumberOfVertices];

      //we have to walk backwards from the targets avoiding dead states
      graph.InitBackwardEdges();
        
      foreach(int i in targets)
        reachableVerts[i]=true;

      System.Collections.Queue queue=new System.Collections.Queue(targets);

      while(queue.Count>0)
        {
          int i=(int)queue.Dequeue();
          foreach(int v in graph.Pred(i))
            {
              if(!reachableVerts[v] && !deadStatesSet.Contains(v))
                {
                  queue.Enqueue(v);
                  reachableVerts[v]=true;
                }

            }
        }
		
      int numberOfReachableVerts=0;
      foreach(bool b in reachableVerts)
        if(b)
          numberOfReachableVerts++;

      Edge[] strategyEdges;
      double [] expectations;
		
      if(numberOfReachableVerts==graph.NumberOfVertices)
        {
          expectations=GetExpectations(graph,/* sources,*/targets,nStates);

          if(expectations==null)
            return new EdgesAndExpectations();
            
          strategyEdges=new Edge[nStates];

          for(int i=0;i<nStates&&i<graph.NumberOfVertices;i++){
                
            if(targets.Contains(i)||deadStatesSet.Contains(i))
              continue;
              
            double min=Single.MaxValue;
              
            Edge stEdge=null;
              
            foreach(Edge l in graph.EdgesAtVertex(i)){
              int j=l.target;
              if(expectations[j]<min){
                min=expectations[j];
                          
                stEdge=l;
              }
            }

              
            strategyEdges[i]=stEdge; 
          }

        }
      else 
        { //numberOfReachableVerts<graph.NumberOfVertices)

          int [] graphToRG=new int[graph.NumberOfVertices];
          //reachable graph to graph
          int [] rGToGraph=new int[numberOfReachableVerts];

          int count=0;
          int rNStates=0;
          for(int i=0;i<reachableVerts.Length;i++)
            if(reachableVerts[i])
              {
                graphToRG[i]=count;
                rGToGraph[count]=i;
                count++;
                if(i<nStates)
                  rNStates++;
              }
		
          System.Collections.ArrayList mustEdges=new System.Collections.ArrayList();

          foreach(Edge l in graph.MustEdges)
            {
              if( reachableVerts[l.source]&& reachableVerts[l.target])
                {
                  Edge ml=new Edge(graphToRG[l.source],graphToRG[l.target], l.label,l.weight);
                  mustEdges.Add(ml);
                }
            }

          System.Collections.ArrayList nondVerts=new System.Collections.ArrayList();

          for(int i=nStates;i<graph.NumberOfVertices;i++)
            {
              if(reachableVerts[i])
                nondVerts.Add(graphToRG[i]);
            }

          Graph rGraph=new Graph(0,mustEdges.ToArray(typeof(Edge)) as Edge[],new Edge[0],
                                 nondVerts.ToArray(typeof(int)) as int[],true,WeakClosureEnum.DoNotClose);


          int []rSources=new int[sources.Length];
          int c=0;
          foreach(int s in sources)
            {
              rSources[c++]=graphToRG[s];
            }

          HSet rTargets=new HSet();

          foreach(int s in targets)
            {
              if( reachableVerts[s])
                {
                  rTargets.Insert(graphToRG[s]);
                }
            }

          double []rExpectations=GetExpectations(rGraph,/*rSources,*/ rTargets,rNStates);

          if(rExpectations==null)
            return new EdgesAndExpectations();

          strategyEdges=new Edge[nStates];

          for(int i=0;i<nStates;i++){
                        
            if(!reachableVerts[i])
              continue;

            if(targets.Contains(i)||deadStatesSet.Contains(i))
              continue;
						
            double min=Single.MaxValue;
                
            Edge stEdge=null;
                
            foreach(Edge l in graph.EdgesAtVertex(i)){                    
              int j=l.target;
                    
              if(reachableVerts[j])
                if(rExpectations[graphToRG[j]]<min){
                  min=rExpectations[graphToRG[j]];
                  stEdge=l;
                }
            }
                
                
            strategyEdges[i]=stEdge; 
          }


          expectations=new double[graph.NumberOfVertices];
          if(expectations==null)
            return new EdgesAndExpectations();
 
          for(int i=0;i<expectations.Length;i++)
            expectations[i]=Int32.MaxValue;
                     
 
          for(int i=0;i<rExpectations.Length;i++)
            expectations[rGToGraph[i]]=rExpectations[i];
                         
               
                    
        }

      graph.CleanTheStrategy(strategyEdges,sources);
        
      return new EdgesAndExpectations(strategyEdges, expectations);
        
    }
  }
    
}
 

  
