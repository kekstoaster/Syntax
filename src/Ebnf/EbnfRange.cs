using System;
using System.IO;

namespace Kekstoaster.Syntax
{
	public class EbnfRange:Ebnf
	{
		private char _from;
		private char _to;

		/// <summary>
		/// Matches any character in a range of fromChar to toChar to no choise list containing every character must be checked for every element.
		/// </summary>
		/// <param name="fromChar">The starting character code that is matched.</param>
		/// <param name="toChar">The last character code that will be matched.</param>
		/// <param name="scopetype">The scopetype when parsing the element.</param>
		public EbnfRange (char fromChar, char toChar, ScopeType scopetype = ScopeType.Default) : base (scopetype)
		{
			// check for order, if toChar < fromChar swap both to create range
			if (fromChar > toChar) {
				this._from = toChar;
				this._to = fromChar;
			} else {
				this._from = fromChar;
				this._to = toChar;
			}

			this.Label = "[" + toChar.ToString () + "-" + fromChar.ToString () + "]";
		}

		internal override object MatchElement (Stream s, EbnfCompiler compiler)
		{
			long startPos = s.Position;
			object result = null;

			char next = '\0';
			if (compiler.Encoder == null) {
				int n = s.ReadByte ();
				if (n == -1) {
					throw ElementException ();
				}
				next = (char)n;
			} else {
				try {
					next = compiler.Encoder.NextChar (s);
				} catch (EofException) {
					throw ElementException ();
				}
			}
			//Debug.Write ((char)next);
			if (next >= this._from && next <= this._to) {
				SyntaxElement c;
				c = new SyntaxText (this.ParseAction, compiler, (char)next);
				c.Label = this.Label;
				result = ParseResult (c, compiler);
			} else {
				if (this._error == null) {
					throw new EbnfElementException ();
				} else {
					throw new EbnfElementException (string.Format (this._error, (char)next));
				}
			}

			return result;
		}

		public override bool CanBeEmpty {
			get {
				return false;
			}
		}

		public override bool IsGeneric {
			get {
				if (this.CompileAction == null) {
					return true;
				} else {
					return false;
				}
			}
		}

		public override Ebnf Clone ()
		{
			EbnfRange n = new EbnfRange (this._from, this._to, this._scopeType);
			n.CompileAction = this.CompileAction;
			n.Label = this.Label;
			n.ParseAction = this.ParseAction;
			n._error = this._error;
			return n;
		}

		internal override string ToString (int depth)
		{
			return "[[ " + this._from.ToString () + "-" + this._to.ToString () + " ]]";
		}
	}
}

