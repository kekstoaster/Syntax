using System;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// Parse action of Ebnf elements.
	/// </summary>
	public class ParseAction
	{
		Action<EbnfCompiler> _init;
		Func<SyntaxElement, bool> _neccessaryElement;
		Action<ScopeContext,  SyntaxElement[]> _parse;

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.ParseAction"/> class.
		/// </summary>
		/// <param name="neccessaryElement">A function that determines if the given element is necessary for the comiling process. If this function returns false, the element is not stored and the parse action cannot access it anymore.</param>
		/// <param name="parse">Actual parsing action. This is usually used for further parsing beyond simple syntax checking.</param>
		public ParseAction (Func<SyntaxElement, bool> neccessaryElement, Action<ScopeContext,  SyntaxElement[]> parse = null) : this (null, neccessaryElement, parse)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.ParseAction"/> class.
		/// </summary>
		/// <param name="parse">Actual parsing action. This is usually used for further parsing beyond simple syntax checking.</param>
		public ParseAction (Action<ScopeContext, SyntaxElement[]> parse = null) : this (null, null, parse)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.ParseAction"/> class.
		/// </summary>
		/// <param name="init">Initilization of an Ebnf element. This function is called only once for all Ebnf Elements with the same label. All Ebnf elements with an init function must be labeled</param>
		/// <param name="parse">Actual parsing action. This is usually used for further parsing beyond simple syntax checking.</param>
		public ParseAction (Action<EbnfCompiler> init, Action<ScopeContext,  SyntaxElement[]> parse = null) : this (init, null, parse)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.ParseAction"/> class.
		/// </summary>
		/// <param name="init">Initilization of an Ebnf element. This function is called only once for all Ebnf Elements with the same label. All Ebnf elements with an init function must be labeled</param>
		/// <param name="neccessaryElement">A function that determines if the given element is necessary for the comiling process. If this function returns false, the element is not stored and the parse action cannot access it anymore.</param>
		/// <param name="parse">Actual parsing action. This is usually used for further parsing beyond simple syntax checking.</param>
		public ParseAction (Action<EbnfCompiler> init, Func<SyntaxElement, bool> neccessaryElement, Action<ScopeContext,  SyntaxElement[]> parse = null)
		{
			this._init = init;
			this._neccessaryElement = neccessaryElement;
			this._parse = parse;
		}

		/// <summary>
		/// Gets a value indicating whether this instance has an initilization function.
		/// </summary>
		/// <value><c>true</c> if this instance has initilization; otherwise, <c>false</c>.</value>
		public bool HasInitilization {
			get { return _init != null; }
		}

		/// <summary>
		/// Determines if the specified arg is a neccessary element for compilation.
		/// </summary>
		/// <returns><c>true</c> if the specified arg is neccessary for compilation; otherwise, <c>false</c>.</returns>
		/// <param name="arg">The syntax element to check for.</param>
		public bool IsNeccessary (SyntaxElement arg)
		{
			try {
				if (this._neccessaryElement == null) {
					return true;
				} else {
					return this._neccessaryElement (arg);
				}
			} catch (ParseException) {
				throw;
			} catch (Exception ex) {
				throw new ParseException (ex.Message, ex);
			}
		}

		/// <summary>
		/// If an init function is present, it is called with the specified ScopeContext
		/// </summary>
		/// <param name="compiler">The compiler initializing the object.</param>
		public void Initialize (EbnfCompiler compiler)
		{
			if (this._init != null) {
				try {
					this._init (compiler);
				} catch (ParseException) {
					throw;
				} catch (Exception ex) {
					throw new ParseException (ex.Message, ex);
				}
			}
		}

		/// <summary>
		/// Parse the Ebnf element in the specified scope context with the nested syntax elements args.
		/// </summary>
		/// <param name="scope">The scope context in which the element is parsed.</param>
		/// <param name="args">The neccessary nested elements that were found for this Ebnf element.</param>
		public void Parse (ScopeContext scope, params SyntaxElement[] args)
		{
			if (this._parse != null) {
				try {
					this._parse (scope, args);
				} catch (ParseException) {
					throw;
				} catch (Exception ex) {
					throw new ParseException (ex.Message, ex);
				}
			}
		}
	}
}

