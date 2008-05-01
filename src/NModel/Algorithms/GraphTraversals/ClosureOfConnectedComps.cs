//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections;

namespace NModel.Algorithms.GraphTraversals
{
	/// <summary>
	/// calculates the new mustLinks and optionalLinks arrays
	/// </summary>
	internal class ClosureOfConnectedComps
	{
	
		Edge[] mustEdges;
		Edge[] optionalEdges;
		int numberOfVertices;
		Edge[][] comps;

		Edge[] newML=null;
		Edge[] newOL=null;
     
		int initVertex;

		bool [] joinedComps;//characteristical function of joined comps
		int []nodesToCompIds; //map from nodes to their component ids
        Hashtable linksToCompIDs;

		int closureID;
		
		internal ClosureOfConnectedComps(
                                       Edge[] mustEdges,
                                       Edge[] optionalEdges, 
                                       int numberOfVertices, 
                                       Edge[][] comps,
                                       int initVertex)
		{
			this.mustEdges=mustEdges;
			this.optionalEdges=optionalEdges;
			this.numberOfVertices=numberOfVertices;
			this.comps=comps;
			this.initVertex=initVertex;
			linksToCompIDs=new Hashtable(mustEdges.Length+optionalEdges.Length);

			for(int i=0;i<comps.Length;i++)
				foreach(Edge link in comps[i])
					linksToCompIDs[link]=i;
				
			this.closureID=comps.Length;

		}

		
		
		void Process(){

            if(this.numberOfVertices==0)
                return;

            if(this.mustEdges==null || this.mustEdges.Length==0){
                newML=new Edge[0];
				newOL=this.optionalEdges;
                return;
            }
			//first check with the hope that we are fine
			//that means that we have only one weakly 
			//connected component containing the initVertex
			if(comps.Length==1){
				foreach(Edge link in comps[0])
					if(this.initVertex==link.target||this.initVertex==link.source){
						newML=this.mustEdges;
						newOL=this.optionalEdges;
						return;
					}
			}

			//connect each component with the initial vertex
	
			//nothing is joined at the beginning
			joinedComps=new bool[comps.Length];


#region organizing the node to component id map
			this.nodesToCompIds=new int[this.numberOfVertices];

			for(int i=0;i<nodesToCompIds.Length;i++)
				nodesToCompIds[i]=-1;  //does not belong to any component

			for(int compID=0;compID<comps.Length;compID++)
				foreach(Edge link in comps[compID])
					nodesToCompIds[link.source]=nodesToCompIds[link.target]=compID;

			//if initVertex does not belong to any components mark it as belonging to the 
			//bigger component
			if(this.nodesToCompIds[this.initVertex]==-1)
				this.nodesToCompIds[this.initVertex]=this.closureID;
			else 
				this.joinedComps[this.nodesToCompIds[this.initVertex]]=true;
			

#endregion


			SingleSourceShortestPaths shortestPaths=
				new SingleSourceShortestPaths(
                                              new BasicGraph(this.initVertex,this.mustEdges, this.optionalEdges),
                                              this.initVertex);
	

			shortestPaths.Calculate();
			

			//use the spanning tree of the SingleSourceShortestPaths
			//to connect the components with initVertex
			
            if(this.nodesToCompIds[this.initVertex]==-1) 
                this.nodesToCompIds[this.initVertex]=this.closureID;
		
			bool done=false;
            //add the first component - may be is the only one
            AddComponent(0,shortestPaths);
			while(!done){

				done=true;
				
				//find non-joined component
				for(int compID=0;compID<comps.Length;compID++){
					if( joinedComps[compID]==false){
						done=false;
						AddComponent(compID,shortestPaths);
						continue;    
					}
				
				}
			}

			this.newML=new Edge[linksToCompIDs.Count];
			
			this.linksToCompIDs.Keys.CopyTo(this.newML,0);

			this.newOL=new Edge[this.optionalEdges.Length-newML.Length+mustEdges.Length];

			int j=0;
			foreach(Edge l in optionalEdges)
				if(linksToCompIDs.Contains(l)==false)
					this.newOL[j++]=l;
           

		}

		

        //add links to the components connecting it with the initVertex
    
		void AddComponent(int compID, SingleSourceShortestPaths shortestPaths){


			this.joinedComps[compID]=true;

			//take a node clsosest to the initVertex
            int node=-1;
            int dist=Int32.MaxValue;
            foreach(Edge l in this.comps[compID]){
                if(shortestPaths.GetDistTo(l.source)<dist){
                    dist=shortestPaths.GetDistTo(l.source);
                    node=l.source;
                }
                
                if(shortestPaths.GetDistTo(l.target)<dist){
                    dist=shortestPaths.GetDistTo(l.target);
                    node=l.target;
                }                    
                
            }

			//find a link entering the component from the outside 
			Edge link=null;
			while(this.nodesToCompIds[node]==compID){
                if(node==this.initVertex) //this component is already connected to the initVertex
                    return;
        
				link=shortestPaths.Pred[node];
				node=link.source;
			}

			while(true) {
				this.linksToCompIDs[link]=this.closureID;
				

				int sourceCompID=nodesToCompIds[link.source];
				if(sourceCompID==-1){
					link=shortestPaths.Pred[link.source];
					nodesToCompIds[link.target]=closureID;
				}
				else if(sourceCompID==closureID){
					break;
				}
				else {//we hit a component from comps
					this.joinedComps[sourceCompID]=true;
					break; 
				}
			}

		}

		internal Edge[] OptionalEdges {
			get {
				if( this.newOL==null)
					Process();
				return this.newOL;
			}
		}

		internal Edge[] MustEdges {
			get {
				if( this.newML==null)
					Process();
				return this.newML;
			}
		}
	}
}
