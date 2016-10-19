using System;
using System.IO;
using System.Collections.Generic;

namespace Kekstoaster.Syntax
{
	public class EbnfPermutation:EbnfEnumerable, IEbnfUnique
	{
		private bool _unique;

		public EbnfPermutation (ScopeType scopetype = ScopeType.Default) : base (scopetype)
		{
			_unique = false;
		}

		public EbnfPermutation (IEnumerable<Ebnf> elements, ScopeType scopetype = ScopeType.Default) : this (scopetype)
		{
			if (elements != null) {
				foreach (var item in elements) {
					_list.Add (item);
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is unique.
		/// A unique element throws a ParseException when it was partly but not fully matched.
		/// </summary>
		/// <value><c>true</c> if this instance is unique; otherwise, <c>false</c>.</value>
		public bool IsUnique { 
			get { 
				return _unique;
			}
		}

		/// <summary>
		/// Sets a value indicating that this instance is unique.
		/// </summary>
		public void Unique ()
		{
			_unique = true;
		}

		internal override object MatchElement (Stream s, EbnfCompiler compiler)
		{
			long startPos = s.Position;
			object result = null;

			object arg;
			long permPos = startPos;
			List<SyntaxElement> allArgs = new List<SyntaxElement> ();
			bool matches = false;
			int length = _list.Count;
			bool[] positions = new bool[length];

			for (int i = 0; i < length; i++) {
				for (int j = 0; j < length; j++) {
					if (positions [j])
						continue;
					try {
						arg = _list [j].MatchElement (s, compiler);
						if (arg is IgnoredElement) {
							continue;
						}
						matches = matches | AddArgToList (_list [j], arg, allArgs, compiler);
						positions [j] = true;
						permPos = s.Position;
						break;
					} catch (EbnfElementException) {
						s.Position = permPos;
					}
				}
			}
			for (int i = 0; i < length; i++) {
				if (!positions [i] && (_list [i].CanBeEmpty)) {
					s.Position = startPos;
					if (result == null) {
						if (matches && this._unique) {
							throw new ParseException ("Unique ELement found but not fully matched");
						} else {
							ThrowElementException ();
						}
					}
				}
			}
			if (!matches) {
				result = IgnoredElement.Instance;
			} else {
				result = ParseResult (allArgs.ToArray (), compiler);
			}

			return result;
		}

		public override bool CanBeEmpty {
			get {
				foreach (var item in _list) {
					if (!item.CanBeEmpty) {
						return false;
					}
				}
				return true;
			}
		}

		public override Ebnf Clone ()
		{
			EbnfPermutation n = new EbnfPermutation (this._list, this._scopeType);
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

			string result = "<";
			bool first = true;
			foreach (var item in _list) {
				result += (first ? "" : ",") + item.ToString (depth - 1);
				first = false;
			}
			result += ">";
			return result;
		}
	}
}

