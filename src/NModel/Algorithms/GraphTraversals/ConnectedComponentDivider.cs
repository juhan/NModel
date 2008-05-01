//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections;

namespace NModel.Algorithms.GraphTraversals
{

	internal class ConnectedComponentDivider
	{
		//links representing the graph
		Edge[] links;
		//array of components ; each component is RBSet of links
		ArrayList comps=new ArrayList();

		int currentComponentID;


		//maps a node to links incident to it - directions does not matter
		ArrayList[] linksByVertices;

		int[] linksToCompIDs;

		int numberOfVertices;

		bool []coveredVertices=null;

		internal ConnectedComponentDivider(Edge[] links, int numberOfVertices)
		{
			//hopefully puts everything to false
			this.coveredVertices=new bool[numberOfVertices];

			int i;
			for(i=0;i<numberOfVertices;i++){
				this.coveredVertices[i]=false;
			}

			this.numberOfVertices=numberOfVertices;
		    this.links=links;
  
			//ArrayList comps=new ArrayList();
			//map from links to their adjacent links
	        linksByVertices=new ArrayList[numberOfVertices];
			
			
			i=0;
			foreach(Edge l in links){
				ArrayList al=linksByVertices[l.source] as ArrayList;
				if(al==null)
					linksByVertices[l.source]=al=new ArrayList();
				al.Add(i);
				al=linksByVertices[l.target] as ArrayList;
				if(al==null)
					linksByVertices[l.target]=al=new ArrayList();
				al.Add(i);			

				i++;

			}


			this.linksToCompIDs=new int[this.links.Length];

	        for(i=0;i<linksToCompIDs.Length;i++)
				linksToCompIDs[i]=-1; //-1 means non discovered yet


			this.currentComponentID=0;

		}


	

		void Divide(){
			for(int i=0;i<this.numberOfVertices;i++){
				if(this.coveredVertices[i]==false && this.linksByVertices[i]!=null ){
			
					DiscoverOneComponent(i);
                    this.comps.Add(this.currentComponentID);
					this.currentComponentID++;
				}
			}

			this.connectedComponents=new Edge[this.currentComponentID][];

			int []counts=new int[this.currentComponentID];
			foreach(int i in this.linksToCompIDs)
				if(i!=-1)
				   counts[i]++;
           
			for(int i=0;i<counts.Length;i++){
				this.connectedComponents[i]=new Edge[counts[i]];
			}

			for(int i=0;i<this.linksToCompIDs.Length;i++){

				int compID=this.linksToCompIDs[i];

				if(compID!=-1)
				  this.connectedComponents[compID][--(counts[compID])]=this.links[i];
			}



		}


        void DiscoverOneComponent(int node){
            Stack stack=new Stack();

            stack.Push(node);

            while (stack.Count>0)
            {

                node=(int)stack.Pop();

                coveredVertices[node]=true;
                
                foreach( int linkIndex in this.linksByVertices[node]){

                    if(linksToCompIDs[linkIndex]==-1){//the component id is not set yet

                        Edge link=this.links[linkIndex];
                        
                        stack.Push(link.source==node?link.target:link.source);

                        linksToCompIDs[linkIndex]=currentComponentID;
                                                
                    }
                }

           }
            
        }

		Edge[][] connectedComponents=null;
	
		internal Edge[][] ConnectedComponents {

			get{ 
				if(this.currentComponentID==0)
					 Divide();
				 
				return connectedComponents;
			}
		}
	}
}
