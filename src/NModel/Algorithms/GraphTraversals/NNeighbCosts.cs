using System;


namespace Microsoft.Algorithms.GraphTraversals
{
	/// <summary>
	/// Keeps map M link -> link.weight+Weight(link.target,i) after i-th iteration
	/// Calculates max(M)
	/// </summary>
	public class NNeighbCosts
	{
		RBMap linksToWeights=new RBMap();
//		class Pair: IComparable
//		{
//
//			public Link link;
//			public Rational cost;
//
//			#region IComparable Members
//
//			public int CompareTo(object obj)
//			{
//				Pair p=obj as Pair;
//				if(p.cost<cost)
//					return 1;
//
//				if(p.cost>cost)
//					return -1;
//
//				return p.link.CompareTo(link);
//
//			}
//
//			#endregion
//		}

		RBTree costs=new RBTree();
		public void InsertOrUpdate(Link l, int cost)
		{
			object o=linksToWeights[l];
			
			if(o!=null)
			{
				int oldCost=(int)o;
				if(oldCost==cost)
					return;
				costs.delete(oldCost);
			}
 
			linksToWeights[l]=cost;
			costs.insert(cost);
	
		}

		public int GetMaxCost 
		{
			get 
			{
				RBNode node=costs.treeMaximum();
				return (int)node.item;
			}
		}

		public NNeighbCosts()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
