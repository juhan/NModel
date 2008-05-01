//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;

using System.Collections;

namespace NModel.Algorithms.GraphTraversals
{

  /// <summary>
  /// Defines an interface for acceptable order. This order is defined on pairs of (float,int) and
  /// is used to select a better strategy. The second component of a pair represents
  /// the maximal weight of a path to the targets and the first one is the probability to
  /// reach the target for some number of steps. See the paper at
  /// http://research.microsoft.com/users/nikolait/papers/OptimalStrategiesForTestingNondeterminsticSystems(ISSTA2004).pdf 
  /// </summary>

  internal interface IOrder 
  {
		/// <summary>
		/// Compare (a0,b0) with (a1,b1).
		/// </summary>
		/// <param name="a0"></param>
		/// <param name="b0"></param>
		/// <param name="a1"></param>
		/// <param name="b1"></param>
		/// <returns></returns>
    bool Less(float a0,int b0, float a1,int b1);
  }

	/// <summary>
	/// Strategy representation for a finite number of steps game.
	/// See http://research.microsoft.com/users/nikolait/papers/OptimalStrategiesForTestingNondeterminsticSystems(ISSTA2004).pdf
	/// </summary>
  internal struct Strategy {
		/// <summary>
		/// The strategy edge
		/// </summary>
    internal Edge edge;
		/// <summary>
		/// The probability to reach targets.
		/// </summary>
    internal float prob;
		/// <summary>
		/// The maximal weight of a path to targets.
		/// </summary>
    internal int weight;
		/// <summary>
		/// </summary>
		/// <param name="edge"></param>
		/// <param name="prob">probability</param>
		/// <param name="weight"></param>
    internal Strategy(Edge edge, float prob, int weight) {
      this.edge=edge;
      this.prob=prob;
      this.weight=weight;
    }
  }

  internal class StrategyWithUpdate 
  {
    internal Edge edge,newEdge;
    internal float prob,newProb;
    internal int weight,newWeight;
	
    /// <summary>
    /// Strategy at node constructor
    /// </summary>
    /// <param name="edge">where to go </param>
    /// <param name="prob">probability</param>
    /// <param name="weight">weight</param>
    internal StrategyWithUpdate(Edge edge, float prob, int weight)
    {
      this.newEdge=this.edge=edge;
      this.newProb=this.prob=prob;
      this.newWeight=this.weight=weight;
	
    }
		

    internal StrategyWithUpdate ()
    {
      newEdge=edge=null;
      newProb=prob=0;
      newWeight=weight=0;
	
    }
		
		
  }
  /// <summary>
  /// This class calculates strategies which are optimal for a finite number of steps.
  /// See the paper at http://research.microsoft.com/users/nikolait/papers/OptimalStrategiesForTestingNondeterminsticSystems(ISSTA2004).pdf
  /// </summary>
  internal class StrategyCalculator
  {
    IGraph graph;
    HSet P=new HSet(); //the target set
    IOrder order;
    //the set of vertices from which front can be reached for less then n+1 steps
    //initially front  is set to P
    HSet front=new HSet();
    HSet newfront=new HSet(); //new generation of front
		

    StrategyWithUpdate[] S;
		  
    int maxSteps;

    /// <summary>
    /// </summary>
    /// <param name="graph">is the graph we are playing the game on</param>
    /// <param name="P">the set to reach</param>
    ///<param name="order">compares strategies</param>
    ///<param name="maxSteps">maximum number of steps to go in the game</param>
    internal StrategyCalculator(IGraph graph,IEnumerable P,IOrder order,int maxSteps)
    {
      this.maxSteps=maxSteps;
      this.order=order;
      this.graph=graph; 	
      S=new StrategyWithUpdate[graph.NumberOfVertices];
				
      for(int i=0;i<graph.NumberOfVertices;i++)
        S[i]=new StrategyWithUpdate();
		
      foreach(int v in P)
        {
            if(v<graph.NumberOfVertices){
                this.P.Insert(v);
                this.front.Insert(v);
                S[v]=new StrategyWithUpdate(null,1,0);
            }
        }
			
    }

