//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;

namespace NModel.Algorithms.GraphTraversals 
{

  internal enum EdgeKind
    {
      Must,
      Optional
    };

	/// <summary>
	/// This enum is used in the weak closure of the must edges set.
	/// A weak closure is the one disregarding edge directions.
	/// </summary>
  internal enum WeakClosureEnum {
		/// <summary>
		/// Instruction to build a closure.
		/// </summary>
    Close,
		/// <summary>
		/// Instruction to not build a closure.
		/// </summary>
		DoNotClose
  }

	/// <summary>
	/// Edge of a graph: has source,target, label and weight.
	/// </summary>
  internal class Edge:IComparable
  {
   /// <summary>
   /// Must be a non-negative number.
   /// </summary>
		internal int source;
		/// <summary>
		/// Must be a non-negative number.
		/// </summary>
		internal int target;
		/// <summary>
		/// Any number.
		/// </summary>
    internal int label;
		/// <summary>
		/// Any number.
		/// </summary>
    internal int weight;

/// <summary>
/// Memberwise comparison
/// </summary>
/// <param name="obj"></param>
/// <returns></returns>
    public override bool Equals(object obj)
    {
      Edge l=obj as Edge;
      if(l==null)
        return false;

      return l.source==this.source && l.target==this.target && l.label==this.label && l.weight==this.weight;
    }

/// <summary>
/// Hashcode - weight does not participate
/// </summary>
/// <returns></returns>
    public override int GetHashCode()
    {
      return source.GetHashCode() | target.GetHashCode() | label.GetHashCode();
    }

		/// <summary>
		/// An empty constructor.
		/// </summary>
    internal Edge(){}

		/// <summary>
		/// Source,target,label constructor.
		/// </summary>
		/// <param name="source">a non-negative integer</param>
		/// <param name="target">a non-negative integer</param>
		/// <param name="label">any integer</param>
    internal Edge(int source,
                int target,
                int label)
    {
      this.source=source;
      this.target=target;
      this.label=label;
      this.weight=1;
    }

		/// <summary>
		/// Source,target,label,weight constructor.
		/// </summary>
		/// <param name="source">a non-negative integer</param>
		/// <param name="target">a non-negative integer</param>
		/// <param name="label">any integer</param>
		/// <param name="weight">any integer, or non-negative if you intend to run Chinese Postman</param>
    internal Edge(int source,
                int target,
                int label,
                int weight){this.source=source;
                this.target=target;
                this.label=label;
                this.weight=weight;
    }
		/// <summary>
		/// "source label target"
		/// </summary>
		/// <returns></returns>
    public override string ToString()
    {
      return source.ToString()+" "+label.ToString()+" "+target.ToString();
    }
	
/// <summary>
/// Compares in the lexicographical order touples (source,target,label,weight).
/// </summary>
/// <param name="obj"></param>
/// <returns></returns>
    public int CompareTo(object obj)
    {
      Edge that=obj as Edge;

      if(this.source!=that.source)
        return this.source.CompareTo(that.source);

      if(this.target!=that.target)
        return this.target.CompareTo(that.target);

      if(this.label!=that.label)
        return this.label.CompareTo(that.label);

      return this.weight.CompareTo(that.weight);


    }


  }


	internal class SuccEnumerator : IEnumerator
	{
		IEnumerator edges;

		public void Reset()
		{
			edges.Reset();
		}

		public object Current
		{
			get
        {
          Edge l=edges.Current as Edge;
          return l.target;
        }
		}

		public bool MoveNext()
		{
      return edges.MoveNext();	
		}

		internal SuccEnumerator(IEnumerator edges)
		{
			this.edges=edges;
		}
	}
	internal class PredEnumerator : IEnumerator
	{
		IEnumerator edges;

		public void Reset()
		{
			edges.Reset();
		}

		public object Current
		{
			get
        {
          Edge l=edges.Current as Edge;
          return l.source;
        }
		}

		public bool MoveNext()
		{
			return edges.MoveNext();	
		}

		internal PredEnumerator(IEnumerator edges)
		{
			this.edges=edges;
		}
	}


	
	class Pred :IEnumerable 
	{
#region IEnumerable Members

		Graph graph;

		int vert;
		
		public IEnumerator GetEnumerator()
		{
			IEnumerable e= graph.backwardEdges[vert];
			if(e==null)
        {
          return (new Edge[]{}).GetEnumerator();
        }
			else
				return new PredEnumerator(e.GetEnumerator());

		}

