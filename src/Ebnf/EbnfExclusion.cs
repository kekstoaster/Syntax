using System;
using System.IO;
using System.Collections.Generic;

namespace Kekstoaster.Syntax
{
	public class EbnfExclusion:Ebnf
	{
		Ebnf _element;
		bool _serious;

		public EbnfExclusion (Ebnf element, ScopeType scopetype = ScopeType.Default) : base (scopetype)
		{
			_element = element;
		}

		internal override object MatchElement (Stream s, EbnfCompiler compiler)
		{
			long startPos = s.Position;
			object result = null;

			object arg = null;

			try {
				arg = _element.MatchElement (s, compiler);
			} catch (EbnfElementException) {
				s.Position = startPos;					
			}
			if (arg != null) {
				if (_serious) {
					if (this._error == null) {
						throw new ParseException ("element is not allowed at this position", this);
					} else {
						throw new ParseException (this._error, this);
					}
				} else {
					throw ElementException ();
				}
			} else {					
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

		public bool IsSerious {
			get { return _serious; }
		}

		public void Serious ()
		{
			_serious = true;
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

