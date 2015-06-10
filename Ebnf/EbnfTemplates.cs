using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// Ebnf templates for frequently used elements.
	/// </summary>
	public static class EbnfTemplates
	{
		/// <summary>
		/// Returnes a single space element.
		/// </summary>
		/// <value>A single space element.</value>
		public static Ebnf Space {
			get { return new Ebnf (' '); }
		}

		/// <summary>
		/// Returns a single char range from a to z
		/// </summary>
		/// <value>The a to z element.</value>
		public static Ebnf a_to_z {
			get { return Ebnf.Range ('a', 'z'); }
		}

		/// <summary>
		/// Returns a single char range from A to Z
		/// </summary>
		/// <value>The A to Z element.</value>
		public static Ebnf A_to_Z {
			get { return Ebnf.Range ('A', 'Z'); }
		}

		/// <summary>
		/// Returns a single char range from 0 to 9
		/// </summary>
		/// <value>The 0 to 9 element.</value>
		public static Ebnf _0_to_9 {
			get { return Ebnf.Range ('0', '9'); }
		}

		/// <summary>
		/// Returns a single char range from a to z or A to Z
		/// </summary>
		/// <value>The Ebnf element.</value>
		public static Ebnf Alpha {
			get { return Ebnf.Range ('a', 'z', ScopeType.Parent) | Ebnf.Range ('A', 'Z', ScopeType.Parent); }
		}

		/// <summary>
		/// Returns a single char range from a to z, A to Z or 0 to 9
		/// </summary>
		/// <value>The Ebnf element.</value>
		public static Ebnf AlphaNum {
			get { return Ebnf.Range ('a', 'z', ScopeType.Parent) | Ebnf.Range ('A', 'Z', ScopeType.Parent) | Ebnf.Range ('0', '9', ScopeType.Parent); }
		}

		/// <summary>
		/// Returns a single char element that matches any whitespace space ' ', tabulator '\t', carriage return '\r' or linefeed '\n'
		/// </summary>
		/// <value>The Ebnf element.</value>
		public static Ebnf Whitespace {
			get {
				Ebnf space = new Ebnf(' ', ScopeType.Parent),
				     tab = new Ebnf('\t', ScopeType.Parent),
				     carriageReturn = new Ebnf('\r', ScopeType.Parent),
				     lineFeed = new Ebnf('\n', ScopeType.Parent);
								     
				return space | tab | carriageReturn | lineFeed;
			}
		}

		/// <summary>
		/// Returns an element matching a general identifier.
		/// The identifier starts with any letter or underscore and is followed by an arbitrary count of letter, underscore or digit characters.
		/// </summary>
		/// <value>The Ebnf element.</value>
		public static Ebnf Identifier {
			get {
				Ebnf az = Ebnf.Range ('a', 'z', ScopeType.Parent);
				Ebnf AZ = Ebnf.Range ('A', 'Z', ScopeType.Parent);
				Ebnf _09 = Ebnf.Range ('0', '9', ScopeType.Parent);
				Ebnf _ = new Ebnf ('_', ScopeType.Parent);

				Ebnf c1 = az | AZ | _;
				c1.ScopeType = ScopeType.Parent;

				Ebnf cn = az | AZ | _09 | _;
				cn.ScopeType = ScopeType.Parent;

				Ebnf cnn = ~cn;
				cnn.ScopeType = ScopeType.Parent;

				Ebnf c = c1 & cnn;
				return c;
			}
		}

		/// <summary>
		/// Returns a seperator matching element.
		/// This is a negation element; negating letters, digits and underscore
		/// </summary>
		/// <value>The seperator.</value>
		public static Ebnf Seperator {
			get {
				return Ebnf.Not(Ebnf.Range ('a', 'z', ScopeType.Parent) | Ebnf.Range ('A', 'Z', ScopeType.Parent) | Ebnf.Range ('0', '9', ScopeType.Parent) | new Ebnf('_', ScopeType.Parent));
			}
		}
	}
}

