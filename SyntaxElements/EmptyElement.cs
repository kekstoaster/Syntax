using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// Empty syntax element, meaning it contains no nested Elements or the nested elements are not important.
	/// </summary>
	public sealed class EmptyElement:SyntaxElement {
		private CompileAction _compile = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.EmptyElement"/> class.
		/// </summary>
		/// <param name="compiler">The compiler compiling the element.</param>
		public EmptyElement(EbnfCompiler compiler):base(compiler) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.EmptyElement"/> class.
		/// </summary>
		/// <param name="compiler">The compiler compiling the element.</param>
		/// <param name="compile">The compile action used to compile the element.</param>
		public EmptyElement(EbnfCompiler compiler, CompileAction compile):base(compiler) {
			this._compile = compile;
		}

		/// <summary>
		/// Gets the text representation of the element.
		/// </summary>
		/// <value>The text.</value>
		public override string Text {
			get {
				return "";
			}
		}

		/// <summary>
		/// Compile this element in the specified ScopeContext.
		/// </summary>
		/// <param name="parentContext">Parent context.</param>
		internal override object Compile(ScopeContext parentContext) {
			if (this._compile != null) {
				return this._compile.Compile (parentContext, null);
			} else {
				return null;
			}
		}
	}
}

