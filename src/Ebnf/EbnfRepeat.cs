using System;
using System.IO;
using System.Collections.Generic;

namespace Kekstoaster.Syntax
{
	public class EbnfRepeat:Ebnf
	{
		Ebnf _element;
		int[] _range;

		public EbnfRepeat (Ebnf element, ScopeType scopetype = ScopeType.Default) : base (scopetype)
		{
			_element = element;
			_range = null;
		}

		public EbnfRepeat (Ebnf element, int min, ScopeType scopetype = ScopeType.Default) : base (scopetype)
		{
			_element = element;
			_range = new int[] { min };
		}

		public EbnfRepeat (Ebnf element, int min, int max, ScopeType scopetype = ScopeType.Default) : base (scopetype)
		{
			if (min > max) {
				throw new ArgumentException ("Minimun value must be smaller than maximum value.");
			}
			_element = element;
			_range = new int[] { min, max };
		}

		internal override object MatchElement (Stream s, EbnfCompiler compiler)
		{
			long startPos = s.Position;
			object result = null;

			object arg;
			int count = 0;
			List<SyntaxElement> allArgs = new List<SyntaxElement> ();
			try {
				while (true) {
					arg = _element.MatchElement (s, compiler);
					AddArgToList (_element, arg, allArgs, compiler);
					count++;
					startPos = s.Position;
				}						
			} catch (EbnfElementException) {
				s.Position = startPos;
			}

			if (_range != null && !(_range [0] <= count && (_range.Length <= 1 || _range [1] >= count))) {
				throw new EbnfElementException ();
			}

			if (allArgs.Count == 0) {
				result = IgnoredElement.Instance;
			} else {
				result = ParseResult (allArgs.ToArray (), compiler);
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
				if (_range != null && _range.Length > 0 && _range [0] > 0) {
					return false;
				} else {
					return true;
				}
			}
		}

		public override Ebnf Clone ()
		{
			EbnfRepeat n = new EbnfRepeat (_element, this._scopeType);
			n._range = new int[this._range.Length];
			this._range.CopyTo (n._range, 0);
			n.CompileAction = this.CompileAction;
			n.Label = this.Label;
			n.ParseAction = this.ParseAction;
			n._error = this._error;
			return n;
		}

		internal override string ToString (int depth)
		{			
			return "{" + this._element.ToString (depth - 1) + "}";
		}
	}

}