		/// <summary>
		/// </summary>
		/// <param name="graph">the test graph</param>
		/// <param name="P">set of target vertices</param>
		/// <param name="maxSteps">maximum number of steps in the game</param>
    internal StrategyCalculator(IGraph graph,IEnumerable P,int maxSteps):this(graph,P,new Graph.Order(),maxSteps)
    {
    }

    
    void Step(){
      for(int j=0;j<graph.NumberOfVertices;j++)
        foreach(Edge edge in this.graph.EdgesAtVertex(j)) 
          if( front.Contains(edge.target)) 
            Process(edge);
				
							
      front=newfront;
      newfront=new HSet();
      PropagateChanges();
    }

    internal  Strategy[] NextStrategy(){
      Step();
      return GetStrategies();
    }
		
    void PropagateChanges()
    {
      foreach(int v in this.front){
        StrategyWithUpdate s=this.S[v];
        s.edge=s.newEdge;
        s.prob=s.newProb;
        s.weight=s.newWeight;
      }
			
    }

    Strategy[] GetStrategies(){

      Strategy[] strategies=new Strategy[this.graph.NumberOfVertices];
      for( int k=0;k<S.Length;k++) {
        strategies[k].edge=S[k].edge;

                
        strategies[k].prob=S[k].prob;
        strategies[k].weight=S[k].weight;
      }
      return strategies;
    }

		/// <summary>
		/// The main calclulation; works no more than maxSteps times.
		/// </summary>
		/// <returns></returns>
    internal Strategy[] Calculate()
    {			
			
      for(int i=0;front.Count>0&&i<maxSteps;i++)
        Step();

      return GetStrategies();
    }
		

    bool VertIsNondet(int j)
    {
      foreach(Edge l in graph.EdgesAtVertex(j))
        {
          return graph.EdgeProbability(l)!=1;
				
        }
      return false;
				
    }

		
    /// <summary>
    /// edge[u,v] where v is in the front
    /// take values from S and updates Sn
    /// </summary>
    /// <param name="edge"></param>
    void Process(Edge edge)
    {
      int u=edge.source;
      int v=edge.target;
      if(P.Contains(u))
        return;

      if(VertIsNondet(u))
        {	
          if( this.newfront.Contains(u))
            return;

				
          double p=0.0;
          int w=0;
          foreach(Edge ulink in this.graph.EdgesAtVertex(u))
            {
                
                p+=graph.EdgeProbability(ulink)*S[ulink.target].prob;
                
                
                int wn=ulink.weight+S[ulink.target].weight;
                if(wn>w)
                    w=wn;
                
                
            }
          
					
          S[u].newProb=(float)p;
          S[u].newWeight=w;
          newfront.Insert(u);
				
        }
      else if(Improving(edge)) 
        {
          S[u].newEdge=edge;
          S[u].newProb=S[v].prob;
          S[u].newWeight=edge.weight+S[v].weight;
                    
          newfront.Insert(u);
        }
    }

    

		/// <summary>
		/// Returns a strategy to reach targets from a vertex.
		/// </summary>
		/// <param name="vertex"></param>
		/// <returns></returns>
    internal Strategy[] GetStrategyReachableFromVertex(int vertex){

      Strategy [] strategies=this.GetStrategies();

      

      if(VertIsNondet(vertex))
        throw new ArgumentException("vertex is a choice point");
      
      //ArrayList links=new ArrayList();


      HSet linkSet=new HSet();
      
      foreach(Strategy s in strategies)
        if(s.edge!=null){

          linkSet.Insert(s.edge);

          if(VertIsNondet( s.edge.target))
            foreach(Edge l in graph.EdgesAtVertex(s.edge.target))
              linkSet.Insert(l);
          
         }
        
      

      
      
      BasicGraph bg=new BasicGraph(0, linkSet.ToArray(typeof(Edge)) );

      bool []reachables=bg.GetReachableArray(vertex);

      for(int i=0;i<reachables.Length;i++){
        if(reachables[i]==false)
          strategies[i].edge=null;        
      }

      
      return strategies;

    }
		
        
    bool Improving(Edge l)
    {         
      return order.Less( S[l.target].prob,
                         l.weight+S[l.target].weight,
                         S[l.source].newProb,S[l.source].newWeight);
    }

  }
}
