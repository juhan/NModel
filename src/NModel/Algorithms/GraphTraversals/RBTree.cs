//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------

using System;
using System.Text;
using System.Collections;


namespace NModel.Algorithms.GraphTraversals
{
	internal class RBTreeEnumerator:IEnumerator 
	{
		bool initialState;
		RBTree tree;
		RBNode c;
		public object Current
		{
			get {return c.item;}
		}
		public void Reset()
		{
			initialState=true;
		}
		
		public bool MoveNext()
		{
			if(tree.isEmpty())
				return false;

			if(initialState==true)
			{
				initialState=false;
				c=tree.treeMinimum();
			}
			else 
			{
				c=tree.treeSuccessor(c);			    
			}
			return c!=tree.NIL;
		}

		internal RBTreeEnumerator(RBTree tree)
		{
			this.tree=tree;
			Reset();

		}
	}


	[Serializable]
	internal class RBNode 
	{
		
		internal RBTree.Color color;
		internal object /*IComparable*/ item;
		internal RBNode p,left,right;
		internal RBNode(RBTree.Color color){this.color=color;}
		internal RBNode(RBTree.Color color, IComparable item, RBNode p, RBNode left, RBNode right)
		{
			this.color=color;
			this.p=p;
			this.left=left;
			this.right=right;
			this.item=item;
		}
		internal RBNode(){}

	}

	[Serializable]
	internal class RBTree:IEnumerable
	{

		public IEnumerator GetEnumerator(){return new RBTreeEnumerator(this);}
		internal enum Color
		{
			R,
			B
		}

		
		internal RBNode NIL=new RBNode(Color.B);

		internal RBNode root;
  

		internal RBNode treeSuccessor(RBNode x)
		{
			if(x.right!=NIL)
				return treeMinimum(x.right);
			RBNode y=x.p;
			while(y!=NIL && x==y.right)
			{
				x=y;
				y=y.p;
			}
			return y;
		}

		internal RBNode treeMinimum(RBNode x)
		{
			while(x.left!=NIL)
				x=x.left;
			return x;
		}
		
		internal RBNode treeMinimum()
		{
			return treeMinimum(root);
		}

	
		internal RBNode treeMaximum(RBNode x)
		{
			while(x.right!=NIL)
				x=x.right;
			return x;
		}
	
		internal RBNode treeMaximum()
		{
			return treeMaximum(this.root);
		}
	

		internal RBTree Clone()
		{
            RBTree clone=new RBTree();
			foreach(IComparable n in this)
			{
				clone.insert(n);
			}
			return clone;
		}


		public override string ToString()
		{
            StringBuilder ret = new StringBuilder("{");
			int i=0;
			foreach(IComparable p in this)
			{
				ret.Append(p.ToString());
				if(i != this.Count-1)
				{
					ret.Append(",");
				}

				i++;
			}
            ret.Append("}");
            return ret.ToString();
		}


		internal RBNode deleteTree(RBNode z)
		{
			if(z==NIL)
				throw new ArgumentException("z cannot be NIL");
			RBNode y,x;
			if (z.left == NIL || z.right == NIL) 
			{
				/* y has a NIL node as a child */
				y = z;
			} 
			else 
			{
				/* find tree successor with a NIL node as a child */
				y = z.right;
				while (y.left != NIL) y = y.left;
			}

			/* x is y's only child */
			if (y.left != NIL)
				x = y.left;
			else
				x = y.right;
           
			x.p=y.p;
			if(y.p==NIL)
				root=x;
			else 
			{
				if(y==y.p.left)
					y.p.left=x;
				else
					y.p.right=x;
			}
			if(y!=z)
				z.item=y.item;
			if(y.color==Color.B)
				deleteFixup(x);

			//	checkTheTree();

			return y;
			
		}

		int count=0;

		internal int Count 
		{
			get {return count;}
		}

		internal RBNode delete(IComparable i)
		{
			RBNode n=find(i);
			if(n!=NIL)
			{
				count--;
				return deleteTree(n);
			}
			return NIL;
		}

		internal RBNode find(RBNode x, IComparable i)
		{
			while(x!=NIL && i.CompareTo(x.item)!=0)
				x=i.CompareTo(x.item)<0?x.left:x.right;

			return x;
		}

		internal RBNode find(IComparable i)
		{
			return find(root,i);
		}

		internal bool contains(IComparable i)
		{
			return find(i)!=NIL;
		}

		void deleteFixup(RBNode x)
		{
			while(x!=root && x.color==Color.B)
			{
				if(x==x.p.left)	
				{
					RBNode w=x.p.right;
					if(w.color==Color.R)					
					{
						w.color=Color.B;
						x.p.color=Color.R;
						LeftRotate(x.p);
						w=x.p.right;
					}
					if(w.left.color==Color.B && w.right.color==Color.B)	
					{
						w.color=Color.R;
						x=x.p;	
					}
					else 
					{
						if (w.right.color==Color.B)
						  {
							  w.left.color=Color.B;
							  w.color=Color.R;
							  RightRotate(w);
							  w=x.p.right; 
						  }
						w.color=x.p.color;
						x.p.color=Color.B;
						w.right.color=Color.B;
						LeftRotate(x.p);
						x=root;}
				}
				else 
				{
					RBNode w=x.p.left;
					if(w.color==Color.R)
					{
						w.color=Color.B;
						x.p.color=Color.R;
						RightRotate(x.p);
						w=x.p.left;					
					}
					if(w.right.color==Color.B && w.left.color==Color.B)					
					{
						w.color=Color.R;
						x=x.p;					
					}
					else 					
					{
						if (w.left.color==Color.B)						  
						{
							w.right.color=Color.B;
							w.color=Color.R;
							LeftRotate(w);
							w=x.p.left;						  
						}
						w.color=x.p.color;
						x.p.color=Color.B;
						w.left.color=Color.B;
						RightRotate(x.p);
						x=root;					
					}				
				}
			}
			x.color=Color.B;
		}
        RBNode TreeInsertIfUniq(IComparable z) //TreeInsertIfUniq(RBNode node, IComparable z)
		{ 
			RBNode y=NIL;
			RBNode x=this.root;
			
			while(x!=NIL)
			{
				y=x;
				int comp=(x.item as IComparable).CompareTo(z);
                if (comp==0)
					return NIL;
				x=comp>0? x.left:x.right;			
			}
		
			RBNode nz=new RBNode(Color.B,z,y,NIL,NIL);
        
			if(y==NIL)
				root=nz;
			else if ( z.CompareTo(y.item)<0)
				y.left=nz;
			else 
				y.right=nz;

			return nz;         
		}
  
