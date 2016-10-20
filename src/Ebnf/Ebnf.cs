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
	public abstract partial class Ebnf:ICloneable
	{
		// error message for bad parsing
		protected string _error = null;
		// unique flag, only applicable to list and permutation
		// if unique elements are partly matched but fail parsing,
		// the entire document fails parsing
		// i.e. a wrong string "foo bar
		// fails and cannot be something else so the document is broken
		//private bool _unique = false;
		// label for identifing the element
		// elements with equal label are considered to be the same element
		private string _label = null;

		private ParseAction _parse = null;
		private CompileAction _compile = null;
		protected ScopeType _scopeType;

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.Ebnf"/> class as a character element.
		/// </summary>
		/// <param name="c">The character to check for.</param>
		/// <param name="scopetype">The scopetype when parsing the element.</param>
		internal Ebnf (ScopeType scopetype = ScopeType.Default)
		{
			this._scopeType = scopetype;
		}

		/// <summary>
		/// Gets or sets the label of the Ebnf element. It is used to identify the element among others, i.e. for initialization.
		/// Elements with equal label are considered to be the same element/same purpose.
		/// </summary>
		/// <value>The label of the element.</value>
		public string Label {
			get{ return this._label; }
			set{ this._label = value; }
		}

		public abstract bool CanBeEmpty { get; }

		public abstract bool IsGeneric { get; }

		// ********************************************************************
		// ***************************** Choice *******************************
		// ********************************************************************

		/// <param name="x1">The first value for the choise.</param>
		/// <param name="x2">The second value for the choise.</param>
		public static EbnfChoice operator | (Ebnf x1, Ebnf x2)
		{
			return Or (x1, x2);
		}

		/// <summary>
		/// Creates a choise element combining x1 and x2 to the list.
		/// If either x1, x2 or both are already choises the entries are added to the new choise rather adding both elements directly.
		/// But if any element x1 or x2 already have a compile action, it will be added directly to ensure the compilation.
		/// </summary>
		/// <param name="x1">The first value for the choise.</param>
		/// <param name="x2">The second value for the choise.</param>
		public static EbnfChoice Or (Ebnf x1, Ebnf x2)
		{
			// create a new choise to get a new reference
			EbnfChoice n = new EbnfChoice ();

			// if any x1 or x2 is a choise, combine the elements
			if (x1 is EbnfChoice && x1.IsGeneric) {
				n._list.AddRange (((EbnfChoice)x1)._list);
				if (x2  is EbnfChoice && x2.IsGeneric) {
					foreach (var item in ((EbnfChoice)x2)._list) {
						if (!(n._list.Contains (item))) {
							n._list.Add (item);
						}
					}
				} else {
					if (!(n._list.Contains (x2))) {
						n._list.Add (x2);
					}
				}
			} else {
				n._list.Add (x1);
				if (x2 is EbnfChoice && x2.IsGeneric) {
					foreach (var item in ((EbnfChoice)x2)._list) {
						if (!(n._list.Contains (item))) {
							n._list.Add (item);
						}
					}
				} else {
					// if no choise is used, simply add the elements
					if (x1 != x2) {
						n._list.Add (x2);
					}
				}
			}
			return n;
		}

		public EbnfChoice Or (Ebnf x)
		{
			return Or (this, x);
		}

		// ********************************************************************
		// ****************************** List ********************************
		// ********************************************************************

		/// <param name="x1">The first value for the list.</param>
		/// <param name="x2">The second value for the list.</param>
		public static EbnfList operator & (Ebnf x1, Ebnf x2)
		{
			return And (x1, x2);
		}

		/// <summary>
		/// Creates a list element combining x1 and x2 to the list.
		/// If either x1, x2 or both are already lists the entries are added to the new list rather adding both elements directly.
		/// But if any element x1 or x2 already have a compile action, it will be added directly to ensure the compilation.
		/// </summary>
		/// <param name="x1">The first x value.</param>
		/// <param name="x2">The second x value.</param>
		public static EbnfList And (Ebnf x1, Ebnf x2)
		{
			EbnfList n = new EbnfList ();
			n._list = new List<Ebnf> ();

			if (x1 is EbnfList && x1.IsGeneric) {
				n._list.AddRange (((EbnfList)x1)._list);
				if (x2 is EbnfList && x2.IsGeneric) {
					n._list.AddRange (((EbnfList)x2)._list);
				} else {
					n._list.Add (x2);
				}
			} else {
				n._list.Add (x1);
				if (x2 is EbnfList && x2.IsGeneric) {
					n._list.AddRange (((EbnfList)x2)._list);
				} else {
					n._list.Add (x2);
				}
			}
			return n;
		}

		public EbnfList And (Ebnf x)
		{
			return And (this, x);
		}

		// ********************************************************************
		// ***************************** Repeat *******************************
		// ********************************************************************

		/// <param name="x">The element that will be repeated.</param>
		public static EbnfRepeat operator ~ (Ebnf x)
		{
			return Repeat (x);
		}

		/// <summary>
		/// Creates a Repeat element with is matched 0 or an arbitrary number of times.
		/// if is matched 0 times, the element will be ignored.
		/// </summary>
		/// <param name="x">The element that will be repeated.</param>
		public static EbnfRepeat Repeat (Ebnf x)
		{
			EbnfRepeat n;
			if (x is EbnfRepeat) {
				n = (EbnfRepeat)x;
			} else {
				n = new EbnfRepeat (x);
			}
			return n;
		}

		/// <summary>
		/// Creates a Repeat element with is matched at least min times or an arbitrary number of times.
		/// If min equals 0 and is matched 0 times, the element will be ignored.
		/// </summary>
		/// <param name="x">The element that will be repeated.</param>
		/// <param name="min">Minimum number of occurence.</param>
		public static EbnfRepeat Repeat (Ebnf x, int min)
		{
			EbnfRepeat n = new EbnfRepeat (x, min);
			return n;
		}

		/// <summary>
		/// Creates a Repeat element with is matched at least min times and a maximum of max times.
		/// If min equals 0 and is matched 0 times, the element will be ignored.
		/// </summary>
		/// <param name="x">The element that will be repeated.</param>
		/// <param name="min">Minimum number of occurence.</param>
		/// <param name="max">Maximum number of occurence.</param>
		public static EbnfRepeat Repeat (Ebnf x, int min, int max)
		{
			EbnfRepeat n = Repeat (x, min, max);
			return n;
		}

		public EbnfRepeat Repeat ()
		{
			return Repeat (this);
		}

		public EbnfRepeat Repeat (int min)
		{
			return Repeat (this, min);
		}

		public EbnfRepeat Repeat (int min, int max)
		{
			return Repeat (this, min, max);
		}

		// ********************************************************************
		// **************************** Optional ******************************
		// ********************************************************************

		/// <param name="x">The element that will be made optional.</param>
		public static EbnfOptional operator ! (Ebnf x)
		{
			return Optional (x);
		}

		/// <summary>
		/// Creates an optional element.
		/// The same behaviour can be achieved by using <code>Ebnf.Repeat(x, 0, 1)</code>
		/// </summary>
		/// <param name="x">The element that will be made optional.</param>
		public static EbnfOptional Optional (Ebnf x)
		{
			EbnfOptional n;
			if (x is EbnfOptional) {
				n = (EbnfOptional)x;
			} else {
				n = new EbnfOptional (x);	
			}
			return n;
		}

		public EbnfOptional Optional ()
		{
			return Optional (this);
		}

		// ********************************************************************
		// **************************** Negation ******************************
		// ********************************************************************

		/// <param name="x">The element that must not be matched.</param>
		public static EbnfExclusion operator - (Ebnf x)
		{
			return Not (x);
		}

		/// <summary>
		/// Negates the specified element.
		/// </summary>
		/// <param name="x">The element that must not be matched.</param>
		public static EbnfExclusion Not (Ebnf x)
		{
			EbnfExclusion n = new EbnfExclusion (x);
			return n;
		}

		public EbnfExclusion Exclude ()
		{
			return Not (this);
		}

		// ********************************************************************
		// *************************** Permutation ****************************
		// ********************************************************************

		/// <param name="x1">The first element in the permutation.</param>
		/// <param name="x2">The second element in the permutation.</param>
		public static Ebnf operator ^ (Ebnf x1, Ebnf x2)
		{
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
		public static EbnfPermutation Permutation (Ebnf x1, Ebnf x2)
		{
			EbnfPermutation n = new EbnfPermutation ();

			if (x1 is EbnfPermutation && x1.IsGeneric) {
				n._list.AddRange (((EbnfPermutation)x1)._list);
				if (x2 is EbnfPermutation && x2.IsGeneric) {
					foreach (var item in ((EbnfPermutation)x2)._list) {
						if (!n._list.Contains (item)) {
							n._list.Add (item);
						}
					}
				} else {
					if (!n._list.Contains (x2)) {
						n._list.Add (x2);
					}
				}
			} else {
				n._list.Add (x1);
				if (x2 is EbnfPermutation && x2.IsGeneric) {
					foreach (var item in ((EbnfPermutation)x2)._list) {
						if (!n._list.Contains (item)) {
							n._list.Add (item);
						}
					}
				} else {
					if (x1 != x2) {
						n._list.Add (x2);
					}
				}
			}
			return n;
		}

		/// <param name="s">The string that is converted in a Ebnf list, matching exactly the passed string.</param>
		public static implicit operator Ebnf (string s)
		{
			EbnfList n = new EbnfList ();
			foreach (char c in s) {
				n._list.Add (new EbnfChar (c));
			}
			n._label = s;
			return n;
		}

		/// <param name="c">The character that is converted to a Ebnf character, that must be matched.</param>
		public static implicit operator Ebnf (char c)
		{
			return new EbnfChar (c);
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
		public abstract Ebnf Clone ();

		/// <param name="x1">The first element to compare.</param>
		/// <param name="x2">The second element to compare.</param>
		public static bool operator == (Ebnf x1, Ebnf x2)
		{
			return x1.Equals (x2);
		}

		/// <param name="x1">The first element to compare.</param>
		/// <param name="x2">The second element to compare.</param>
		public static bool operator != (Ebnf x1, Ebnf x2)
		{
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
				this._compile = value;
			}
		}

		/// <summary>
		/// Gets or sets the parse action. If set, the scopetype will be set to Force.
		/// </summary>
		/// <value>The parse action.</value>
		public ParseAction ParseAction {
			get {
				return this._parse == null ? DefaultParseAction : this._parse;
			}
			set {
				this._parse = value;
			}
		}

		internal ScopeType GetScopeType (ScopeType stdScope)
		{
			ScopeType scope = this._scopeType;

			switch (this._scopeType) {
			case ScopeType.Default:
				if (this.IsGeneric) {
					scope = stdScope == ScopeType.Inhired ? ScopeType.Default : stdScope;
				} else {
					scope = ScopeType.Force;
				}
				break;
			case ScopeType.Inhired:
				if (this.IsGeneric) {
					scope = ScopeType.Default;
				} else {
					scope = ScopeType.Force;
				}
				break;
			default:
				break;
			}
			return scope;
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
		public static EbnfEOF EOF {
			get {
				EbnfEOF result = new EbnfEOF ();
				return result;
			}
		}

		/// <summary>
		/// Gets a new any-char element.
		/// </summary>
		/// <value>The Any-char element.</value>
		public static EbnfAny AnyChar {
			get {
				EbnfAny result = new EbnfAny ();
				return result;
			}
		}

		/// <summary>
		/// Gets a new any-char element.
		/// </summary>
		/// <value>The Any-char element.</value>
		public static EbnfAny AnyCharOrEOF {
			get {
				EbnfAny result = new EbnfAny ();
				result._allowEOF = true;
				return result;
			}
		}

		/// <summary>
		/// Gets a new empty list.
		/// </summary>
		/// <value>The new empty list.</value>
		public static EbnfList EmptyList {
			get {
				EbnfList n = new EbnfList ();
				return n;
			}
		}

		/// <summary>
		/// Gets a new empty Choise.
		/// </summary>
		/// <value>The new empty list.</value>
		public static EbnfChoice EmptyChoise {
			get {
				EbnfChoice n = new EbnfChoice ();
				return n;
			}
		}

		/// <summary>
		/// Gets a new empty permutation.
		/// </summary>
		/// <value>The new empty permutation.</value>
		public static EbnfPermutation EmptyPermutation {
			get {
				EbnfPermutation n = new EbnfPermutation ();
				return n;
			}
		}

		internal abstract string ToString (int depth);

		public string ErrorMessage {
			get{ return _error; }
			set{ _error = value; }
		}
	}
}