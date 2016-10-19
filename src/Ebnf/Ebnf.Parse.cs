using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

// TODO: Implement Parsing time: just in time, after syntax check
// right now just after syntax

namespace Kekstoaster.Syntax
{
	public partial class Ebnf
	{
		internal static ParseAction DefaultParseAction = new ParseAction (new Action<ScopeContext, SyntaxElement[]> (delegate(ScopeContext s1, SyntaxElement[] s2) {
		}));

		internal SyntaxElement Parse (Stream s, EbnfCompiler compiler, out ScopeContext context)
		{
			//EbnfStandardBehavior behav = EbnfStandardBehavior.List, ScopeType stdScope = ScopeType.Inhired
			ScopeType scopetype = this.GetScopeType (compiler.StandardScope);
			if (scopetype == ScopeType.Parent) {
				throw new ParseException ("Cannot parse element with ScopeType.Parent");
			}

			SyntaxElement e = (SyntaxElement)MatchElement (s, compiler);
			SyntaxScope baseScope = new SyntaxScope (compiler, null, null);
			e.Parent = baseScope;
			context = new ScopeContext (baseScope);
			e.Parse (context);
			return e;
		}

		private SyntaxElement ToDefaultCompile (EbnfCompiler compiler, params SyntaxElement[] args)
		{
			SyntaxElement result;
			if (args != null) {				
				switch (compiler.StandardCompile) {
				case EbnfCompileBehavior.List:
					{
						List<SyntaxElement> el = new List<SyntaxElement> ();
						foreach (var item in args) {
							if (_parse.IsNeccessary (item)) {
								el.Add (item);
							}
						}
						result = new SyntaxList (this._parse, compiler, el.ToArray ());
						break;
					}
				case EbnfCompileBehavior.Text:
					{
						List<SyntaxElement> el = new List<SyntaxElement> ();
						foreach (var item in args) {
							if (_parse.IsNeccessary (item)) {
								el.Add (item);
							}
						}
						result = new SyntaxText (this._parse, compiler, el);
						break;
					}
				default: // Ignore
					result = new EmptyElement (this._parse, compiler);
					break;
				}
			} else {
				result = new EmptyElement (this._parse, compiler);
			}
			return result;
		}

		protected void ThrowElementException ()
		{
			if (this._error == null) {
				throw new EbnfElementException ();
			} else {
				throw new EbnfElementException (this._error);
			}
		}

		protected object ParseResult (object arg, EbnfCompiler compiler)
		{
			object result = null;
			ScopeType scopetype = GetScopeType (compiler.StandardScope);
			if (this._parse == null) {
				this._parse = DefaultParseAction;
			}

			switch (scopetype) {
			case ScopeType.Force:				
				try {
					result = new SyntaxScope (compiler, this._parse, this._compile);
					((SyntaxScope)result).Label = this._label;
					if (this._parse.HasInitilization) {
						if (this._label == null) {
							throw new ParseException ("Only labeled elements can be initialized");
						}
						if (compiler.Initialization (this._label)) {
							this._parse.Initialize (compiler);
						}
					}
				} catch (ParseException) {
					throw;
				} catch (Exception ex) {
					if (this._error == null) {
						throw new ParseException (ex.Message, ex);
					} else {
						throw new ParseException (this._error, ex, this);
					}
				}

				if (arg is SyntaxElement) {
					if (!(arg is IgnoredElement) && ((SyntaxScope)result).IsNeccessary ((SyntaxElement)arg)) {
						((SyntaxScope)result).Add ((SyntaxElement)arg);
						((SyntaxElement)arg).Parent = (SyntaxScope)result;
					}
				} else { // SyntaxElement[]
					// TODO: List<SyntaxElement>
					foreach (var a in (SyntaxElement[])arg) {
						if (((SyntaxScope)result).IsNeccessary (a)) {
							((SyntaxScope)result).Add (a);
							a.Parent = (SyntaxScope)result;
						}
					}
				}
				break;
			case ScopeType.Parent:
				result = arg;
				break;
			case ScopeType.Empty:
				result = new EmptyElement (this._parse, compiler, this._compile);
				((SyntaxElement)result).Label = this._label;
				if (this._parse.HasInitilization) {
					if (this._label == null) {
						throw new ParseException ("Only labeled elements can be initialized");
					}
					if (compiler.Initialization (this._label)) {
						this._parse.Initialize (compiler);
					}
				}
				break;
			default:
				if (arg is SyntaxElement) {
					result = ToDefaultCompile (compiler, (SyntaxElement)arg);
				} else { // SyntaxElement[]
					result = ToDefaultCompile (compiler, (SyntaxElement[])arg);
				}
				((SyntaxElement)result).Label = this._label;
				break;
			}

			return result;
		}

		protected bool AddArgToList (Ebnf item, object arg, List<SyntaxElement> allArgs, EbnfCompiler compiler)
		{
			if (arg is SyntaxElement) {
				if (arg is IgnoredElement) {
					return false;
				}
				allArgs.Add ((SyntaxElement)arg);

				return true;
			} else {
				// arg is SyntaxElement[]
				// returned list contains no IgnoredElements
				if (((SyntaxElement[])arg).Length != 0) {
					allArgs.AddRange ((SyntaxElement[])arg);
					return true;
				}
			}
			return false;
		}

		// ScopeType.Parent:  returns SyntaxElement[]
		// ScopeType.Force:   returns SyntaxScope
		// ScopeType.Inhired: returns SyntaxElement
		internal abstract object MatchElement (Stream s, EbnfCompiler compiler);
	}
}