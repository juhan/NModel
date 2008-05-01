//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Text;
using System.Collections;

namespace NModel.Algorithms.GraphTraversals
{
	/// <summary>
	/// A set implementation using Hashtable.
	/// </summary>
	internal class HSet: MarshalByRefObject, ICollection
	{
		Hashtable table=new Hashtable();
		/// <summary>
		/// Insert an element; does not have effect if the element is already inside.
		/// </summary>
		/// <param name="o"></param>
		internal void Insert(object o){ 
			if( table.ContainsKey(o)==false)
				table[o]=o;
		}
		/// <summary>
		/// Check for the containments.
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public bool Contains(object o){ return table.ContainsKey(o);}
		/// <summary>
		/// Deletes an element from the set; does not have effect if the element is not inside.
		/// </summary>
		/// <param name="o"></param>
		public void Delete(object o){table.Remove(o);}
		/// <summary>
		/// Yields the number of element in the set
		/// </summary>
		public int Count {get {return table.Count;}}
		/// <summary>
		/// Emumerates through the set elements.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator(){return table.Values.GetEnumerator();}
		/// <summary>
		/// Finds an object in the set.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public object Find(object key){
			return this.table[key];
		}
		

    /// <summary>
    /// Copies the set to array.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="index"></param>
		public void CopyTo(Array array, int index){

            table.Keys.CopyTo(array,index);
        }
        
		/// <summary>
		/// Returns a string representation of the set.
		/// </summary>
		public override string ToString()
		{
            StringBuilder sb = new StringBuilder("{");
			int i=0;
			foreach( object o in this)
			{
                sb.Append(o.ToString());
				i++;
                if (i < Count)
                    sb.Append(",");
			}
            sb.Append("}");
			return sb.ToString();
		}

		/// <summary>
		/// Clears the set.
		/// </summary>
		public void Clear(){
			this.table.Clear();
		}

		/// <summary>
		/// Clones the set.
		/// </summary>
		/// <returns></returns>
		public HSet Clone()
		{
			HSet ret=new HSet();
			foreach(object i in this)
			{
				ret.Insert(i);
			}
			return ret;
		}

		/// <summary>
		/// Puts all elements of i to the set
		/// </summary>
		/// <param name="i">all elements of i woild be put in to the set</param>
		public HSet(IEnumerable i){
			foreach(object j in i)
				this.Insert(j);
		}

/// <summary>
/// Return the set as array.
/// </summary>
/// <param name="type"></param>
/// <returns></returns>
		public System.Array ToArray(System.Type type){
			System.Array ret=System.Array.CreateInstance(type,this.Count);
			int i=0;
			foreach(object o in this){
				ret.SetValue(o,i++);
			}
			return ret;
		}

/// <summary>
/// Set difference.
/// </summary>
/// <param name="a"></param>
/// <param name="b"></param>
/// <returns></returns>
        public static HSet operator-(HSet a, HSet b){
            HSet ret=new HSet();
            foreach( object o in a)
                if(!b.Contains(o))
                    ret.Insert(o);

            return ret;
        }
        
		/// <summary>
		/// Set union.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
        public static HSet operator+(HSet a, HSet b){
            HSet ret=new HSet(a);
            foreach( object o in b)
                ret.Insert(o);

            return ret;
        }
    /// <summary>
		/// </summary>
    public bool  IsSynchronized {get {return table.IsSynchronized; }}
    /// <summary>
    /// </summary>
    public object SyncRoot {get {return table.SyncRoot;}}

		/// <summary>
		/// An empty constructor.
		/// </summary>
		internal HSet()
		{
		}
    

	}
}
