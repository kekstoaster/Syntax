using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// Compile action of Ebnf elements.
	/// </summary>
	public class CompileAction {
		Func<ScopeContext, object[], object> _compile;

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.CompileAction"/> class.
		/// </summary>
		/// <param name="compile">Actual compiling action.</param>
		public CompileAction (Func<ScopeContext,  object[], object> compile)
		{
			this._compile = compile;
		}

		/// <summary>
		/// Compile the Ebnf element in the specified scope context with the nested syntax elements args.
		/// </summary>
		/// <param name="scope">The scope context in which the element is compiled.</param>
		/// <param name="args">The neccessary nested elements that were found for this Ebnf element.</param>
		public object Compile(ScopeContext scope, params object[] args) {
			try {
				return _compile (scope, args);
			} catch (CompileException) {
				throw;
			} catch (Exception ex) {
				throw new CompileException (ex.Message, ex);
			}
		}
	}
}

