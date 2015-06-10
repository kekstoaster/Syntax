using System;
using System.Text;
using System.Collections.Generic;


namespace Kekstoaster.Syntax
{
	/// <summary>
	/// A syntax element that is used if an Ebnf element has a custom parse or compile action.
	/// </summary>
	public class SyntaxScope : SyntaxElement
	{
		private List<SyntaxElement> _scopeContent;
		private ParseAction _parse = null;
		private CompileAction _compile = null;
		private ScopeContext _context;

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.SyntaxScope"/> class.
		/// </summary>
		/// <param name="compiler">The compiler used to compile the corresponding Ebnf Element</param>
		/// <param name="parse">The parse action of the Ebnf element</param>
		/// <param name="compile">The compile action of the Ebnf element</param>
		public SyntaxScope(EbnfCompiler compiler, ParseAction parse, CompileAction compile):this(compiler, parse, compile, null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.SyntaxScope"/> class.
		/// </summary>
		/// <param name="compiler">The compiler used to compile the corresponding Ebnf Element</param>
		/// <param name="parse">The parse action of the Ebnf element</param>
		/// <param name="compile">The compile action of the Ebnf element</param>
		/// <param name="parent">The parent syntax scope</param>
		public SyntaxScope(EbnfCompiler compiler, ParseAction parse, CompileAction compile, SyntaxScope parent):base(compiler, parent) {
			this._compile = compile;
			this._parse = parse;
			this._scopeContent = new List<SyntaxElement> ();
			this._context = new ScopeContext (this);
		}

		/// <summary>
		/// Gets the text associated with the element.
		/// </summary>
		public override string Text {
			get {
				string s = "";
				foreach (var item in _scopeContent) {
					s += item.Text;
				}
				return s;
			}
		}

		/// <summary>
		/// Adds the specified text.
		/// </summary>
		/// <param name="text">Text.</param>
		public void Add(string text) {
			_scopeContent.Add (new SyntaxText(this.Compiler, text));
		}

		internal void Add(SyntaxElement element) {
			_scopeContent.Add (element);
		}

		internal void AddRange(SyntaxElement[] elements) {
			foreach (var item in elements) {
				Add (item);
			}
		}

		internal override object Compile(ScopeContext parentContext) {
			object[] compiled = new object[_scopeContent.Count];
			for (int i = 0; i < _scopeContent.Count; i++) {
				compiled [i] = _scopeContent [i].Compile (this.Context);
			}
			if (this._compile != null) {
				return this._compile.Compile (parentContext, compiled);
			} else {
				switch (this.Compiler.StandardCompile) {
					case EbnfCompileBehavior.Text:
						StringBuilder sb = new StringBuilder(compiled.Length);
						foreach (var item in compiled) {
							sb.Append(item.ToString());
						}
						return sb.ToString();
					case EbnfCompileBehavior.List:
						return compiled;
					default: // Ignore
						return null;
				}
			}
		}

		internal void Parse(ScopeContext parentContext) {
			foreach (var item in _scopeContent) {
				if(item is SyntaxScope) {
					((SyntaxScope)item).Parse (this.Context);
				}
			}
			this._parse.Parse (parentContext, _scopeContent.ToArray ());
		}

		/// <summary>
		/// Gets the scope context of this syntax scope.
		/// A scope context is used for parsing and compiling child items.
		/// For parsing and compiling this syntax scope the context of the 
		/// parent scope is used.
		/// </summary>
		/// <value>The context.</value>
		public ScopeContext Context {
			get {
				return this._context;
			}
		}

		/// <summary>
		/// Determines if the specified arg is a neccessary element for compilation.
		/// </summary>
		/// <returns><c>true</c> if the specified arg is neccessary for compilation; otherwise, <c>false</c>.</returns>
		/// <param name="arg">The syntax element to check for.</param>
		public bool IsNeccessary (SyntaxElement arg) {
			return this._parse.IsNeccessary (arg);
		}
	}
}

