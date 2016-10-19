using System;
using System.IO;

namespace Kekstoaster.Syntax
{
	public class EbnfExclusion:Ebnf
	{
		Ebnf _element;

		public EbnfExclusion (Ebnf element, ScopeType scopetype = ScopeType.Default) : base (scopetype)
		{
			_element = element;
		}

		internal override object MatchElement (Stream s, EbnfCompiler compiler)
		{
			long startPos = s.Position;
			object result = null;
			ScopeType scopetype = this.GetScopeType (compiler.StandardScope);

			object arg = null;

			try {
				arg = _element.MatchElement (s, compiler);
			} catch (EbnfElementException) {
				s.Position = startPos;					
			}
			if (arg != null) {
				ThrowElementException ();
			} else {					
				result = IgnoredElement.Instance;
			}

			return result;
		}

		public override bool IsGeneric {
			get {
				if (this.CompileAction == null) {
					return _element.IsGeneric;
				} else {
					return false;
				}
			}
		}

		public override bool CanBeEmpty {
			get {
				return true;
			}
		}

		public override Ebnf Clone ()
		{
			EbnfExclusion n = new EbnfExclusion (_element, this._scopeType);
			n.CompileAction = this.CompileAction;
			n.Label = this.Label;
			n.ParseAction = this.ParseAction;
			n._error = this._error;
			return n;
		}

		internal override string ToString (int depth)
		{
			return "-" + _element.ToString (depth - 1);
		}
	}
}

