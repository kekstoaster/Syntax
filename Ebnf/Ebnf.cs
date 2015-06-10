using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// Ebnf Element for Syntax Parsing of text files
	/// 
	/// <![CDATA[
	/// There are 9 types of Ebnf-Elements that can be created
	///    * Char        -   - base-element, a single character, that must be matched
	///    * Range       -   - base-element, a range of characters, where one in that range must be matched
	///    * Any         -   - base-element, any single Character that can be found and EOF
	///    * EOF,        -   - base-element, end of file or end of stream
	///    * List        - & - a list of Ebnf-Elements that must occour in the exact order specified
	///    * Optional    - ! - an optional element can occour at the given position or is ignored
	///    * Choise      - | - a list of Ebnf-elements where at least one element must match
	///    * Repeat      - ~ - a single Ebnf-element that can occour 0 or arbitrary times
	///    * Not         - - - an element that must not occour at that position
	///    * Permutation - ^ - a list of Ebnf-elements that occour exactly once but in random order
	///                        (this is not a real EBNF-Element and can be created with the above elements,
	///                         but creates much easier syntax-style with better performance)
	/// 
	///   By default only Text-Elements can be created as new instance, all other elements
	///   can just be created by the use of operations
	/// 
	/// 
	/// 
	///   **************************
	///   ********   CHAR   ********
	///   **************************
	/// 
	///   A single char that must be matched
	///   The character can be any byte value, from 0 to 255
	/// 
	///   Creation: Ebnf x = new Ebnf('x');
	///             Ebnf x = 'x';
	/// 
	/// 
	///   **************************
	///   *******   RANGE   ********
	///   **************************
	/// 
	///   A single char in a given character range that must be matched
	///   Range must be in between the byte value range 0 to 255
	/// 
	///   Creation: Ebnf x = Ebnf.Range('a', 'z');
	/// 
	/// 
	///   **************************
	///   ********   Any   *********
	///   **************************
	/// 
	///   A single char. It always matches, and it also matches EOF.
	/// 
	///   Creation: Ebnf any = Ebnf.AnyChar;
	/// 
	///   In the end of stream, checking for AnyChar always succeeds, so having a list of
	///   repeat(Any) as in
	/// 
	///   Ebnf r = ~(Ebnf.AnyChar);
	/// 
	///   will never terminate, ending up in an endless loop. So be sure always combining
	///   the Any repeat statement with a Not combination.
	///   
	///   Example: Match until end of line:
	/// 
	///      Ebnf endOfLine = ~(Ebnf.EmptyList + -(Ebnf.EOF) + Ebnf.AnyChar);
	/// 
	/// 
	///   **************************
	///   ********   EOF   *********
	///   **************************
	/// 
	///   Matches only end of file or end of stream, so all other characters in that stream must
	///   be matched before it it true.
	/// 
	///   Creation: Ebnf eof = Ebnf.EOF;
	/// 
	///   It is always a good idea to have a final Element matching the entire file like
	/// 
	///      Ebnf file = whitespaces & content & whitespaces & oef; 
	/// 
	///   When a Ebnf element is matched, the parsing ends. So if any characters follow the
	///   defined element that are not included in the syntax, it will not be recogniced.
	///   If you want all following characters to be ignored, this is what you want. Otherwise
	///   ending with an Ebnf.EOF element ensures that the entire file matches the systax you
	///   defined.
	/// 
	/// 
	///   **************************
	///   ********   List   ********
	///   **************************
	/// 
	///   Matches all specified elements in the given order. If parsing of one element fails,
	///   parsing of the entire list is considered a failure.
	/// 
	///   Creation: Ebnf list = Ebnf.EmptyList + element1 + element2 + ...;
	///             Ebnf list = element1 & element2 & ...;
	/// 
	///   Can be flaged as Unique. If a unique element is partly matched and an error occours
	///   such as the element is not completely matched, the entire parsing process is aborted.
	///   Example: match [x] or {x} or (x), where [x], {x} and (x) are 3 unique lists
	///            text found: [x
	/// 
	///            so instead of trying to also parse {x} and (x) the parsing process is
	///            canceled because no other element can match [x
	///            
	///            whereas [x] or [y], with [x] and [y] being lists, they cannot be made unique,
	///            because both start with [
	/// 
	///   The common usage is the & concatanation of elements.
	///   Use the EmptyList method for reference combinations, the & method just copies values
	///   Example: match xyxyxyxyxyxyx...
	///            Ebnf x = Ebnf.EmptyList;
	///            Ebnf y = Ebnf.EmptyList + (new Ebnf('y')) + !x;
	///                 x = Ebnf.EmptyList + (new Ebnf('x')) + !y;
	/// 
	///   If two lists a with n elements and b with m elements are combined with a & b,
	///   a new list with m + n elements is created. If the + syntax is used a new list
	///   with only 2 elements is created, the first element being a, the second element
	///   being b;
	///   
	///   **************************
	///   ******   Optional   ******
	///   **************************
	/// 
	///   Matches the specified element or continues with the next without a failure.
	/// 
	///   Creation: Ebnf opt = !element;
	///   
	///   Example: match 1 with optional -, so 1 or -1:
	///            Ebnf one = !(new Ebnf('-')) & (new Ebnf('1'));
	/// 
	///   
	///   **************************
	///   *******   Choise   *******
	///   **************************
	/// 
	///   Returns the first matching element of a list of Ebnf elements.
	/// 
	///   Creation: Ebnf choise = Ebnf.EmptyChoise + element1 + element2 + ...;
	///             Ebnf choise = element1 | element2 | ...;
	/// 
	///   Like the list, the common syntax is |-combination of elements. Use EmptyChoise
	///   method for reference.
	///   Example: Match x, or (x) or ((x)) or (((x))) or ...
	///            Ebnf x = Ebnf.EmptyChoise;
	///            Ebnf l = '(', r = ')';
	///            x = x + (new Ebnf('x')) + (Ebnd.EmptyList + l + x + r);
	/// 
	/// 
	///   **************************
	///   *******   Repeat   *******
	///   **************************
	/// 
	///   Matches any number of occurences of a single Ebnf element.
	/// 
	///   Creation: Ebnf repeat = ~element
	/// 
	///   Example: match any number of spaces, but at least one
	///            Ebnf space = ' ';
	///            Ebnf spaces = space & ~space;
	/// 
	/// 
	///   **************************
	///   ********   Not   *********
	///   **************************
	/// 
	///   Matches any element, that is not the specified element
	///   In contrast to all other elements is the stream position pointer not set to the end 
	///   but reset to the start of the element. So any number of Not-elements can be tested
	///   
	///   Creation: Ebnf repeat = -element
	/// 
	///   Example: match a string with like "anyString", meaning the quotes are leading and
	///            trailing, but not inbetween the string
	///            Ebnf quote = '"';
	///            Ebnf str = Ebnf.EmptyList + quote + ~(-quote & Ebnf.AnyChar) + quote
	/// 
	/// 
	///   **************************
	///   ****   Permutation   *****
	///   **************************
	/// 
	///   Matches a list of Ebnf-elements. Each element is matched just once, but the order
	///   is not specified.
	/// 
	///   Creation: Ebnf perm = Ebnf.EmptyPermutation + element1 + element2 + ...;
	///             Ebnf perm = element1 ^ element2 ^ ...
	/// 
	///   Can be flaged as Unique. If a unique element is partly matched and an error occours
	///   such as the element is not completely matched, the entire parsing process is aborted.
	///   (See List for more explanation)
	/// 
	///   If a permutation is combined with elements that are optional, like Optional,
	///   Repeat or Not, a permutation is optional. If none of the elements are matched,
	///   it is ignored itself.   
	///   Example: 'function', or 'public static function', or 'static public function'
	///            or 'static function', or 'public function'
	///            Ebnf pub = "public ";
	///            Ebnf sta = "static ";
	///            Ebnf fnc = "function";
	///            Ebnd funcCombination = (!pub ^ !sta) & function
	///   Important in this example is to include the spaces in the "public " and "static "
	///   or it will not work. 
	/// 
	///   Example: match: exactly xy or yx
	///            Ebnf x = 'x';
	///            Ebnf y = 'y';
	///            Ebnf xy = x ^ y;
	/// ]]>
	/// </summary>
	public partial class Ebnf:ICloneable
	{
		/// <summary>
		/// The default label of the element returned when calling Ebnf.AnyChar
		/// </summary>
		public const string ANYCHAR_LABEL = "[[ANYCHAR]]";
		/// <summary>
		/// The default label of the element returned when calling Ebnf.EOF
		/// </summary>
		public const string EOF_LABEL = "[[EOF]]";

		private EbnfType _type;
		private char[] _char;
		private List<Ebnf> _list;
		private string _error = null;
		private bool _unique = false;
		private string _label = null;

		private ParseAction _parse = null;
		private CompileAction _compile = null;
		private ScopeType _scopeType;

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.Ebnf"/> class as a character element.
		/// </summary>
		/// <param name="c">The character to check for.</param>
		/// <param name="scopetype">The scopetype when parsing the element.</param>
		public Ebnf (char c, ScopeType scopetype = ScopeType.Default) {
		    // Matching of chars with Stream.ReadByte, so Character will only be from 0 to 255
			if ((int)c >= 0 || (int)c <= 255) {
				this._type = EbnfType.Char;
				this._char = new char[] { c };
				this._label = c.ToString();
				_scopeType = scopetype;
			} else {
				throw new ArgumentException("The character for parsing can only be from 0 to 255");
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.Ebnf"/> class with a list of char-elements that must match the string when parsing.
		/// </summary>
		/// <param name="s">The string that must be matched.</param>
		/// <param name="scopetype">The scopetype when parsing the element.</param>
		public Ebnf (string s, ScopeType scopetype = ScopeType.Default) {
			Ebnf n = (Ebnf)s;
			this._list = n._list;
			this._type = EbnfType.List;
			this._label = s;
			_scopeType = scopetype;
		}

		// Used for initilizing all Ebnf types
		private Ebnf(EbnfType t) {
			this._type = t;
			_scopeType = ScopeType.Default;
		}

		/// <summary>
		/// Matches any character in a range of fromChar to toChar to no choise list containing every character must be checked for every element.
		/// </summary>
		/// <param name="fromChar">The starting character code that is matched.</param>
		/// <param name="toChar">The last character code that will be matched.</param>
		/// <param name="scopetype">The scopetype when parsing the element.</param>
		public static Ebnf Range(char fromChar, char toChar, ScopeType scopetype = ScopeType.Default) {
			// Matching of chars with Stream.ReadByte, so Character will only be from 0 to 255
			if (fromChar >= 0 && fromChar <= 255 && toChar >= 0 && toChar <= 255) {
				// Both chars equal -> normal character type is used
				if (fromChar == toChar) {
					return new Ebnf (fromChar, scopetype);
				} else {
					Ebnf n = new Ebnf (EbnfType.Range);
					n._scopeType = scopetype;
					// check for order, if toChar < fromChar swap both to create range
					if (fromChar < toChar) {
						n._char = new char[] { fromChar, toChar };
						n._label = "[" + fromChar.ToString() + "-" + toChar.ToString() + "]";
					} else {
						n._char = new char[] { toChar, fromChar };
						n._label = "[" + toChar.ToString() + "-" + fromChar.ToString() + "]";
					}

					return n;
				}
			} else {
				throw new ArgumentException("The characters for parsing can only be from 0 to 255");
			}
		}

		/// <summary>
		/// Gets or sets the label of teh Ebnf element. It is used to identify the element among others, i.e. for initialization.
		/// Elements with equal label are considered to be the same element/same purpose.
		/// </summary>
		/// <value>The label of the element.</value>
		public string Label {
			get{ return this._label; }
			set{ this._label = value; }
		}

		// ********************************************************************
		// ***************************** Append *******************************
		// ********************************************************************

		/// <param name="x1">The element that will be extended.</param>
		/// <param name="x2">The element that will be added to the first operand.</param>
		public static Ebnf operator + (Ebnf x1, Ebnf x2) {
			return x1.Append (x2);
		}

		/// <summary>
		/// Append the specified element x to the list.
		/// Can only be used on Choise, List, Permutation
		/// </summary>
		/// <param name="x">The element that will be added to the list.</param>
		public Ebnf Append(Ebnf x) {
			if(this._type != EbnfType.Choise && this._type != EbnfType.List && this._type != EbnfType.Permutation) {
				throw new ArgumentException("Cannot append object to non-List nor non-choise type");
			}
			this._list.Add (x);
			return this;
		}

		// ********************************************************************
		// ***************************** Choice *******************************
		// ********************************************************************

		/// <param name="x1">The first value for the choise.</param>
		/// <param name="x2">The second value for the choise.</param>
		public static Ebnf operator | (Ebnf x1, Ebnf x2) {
			return Or (x1, x2);
		}		

		/// <summary>
		/// Creates a choise element combining x1 and x2 to the list.
		/// If either x1, x2 or both are already choises the entries are added to the new choise rather adding both elements directly.
		/// But if any element x1 or x2 already have a compile action, it will be added directly to ensure the compilation.
		/// </summary>
		/// <param name="x1">The first value for the choise.</param>
		/// <param name="x2">The second value for the choise.</param>
		public static Ebnf Or(Ebnf x1, Ebnf x2) {
			// create a new choise to get a new reference
			Ebnf n = new Ebnf (EbnfType.Choise);
			n._list = new List<Ebnf> ();

			// if any x1 or x2 is a choise, combine the elements
			if(x1._type == EbnfType.Choise && x1._compile == null) {
				n._list.AddRange (x1._list);
				if(x2._type == EbnfType.Choise && x1._compile == null) {
					foreach (var item in x2._list) {
						if(!n._list.Contains(item)) {
							n._list.Add (item);
						}
					}
				} else {
					if (!n._list.Contains (x2)) {
						n._list.Add (x2);
					}
				}
			} else {
				if(x2._type == EbnfType.Choise && x2._compile == null) {
					n._list.AddRange (x2._list);
					if (!n._list.Contains (x1)) {
						n._list.Add (x1);
					}
				} else {
					// if no choise is used, simply add the elements
					n._list.Add (x1);
					if (x1 != x2) {
						n._list.Add (x2);
					}
				}
			}
			return n;
		}

		// ********************************************************************
		// ****************************** List ********************************
		// ********************************************************************

		/// <param name="x1">The first value for the list.</param>
		/// <param name="x2">The second value for the list.</param>
		public static Ebnf operator & (Ebnf x1, Ebnf x2) {
			return And (x1, x2);
		}

		/// <summary>
		/// Creates a list element combining x1 and x2 to the list.
		/// If either x1, x2 or both are already lists the entries are added to the new list rather adding both elements directly.
		/// But if any element x1 or x2 already have a compile action, it will be added directly to ensure the compilation.
		/// </summary>
		/// <param name="x1">The first x value.</param>
		/// <param name="x2">The second x value.</param>
		public static Ebnf And(Ebnf x1, Ebnf x2) {
			Ebnf n = new Ebnf (EbnfType.List);
			n._list = new List<Ebnf> ();

			if(x1._type == EbnfType.List && x1._compile == null) {
				n._list.AddRange (x1._list);
				if(x2._type == EbnfType.List && x2._compile == null) {
					n._list.AddRange (x2._list);
				} else {
					n._list.Add (x2);
				}
			} else {
				n._list.Add (x1);
				if(x2._type == EbnfType.List && x2._compile == null) {
					n._list.AddRange (x2._list);
				} else {
					n._list.Add (x2);
				}
			}
			return n;
		}

		// ********************************************************************
		// ***************************** Repeat *******************************
		// ********************************************************************

		/// <param name="x">The element that will be repeated.</param>
		public static Ebnf operator ~(Ebnf x) {
			return Repeat (x);
		}

		/// <summary>
		/// Creates a Repeat element with is matched 0 or an arbitrary number of times.
		/// if is matched 0 times, the element will be ignored.
		/// </summary>
		/// <param name="x">The element that will be repeated.</param>
		public static Ebnf Repeat(Ebnf x) {
			Ebnf n = new Ebnf (EbnfType.Repeat);
			if(x._type != EbnfType.Repeat) 
			{
				n._list = new List<Ebnf> (1);
				n._list.Add (x);
			} else {
				n = x;
			}
			return n;
		}

		/// <summary>
		/// Creates a Repeat element with is matched at least min times or an arbitrary number of times.
		/// If min equals 0 and is matched 0 times, the element will be ignored.
		/// </summary>
		/// <param name="x">The element that will be repeated.</param>
		/// <param name="min">Minimum number of occurence.</param>
		public static Ebnf Repeat(Ebnf x, byte min) {
			Ebnf n = Repeat (x);
			n._char = new char[] { (char)min };
			return n;
		}

		/// <summary>
		/// Creates a Repeat element with is matched at least min times and a maximum of max times.
		/// If min equals 0 and is matched 0 times, the element will be ignored.
		/// </summary>
		/// <param name="x">The element that will be repeated.</param>
		/// <param name="min">Minimum number of occurence.</param>
		/// <param name="max">Maximum number of occurence.</param>
		public static Ebnf Repeat(Ebnf x, byte min, byte max) {
			Ebnf n = Repeat (x);
			if(min > max) {
				throw new ArgumentException ("Minimun value must be smaller than maximum value.");
			}
			n._char = new char[] { (char)min, (char)max };
			return n;
		}

		// ********************************************************************
		// **************************** Optional ******************************
		// ********************************************************************

		/// <param name="x">The element that will be made optional.</param>
		public static Ebnf operator !(Ebnf x) {
			return Optional (x);
		}

		/// <summary>
		/// Creates an optional element.
		/// The same behaviour can be achieved by using <code>Ebnf.Repeat(x, 0, 1)</code>
		/// </summary>
		/// <param name="x">The element that will be made optional.</param>
		public static Ebnf Optional(Ebnf x) {
			Ebnf n = new Ebnf (EbnfType.Optional);
			if(x._type != EbnfType.Optional) {
				n._list = new List<Ebnf> (1);
				n._list.Add (x);
			} else {
				n = x;
			}
			return n;
		}

		// ********************************************************************
		// **************************** Negation ******************************
		// ********************************************************************

		/// <param name="x">The element that must not be matched.</param>
		public static Ebnf operator -(Ebnf x) {
			return Not (x);
		}

		/// <summary>
		/// Negates the specified element.
		/// </summary>
		/// <param name="x">The element that must not be matched.</param>
		public static Ebnf Not(Ebnf x) {
			Ebnf n = new Ebnf (EbnfType.Not);
			n._list = new List<Ebnf> (1);
			n._list.Add (x);
			return n;
		}

		// ********************************************************************
		// *************************** Permutation ****************************
		// ********************************************************************

		/// <param name="x1">The first element in the permutation.</param>
		/// <param name="x2">The second element in the permutation.</param>
		public static Ebnf operator ^ (Ebnf x1, Ebnf x2) {
			return Permutation (x1, x2);
		}

		/// <summary>
		/// Creates a new permutation element combining the two elements x1 and x2.
		/// If either x1, x2 or both are already permutations the entries are added to the new list rather adding both elements directly.
		/// But if any element x1 or x2 already have a compile action, it will be added directly to ensure the compilation.
		/// <remarks>Please notice, that unlike list and choise, parsing showes a different behavior when a compilation action is present.
		/// (a^b) ^ (c^d) matches any combination of 'abcd' without compilation action, but only abcd, bacd, abdc, badc, cdab, cdba, dcab, dcba with compilation action.
		/// </remarks>
		/// </summary>
		/// <param name="x1">The first element in the permutation.</param>
		/// <param name="x2">The second element in the permutation.</param>
		public static Ebnf Permutation(Ebnf x1, Ebnf x2) {
			Ebnf n = new Ebnf (EbnfType.Permutation);
			n._list = new List<Ebnf> ();

			if(x1._type == EbnfType.Permutation && x1._compile == null) {
				n._list.AddRange (x1._list);
				if(x2._type == EbnfType.Permutation && x1._compile == null) {
					foreach (var item in x2._list) {
						if(!n._list.Contains(item)) {
							n._list.Add (item);
						}
					}
				} else {
					if (!n._list.Contains (x2)) {
						n._list.Add (x2);
					}
				}
			} else {
				if(x2._type == EbnfType.Permutation && x2._compile == null) {
					n._list.AddRange (x2._list);
					if (!n._list.Contains (x1)) {
						n._list.Add (x1);
					}
				} else {
					n._list.Add (x1);
					if (x1 != x2) {
						n._list.Add (x2);
					}
				}
			}
			return n;
		}

		/// <param name="s">The string that is converted in a Ebnf list, matching exactly the passed string.</param>
		public static implicit operator Ebnf(string s) {
			Ebnf n = new Ebnf (EbnfType.List);
			n._list = new List<Ebnf> ();
			foreach(char c in s) {
				n._list.Add (new Ebnf (c));
			}			
			n._label = s;
			return n;
		}

		/// <param name="c">The character that is converted to a Ebnf character, that must be matched.</param>
		public static implicit operator Ebnf(char c) {
			return new Ebnf (c);
		}

		#region ICloneable implementation

		object ICloneable.Clone ()
		{
			return this.Clone ();
		}

		#endregion

		/// <summary>
		/// Creates a clone of this instance.
		/// </summary>
		public Ebnf Clone() {
			if(this._unique){
				Trace.TraceWarning("When cloning a unique element, it's not quite unique anymore.");
			}

			Ebnf n = new Ebnf (this._type);
			n._char = this._char;
			n._compile = this._compile;
			n._label = this._label;
			n._error = this._error;
			n._parse = this._parse;
			n._scopeType = this._scopeType;
			n._unique = this._unique;

			if (this._list != null) {
				n._list = new List<Ebnf> ();
				foreach (var item in _list) {
					n._list.Add (item);
				}
			}
			return n;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Kekstoaster.Syntax.Ebnf"/>.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Kekstoaster.Syntax.Ebnf"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="Kekstoaster.Syntax.Ebnf"/>; otherwise, <c>false</c>.</returns>
		public override bool Equals (object obj)
		{
			if (obj != null) {
				Ebnf x2 = obj as Ebnf;
				if (this._type == EbnfType.Char || this._type == EbnfType.EOF) {
					if (x2 != null) {
						if (x2._type == EbnfType.Char || x2._type == EbnfType.EOF) {
							return this._char == x2._char;
						}
					}
				}
			}
			return base.Equals (obj);
		}

		/// <param name="x1">The first element to compare.</param>
		/// <param name="x2">The second element to compare.</param>
		public static bool operator ==(Ebnf x1, Ebnf x2) {
			return x1.Equals (x2);
		}

		/// <param name="x1">The first element to compare.</param>
		/// <param name="x2">The second element to compare.</param>
		public static bool operator !=(Ebnf x1, Ebnf x2) {
			return !x1.Equals (x2);
		}

		/// <summary>
		/// Gets or sets the compile action. If set, the scopetype will be set to Force.
		/// </summary>
		/// <value>The compile action.</value>
		public CompileAction CompileAction {
			get {
				return this._compile;
			}
			set {
				this.ScopeType = ScopeType.Force;
				this._compile = value;
			}
		}

		/// <summary>
		/// Gets or sets the parse action. If set, the scopetype will be set to Force.
		/// </summary>
		/// <value>The parse action.</value>
		public ParseAction ParseAction {
			get {
				return this._parse;
			}
			set {
				this.ScopeType = ScopeType.Force;
				this._parse = value;
			}
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="Kekstoaster.Syntax.Ebnf"/> object.
		/// </summary>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		/// <summary>
		/// Inserts the specified element x to the top of the list.
		/// Can only be used on Choise, List, Permutation.
		/// </summary>
		/// <param name="x">The element that will be added to the list.</param>
		public Ebnf Preppend(Ebnf x) {
			if(this._type == EbnfType.Choise || this._type == EbnfType.List) {
				this._list.Insert(0, x);
			}
			return this;
		}

		internal bool Scoped(ScopeType stdScope) {
			return (this._scopeType == ScopeType.Default && (stdScope != ScopeType.Default && stdScope != ScopeType.Inhired)) || (this._scopeType != ScopeType.Inhired && this._scopeType != ScopeType.Default) || this._parse != null || this._compile != null;
		}

		internal ScopeType GetScopeType(ScopeType stdScope) {
			if(this._scopeType == ScopeType.Default){
				if(stdScope == ScopeType.Default) {
					return ScopeType.Inhired;
				}else{
					return stdScope;
				}
			}else{
				return this._scopeType;
			}
		}

		/// <summary>
		/// Gets or sets the scope type.
		/// </summary>
		/// <value>The scope type.</value>
		public ScopeType ScopeType {
			get {
				return this._scopeType;
			}
			set {
				this._scopeType = value;
			}
		}

		/// <summary>
		/// Gets a new EOF element
		/// </summary>
		/// <value>The EOF element.</value>
		public static Ebnf EOF {
			get {
				Ebnf result = new Ebnf (EbnfType.EOF);
				result.Label = EOF_LABEL;
				return result;
			}
		}

		/// <summary>
		/// Gets a new any-char element.
		/// </summary>
		/// <value>The Any-char element.</value>
		public static Ebnf AnyChar {
			get {
				Ebnf result = new Ebnf (EbnfType.Any);
				result.Label = ANYCHAR_LABEL;
				return result;
			}
		}

		/// <summary>
		/// Gets a new empty list.
		/// </summary>
		/// <value>A new empty list.</value>
		public static Ebnf EmptyList {
			get {
				Ebnf n = new Ebnf (EbnfType.List);
				n._list = new List<Ebnf> ();
				return n;
			}
		}

		/// <summary>
		/// Gets a new empty choise.
		/// </summary>
		/// <value>A new empty choise.</value>
		public static Ebnf EmptyChoise {
			get {
				Ebnf n = new Ebnf (EbnfType.Choise);
				n._list = new List<Ebnf> ();
				return n;
			}
		}

		/// <summary>
		/// Gets a new empty permutation.
		/// </summary>
		/// <value>A new empty permutation.</value>
		public static Ebnf EmptyPermutation {
			get {
				Ebnf n = new Ebnf (EbnfType.Permutation);
				n._list = new List<Ebnf> ();
				return n;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is unique.
		/// Only List and permutations can be made unique.
		/// A unique element throws a ParseException when it was partly but not fully matched.
		/// </summary>
		/// <value><c>true</c> if this instance is unique; otherwise, <c>false</c>.</value>
		public bool IsUnique {
			get {
				return this._unique;
			}
			set {
				if(_type == EbnfType.List || _type == EbnfType.Permutation) {
					//if(this._unique && !value){
					//	throw new InvalidOperationException("Cannot remove unique Flag from Ebnf-element.");
					//}
					this._unique = value;
				} else {
					throw new InvalidOperationException("Only Lists and Permutation Ebnf-elements can be unique.");
				}
			}
		}

		/// <summary>
		/// Sets a value indicating that this instance is unique.
		/// </summary>
		public void Unique() {
			this._unique = true;
		}

		private enum EbnfType {
			Any,
			Range,
			EOF,
			Char,
			List,
			Optional,
			Choise,
			Repeat,
			Not,
			Permutation
		}
	}
}