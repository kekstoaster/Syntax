using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// Static class for creating unique elements.
	/// </summary>
	public static class Ubnf
	{
		/// <summary>
		/// Gets a new empty list that is marked unique.
		/// </summary>
		/// <value>The new empty list.</value>
		public static EbnfList EmptyList {
			get {
				EbnfList n = new EbnfList ();
				n.Unique ();
				return n;
			}
		}

		/// <summary>
		/// Gets a new empty permutation that is marked unique.
		/// </summary>
		/// <value>The new empty permutation.</value>
		public static EbnfPermutation EmptyPermutation {
			get {
				EbnfPermutation n = new EbnfPermutation ();
				n.Unique ();
				return n;
			}
		}
	}
}

