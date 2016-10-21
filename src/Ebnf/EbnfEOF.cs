using System;
using System.IO;

namespace Kekstoaster.Syntax
{
	public class EbnfEOF:Ebnf
	{
		/// <summary>
		/// The default label of the element returned when calling Ebnf.EOF
		/// </summary>
		public const string EOF_LABEL = "[[EOF]]";

		public EbnfEOF (ScopeType scopetype = ScopeType.Default) : base (scopetype)
		{
			this.Label = EOF_LABEL;
		}

		internal override object MatchElement (Stream s, EbnfCompiler compiler)
		{
			long startPos = s.Position;
			object result = null;

			int next = s.ReadByte ();
			if (next == -1) {
				SyntaxElement eof = new EmptyElement (this.ParseAction, compiler);
				eof.Label = EOF_LABEL;
				result = ParseResult (eof, compiler);
			} else {
				throw ElementException ();
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
			EbnfEOF n = new EbnfEOF (this._scopeType);
			n.CompileAction = this.CompileAction;
			n.Label = this.Label;
			n.ParseAction = this.ParseAction;
			n._error = this._error;
			return n;
		}

		internal override string ToString (int depth)
		{
			return "[[ EOF ]]";
		}
	}
}

