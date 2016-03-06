using System;
using System.IO;
using System.Collections.Generic;

namespace Kekstoaster.Syntax
{
	/// <summary>
	/// A compiler that is able to parse a string or file against a given Ebnf element
	/// describing the syntax. After parsing was successfull, the text is compiled.
	/// Be aware that the compiler sets internal variables. Compiling two different
	/// files usually also needs two independent EbnfCompiler instances. 
	/// Whereas all EbnfCompiler instances can be initiated with the exact same
	/// Ebnf element instance.
	/// </summary>
	public sealed class EbnfCompiler
	{
		private Ebnf _root;
		private ItemList<object> _globals;
		private EbnfCompileBehavior _behav;
		private ScopeType _stdScope;
		private HashSet<string> _inits;
		private bool _compiling;
		private IDocumemtEncoder _encoder;

		/// <summary>
		/// This event is raised when the parsing process was successfull.
		/// </summary>
		public event EventHandler ParsingComplete;
		/// <summary>
		/// This event is raised after the entire Parsing and compiling
		/// completed without errors.
		/// </summary>
		public event EventHandler CompilingComplete;

		/// <summary>
		/// Initializes a new instance of the <see cref="Kekstoaster.Syntax.EbnfCompiler"/> class.
		/// </summary>
		/// <param name="root">The <see cref="Kekstoaster.Syntax.Ebnf"/> element containing the complete syntax.</param>
		/// <param name="standardScopeType">[Optional] The <see cref="Kekstoaster.Syntax.ScopeType"/> that is used,
		/// if no scope type was specified for a <see cref="Kekstoaster.Syntax.Ebnf"/>.</param>
		/// <param name="standardCompileBehavior">[Optional] The default compiling result that is returned
		/// if no CompileAction was set for a <see cref="Kekstoaster.Syntax.Ebnf"/> element.</param>
		/// /// <param name="encoder">[Optional] The encoding of the document being parsed.
		/// If no encoder was set for a <see cref="Kekstoaster.Syntax.Ebnf"/> element,
		/// every byte will be used as an individual character.</param>
		public EbnfCompiler (Ebnf root, ScopeType standardScopeType = ScopeType.Inhired, EbnfCompileBehavior standardCompileBehavior = EbnfCompileBehavior.Text, IDocumemtEncoder encoder = null)
		{
			this._root = root;
			this._globals = new ItemList<object> ();
			this._behav = standardCompileBehavior;
			this._stdScope = standardScopeType;
			this._inits = new HashSet<string> ();
			this._compiling = false;
			this._encoder = encoder;
		}

		/// <summary>
		/// Gets the standard compiling behavior if no CompileAction was set for a <see cref="Kekstoaster.Syntax.Ebnf"/> element.
		/// </summary>
		public EbnfCompileBehavior StandardCompile {
			get { return this._behav; }
		}

		/// <summary>
		/// Gets the standard scope that parsed <see cref="Kekstoaster.Syntax.Ebnf"/>
		/// elements have, if none was specified.
		/// </summary>
		public ScopeType StandardScope {
			get { return this._stdScope; }
		}

		/// <summary>
		/// Gets the <see cref="Kekstoaster.Syntax.IDocumemtEncoder"/> 
		/// that is used to retrieve the next char item from the
		/// supplied byte stream.
		/// </summary>
		public IDocumemtEncoder Encoder {
			get { return this._encoder; }
		}

		/// <summary>
		/// Gets the list of Global variables that is used during parsing and compiling actions.
		/// </summary>
		public ItemList<object> Globals {
			get {
				return this._globals;
			}
		}

		/// <summary>
		/// Compile the specified stream. This can be any stream that is readable
		/// and where the position pointer can be set to any position from the start position on.
		/// A real file stream is usually much slower than a memory stream.
		/// </summary>
		/// <param name="stream">The Stream that is used for compiling</param>
		/// <returns>The final compiling result. This could be anything.</returns>
		public object Compile (Stream stream)
		{
			if (!_compiling) {
				_compiling = true;
				object compile;
				if (stream.CanRead && stream.CanSeek) {			
					try {
						// Parse the Ebnf Element
						ScopeContext con = null;
						SyntaxElement parse = _root.Parse (stream, this, out con);
						// Fire event
						if (this.ParsingComplete != null) {
							this.ParsingComplete (this, EventArgs.Empty);
						}
						// use same base context for possible local variables
						compile = parse.Compile (con);
						// Fire event
						if (this.CompilingComplete != null) {
							this.CompilingComplete (this, EventArgs.Empty);
						}
					} catch (EbnfElementException ex) {
						// EbnfElement is internal and can only occur during parsing.
						// This happens, when the stream contains no valid syntax
						throw new ParseException (ErrorText (stream, ex));
					} catch (Exception) {
						// forward any other exception, usually ParseException or CompileException
						throw;
					} finally {
						_compiling = false;
					}

					return compile;
				} else {
					_compiling = false;
					throw new ArgumentException ("Stream must be readable and must support seaking", "stream");
				}
			} else {
				throw new CompileException ("Compiler already in use.");
			}
		}

		/// <summary>
		/// Compile the specified text.
		/// </summary>
		/// <param name="text">The text for compiling.</param>
		/// <returns>The final compiling result. This could be anything.</returns>
		public object Compile (string text)
		{
			MemoryStream stream = new MemoryStream ();
			StreamWriter writer = new StreamWriter (stream);
			writer.Write (text);
			writer.Flush ();

			stream.Position = 0;

			return Compile (stream);
		}

		/// <summary>
		/// Compiles the specified file.
		/// </summary>
		/// <returns>The final compiling result. This could be anything.</returns>
		/// <param name="path">The path to the file.</param>
		public object CompileFile (string path)
		{
			return Compile (File.ReadAllText (path));
		}

		// Hases all labels, that an Ebnf element has, and marks it as initialized.
		// For two different Ebnf elements with the same label, only for the first occuring
		// the initilize action is called. The other one is asumed to be already initilized.
		internal bool Initialization (string name)
		{
			if (_inits.Contains (name)) {
				return false;
			} else {
				_inits.Add (name);
				return true;
			}
		}

		// Create the Error message with line number and line position
		private static string ErrorText (Stream s, EbnfElementException ex)
		{
			string resStr;
			long pos = s.Position;
			s.Position = 0;
			int line = 1;
			int linePos = 1;
			int next;

			for (long i = 0; i < pos; ++i) {
				next = s.ReadByte ();
				linePos++;

				if (next == (int)'\n') {
					line++;
					linePos = 1;
				}
			}

			if (string.IsNullOrEmpty (ex.Message)) {
				resStr = string.Format ("syntax error on line {0}:{1}", line, linePos);
			} else {
				resStr = string.Format ("syntax error on line {0}:{1}\r\n{2}", line, linePos, ex.Message);
			}

			return resStr;
		}
	}
}