		internal Pred(Graph g,int v) 
		{
			this.graph=g;
			this.vert=v;
		}

#endregion
	}


	
	class Succ :IEnumerable 
	{
        #region IEnumerable Members

		IGraph graph;

		int vert;
		
		public IEnumerator GetEnumerator()
		{
			return new SuccEnumerator (graph.EdgesAtVertex(vert).GetEnumerator());	
		}

		internal Succ(IGraph g,int v) 
		{
			this.graph=g;
			this.vert=v;
		}

        #endregion
	}

 /// <summary>
 /// Some shortest path algorithms of GraphTraversal use this interface.
  /// Class Graph implements IGraph.
 /// </summary>
	internal interface IGraph
  {

    /// <summary>
    /// Vertices should be enumerated from 0 to NumberOfVertices()-1 - the holes are acceptable
    /// </summary>
    int NumberOfVertices{get;}
		/// <summary>
		/// Enumerates through initial vertices
		/// </summary>
		/// <returns></returns>
    IEnumerable InitialVertices();
		/// <summary>
		/// Enumerates through outgoing edges
		/// </summary>
		/// <param name="vertex"></param>
		/// <returns></returns>
    IEnumerable EdgesAtVertex(int vertex);
  
		/// <summary>
    ///Gets the weight of an edge 
    /// </summary>
    /// <param name="vertex"></param>
    /// <returns></returns>
    int GetVertexWeight(int vertex);				

		/// <summary>
		/// Gets the probability of an edge
		/// </summary>
		/// <param name="edge"></param>
		/// <returns></returns>
		double EdgeProbability(Edge edge);

  }


    internal class Lnk
    {
        internal int u;
        internal int v;
        internal int l;
        internal int c;
        internal EdgeKind ek;
        internal Lnk next;
        internal Lnk(int u,
                   int v,
                   int l,
                   int c
                   )
        {
            this.u = u;
            this.v = v;
            this.l = l;
            this.c = c;
        }
        internal Lnk(int u,
                   int v,
                   int l,
                   int c,
                   EdgeKind ek)
        {
            this.u = u;
            this.v = v;
            this.l = l;
            this.c = c;
            this.ek = ek;
        }

        internal Lnk(
                   int u,
                   int v,
                   int l,
                   int c,
                   EdgeKind ek,
                   Lnk next)
        {
            this.u = u;
            this.v = v;
            this.l = l;
            this.c = c;
            this.ek = ek;
            this.next = next;
        }
        internal Lnk(Lnk e)
        {
            this.u = e.u;
            this.v = e.v;
            this.l = e.l;
            this.c = e.c;
            this.ek = e.ek;
        }
    }

  /// <summary>
  /// A graph object has a distinguished start vertex, initVertex, and a set of directed 
  /// labeled and weighted edges. The weights of the edges are integers. 
  /// Graph has optional and must edges. Must edges are taken by Rural Chinese Postman tour and
  /// optional ones can be skipped by it. If you don't use Rural Chinese Postman set optional links to an empty array.
  /// Some vertices can be marked as nondeterministic and a double valued function edgeProbabilites
  /// is defined on edges exiting such vertices.
  /// </summary>
	
  internal class Graph:IGraph
  {
    CHPP_N_M_logN graph;
		
    int initVertex;
		/// <summary>
		/// The initial vertex - usually 0.
		/// </summary>
    internal int InitVertex {
      get{return initVertex;}
      set{initVertex=value;}
    }
		
    internal const int NONE=-1;
    internal const int INF=0x7fFFffFF;


		/// <summary>
		/// Returns a very big integer.
		/// </summary>
    static internal int Infinity {
      get{return INF;}
    }

    
    internal ArrayList[] backwardEdges=null;

    HSet[] nondBackwardNeighbors=null;
		
    //for each nondeterministic vertex 'i'
    // nondNeighbours[i] gives a set of the vertex neighbours
    internal RBMap nondNeighbours=new RBMap();
        
