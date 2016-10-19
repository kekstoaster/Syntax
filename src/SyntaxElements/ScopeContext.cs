using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// Scope context containing local and global variables for parsing and compiling.
	/// </summary>
	public class ScopeContext {

		private ItemList<object> _locals;
		private SyntaxScope _scope;

		internal ScopeContext(SyntaxScope scope) {
			this._scope = scope;
			this._locals = new ItemList<object> ();
		}

		/// <summary>
		/// Gets the global variables of the EbnfCompiler
		/// </summary>
		/// <value>The global variable ItemList.</value>
		public ItemList<object> Globals {
			get {
				return this._scope.Compiler.Globals;
			}
		}

		/// <summary>
		/// Gets the locals variable list
		/// </summary>
		/// <value>ItemList of the local variables in the SyntaxScope</value>
		public ItemList<object> Locals {
			get {
				return _locals;
			}
		}

		/// <summary>
		/// Gets the parent scope.
		/// </summary>
		/// <value>The parent scope</value>
		public ScopeContext Parent {
			get {
				return _scope.Parent != null ? _scope.Parent.Context : null;
			}
		}
	}
}

