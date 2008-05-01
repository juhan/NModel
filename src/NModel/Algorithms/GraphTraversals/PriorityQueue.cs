//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;

using System.Collections;

namespace NModel.Algorithms.GraphTraversals 
{
	/// <summary>
	/// Summary description for PriorityQueue.
	/// </summary>
	internal class PriorityQueue
	{
		class PQMember:IComparable 
		{
			internal int o;
			internal int priority;
			internal PQMember(int o,int priority)
			{
				this.o=o;
				this.priority=priority;
			}
			public int CompareTo(object obj)
			{
                PQMember qm = obj as PQMember;
				if (qm == null)
					throw new ArgumentException ("obj must be of type 'PQMember'");
				int v=priority-qm.priority;
				if (v!=0)
					return v;
				
				return this.o-qm.o;

			}
		}
		RBTree tree;

		internal PriorityQueue()
		{
		     tree=new RBTree();		
		 	
		}   

		internal void insert(int o, int priority)
		{
			PQMember pqm=new PQMember(o,priority);
		
			tree.insertIfUniq(pqm);
		}

		internal bool isEmpty()
		{
			return tree.isEmpty();
		}

		internal int del_min()
		{
			if( isEmpty())
				throw new InvalidOperationException("deleting from an empty queue");

			RBNode node=tree.treeMinimum();
			tree.deleteTree(node);
			return (node.item as PQMember).o;

		}

		/// <summary>
		/// sets the object priority to c
		/// </summary>
		internal void set_priority(int o,int oldPriority,int newPriority)
		{
			PQMember pqm=new PQMember(o,oldPriority);
			
			RBNode node=tree.delete(pqm);

			if(node==tree.NIL)
			{
				throw new InvalidOperationException("in set_priority");
			}

			pqm.priority=newPriority;

			tree.insertIfUniq(pqm);
		
		}
	}

}