		/// <summary>
		/// Enumerates through the vertex successors.
		/// </summary>
		internal IEnumerable Succ(int v)
		{
			return new Succ(this,v);
		}
		/// <summary>
		/// Enumerates through the vertex predecessors.
		/// </summary>
		internal IEnumerable Pred(int v)
		{
			return new Pred(this,v);
		}
	
		
    internal void InitEdgeProbabilities()
    {
      if(this.nondNeighbours.Count==0 || (edgeProbabilities!=null&&edgeProbabilities.Count>0))
        return;

      edgeProbabilities=new RBMap();
			
      foreach( DictionaryEntry p in this.nondNeighbours)
        {
          int i=(int)p.Key;
          ArrayList edges=EdgesAtVertex(i) as ArrayList;
				
          double pr=1.0/edges.Count;
          foreach(Edge l in edges)
            this.edgeProbabilities[l]=pr;
        }


    }
    /// <summary>
    /// Returns map from V*{0..n} to Strategy
    /// </summary>
    /// <param name="P">the set ov verices to reach</param>
    /// <param name="order">must be an acceptable order 
    /// We call an order O on pairs of real numbers acceptable if 
    /// for any integer m for any real numbers Pi, Ci, P'i, C'i, pi and ci where i changes from 0 to m if 
    /// (pi, ci) less or equal (p'i, c'i) according to O for every i from 0 to m
    ///		pi > 0 for every i from 0 to m and sum{ pi:  0 \leq i \leq n }=1
    ///then (sum{ piPi: 0 \leq i leq n },max{ ci+Ci: 0 \leq i \leq n }) \leq (sum{ piP'i: 0 \leq i \leq n },max{ ci+C'i: 0 \leq i \leq n }) </param>
    /// <param name="maxSteps">maximal number of edges to use for reachin P</param>
    /// <returns>array of strategies Strategy with length equals to the number of vertices</returns>
    internal Strategy[] GetStrategiesToP(IEnumerable P,IOrder order,int maxSteps)
    {
      InitEdgeProbabilities();
      //	InitBackwardEdges();
      StrategyCalculator sc=new StrategyCalculator(this,P,order,maxSteps);
      return sc.Calculate();
    }
    internal class Order:IOrder {
      const float epsilon=0.0000001F;
      
      public bool Less(float a0, int b0, float a1, int b1) {
        if(a0>a1+epsilon)
          return true;
        if(a0<a1-epsilon)
          return false;
        return b0<b1;
      }
    }
    
    //specialized version  
    internal Strategy[] GetStrategiesToP(IEnumerable P,int maxSteps)
    {
      return GetStrategiesToP(P, new Order(),maxSteps);
    }


    internal Strategy[] GetStrategiesToP(int vertex,IEnumerable P,int maxSteps)
    {
      InitEdgeProbabilities();
        
        
      StrategyCalculator sc=new StrategyCalculator(this,P,maxSteps);
      sc.Calculate();
      return sc.GetStrategyReachableFromVertex(vertex);
    }
      

   

    internal int NumberOfEnteringEdges(int i){
      InitBackwardEdges();
      ArrayList al=(ArrayList)backwardEdges[i];
      if(al==null)
        return 0;
      return al.Count;
                
    }
        
    internal void InitBackwardEdges()
    {
#region One time init
      //this block will be executed only once
      if(this.backwardEdges==null)
        {
          this.backwardEdges=new ArrayList[NumberOfVertices];
          this.nondBackwardNeighbors=new HSet[NumberOfVertices];

          for(int i=0;i<NumberOfVertices;i++)
            {  
              bool iIsNond=IsChoicePoint(i);
				
              foreach(Edge l in graph.EdgesAtVertex(i))
                {
                  ArrayList al=this.backwardEdges[l.target] as ArrayList;
                  if(al==null)
                    al=this.backwardEdges[l.target]=new ArrayList();

                  al.Add(l);

                  if(iIsNond)
                    {
                      HSet s=this.nondBackwardNeighbors[l.target];
                      if(s==null)
                        this.nondBackwardNeighbors[l.target]=s=new HSet();
                      s.Insert(i); //i is the edge 'l' start
                    }
                }
            }
								
        }
#endregion 
		
    }


    
		/// <summary>
		///Calculates all vertices from which accepting states are not reachable. 
		/// </summary>
		/// <param name="acceptingStates"></param>
		/// <returns>the set of dead states</returns>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    internal HSet DeadStates(int[] acceptingStates){

