using System;
using Kekstoaster.Syntax;
using System.Collections.Generic;

namespace BrainfuckExample
{
	public class Brainfuck
	{
		public const int SIZE = 30000;
		private int[] _store;
		private int _ptr;
		private BrainfuckCommand _currentCommand;
		private BrainfuckCommand _root;

		internal Brainfuck (BrainfuckCommand root)
		{
			_root = root;
		}

		internal void PointerIncrement ()
		{
			++_ptr;
			if (_ptr > _store.Length) {
				throw new Exception ("Pointer exceeds array size.");
			}
		}

		internal void PointerDecrement ()
		{
			--_ptr;
			if (_ptr < 0) {
				throw new Exception ("Pointer below 0.");
			}
		}

		internal void Increment ()
		{
			_store [_ptr]++;
			if (_store [_ptr] > 255) {
				//	throw new Exception("overflow, item exceeds 255");
			}
		}

		internal void Decrement ()
		{
			_store [_ptr]--;
			if (_store [_ptr] < 0) {
				//throw new Exception("Min value must be 0");
			}
		}

		internal void PutChar ()
		{
			Console.Write ((char)_store [_ptr]);
		}

		internal void GetChar ()
		{
			int c = Console.Read ();
			if (c >= 0 && c <= 255) {
				_store [_ptr] = c;
			} else {
				throw new Exception ("char must be between 0 and 255");
			}
		}

		public void Run ()
		{
			_store = new int[SIZE];
			_ptr = 0;
			_currentCommand = _root;

			while (_currentCommand != null) {
				_currentCommand.Call (this);
				_currentCommand = _currentCommand.Next;
			}

		}

		public int CurrentValue {
			get {
				return _store [_ptr];
			}
		}

		public int CurrentPosition {
			get {
				return _ptr;
			}
		}

		internal BrainfuckCommand Current {
			get {
				return _currentCommand;
			}
			set {
				this._currentCommand = value;
			}
		}

		private static Ebnf _syntax = Syntax;

		public static Brainfuck GetProgram (string text)
		{
			EbnfCompiler cmp = new EbnfCompiler (_syntax);
			Brainfuck done = (Brainfuck)cmp.Compile (text);
			return done;
		}

		private static Ebnf Syntax {
			get {
				Ebnf ptrIncr = '>',
				ptrDecr = '<',
				valIncr = '+',
				valDecr = '-',
				putChar = '.',
				getChar = ',',
				leftPar = '[',
				righPar = ']';

				Ebnf allChars = ptrIncr | ptrDecr | valIncr | valDecr | putChar | getChar | leftPar | righPar;
				allChars.ScopeType = ScopeType.Parent;

				Ebnf comment = (Ebnf.EmptyList + (-allChars) + (-Ebnf.EOF) + Ebnf.AnyChar);
				comment.ScopeType = ScopeType.Empty;

				EbnfEnumerable loop = Ubnf.EmptyList;
				EbnfEnumerable command = Ebnf.EmptyChoise + ptrIncr + ptrDecr + valIncr + valDecr + putChar + getChar + loop + comment;
				command.ScopeType = ScopeType.Parent;

				Ebnf anyLoop = Ebnf.Repeat (command);
				anyLoop.ScopeType = ScopeType.Parent;
				loop = loop + leftPar + anyLoop + righPar;

				Ebnf program = ~command;


				ptrIncr.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext scope, object[] args) {
					return new BrainfuckCommand (BfCommandType.PointerIncrement);
				}));
				ptrDecr.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext scope, object[] args) {
					return new BrainfuckCommand (BfCommandType.PointerDecrement);
				}));
				valIncr.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext scope, object[] args) {
					return new BrainfuckCommand (BfCommandType.ValueIncrement);
				}));
				valDecr.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext scope, object[] args) {
					return new BrainfuckCommand (BfCommandType.ValueDecrement);
				}));
				putChar.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext scope, object[] args) {
					return new BrainfuckCommand (BfCommandType.PutChar);
				}));
				getChar.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext scope, object[] args) {
					return new BrainfuckCommand (BfCommandType.GetChar);
				}));

				loop.ParseAction = new ParseAction (new Func<SyntaxElement, bool> (delegate(SyntaxElement arg) {
					switch (arg.Label) {
					case "]":
					case "[":
						return false;
					default:
						return true;
					}
				}));

				loop.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext scope, object[] args) {
					List<BrainfuckCommand> loopcmds = new List<BrainfuckCommand> (args.Length + 2);
					BrainfuckLoop left = new BrainfuckLoop (BfCommandType.LeftLoop),
					right = new BrainfuckLoop (BfCommandType.RightLoop);
					left.Jump = right;

					loopcmds.Add (left);
					foreach (var item in args) {
						if (item != null) {
							if (item is BrainfuckCommand) {
								loopcmds.Add ((BrainfuckCommand)item);
							} else { // BrainfuckCommand[]
								loopcmds.AddRange ((BrainfuckCommand[])item);
							}
						}
					}
					loopcmds.Add (right);

					return loopcmds.ToArray ();
				}));

				program.CompileAction = new CompileAction (new Func<ScopeContext, object[], object> (delegate(ScopeContext scope, object[] args) {
					List<BrainfuckCommand> loopcmds = new List<BrainfuckCommand> (args.Length + 2);

					foreach (var item in args) {
						if (item != null) {
							if (item is BrainfuckCommand) {
								loopcmds.Add ((BrainfuckCommand)item);
							} else { // BrainfuckCommand[]
								loopcmds.AddRange ((BrainfuckCommand[])item);
							}
						}
					}

					bool skip = true;
					BrainfuckCommand last = null;
					foreach (var item in loopcmds) {
						if (skip) {
							last = item;
							skip = false;
							continue;
						}
						last.Next = item;
						last = item;
					}
					Brainfuck store = new Brainfuck (loopcmds [0]);

					return store;
				}));

				return program;
			}
		}
	}
}

