using System;
using System.Collections.Generic;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// A syntax element that combines all nested elements to a single string
	/// </summary>
	public class SyntaxText:SyntaxElement
	{
		private string _text;

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.SyntaxText"/> class.
		/// </summary>
		/// <param name="compiler">The compiler used to compile the corresponding Ebnf Element</param>
		/// <param name="list">The list of all nested syntax elements</param>
		internal SyntaxText (ParseAction parse, EbnfCompiler compiler, IEnumerable<SyntaxElement> list) : base (parse, compiler)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			foreach (var item in list) {
				sb.Append (item.Text);
			}
			_text = sb.ToString ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.SyntaxText"/> class.
		/// </summary>
		/// <param name="compiler">The compiler used to compile the corresponding Ebnf Element</param>
		/// <param name="text">Sets the text content of this element to the specified text</param>
		public SyntaxText (ParseAction parse, EbnfCompiler compiler, string text) : base (parse, compiler)
		{
			this._text = text;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.SyntaxText"/> class.
		/// </summary>
		/// <param name="compiler">The compiler used to compile the corresponding Ebnf Element</param>
		/// <param name="c">Sets the text content of this element to the specified character</param>
		public SyntaxText (ParseAction parse, EbnfCompiler compiler, char c) : base (parse, compiler)
		{
			this._text = c.ToString ();
		}

		internal override object Compile (ScopeContext parentContext)
		{
			return Text;
		}

		/// <summary>
		/// Gets the text associated with the element.
		/// </summary>
		public override string Text {
			get {
				return _text;
			}
		}
	}
}