      try {

        InitBackwardEdges();
            
        Queue q=new Queue();


        HSet aliveSet=new HSet(acceptingStates);
                
        foreach(int i in acceptingStates)
          if(i<NumberOfVertices)
            q.Enqueue(i);
                

        while(q.Count>0){
                        
          int u=(int)q.Dequeue();
          foreach( int v in Pred(u))
            {
              if(!aliveSet.Contains(v))
                {
                  aliveSet.Insert(v);
                  q.Enqueue(v);
                }
            }				
        }


        HSet deadSet=new HSet();
        //adding the deads
        int n=NumberOfVertices;
        for(int i=0;i<n;i++)
          if(! aliveSet.Contains(i))
            {
              deadSet.Insert(i);
            }
                
        return deadSet;
      }
      catch{
        return new HSet();
      }
 
    }

        
    /*
      Simple Stochastic Parity Games.
      Krishnendu Chatterjee, Marcin Jurdzi¶nski, and Thomas A. Henzinger

      Andreas Blass contributed
    */
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    internal HSet DeadStatesWithoutChangingChoicePoints(int[] acceptingStates){

      try {
        InitBackwardEdges();
            
        Queue q=new Queue();
          
        HSet deadSet=new HSet();
        bool done=false;

        HSet targetSet=new HSet();

        foreach(int i in acceptingStates)
          if(i<NumberOfVertices)
            targetSet.Insert(i);
                
                
        while(!done){

          done=true;
          //alives can reach acceptingStates by not passing through deads
          HSet aliveSet=new HSet(targetSet);
          foreach(int i in targetSet)
            q.Enqueue(i);
  
          while(q.Count>0)
            {
            
              int u=(int)q.Dequeue();
              foreach( int v in Pred(u))
                {
                  if(!aliveSet.Contains(v)&&!deadSet.Contains(v))
                    {
                      aliveSet.Insert(v);
                      q.Enqueue(v);
                    }
                }				
            }


          //adding new deads
          int n=NumberOfVertices;
          for(int i=0;i<n;i++)
            if(! aliveSet.Contains(i) && !deadSet.Contains(i))
              {
                done=false; //we are not done since we've found a new dead
                q.Enqueue(i);
                deadSet.Insert(i);
              }


                
          while(q.Count>0)
            {
              int u=(int)q.Dequeue();
              foreach(int v in Pred(u))
                {
                  if(deadSet.Contains(v)==false&& targetSet.Contains(v)==false)
                    {
                      if(this.IsChoicePoint(v))
                        {  
                          deadSet.Insert(v);
                          q.Enqueue(v);
                        }
                      else {
                        bool isDead=true;
                        foreach(int w in Succ(v))
                          {
                            if(deadSet.Contains(w)==false)
                              {
                                isDead=false;
                                break;
                              }
                          }
                        if(isDead)
                          {
                            deadSet.Insert(v);
                            q.Enqueue(v);
                          }
                      }
                    }

                }
            }
        }

        //add to deadSet everything that cannot be reached from the initial vertex
            
        HSet alSet=new HSet();
				if(NumberOfVertices>0)
					if(!deadSet.Contains(initVertex))
            {
              q.Enqueue(initVertex);
              alSet.Insert(initVertex);
              while(q.Count>0)
                {
                  int u=(int)q.Dequeue();
                  foreach(int v in Succ(u))
                    if(!deadSet.Contains(v))
                      if(!alSet.Contains(v))
                        {
                          alSet.Insert(v);
                          q.Enqueue(v);
                        }
                }                
            }

				for(int i=0;i<NumberOfVertices;i++){
          if(!alSet.Contains(i))
            deadSet.Insert(i);
        }
            
        return deadSet;
      }
      catch{
        return new HSet();
      }
 
    }


 
    
