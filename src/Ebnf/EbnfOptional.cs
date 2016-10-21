using System;
using System.IO;
using System.Collections.Generic;

namespace Kekstoaster.Syntax
{
	public class EbnfOptional:Ebnf
	{
		Ebnf _element;

		public EbnfOptional (Ebnf element, ScopeType scopetype = ScopeType.Default) : base (scopetype)
		{
			_element = element;
		}

		internal override object MatchElement (Stream s, EbnfCompiler compiler)
		{
			long startPos = s.Position;
			object result = null;
			try {
				object arg;
				arg = _element.MatchElement (s, compiler);
				result = ParseResult (arg, compiler);
			} catch (EbnfElementException) {
				s.Position = startPos;
				result = IgnoredElement.Instance;
			}

			return result;
		}

		internal protected override bool CheckGeneric (System.Collections.Generic.HashSet<Ebnf> hashset)
		{
			if (hashset == null)
				hashset = new HashSet<Ebnf> ();

			hashset.Add (this);
			if (this.CompileAction == null) {
				if (hashset.Contains (_element)) {
					return true;
				} else {
					return _element.CheckGeneric (hashset);
				}
			} else {
				return false;
			}
		}

		public override bool CanBeEmpty {
			get {
				return true;
			}
		}

		public override Ebnf Clone ()
		{
			EbnfOptional n = new EbnfOptional (_element, this._scopeType);
			n.CompileAction = this.CompileAction;
			n.Label = this.Label;
			n.ParseAction = this.ParseAction;
			n._error = this._error;

			return n;
		}

		internal override string ToString (int depth)
		{
			return "[" + _element.ToString (depth - 1) + "]";
		}
	}
}

