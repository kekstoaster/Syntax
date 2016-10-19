using System;
using System.IO;
using System.Collections.Generic;

namespace Kekstoaster.Syntax
{
	public class EbnfList:EbnfEnumerable, IEbnfUnique
	{
		bool _unique;

		public EbnfList (ScopeType scopetype = ScopeType.Default) : base (scopetype)
		{
			_unique = false;
		}

		public EbnfList (IEnumerable<Ebnf> elements, ScopeType scopetype = ScopeType.Default) : this (scopetype)
		{
			if (elements != null) {
				_list.AddRange (elements);
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
			List<SyntaxElement> allArgs = new List<SyntaxElement> ();
			bool matches = false;
			try {						
				foreach (var item in _list) {
					arg = item.MatchElement (s, compiler);
					matches = matches | AddArgToList (item, arg, allArgs, compiler);
				}
			} catch (Exception ex) {
				if (matches && this._unique) {
					throw new ParseException ("Unique ELement found but not fully matched");
				} else {
					if (this._error == null) {
						throw ex;
					} else {
						throw new EbnfElementException (this._error, ex, this);
					}
				}
			}
			result = ParseResult (allArgs.ToArray (), compiler);

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
			EbnfList n = new EbnfList (this._list, this._scopeType);
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

			string result = "";
			foreach (var item in _list) {
				result += item.ToString (depth - 1);
			}
			return result;
		}
	}
}