		/// <summary>
		/// removes edges which are not reachable from the initial vertex
		/// </summary>
		/// <param name="edgeToGo">from the vertix i we go to edgeToGo[i]</param>
		/// <param name="source">the sources where we can start the game</param>
    internal void CleanTheStrategy(Edge[] edgeToGo,int[] source){
      Queue q=new Queue();
      foreach(int s in source)
        q.Enqueue(s);
      
      Edge []ls=edgeToGo.Clone() as Edge[];

      while(q.Count>0){
        int v=(int)q.Dequeue();
        if(IsChoicePoint(v))
          foreach(Edge l in EdgesAtVertex(v))
            q.Enqueue(l.target);
        else if(v<ls.Length){
          Edge l=ls[v];
          if(l!=null){
            q.Enqueue(l.target);
            ls[v]=null;
          }
        }
      }

      //eraze everything non-reachable from the source
      for(int i=0;i<ls.Length;i++)
        if(ls[i]!=null)
          edgeToGo[i]=null;

    }

   
		/// <summary>
		/// </summary>
		/// <param name="initialVertex">the initial vertex - usually 0</param>
		/// <param name="mustEdges">the edge considered as must in Chinese Postman route</param>
		/// <param name="optionalEdges">the edges considered as optional in Chinese Postman route</param>
		/// <param name="nondetVertices">the vertices where the system behaves non-deterministically</param>
		/// <param name="closureInstr">this instruction will shuffle some optional edges to must ones. Chinese Rural Postman works only when the set of must links is weakly closed</param>
		internal Graph(int initialVertex, Edge[] mustEdges,Edge[] optionalEdges, int [] nondetVertices,WeakClosureEnum closureInstr)
        :this(initialVertex,mustEdges,optionalEdges,closureInstr)
    {
      this.nondNeighbours=new RBMap();
			
      foreach(int p in nondetVertices)
        {
          if(p<this.NumberOfVertices)
            {
              HSet s=(this.nondNeighbours[p]=new HSet()) as HSet;
              foreach( Edge l in this.graph.EdgesAtVertex(p))										
                s.Insert(l.target);
            }
        }
    }

    internal Graph(int initialVertex, Edge[] mustEdges,Edge[] optionalEdges, int [] nondetVertices,bool builtNondNeigh,WeakClosureEnum closerInstuction)
        :this(initialVertex,mustEdges,optionalEdges,closerInstuction)
    {
      if(builtNondNeigh==true)
        {
          this.nondNeighbours=new RBMap();
			
          foreach(int p in nondetVertices)
            {
              if(p<this.NumberOfVertices)
                {
                  HSet s=(this.nondNeighbours[p]=new HSet()) as HSet;
                  foreach( Edge l in this.graph.EdgesAtVertex(p))										
                    s.Insert(l.target);
                }
            }
        }
      else
        this.nondNeighbours=null;

		
    }
		/// <summary>
		/// </summary>
		/// <param name="initialVertex">the initial vertex - usually 0</param>
		/// <param name="mustEdges">the edge considered as must in Chinese Postman route</param>
		/// <param name="optionalEdges">the edges considered as optional in Chinese Postman route</param>
		/// <param name="closureInstruction">this instruction will shuffle some optional edges to must ones. </param>
    internal Graph(int initialVertex, Edge[] mustEdges,Edge[] optionalEdges, WeakClosureEnum closureInstruction)
    {
      if( Environment.GetEnvironmentVariable("graphtraversaldebug")=="on"){ 
        StreamWriter sw=new StreamWriter("c:/tmp/inputForGraph");
        sw.WriteLine("start vertex");
        sw.WriteLine(initialVertex.ToString());
        sw.WriteLine("must");
        foreach(Edge l in mustEdges){
          sw.WriteLine(l.source.ToString()+" "+l.label+" "+l.target+" "+l.weight);
        }
        sw.WriteLine("optional");
        foreach(Edge l in optionalEdges){
          sw.WriteLine(l.source.ToString()+" "+l.label+" "+l.target+" "+l.weight);
        }

        sw.WriteLine("nond vertices");
                
        sw.Close();
      }
      int n=getMaxVertex(-1,mustEdges,optionalEdges);

      this.initVertex=initialVertex;
			
      //this can change the must edges and optional edges arrays
      if(closureInstruction==WeakClosureEnum.Close)
        CreateWeakClosureForMustEdges(
                                      ref mustEdges,
                                      ref optionalEdges,
                                      n+1);
            

			
      this.graph=new CHPP_N_M_logN(initialVertex, mustEdges,optionalEdges,n+1);			
    }
	
    /// <summary>
    /// The function will shuffle some optional edges to the must edges
    /// in a way that must  edges will create a weakly connected component 
    /// including initVertex. For this function n is already known.
    /// 
    /// </summary>
    /// <param name="mustEdges"></param>
    /// <param name="optionalEdges"></param>
    /// <param name="numberOfVertices"></param>
    void CreateWeakClosureForMustEdges(ref Edge[] mustEdges,
                                       ref Edge[] optionalEdges, int numberOfVertices){
			
      ConnectedComponentDivider divider=new ConnectedComponentDivider(mustEdges,numberOfVertices);

      Edge[][] comps=divider.ConnectedComponents;
            
      ClosureOfConnectedComps closure=new ClosureOfConnectedComps(mustEdges,
                                                                  optionalEdges,
                                                                  numberOfVertices,
                                                                  comps,this.initVertex);

      mustEdges=closure.MustEdges;
      optionalEdges=closure.OptionalEdges;

    }
/// <summary>
/// Edges which have to be taken by the Chinese Postman tour or path.
/// </summary>
    internal Edge[] MustEdges 
    {
      get {return this.graph.MustEdges;}
    }

/// <summary>
/// Edges which don't have to be taken by the Chinese Postman tour or path.
/// </summary>
    internal Edge[] OptionalEdges 
    {
      get {return this.graph.OptionalEdges;}
    }

