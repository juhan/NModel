//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;


namespace NModel.Algorithms.GraphTraversals
{
	class HeapElem
	{
		internal int indexToA;
		internal int priority;  
		internal int v;//value
		internal HeapElem(int index,int priority,int v)
		{
			this.indexToA=index; 
			this.priority=priority;
			this.v=v;
		}
	}
	
	/// <summary>
	/// Summary description for IntBinaryHeapPriorityQueue.
	/// </summary>
	internal struct Elem 
	{
		internal int v;
		internal int priority;
		internal Elem(int v,int pr){ this.v=v;this.priority=pr;}

	}
	internal class IntBinaryHeapPriorityQueue
	{
		//indexing for A starts from 1
	
		internal void Clear()
		{
			if(heapSize>0)
			{
				for(int i=0;i<cache.Length;i++)
					this.cache[i]=null;

				heapSize=0; 
			}

		}
		
		internal Elem Elem(int i){return new Elem(A[i].v,A[i].priority);} 
		HeapElem[] A;//array of heap elements
		/// <summary>
		/// cache[k]=A[cache[k].indexToA] this is the invariant
		/// </summary>
		HeapElem[] cache;
		internal int Count {get{return heapSize;}}
		int heapSize;
		/// <summary>
		/// the constructor
		/// we suppose that all integers inserted into the queue will be less then n
		/// </summary>
		/// <param name="n">it is the number of different integers taht will be inserted into the queue </param>
		internal IntBinaryHeapPriorityQueue(int n) 
		{
			//System.Diagnostics.Debug.WriteLine("constructed");

			cache=new HeapElem[n];
			A=new HeapElem[n+1];//because indexing for A starts from 1
			heapSize=0;
		}   


		void SwapWithParent(int i)
		{
			HeapElem parent=A[i>>1];
           
			putAtI(i>>1,A[i]);
			putAtI(i,parent);
		}

		internal void insert(int o, int priority)
		{
			//System.Diagnostics.Debug.WriteLine("insert "+ o.ToString() + " with pr "+ priority.ToString());

			heapSize++;
			int i=heapSize;
			HeapElem h;
			if(cache[o]!=null)
			{
				throw new InvalidOperationException("the element already in the queue");
			}
			A[i]=cache[o]=h=new HeapElem(i,priority,o);
			while(i>1 && A[i>>1].priority>priority)
			{
				SwapWithParent(i);
				i>>=1;
			}
			A[i]=h;
			
		}

		internal bool isEmpty()
		{
			return heapSize==0;
		}

		void putAtI(int i,HeapElem h)
		{
			A[i]=h;
			h.indexToA=i;
		}

		internal int del_min()
		{
			if(heapSize==0)
				throw new InvalidOperationException("del_min on empty IntBinaryPriorityQueue");

			int ret= A[1].v;
		
			cache[ret]=null;
            
//			System.Diagnostics.Debug.WriteLine("del_min "+ ret.ToString()+" with prio "+A[1].priority.ToString() );

			putAtI(1,A[heapSize]);
			int i=1;
			int l,r;
			int smallest;
			while(true)
			{
				smallest=i;
				l=i<<1;
				
				if(l<=heapSize && A[l].priority<A[i].priority)				
					smallest=l;
				
				r=l+1;
				
				if(r<=heapSize && A[r].priority<A[smallest].priority)				
					smallest=r;

				if(smallest!=i)
					SwapWithParent(smallest);  
				else
					break;

				i=smallest;

			}
		    


			heapSize--;

			return ret;

		}

		/// <summary>
		/// sets the object priority to c
		/// </summary>
		internal void decrease_priority(int o,int newPriority)
		{
		    
			//System.Diagnostics.Debug.WriteLine("delcrease "+ o.ToString()+" to "+ newPriority.ToString());

			HeapElem h=cache[o];
			h.priority=newPriority;
			int i=h.indexToA;
			while(i>1)
			{
				if( A[i].priority<A[i>>1].priority)
					SwapWithParent(i);
				else
					break;
				i>>=1;
			}
		
		}
	}
}


