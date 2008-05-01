//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections;

namespace NModel.Algorithms.GraphTraversals
{
	/// <summary>
	/// Summary description for BasicGraph.
	/// </summary>
	internal class BasicGraph:IGraph
	{
		int numberOfVertices;
		ArrayList [] linksByVertices;
		int initVertex;
		


		/// <summary>
		/// 
		/// </summary>
		/// <param name="initVertex"></param>
		/// <param name="links"></param>
		internal BasicGraph(int initVertex,params ICollection[] links)
		{
			this.initVertex=initVertex;

			foreach(ICollection ls in links)
		    	this.numberOfVertices=Graph.getMaxVertex(this.numberOfVertices,ls);

			this.numberOfVertices++;

			this.linksByVertices=new ArrayList[numberOfVertices];

			foreach(ICollection ls in links)
				foreach(Edge l in ls){
					int node=l.source;
					if(linksByVertices[node]==null)
						linksByVertices[node]=new ArrayList();
					
					linksByVertices[node].Add(l);
				}
			
		}

    void DFS(int node,bool []discovered){
      discovered[node]=true;
      foreach(Edge l in EdgesAtVertex(node)){
        if(discovered[l.target]==false)
          DFS(l.target,discovered);
      }
    }
    
    internal bool[] GetReachableArray(int nodeToStart){
       bool [] discovered=new bool[this.NumberOfVertices];
       DFS(nodeToStart,discovered);

       return discovered;
    }
  
		#region IGraph Members

		public int NumberOfVertices {
			get {
				return this.numberOfVertices;
			}
		}

		public IEnumerable InitialVertices() {
			
			return new int[]{this.initVertex};
		}

		public IEnumerable EdgesAtVertex(int node) {			
			if( this.linksByVertices[node]!=null)
				return this.linksByVertices[node];

			return new Edge[]{};
		}

		public int GetVertexWeight(int node) {
			return 1;
		}

		public double EdgeProbability(Edge link) {
			return 1; 
		}

		#endregion
	}
}