    internal static int getMaxVertex(int n, 
                                     params IEnumerable[]edgeCollections)
    {
      int ret=n;
      foreach(IEnumerable edges in edgeCollections)
        foreach(Edge e in edges)
        {
          if (e.source>ret)
            ret=e.source;
		
          if (e.target>ret)
            ret=e.target;
        }
      return ret;
    }
	

    static Edge[] LnkToEdge(Lnk edge)
    {
      int n=0;
      Lnk l=edge;
      while(l != null)
        {
          n++;
          l=l.next;
        }
      Edge[] ret=new Edge[n];
      l=edge;
      n=0;
      while(l != null)
        {
          ret[n++]=new Edge(l.u,l.v,l.l,l.c);
          l=l.next;
        }
      return ret;
    }
/// <summary>
/// Returns a tour, a cycle, passing through all must edges and using optional edges if it has to.
/// </summary>
/// <returns></returns>
    internal Edge[] GetRuralChinesePostmanTour()
    {
      return graph.GetRuralChinesePostmanTour();
    }

		
	
    bool EdgeIsNond(Edge l)
    {
      return IsChoicePoint(l.source);
    }

   

    RBMap edgeProbabilities;

    internal RBMap EdgeProbabilities {
      get {return edgeProbabilities;}
      set{edgeProbabilities=value;}
    }

	/// <summary>
	/// Enumerates through the vertex outgoing edges.
	/// </summary>
	/// <param name="i"></param>
	/// <returns></returns>
    public IEnumerable EdgesAtVertex(int i)
    {
      return graph.EdgesAtVertex(i);
    }


    internal int NumberOfEdgesAtVertex(int i)
    {
      return graph.NumberEdgesExitingTheVertex(i);
    }

    internal bool IsChoicePoint(int v)
    {
      if(this.nondNeighbours!=null)
        return this.nondNeighbours.contains(v);
			
      if(this.edgeProbabilities==null)
        return false;

      foreach(Edge l in this.EdgesAtVertex(v))
        return edgeProbabilities.contains(l);

      return false;


    }
		/// <summary>
		/// Returns a path, that is a connected sequence of edges, not necesserely closed in a cycle
		/// containing all graph must edges.
		/// </summary>
		/// <returns></returns>
    internal Edge[] GetRuralChinesePostmanPath()
    {
      return graph.GetRuralChinesePostmanPath();
			
    }

    /// <summary>
    /// Returns a shortest path between the vertices.
    /// </summary>
    internal Edge[] ShortestPath(int start, int end)
    {
      return graph.ShortestPath(start,end);

    }

    static Edge ReadEdge(string s,RBMap probabilities){
      string [] split=s.Split(' ');
      Edge edge=new Edge(Int32.Parse(split[0]), Int32.Parse(split[2]),Int32.Parse(split[1]),Int32.Parse(split[3]));

      if(split.Length==5){

        probabilities[edge]=Double.Parse(split[4]);
        
      }

      return edge;
      

    }

