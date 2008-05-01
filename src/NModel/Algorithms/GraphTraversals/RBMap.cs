//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Text;
using NModel.Algorithms.GraphTraversals;
using System.IO;
using System.Collections;

namespace NModel.Algorithms.GraphTraversals  
 
{


	[Serializable]
	internal class RBMap:  MarshalByRefObject, IEnumerable
	{
		public IEnumerator GetEnumerator(){return table.GetEnumerator();}

		public IEnumerable Keys 
		{
			get
			{
				return table.Keys;
			}
		}

		public IEnumerable Values 
		{
			get
			{
				return table.Values;
			}
		}

		internal Hashtable table;

		internal RBMap()
		{

			table=new Hashtable();

		}

		public object delete(object k)
		{

			object ret=table[k];

			if(ret!=null)
			{
			    table.Remove(k);
			}
          
			return ret;

		}

		public bool contains(object k)
		{
			return table.ContainsKey(k);
				
		}


		public int Count 
		{
			get 
			{
				return this.table.Count;
			}
		}

		public override string ToString()
		{
            StringBuilder ret = new StringBuilder("{");
			int i=0;
			foreach(DictionaryEntry p in this)
			{
                ret.Append(p.Key.ToString());
                ret.Append("->");
                ret.Append(p.Value.ToString());
				if(i != this.Count-1)
				{
                    ret.Append(",");
				}
				i++;
			}
            ret.Append("}");
            return ret.ToString();
		}


		public object this[object k]
		{

			get
			{

				return table[k];

			}

			set
			{
				this.table[k]=value;
			}
		}
	}
}