		internal bool isEmpty(){return root==NIL;}
        RBNode TreeInsert(IComparable z) //RBNode TreeInsert(RBNode node, IComparable z)
		{ 
			RBNode y=NIL;
			RBNode x=this.root;
			while(x!=NIL)
			{
				y=x;
				x=(x.item as IComparable).CompareTo(z)>0? x.left:x.right;			
			}
			RBNode nz=new RBNode(Color.B,z,y,NIL,NIL);
        
			if(y==NIL)
				root=nz;
			else if ( z.CompareTo(y.item)<0)
				y.left=nz;
			else 
				y.right=nz;

			return nz;         
		}


		void insertPrivate(RBNode x)
		{
			count++;
			x.color=Color.R;
			while(x!=root && x.p.color==Color.R)
			{
				if(x.p==x.p.p.left)
				{
					RBNode y=x.p.p.right;
					if(y.color==Color.R)
					{
						x.p.color=Color.B;
						y.color=Color.B;
						x.p.p.color=Color.R;
						x=x.p.p;
					}
					else
					{
						if  (x==x.p.right)
						{
							x=x.p;
							LeftRotate(x);
						}
						x.p.color=Color.B;
						x.p.p.color=Color.R;
						RightRotate(x.p.p);
					}        
				}
				else
				{
					RBNode y=x.p.p.left;
					if(y.color==Color.R)
					{
						x.p.color=Color.B;
						y.color=Color.B;
						x.p.p.color=Color.R;
						x=x.p.p;
					}
					else
					{
						if  (x==x.p.left)
						{
							x=x.p;
							RightRotate(x);
						}
						x.p.color=Color.B;
						x.p.p.color=Color.R;
						LeftRotate(x.p.p);
					}        
				}
                
			}
			
			root.color=Color.B;
    
			//checkTheTree();
		}

		internal void insertIfUniq(IComparable v)
		{
            RBNode x = TreeInsertIfUniq(v); //TreeInsertIfUniq(root,v);
			if(x==NIL)
				return;

			insertPrivate(x);

		}		

		internal void insert(IComparable v)
		{
            RBNode x = TreeInsert(v);//TreeInsert(root,v);
			insertPrivate(x);
		}

		void LeftRotate(RBNode x)
		{       
			RBNode y=x.right;
			x.right=y.left;
			if(y.left!=NIL)
				y.left.p=x;
			y.p=x.p;
			if(x.p==NIL)
				this.root=y;
			else if (x==x.p.left)
				x.p.left=y;
			else
				x.p.right=y;
    
			y.left=x;
			x.p=y;
    
		}
		void RightRotate(RBNode x)
		{       
			RBNode y=x.left;
			x.left=y.right;
			if(y.right!=NIL)
				y.right.p=x;
			y.p=x.p;
			if(x.p==NIL)
				this.root=y;
			else if (x==x.p.right)
				x.p.right=y;
			else
				x.p.left=y;
    
			y.right=x;
			x.p=y;
    
		}

		internal RBTree()
		{
			root=NIL=new RBNode(Color.B);      
		}
		void checkTheTree()
		{
			int blacks=-1;
			checkBlacks(root,0,ref blacks);
			checkColors(root);

		}

		

		void checkBlacks(RBNode node, int nnow, ref int blacks)
		{
			if (node!=NIL)
			{
				RBNode l=node.left,r=node.right;
                
				if(l!=NIL)
					if(( l.item as IComparable).CompareTo(node.item)>=0)
						throw new InvalidOperationException("order");

				if(r!=NIL)
					if( (r.item as IComparable).CompareTo(node.item)<=0)
                        throw new InvalidOperationException("order");


			}

			if(node==NIL)
			{
				if (blacks!=nnow+1 && blacks!=-1)
					throw new InvalidOperationException("blacks");
				
				blacks=nnow+1; 
				
			}
			else if(node.color==Color.B)
			{
				checkBlacks(node.left,nnow+1,ref blacks);
				checkBlacks(node.right,nnow+1,ref blacks);
			}
			else 
			{
				checkBlacks(node.left,nnow,ref blacks);
				checkBlacks(node.right,nnow,ref blacks);
			}


		}

		void checkColors(RBNode x)
		{
			if(x==NIL)
				return;
			//property 3
			if(x.color==Color.R)
				if(x.left.color==Color.R || x.right.color==Color.R)
					throw new InvalidOperationException("colors");

			checkColors(x.left);
			checkColors(x.right);
		}
	}
}
