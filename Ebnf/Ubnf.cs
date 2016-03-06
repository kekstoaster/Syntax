using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// Static class for creating unique elements.
	/// </summary>
	public static class Ubnf {
		/// <summary>
		/// Gets a new empty list that is marked unique.
		/// </summary>
		/// <value>The new empty list.</value>
		public static Ebnf EmptyList {
			get {
				Ebnf n = Ebnf.EmptyList;
				n.Unique ();
				return n;
			}
		}

		/// <summary>
		/// Gets a new empty permutation that is marked unique.
		/// </summary>
		/// <value>The new empty permutation.</value>
		public static Ebnf EmptyPermutation {
			get {
				Ebnf n = Ebnf.EmptyPermutation;
				n.Unique ();
				return n;
			}
		}
	}
}

