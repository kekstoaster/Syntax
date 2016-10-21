using System;
using System.Collections;
using System.Collections.Generic;

namespace Kekstoaster.Syntax
{
	public abstract class EbnfEnumerable:Ebnf, IList<Ebnf>
	{
		internal protected List<Ebnf> _list;

		protected EbnfEnumerable (ScopeType scopetype = ScopeType.Default) : base (scopetype)
		{
			_list = new List<Ebnf> ();
		}

		// ********************************************************************
		// ***************************** Append *******************************
		// ********************************************************************

		/// <param name="x1">The element that will be extended.</param>
		/// <param name="x2">The element that will be added to the first operand.</param>
		public static EbnfEnumerable operator + (EbnfEnumerable x1, Ebnf x2)
		{
			return x1.Append (x2);
		}

		/// <summary>
		/// Append the specified element x to the list.
		/// Can only be used on Choise, List, Permutation
		/// </summary>
		/// <param name="x">The element that will be added to the list.</param>
		public EbnfEnumerable Append (Ebnf x)
		{
			this._list.Add (x);
			return this;
		}

		/// <summary>
		/// Inserts the specified element x to the top of the list.
		/// Can only be used on Choise, List, Permutation.
		/// </summary>
		/// <param name="x">The element that will be added to the list.</param>
		public EbnfEnumerable Preppend (Ebnf x)
		{
			this._list.Insert (0, x);
			return this;
		}

		internal protected override bool CheckGeneric (System.Collections.Generic.HashSet<Ebnf> hashset)
		{
			if (hashset == null)
				hashset = new HashSet<Ebnf> ();
		
			hashset.Add (this);
			if (this.CompileAction == null) {
				foreach (var item in _list) {
					if (!hashset.Contains (item) && !item.CheckGeneric (hashset)) {
						return false;
					}
				}
				return true;
			} else {
				return false;
			}
		}

		#region IList implementation

		int IList<Ebnf>.IndexOf (Ebnf item)
		{
			throw new NotImplementedException ();
		}

		void IList<Ebnf>.Insert (int index, Ebnf item)
		{
			throw new NotImplementedException ();
		}

		void IList<Ebnf>.RemoveAt (int index)
		{
			throw new NotImplementedException ();
		}

		Ebnf IList<Ebnf>.this [int index] {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		#endregion

		#region ICollection implementation

		void ICollection<Ebnf>.Add (Ebnf item)
		{
			throw new NotImplementedException ();
		}

		void ICollection<Ebnf>.Clear ()
		{
			throw new NotImplementedException ();
		}

		bool ICollection<Ebnf>.Contains (Ebnf item)
		{
			throw new NotImplementedException ();
		}

		void ICollection<Ebnf>.CopyTo (Ebnf[] array, int arrayIndex)
		{
			throw new NotImplementedException ();
		}

		bool ICollection<Ebnf>.Remove (Ebnf item)
		{
			throw new NotImplementedException ();
		}

		int ICollection<Ebnf>.Count {
			get {
				throw new NotImplementedException ();
			}
		}

		bool ICollection<Ebnf>.IsReadOnly {
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator<Ebnf> IEnumerable<Ebnf>.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

