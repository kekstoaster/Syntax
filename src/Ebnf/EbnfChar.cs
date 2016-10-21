using System;
using System.IO;

namespace Kekstoaster.Syntax
{
	public class EbnfChar:Ebnf
	{
		private char _char;

		public EbnfChar (char c, ScopeType scopetype = ScopeType.Default) : base (scopetype)
		{
			this._char = c;
			this.Label = c.ToString ();
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
			if (next == this._char) {
				SyntaxElement c;
				c = new SyntaxText (Ebnf.DefaultParseAction, compiler, this._char);
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

		internal protected override bool CheckGeneric (System.Collections.Generic.HashSet<Ebnf> hashset)
		{			
			if (this.CompileAction == null) {
				return true;
			} else {
				return false;
			}
		}

		public override Ebnf Clone ()
		{
			EbnfChar n = new EbnfChar (this._char, this._scopeType);
			n.CompileAction = this.CompileAction;
			n.Label = this.Label;
			n.ParseAction = this.ParseAction;
			n._error = this._error;
			return n;
		}

		internal override string ToString (int depth)
		{
			return this._char.ToString ();
		}
	}
}