    /// <summary>
    /// Creates a graph from a file.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    internal static Graph CreateFromFile(string fileName)
    {
          
      StreamReader sr=new StreamReader(fileName);
			
      if(sr.BaseStream.CanRead==false)
        {
          sr.Close();
          return null;
        }
			

      sr.ReadLine(); //swallow start vertex            
            
      int initVertex= Int32.Parse(sr.ReadLine());

      ArrayList must=new ArrayList();
      sr.ReadLine();//swallow "must"


      RBMap edgeProbs=new RBMap();
      
      string s=sr.ReadLine();
      while(s!="optional"){

            
        //string [] split=s.Split(' ');
              
        must.Add(ReadEdge(s,edgeProbs));

        s=sr.ReadLine();

      }

            
      //"optional" has been swallowed already

      s=sr.ReadLine();  
      ArrayList opt=new ArrayList();
           
      while(s.StartsWith("nond vertices")==false){
            
              
        opt.Add(ReadEdge(s,edgeProbs));

        s=sr.ReadLine();

      }

      string [] spl=s.Split(' ');
      ArrayList nondVertices=new ArrayList();

      for(int i=2;i<spl.Length;i++){
				if(!String.IsNullOrEmpty(spl[i]))
					nondVertices.Add(Int32.Parse(spl[i]));
      }

          
      Graph g=new Graph(initVertex,must.ToArray(typeof(Edge)) as Edge[],
                        opt.ToArray(typeof(Edge)) as Edge[],
                        nondVertices.ToArray(typeof(int)) as int[],WeakClosureEnum.DoNotClose);
      
      g.edgeProbabilities=edgeProbs;
      return g;
		
    }

/// <summary>
/// Calculates distances from graph vertices to some selected set of vertices; accepting states.
/// </summary>
/// <param name="acceptingStates"></param>
/// <returns></returns>
    internal int[] GetDistanceToAcceptingStates(int[] acceptingStates){

      if(MustEdges==null)
        return new int[0];


      //calculate distances from accepting states
      //by running shortest paths on the reversed graph
      Edge[] reversedEdges=new Edge[MustEdges.Length];

      for(int i=0;i<reversedEdges.Length;i++){
        Edge l=MustEdges[i];
        reversedEdges[i]=new Edge(l.target,l.source,l.label,l.weight);
      }
            
      BasicGraph basicGraph=new BasicGraph(0, reversedEdges);

      MultipleSourcesShortestPaths mssp=new MultipleSourcesShortestPaths(basicGraph,acceptingStates);

      mssp.Calculate();

      //now we have the distance from acceptingStates to any state in mssp.GetDistTo(state)

      int[] ret=new int[NumberOfVertices];
            
      for(int i=0;i<NumberOfVertices;i++)
				ret[i]=mssp.GetDistTo(i);


			return ret;
            
    }

        
    void WriteEdge(StreamWriter sw,Edge l)
    {

      sw.Write(l.source.ToString()+" "+l.label.ToString()+" "+l.target.ToString()+" "+l.weight.ToString());
      if(this.edgeProbabilities!=null)
        {
          object o=edgeProbabilities[l];
		
          if(o!=null)			
            sw.Write(" {0}",o);
        
        }

      sw.WriteLine("");

			
    }

		/// <summary>
		/// Writes the graph to a file.
		/// </summary>
		/// <param name="fileName"></param>
    internal void ToFile(string fileName)
    {
      StreamWriter sw=new StreamWriter(fileName);
			
      if(sw.BaseStream.CanWrite==false)
        {
          sw.Close();
          return;
        }
			

      sw.WriteLine("start vertex");
      sw.WriteLine(this.initVertex.ToString());

      sw.WriteLine("must");
      if(this.MustEdges!=null)
        {

          foreach(Edge l in this.MustEdges)
            WriteEdge(sw,l);
        }
			    
      sw.WriteLine("optional");
      if(this.OptionalEdges!=null)
        {

          foreach(Edge l in this.OptionalEdges)
            WriteEdge(sw,l);
        }

      sw.Write("nond vertices ");
      for(int i=0;i<NumberOfVertices;i++)
        {
          if(this.IsChoicePoint(i))
            sw.Write(i.ToString()+" ");
 
        }

      sw.WriteLine("");

      sw.Close();
			

    }
#region IGraph Members
/// <summary>
/// The number of vertices in the graph - actually the max +1 of vertices.
/// It should be taken in to account; do not add edges as new Edge(10000000000,3).
/// The program would go out of memory.
/// </summary>
    public int NumberOfVertices {
      get {
				
        return graph.NumberOfVertices;
      }
    }

		/// <summary>
		/// Enumerates through initial vertices.
		/// </summary>
		/// <returns></returns>
    public IEnumerable InitialVertices() {
			
      return graph.InitialVertices();
    }

		/// <summary>
		/// Gets the vertex weight.
		/// </summary>
		/// <param name="vertex"></param>
		/// <returns></returns>
    public int GetVertexWeight(int vertex) {
			
      return graph.GetVertexWeight(vertex);
    }

