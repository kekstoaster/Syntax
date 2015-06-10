using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// Base syntax element for parsing.
	/// </summary>
	public abstract class SyntaxElement {
		private SyntaxScope _parent;
		private string _label = null;
		private EbnfCompiler _compiler;

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.SyntaxElement"/> class.
		/// </summary>
		/// <param name="compiler">The compiler used to compile the corresponding Ebnf Element</param>
		protected SyntaxElement(EbnfCompiler compiler):this(compiler, null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.SyntaxElement"/> class.
		/// </summary>
		/// <param name="compiler">The compiler used to compile the corresponding Ebnf Element</param>
		/// <param name="parent">The parent syntax scope</param>
		protected SyntaxElement(EbnfCompiler compiler, SyntaxScope parent) {
			this._compiler = compiler;
			this._parent = parent;
		}

		/// <summary>
		/// Gets the parent syntax scope.
		/// </summary>
		/// <value>The parent.</value>
		public SyntaxScope Parent {
			get { return this._parent; }
			internal set { this._parent = value; }
		}

		internal EbnfCompiler Compiler {
			get {
				return this._compiler;
			}
		}

		internal abstract object Compile (ScopeContext parentContext);	

		/// <summary>
		/// Gets the text associated with the element.
		/// </summary>
		public abstract string Text{ get; }

		/// <summary>
		/// Gets the label of SyntaxElement. This is usually the same as the corresponding Ebnf element.
		/// </summary>
		/// <value>The label.</value>
		public string Label {
			get { return this._label; }
			internal set{ this._label = value; }
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Kekstoaster.Syntax.SyntaxElement"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Kekstoaster.Syntax.SyntaxElement"/>.</returns>
		public override string ToString ()
		{
			return Text;
		}
	}
}

