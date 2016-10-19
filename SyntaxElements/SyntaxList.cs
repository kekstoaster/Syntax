using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// A syntax element, that lists all nested syntax elements.
	/// </summary>
	public class SyntaxList:SyntaxElement
	{
		private SyntaxElement[] _list;

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.SyntaxList"/> class.
		/// </summary>
		/// <param name="compiler">The compiler used to compile the corresponding Ebnf Element</param>
		/// <param name="list">The list of all nested syntax elements</param>
		internal SyntaxList (ParseAction parse, EbnfCompiler compiler, SyntaxElement[] list) : base (parse, compiler)
		{
			_list = list;
		}

		internal override object Compile (ScopeContext parentContext)
		{
			object[] objList = new object[_list.Length];
			for (int i = 0; i < _list.Length; i++) {
				if (_list [i] is SyntaxElement) {				
					objList [i] = _list [i].Compile (parentContext);
				} else {
					objList [i] = _list [i];
				}
			}
			return objList;
		}

		/// <summary>
		/// Gets the text associated with the element.
		/// </summary>
		public override string Text {
			get {
				string s = "";
				foreach (var item in this._list) {
					if (item != null) {
						s += item.Text;
					}
				}
				return s;
			}
		}
	}
}