		/// <summary>
		/// Gets the edge probability.
		/// </summary>
		/// <param name="edge"></param>
		/// <returns></returns>
    public double EdgeProbability(Edge edge) {

      if(this.edgeProbabilities==null)
        return 1.0;

      object o=this.edgeProbabilities[edge];
      if(o==null)
        return 1.0;
      

      return (double)o;
      
    }
    
		/// <summary>
		/// Sets edge probability.
		/// </summary>
		/// <param name="edge">the edge</param>
		/// <param name="prob">the probability: a number between 0 and 1</param>
		internal void SetEdgeProbability(Edge edge,double prob) 
		{

			if(this.edgeProbabilities==null)
			{
				edgeProbabilities=new RBMap();
			}

			edgeProbabilities[edge]=prob;
      
		}



#endregion


    void GetReachableFromVertex(int i,HSet covered, int stepsLeft,HSet result,bool skipChoicePoints,int totalSteps)
		{
      if(stepsLeft==0 || covered.Contains(i))
        return;
      covered.Insert(i);

      foreach(Edge l in this.EdgesAtVertex(i)){
        result.Insert(l);
        if(skipChoicePoints==false)
          GetReachableFromVertex(l.target,covered,
                               stepsLeft-1,result,skipChoicePoints,totalSteps);
        else if (this.IsChoicePoint(l.target)==false)
          GetReachableFromVertex(l.target,
                               covered,stepsLeft-1,result,skipChoicePoints,totalSteps);
        else //choice point
          GetReachableFromVertex(l.target,covered,
                               totalSteps,result,skipChoicePoints,totalSteps);


      }
    }
    /// <summary>
    /// Returns the set of vertices reachable from the given initial vertex for no more than n steps.
    /// </summary>
    /// <param name="vertex">an initial vertex to start the search</param>
    /// <param name="steps">limit on the depth</param>
    /// <param name="skipChoicePoints">This parameter influences the definition of a non-reachable state;
    /// if it is true then a  state is unreachable in n steps if there is a strategy on choice points ensuring that.</param>
    /// <returns>set of vertices</returns>
    internal HSet GetReachableEdges(int vertex,int steps,bool skipChoicePoints){
      HSet result=new HSet();
      HSet covered=new HSet();
      GetReachableFromVertex(vertex,covered,steps,result,skipChoicePoints,steps);

      return result;
    }



    //internal double[] GetDistancesToAcceptingStates(int[] sources,int[] acceptingStates)
    internal double[] GetDistancesToAcceptingStates(int[] acceptingStates)
    {
        if(MustEdges==null)
          return null;


      //calculate distances from accepting states
      //by running shortest paths on the reversed graph
      Edge[] reversedEdges=new Edge[MustEdges.Length];

      for(int i=0;i<reversedEdges.Length;i++){
        Edge l=MustEdges[i];
        reversedEdges[i]=new Edge(l.target,l.source,l.label,l.weight);
      }
      
      BasicGraph basicGraph=new BasicGraph(0, reversedEdges);
      
      MultipleSourcesShortestPaths mssp=new MultipleSourcesShortestPaths(basicGraph,acceptingStates);
      
      mssp.Calculate();
      double[] exps=new double[NumberOfVertices];
      for(int i=0;i<NumberOfVertices;i++)
          exps[i]=mssp.GetDistTo(i); 
      

      return exps;
    }

    //in fact these this function return distances but not the expectations
    internal EdgesAndExpectations GetStaticStrategyWithDistancesToAcceptingStates(int[] sources,int[] acceptingStates){


        double[] exps = GetDistancesToAcceptingStates(acceptingStates); //GetDistancesToAcceptingStates(sources, acceptingStates);

      //now we have the distance from acceptingStates to any state in mssp.GetDistTo(state)

      Edge[] ret=new Edge[NumberOfVertices];

      
      for(int i=0;i<NumberOfVertices;i++){
        if(IsChoicePoint(i))
          continue;


				if(exps[i]==0)
					continue; //it is an accpting state

        double minDist=Int32.MaxValue;

        foreach(Edge l in EdgesAtVertex(i)){
          if( exps[l.target]<minDist){
            minDist=exps[l.target];
            ret[i]=l;
          }
        }
      }


      CleanTheStrategy(ret,sources) ;

      for(int i=0;i<NumberOfVertices;i++)
        if(ret[i]==null)
          exps[i]=0;
     
			return new EdgesAndExpectations(ret,exps);
            
    }


  }	

}
