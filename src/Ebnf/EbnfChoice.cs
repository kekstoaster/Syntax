using System;
using System.Collections.Generic;

namespace Kekstoaster.Syntax
{
	public class EbnfChoice:EbnfEnumerable
	{
		public EbnfChoice (ScopeType scopetype = ScopeType.Default) : base (scopetype)
		{

		}

		public EbnfChoice (IEnumerable<Ebnf> elements, ScopeType scopetype = ScopeType.Default) : this (scopetype)
		{
			if (elements != null) {
				_list.AddRange (elements);
			}
		}

		internal override object MatchElement (System.IO.Stream s, EbnfCompiler compiler)
		{
			long startPos = s.Position;
			object result = null;

			object arg;
			//bool success = false;

			foreach (var item in _list) {
				try {
					arg = item.MatchElement (s, compiler);
					result = ParseResult (arg, compiler);
					//success = true;
					//break;
					return result;
				} catch (EbnfElementException) {
					s.Position = startPos;
				}
			}		
			//if (!success) {
			throw ElementException ();
			//}
			//return result;
		}

		public override bool CanBeEmpty {
			get {
				foreach (var item in _list) {
					if (item.CanBeEmpty) {
						return true;
					}
				}
				return false;
			}
		}

		public override Ebnf Clone ()
		{
			EbnfChoice n = new EbnfChoice (this._list, this._scopeType);
			n.CompileAction = this.CompileAction;
			n.Label = this.Label;
			n.ParseAction = this.ParseAction;
			n._error = this._error;
			return n;
		}

		internal override string ToString (int depth)
		{
			if (depth < 0) {
				return " ... ";
			}

			string result;
			bool first = true;
			result = "(";
			foreach (var item in _list) {
				result += (first ? "" : ",") + item.ToString (depth - 1);
				first = false;
			}
			result += ")";
			return result;
		}
	}
}

