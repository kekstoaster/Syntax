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
		private static ParseAction DefaultParseAction = new ParseAction (new Action<ScopeContext, SyntaxElement[]> (delegate(ScopeContext s1, SyntaxElement[] s2) { }));

		internal SyntaxElement Parse(Stream s, EbnfCompiler compiler, out ScopeContext context) {
			//EbnfStandardBehavior behav = EbnfStandardBehavior.List, ScopeType stdScope = ScopeType.Inhired
			ScopeType scopetype = GetScopeType (compiler.StandardScope);
			if(scopetype == ScopeType.Parent) {
				throw new ParseException ("Cannot parse element with ScopeType.Parent");
			}

			SyntaxElement e = (SyntaxElement)ParseElement (s, compiler);
			SyntaxScope baseScope = new SyntaxScope (compiler, null, null);
			e.Parent = baseScope;
			context = new ScopeContext (baseScope);
			if(e is SyntaxScope) {
				(e as SyntaxScope).Parse (context);
			}
			return e;
		}

		private SyntaxElement ToDefaultCompile(EbnfCompiler compiler, params SyntaxElement[] args){
			SyntaxElement result;
			if (args != null || args.Length == 0) {
				if (args.Length == 1) {
					result = args [0];
				} else {
					switch (compiler.StandardCompile) {
						case EbnfCompileBehavior.List:
							result = new SyntaxList (compiler, args);
							break;
						case EbnfCompileBehavior.Text:
							result = new SyntaxText (compiler, args);
							break;
						default: // Ignore
							result = new EmptyElement(compiler);
							break;
					}
				}
			} else {
				result = new EmptyElement(compiler);
			}
			return result;
		}

		private object ParseResult(object arg, EbnfCompiler compiler) {
			object result = null;
			ScopeType scopetype = GetScopeType (compiler.StandardScope);

			if (this.Scoped(compiler.StandardScope)) {
				if(scopetype == ScopeType.Default || scopetype == ScopeType.Inhired || scopetype == ScopeType.Force) {
					if(this._parse == null) {
						this._parse = DefaultParseAction;
					}
					try {
						result = new SyntaxScope (compiler, this._parse, this._compile);
						((SyntaxScope)result).Label = this._label;
						if(this._parse.HasInitilization) {
							if(this._label == null) {
								throw new ParseException("Only labeld elements can be initialized");
							}
							if(compiler.Initialization(this._label)) {
								this._parse.Initialize (compiler);
							}
						}
					} catch(ParseException){
						throw;
					} catch (Exception ex) {
						if (this._error == null) {
							throw ex;
						} else {
							throw new ParseException (this._error, ex, this);
						}
					}

					if(arg is SyntaxElement) {
						if (!(arg is IgnoredElement) && ((SyntaxScope)result).IsNeccessary((SyntaxElement)arg)) {
							((SyntaxScope)result).Add ((SyntaxElement)arg);
							((SyntaxElement)arg).Parent = (SyntaxScope)result;
						}
					} else { // SyntaxElement[]
						// TODO: List<SyntaxElement>
						foreach (var a in (SyntaxElement[])arg) {
							if(((SyntaxScope)result).IsNeccessary(a)) {
								((SyntaxScope)result).Add(a);
								a.Parent = (SyntaxScope)result;
							}
						}
					}
				} else {
					if(scopetype == ScopeType.Empty) {
						result = new EmptyElement (compiler, this._compile);
						((SyntaxElement)result).Label = this._label;
					} else { // ScopeType.Parent
						result = arg;
					}
				}						
			} else { // Default
				if (arg is SyntaxElement) {
					result = ToDefaultCompile (compiler, (SyntaxElement)arg);
				} else { // SyntaxElement[]
					result = ToDefaultCompile (compiler, (SyntaxElement[])arg);
				}
				((SyntaxElement)result).Label = this._label;
			}

			return result;
		}

		private bool AddArgToList(Ebnf item, object arg, List<SyntaxElement> allArgs, EbnfCompiler compiler){
			ScopeType scopetype = this.GetScopeType (compiler.StandardScope);
			if(arg is SyntaxElement) {
				if(arg is IgnoredElement) {
					return false;
				}
				if(item.Scoped(compiler.StandardScope) && (scopetype == ScopeType.Inhired || scopetype == ScopeType.Default )) {
					this._scopeType = ScopeType.Force;
				}
				allArgs.Add((SyntaxElement)arg);

				return true;
			} else {
				// arg is SyntaxElement[]
				// returned list contains no IgnoredElements
				if(((SyntaxElement[])arg).Length != 0) {
					if(scopetype == ScopeType.Inhired || scopetype == ScopeType.Default) {
						this._scopeType = ScopeType.Force;
					}
					allArgs.AddRange((SyntaxElement[])arg);
					return true;
				}
			}
			return false;
		}

		// ScopeType.Parent:  returns SyntaxElement[]
		// ScopeType.Force:   returns SyntaxScope
		// ScopeType.Inhired: returns SyntaxElement
		private object ParseElement(Stream s, EbnfCompiler compiler) {
			long startPos = s.Position;
			object result = null;
			ScopeType scopetype = this.GetScopeType (compiler.StandardScope);

			switch (this._type) {
				case EbnfType.Choise:
				{
					object arg;
					bool success = false;

					foreach (var item in _list) {
						try {
							arg = item.ParseElement(s, compiler);
							if(!(arg is IgnoredElement) && item.Scoped(compiler.StandardScope) && (scopetype == ScopeType.Inhired || scopetype == ScopeType.Default)) {
								this._scopeType = ScopeType.Force;
							}

							result = ParseResult(arg, compiler);
							
							success = true;
							break;
						} catch (EbnfElementException) {
							s.Position = startPos;
						}
					}		
					if (!success) {
						if (this._error == null) {
							throw new EbnfElementException ();
						} else {
							throw new EbnfElementException (this._error);
						}
					}
				break;
				}
				case EbnfType.Optional:
					try {
						object arg;

						arg = _list[0].ParseElement (s, compiler);

						if(!(arg is IgnoredElement) && _list[0].Scoped(compiler.StandardScope) && (scopetype == ScopeType.Inhired || scopetype == ScopeType.Default)) {
							this._scopeType = ScopeType.Force;
						}

						result = ParseResult(arg, compiler);

					} catch (EbnfElementException) {
						s.Position = startPos;
						result = IgnoredElement.Instance;
					}
					break;
				case EbnfType.List:
				{
					object arg;
					List<SyntaxElement> allArgs = new List<SyntaxElement> ();
					bool matches = false;
					try {						
						foreach (var item in _list) {
							arg = item.ParseElement (s, compiler);
							matches = matches | AddArgToList(item, arg, allArgs, compiler);
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
					result = ParseResult(allArgs.ToArray(), compiler);
					break;
				}
				case EbnfType.Repeat:
				{
					object arg;
					int count = 0;
					List<SyntaxElement> allArgs = new List<SyntaxElement> ();
					try {
						while (true) {
							arg = _list [0].ParseElement (s, compiler);
							AddArgToList(_list [0], arg, allArgs, compiler);
							count++;
							startPos = s.Position;
						}						
					} catch (EbnfElementException) {
						s.Position = startPos;
					}

					// range saved ib _char, ensure min and max 
					if(_char != null && !( (int)_char[0] <= count && (_char.Length <= 1 || (int)_char[1]>=count)) ) {
						throw new EbnfElementException ();
					}

					if (allArgs.Count == 0) {
						result = IgnoredElement.Instance;
					} else {
						result = ParseResult(allArgs.ToArray(), compiler);
					}
					break;
				}
				case EbnfType.Not:
				{
					object arg = null;

					try {
						arg = _list [0].ParseElement (s, compiler);
					} catch (EbnfElementException) {
						s.Position = startPos;					
					}
					if(arg != null) {
						if(this._error == null) {
							throw new EbnfElementException ();
						} else {
							throw new EbnfElementException (this._error);
						}
					}else{					
						result = IgnoredElement.Instance;
					}
					break;
				}
				case EbnfType.Permutation:
				{
					object arg;
					long permPos = startPos;
					List<SyntaxElement> allArgs = new List<SyntaxElement> ();
					bool matches = false;
					int length = _list.Count;
					bool[] positions = new bool[length];

					for (int i = 0; i < length; i++) {
						for (int j = 0; j < length; j++) {
							if (positions [j]) continue;
							try {
								arg = _list[j].ParseElement(s, compiler);
								if(arg is IgnoredElement) {
									continue;
								}
								matches = matches | AddArgToList(_list[j], arg, allArgs, compiler);
								positions[j] = true;
								permPos = s.Position;
								break;
							} catch (EbnfElementException) {
								s.Position = permPos;
							}
						}
					}
					for (int i = 0; i < length; i++) {
						if(!positions[i] && (_list[i]._type != EbnfType.Optional && _list[i]._type != EbnfType.Not && _list[i]._type != EbnfType.Repeat)) {
							s.Position = startPos;
							if (result == null) {
								if (matches && this._unique) {
									throw new ParseException ("Unique ELement found but not fully matched");
								} else {
									if (this._error == null) {
										throw new EbnfElementException ();
									} else {
										throw new EbnfElementException (this._error);
									}
								}
							}
						}
					}
					if (!matches) {
						result = IgnoredElement.Instance;
					} else {
						result = ParseResult(allArgs.ToArray(), compiler);
					}
					break;
				}
				case EbnfType.EOF:
				{					
					int next = s.ReadByte ();
					if (next == -1) {
						SyntaxElement eof = new EmptyElement (compiler);
						eof.Label = Ebnf.EOF_LABEL;
						result = ParseResult(eof, compiler);
					} else {
						if(this._error == null) {
							throw new EbnfElementException ();
						} else {
							throw new EbnfElementException (this._error);
						}
					}
					break;
				}
				case EbnfType.Any:
				{					
					SyntaxElement c;
					int next = s.ReadByte ();
					//Debug.Write ((char)next);
					if(next != -1) {
						c = new SyntaxText (compiler,(char)next);
						c.Label = this._label;
					}else{
						c = new EmptyElement(compiler);
						c.Label = Ebnf.EOF_LABEL;
					}
					result = ParseResult(c, compiler);
					break;
				}
				case EbnfType.Range:
				{					
					int next = s.ReadByte ();
					//Debug.Write ((char)next);
					if (next >= this._char[0] && next <= this._char[1]) {
						SyntaxElement c;
						c = new SyntaxText (compiler, (char)next);
						c.Label = this._label;
						result = ParseResult(c, compiler);
					} else {
						if(this._error == null) {
							throw new EbnfElementException ();
						} else {
							throw new EbnfElementException (string.Format (this._error, (char)next));
						}
					}
					break;
				}
				default: // Char
				{
					int next = s.ReadByte ();
					//Debug.Write ((char)next);
					if ((char)next == this._char[0]) {
						SyntaxElement c;
						c = new SyntaxText (compiler, this._char[0]);
						c.Label = this._label;
						result = ParseResult(c, compiler);
					} else {
						if(this._error == null) {
							throw new EbnfElementException ();
						} else {
							throw new EbnfElementException (string.Format (this._error, (char)next));
						}
					}
					break;
				}
			}

			return result;
		}
	}
}