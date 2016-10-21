using System;
using System.IO;

namespace Kekstoaster.Syntax
{
	public class EbnfAny:Ebnf
	{
		/// <summary>
		/// The default label of the element returned when calling Ebnf.AnyChar
		/// </summary>
		public const string ANYCHAR_LABEL = "[[ANYCHAR]]";
		internal bool _allowEOF = false;

		public EbnfAny (ScopeType scopetype = ScopeType.Default) : base (scopetype)
		{
			this.Label = ANYCHAR_LABEL;
		}

		internal override object MatchElement (Stream s, EbnfCompiler compiler)
		{
			long startPos = s.Position;
			object result = null;

			SyntaxElement c;
			if (compiler.Encoder == null) {
				int next = s.ReadByte ();
				//Debug.Write ((char)next);
				if (next != -1) {
					c = new SyntaxText (this.ParseAction, compiler, (char)next);
					c.Label = this.Label;
				} else {
					if (_allowEOF) {
						c = new EmptyElement (this.ParseAction, compiler);
						c.Label = EbnfEOF.EOF_LABEL;
					} else {
						throw ElementException ();
					}
				}
			} else {
				try {
					char nc = compiler.Encoder.NextChar (s);
					c = new SyntaxText (this.ParseAction, compiler, nc);
					c.Label = this.Label;
				} catch (EofException) {
					if (_allowEOF) {
						c = new EmptyElement (this.ParseAction, compiler);
						c.Label = EbnfEOF.EOF_LABEL;
					} else {
						throw ElementException ();
					}					
				}
			}
			result = ParseResult (c, compiler);

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
			EbnfAny n = new EbnfAny (this._scopeType);
			n.CompileAction = this.CompileAction;
			n.Label = this.Label;
			n.ParseAction = this.ParseAction;
			n._error = this._error;
			return n;
		}

		internal override string ToString (int depth)
		{
			return "(*)";
		}
	}
}

